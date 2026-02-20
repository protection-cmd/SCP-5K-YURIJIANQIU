using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities;
using MEC;
using System;
using System.Linq;
using YuRiLS.PlayerDataSystem;

namespace SCP5K.MVPSystem
{
    public class MVPSystem
    {
        private readonly MusicPlayer _musicPlayer;

        public MVPSystem(MusicPlayer musicPlayer)
        {
            _musicPlayer = musicPlayer;
        }

        public void RoundEnded(RoundEndedEventArgs ev)
        {
            Player mvpPlayer = null;
            int mvpScore = 0;
            int mvpKills = 0;
            int mvpDamage = 0;

            if (Plugin.Instance.Config.IsEnableMVP)
            {
                var playerScores = CalculatePlayerScores();

                if (playerScores.Any())
                {
                    var mvpEntry = playerScores.OrderByDescending(kv => kv.Value).First();
                    mvpPlayer = mvpEntry.Key;
                    mvpScore = (int)Math.Round(mvpEntry.Value);

                    // 获取MVP的具体数据
                    mvpKills = MvpEvent.PlayerActualKills.ContainsKey(mvpPlayer) ?
                        MvpEvent.PlayerActualKills[mvpPlayer] : 0;
                    mvpDamage = MvpEvent.PlayerDamageRecord.ContainsKey(mvpPlayer) ?
                        MvpEvent.PlayerDamageRecord[mvpPlayer] : 0;

                    // 1. 显示MVP公告
                    AnnounceMVP(mvpPlayer, mvpScore, mvpKills, mvpDamage);

                    // 2. ★【核心修改】直接调用 YuRiLS API 发放奖励 ★
                    // 不再依赖文件读写，直接通过代码通讯
                    if (mvpPlayer != null)
                    {
                        try
                        {
                            // 发放 10 经验
                            PlayerDataManager.Instance.AddExperience(mvpPlayer.UserId, 10);

                            // 发放 2 迷途 (CurrencyA)
                            PlayerDataManager.Instance.AddCurrencyA(mvpPlayer.UserId, 2);

                            Log.Info($"[SCP-5K] 已通过API向 YuRiLS 发送 MVP 奖励 (10经验, 2迷途) - 玩家: {mvpPlayer.Nickname}");

                            // (可选) 给玩家发个提示
                            mvpPlayer.ShowHint("<color=#FFD700>获得 MVP 奖励: +10 经验, +2 迷途</color>", 5f);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[SCP-5K] 调用 YuRiLS API 失败 (请检查 YuRiLS 是否已安装): {ex}");
                        }
                    }

                    // 3. (可选) 依然写入记录文件作为备份，或者你可以注释掉下面这块
                    /*
                    bool recordWritten = MVPRoundRecord.Instance.WriteMVPRecord(mvpPlayer, mvpScore, mvpKills, mvpDamage);
                    if (recordWritten)
                    {
                        Log.Info($"MVP记录已保存 (本地备份)");
                    }
                    */

                    // 延迟显示排行榜和个人数据
                    Timing.CallDelayed(1f, () => ShowTopPlayers(playerScores));
                    Timing.CallDelayed(1f, () => ShowPersonalStatsToEachPlayer(playerScores));
                }
                else
                {
                    AnnounceNoMVP();
                    Timing.CallDelayed(2f, () => ShowPersonalStatsToEachPlayer(playerScores));
                }
            }
            else
            {
                Timing.CallDelayed(2f, () => ShowPersonalStatsToEachPlayer(CalculatePlayerScores()));
            }
        }

        // --- 下面的代码保持不变 ---

