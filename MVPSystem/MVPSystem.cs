using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using MEC;
using System;
using System.Linq;

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

                    // 首先显示MVP公告
                    AnnounceMVP(mvpPlayer, mvpScore, mvpKills, mvpDamage);

                    // 写入MVP记录文件
                    bool recordWritten = MVPRoundRecord.Instance.WriteMVPRecord(mvpPlayer, mvpScore, mvpKills, mvpDamage);
                    if (recordWritten)
                    {
                        Log.Info($"MVP记录已保存，等待LS插件处理");
                    }

                    // 延迟2秒显示排行榜
                    Timing.CallDelayed(1f, () => ShowTopPlayers(playerScores));

                    // 延迟4秒显示个人数据
                    Timing.CallDelayed(1f, () => ShowPersonalStatsToEachPlayer(playerScores));
                }
                else
                {
                    AnnounceNoMVP();
                    // 即使没有MVP也显示个人数据
                    Timing.CallDelayed(2f, () => ShowPersonalStatsToEachPlayer(playerScores));
                }
            }
            else
            {
                // 如果MVP系统未启用，也显示个人数据
                Timing.CallDelayed(2f, () => ShowPersonalStatsToEachPlayer(CalculatePlayerScores()));
            }
        }

        private Dictionary<Player, double> CalculatePlayerScores()
        {
            var playerScores = new Dictionary<Player, double>();

            foreach (var player in Player.List)
            {
                double finalScore = 0;

                // 基础分数计算（击杀+伤害）
                int killScore = MvpEvent.PlayerKillRecord.ContainsKey(player) ? MvpEvent.PlayerKillRecord[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ? MvpEvent.PlayerDamageRecord[player] : 0;
                finalScore = killScore + (damage * 0.01);

                playerScores[player] = finalScore;
            }

            return playerScores;
        }

        // 显示得分前五名的方法
        private void ShowTopPlayers(Dictionary<Player, double> playerScores)
        {
            // 获取得分最高的前5名玩家
            var topPlayers = playerScores
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .ToList();

            // 构建排行榜文本
            string leaderboardText = "<align=right><size=35><color=#FFFF00>=== 本局得分排行榜 ===</color></size>\n";
            leaderboardText += "<size=30>"; // 排行榜内容使用30号字

            for (int i = 0; i < topPlayers.Count; i++)
            {
                var player = topPlayers[i].Key;
                double score = topPlayers[i].Value;
                string formattedScore = FormatScore(score);

                int kills = MvpEvent.PlayerActualKills.ContainsKey(player) ?
                    MvpEvent.PlayerActualKills[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ?
                    MvpEvent.PlayerDamageRecord[player] : 0;

                string playerInfo = $"{i + 1}. {player.Nickname}\n   击杀: {kills} | 伤害: {damage} | 得分: {formattedScore}\n";

                // 根据名次添加不同颜色
                string color = i == 0 ? "#FFD700" : "#FFFFFF"; // 第一名金色，其他白色
                leaderboardText += $"<color={color}>{playerInfo}</color>";
            }

            leaderboardText += "</size>"; // 结束内容部分

            // 显示排行榜（10秒后隐藏）
            ShowHintToAllPlayers(leaderboardText, true, 10f);
        }

        // 格式化得分的方法
        private string FormatScore(double score)
        {
            // 四舍五入到1位小数
            double rounded = Math.Round(score, 1);

            // 检查小数部分是否为0
            if (rounded % 1 == 0)
            {
                // 整数部分直接显示
                return $"{(int)rounded}";
            }
            else
            {
                // 有小数时显示为 ">X.X" 格式
                return $">{rounded:0.0}";
            }
        }

        // 更新方法：添加duration参数控制显示时间
        private void ShowHintToAllPlayers(string hintText, bool isLeaderboard = false, float duration = 5f)
        {
            foreach (Player player in Player.List)
            {
                var display = PlayerDisplay.Get(player);
                if (display == null) continue;

                var hint = new HintServiceMeow.Core.Models.Hints.Hint
                {
                    Text = hintText,
                    FontSize = 0, // 禁用全局设置，完全使用文本内标签
                    Alignment = isLeaderboard ? HintAlignment.Right : HintAlignment.Center,
                    // MVP公告在顶部(Y=20)，排行榜在右侧居中(Y=500)
                    YCoordinate = isLeaderboard ? 640 : 115
                };

                // 添加提示并保存引用
                display.AddHint(hint);

                // 延迟指定时间后隐藏提示
                Timing.CallDelayed(duration, () =>
                {
                    try
                    {
                        hint.Hide = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"隐藏提示时出错: {ex.Message}");
                    }
                });
            }
        }

        // 更新AnnounceMVP方法
        private void AnnounceMVP(Player mvpPlayer, int mvpScore, int mvpKills, int mvpDamage)
        {
            string formattedScore = FormatScore(mvpScore);
            Log.Info($"MVP是{mvpPlayer.Nickname}，击杀: {mvpKills}, 伤害: {mvpDamage}, 得分: {formattedScore}");

            Timing.CallDelayed(0.5f, () =>
            {
                string musicName = _musicPlayer.TryPlayMusic(mvpPlayer);
                string hintText = CreateMVPAnnouncementText(mvpPlayer, formattedScore, musicName, mvpKills, mvpDamage);

                // MVP公告显示15秒
                ShowHintToAllPlayers(hintText, false, 15f);
            });
        }

        // 更新创建MVP公告文本的方法
        private string CreateMVPAnnouncementText(Player mvpPlayer, string formattedScore, string musicName, int kills, int damage)
        {
            // 使用50号字显示MVP公告
            string text = $"<size=50><color=#FC0000>本局MVP</color>是 <color=#FFD700>{mvpPlayer.Nickname}</color></size>\n";
            text += $"<size=40>击杀: <color=#FF1493>{kills}</color> | 伤害: <color=red>{damage}</color></size>\n";
            text += $"<size=40>得分: <color=#00FF00>{formattedScore}</color></size>\n<size=40><color=#FF8899>感谢游玩!我们一同于聿日箋秋中，描绘崭新未来!</color></size>";

            if (!string.IsNullOrEmpty(musicName))
            {
                text += $"\n<size=35>正在播放MVP专属音乐: <color=#00FFFF>{musicName}</color></size>";
            }

            return text;
        }

        private void AnnounceNoMVP()
        {
            Log.Info("本局没有符合条件的MVP或MVP数据无效");
            string hintText = "<size=50>本局<color=#FC0000>MVP</color>：虚位以待</size>\n<size=40><color=#FF8899>感谢游玩!我们一同于聿日箋秋中，描绘崭新未来!</color></size>";

            // 无MVP提示显示8秒
            ShowHintToAllPlayers(hintText, false, 8f);
        }

        // 新增：为每个玩家显示个人数据
        private void ShowPersonalStatsToEachPlayer(Dictionary<Player, double> playerScores)
        {
            foreach (var player in Player.List)
            {
                // 获取玩家数据
                int kills = MvpEvent.PlayerActualKills.ContainsKey(player) ?
                    MvpEvent.PlayerActualKills[player] : 0;
                int damage = MvpEvent.PlayerDamageRecord.ContainsKey(player) ?
                    MvpEvent.PlayerDamageRecord[player] : 0;

                // 获取玩家得分
                double score = playerScores.ContainsKey(player) ?
                    playerScores[player] : 0;
                string formattedScore = FormatScore(score);

                // 构建个人数据文本
                string personalText = $"<size=25><color=#77DD77>你的本局数据</color>\n";
                personalText += $"击杀: <color=#FF1493>{kills}</color> | ";
                personalText += $"伤害: <color=red>{damage}</color>\n";
                personalText += $"得分: <color=#00FFFF>{formattedScore}</color></size>";

                // 显示个人数据提示 - 右下角（显示15秒）
                ShowPersonalHint(player, personalText, 25,15f, HintAlignment.Right,860); // 在排行榜下方显示
            }
        }

        // 更新：显示个人数据提示，添加duration参数
        private void ShowPersonalHint(Player player, string hintText, int FontSize , float duration, HintAlignment Alignment,int YCoordinate,int XCoordinate=0)
        {
            var display = PlayerDisplay.Get(player);
            if (display == null) return;


            var hint = new HintServiceMeow.Core.Models.Hints.Hint
            {
                Text = hintText,
                FontSize = FontSize,
                Alignment = Alignment,
                YCoordinate = YCoordinate ,
                XCoordinate = XCoordinate
            };

            display.AddHint(hint);

            // 延迟指定时间后隐藏提示
            Timing.CallDelayed(duration, () =>
            {
                try
                {
                    hint.Hide = true;
                    display.RemoveHint(hint);
                }
                catch (Exception ex)
                {
                    Log.Error($"隐藏个人数据提示时出错: {ex.Message}");
                }
            });
        }
    }
}