        private Dictionary<Player, double> CalculatePlayerScores()
        {
            var playerScores = new Dictionary<Player, double>();

            foreach (var player in Player.List)
            {
                double finalScore = 0;
                int killScore = MvpEvent.PlayerKillRecord.ContainsKey(player) ? MvpEvent.PlayerKillRecord[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ? MvpEvent.PlayerDamageRecord[player] : 0;
                finalScore = killScore + (damage * 0.01);
                playerScores[player] = finalScore;
            }
            return playerScores;
        }

        private void ShowTopPlayers(Dictionary<Player, double> playerScores)
        {
            var topPlayers = playerScores.OrderByDescending(kv => kv.Value).Take(5).ToList();
            string leaderboardText = "<align=right><size=35><color=#FFFF00>=== 本局得分排行榜 ===</color></size>\n<size=30>";

            for (int i = 0; i < topPlayers.Count; i++)
            {
                var player = topPlayers[i].Key;
                double score = topPlayers[i].Value;
                string formattedScore = FormatScore(score);
                int kills = MvpEvent.PlayerActualKills.ContainsKey(player) ? MvpEvent.PlayerActualKills[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ? MvpEvent.PlayerDamageRecord[player] : 0;
                string playerInfo = $"{i + 1}. {player.Nickname}\n   击杀: {kills} | 伤害: {damage} | 得分: {formattedScore}\n";
                string color = i == 0 ? "#FFD700" : "#FFFFFF";
                leaderboardText += $"<color={color}>{playerInfo}</color>";
            }
            leaderboardText += "</size>";
            ShowHintToAllPlayers(leaderboardText, true, 10f);
        }

        private string FormatScore(double score)
        {
            double rounded = Math.Round(score, 1);
            return (rounded % 1 == 0) ? $"{(int)rounded}" : $">{rounded:0.0}";
        }

        private void ShowHintToAllPlayers(string hintText, bool isLeaderboard = false, float duration = 5f)
        {
            foreach (Player player in Player.List)
            {
                var display = PlayerDisplay.Get(player);
                if (display == null) continue;

                var hint = new HintServiceMeow.Core.Models.Hints.Hint
                {
                    Text = hintText,
                    FontSize = 0,
                    Alignment = isLeaderboard ? HintAlignment.Right : HintAlignment.Center,
                    YCoordinate = isLeaderboard ? 640 : 115
                };
                display.AddHint(hint);
                Timing.CallDelayed(duration, () => { try { hint.Hide = true; } catch (Exception ex) { Log.Error($"隐藏提示时出错: {ex.Message}"); } });
            }
        }

        private void AnnounceMVP(Player mvpPlayer, int mvpScore, int mvpKills, int mvpDamage)
        {
            string formattedScore = FormatScore(mvpScore);
            Log.Info($"MVP是{mvpPlayer.Nickname}，击杀: {mvpKills}, 伤害: {mvpDamage}, 得分: {formattedScore}");

            Timing.CallDelayed(0.5f, () =>
            {
                string musicName = _musicPlayer.TryPlayMusic(mvpPlayer);
                string hintText = CreateMVPAnnouncementText(mvpPlayer, formattedScore, musicName, mvpKills, mvpDamage);
                ShowHintToAllPlayers(hintText, false, 15f);
            });
        }

        private string CreateMVPAnnouncementText(Player mvpPlayer, string formattedScore, string musicName, int kills, int damage)
        {
            string text = $"<size=50><color=#FC0000>本局MVP</color>是 <color=#FFD700>{mvpPlayer.Nickname}</color></size>\n";
            text += $"<size=40>击杀: <color=#FF1493>{kills}</color> | 伤害: <color=red>{damage}</color></size>\n";
            text += $"<size=40>得分: <color=#00FF00>{formattedScore}</color></size>\n<size=40><color=#FF8899>感谢游玩!我们一同于聿日箋秋中，描绘崭新未来!</color></size>";
            if (!string.IsNullOrEmpty(musicName)) text += $"\n<size=35>正在播放MVP专属音乐: <color=#00FFFF>{musicName}</color></size>";
            return text;
        }

        private void AnnounceNoMVP()
        {
            Log.Info("本局没有符合条件的MVP或MVP数据无效");
            string hintText = "<size=50>本局<color=#FC0000>MVP</color>：虚位以待</size>\n<size=40><color=#FF8899>感谢游玩!我们一同于聿日箋秋中，描绘崭新未来!</color></size>";
            ShowHintToAllPlayers(hintText, false, 8f);
        }

        private void ShowPersonalStatsToEachPlayer(Dictionary<Player, double> playerScores)
        {
            foreach (var player in Player.List)
            {
                int kills = MvpEvent.PlayerActualKills.ContainsKey(player) ? MvpEvent.PlayerActualKills[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ? MvpEvent.PlayerDamageRecord[player] : 0;
                double score = playerScores.ContainsKey(player) ? playerScores[player] : 0;
                string formattedScore = FormatScore(score);

                string personalText = $"<size=25><color=#77DD77>你的本局数据</color>\n击杀: <color=#FF1493>{kills}</color> | 伤害: <color=red>{damage}</color>\n得分: <color=#00FFFF>{formattedScore}</color></size>";
                ShowPersonalHint(player, personalText, 25, 15f, HintAlignment.Right, 860);
            }
        }

        private void ShowPersonalHint(Player player, string hintText, int FontSize, float duration, HintAlignment Alignment, int YCoordinate, int XCoordinate = 0)
        {
            var display = PlayerDisplay.Get(player);
            if (display == null) return;

            var hint = new HintServiceMeow.Core.Models.Hints.Hint
            {
                Text = hintText,
                FontSize = FontSize,
                Alignment = Alignment,
                YCoordinate = YCoordinate,
                XCoordinate = XCoordinate
            };
            display.AddHint(hint);
            Timing.CallDelayed(duration, () =>
            {
                try { hint.Hide = true; display.RemoveHint(hint); } catch (Exception ex) { Log.Error($"隐藏个人数据提示时出错: {ex.Message}"); }
            });
        }
    }
}