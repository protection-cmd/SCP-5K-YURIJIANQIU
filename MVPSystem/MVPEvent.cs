using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp079;
using MEC;
using PlayerRoles;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using System.Linq;
using System;
using Hint = HintServiceMeow.Core.Models.Hints.Hint;

namespace SCP5K.MVPSystem
{
    public class MvpEvent
    {
        public static Dictionary<Player, int> PlayerDamageRecord = new Dictionary<Player, int>();
        public static Dictionary<Player, int> PlayerKillRecord = new Dictionary<Player, int>();
        public static Dictionary<Player, int> PlayerActualKills = new Dictionary<Player, int>();
        public static Dictionary<Player, float> PlayerScp079ExpRecord = new Dictionary<Player, float>();

        // 添加对MVPConfigManager的引用
        private MVPConfigManager _configManager;

        public MvpEvent(MVPConfigManager configManager = null)
        {
            _configManager = configManager;
        }

        public void SetConfigManager(MVPConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void WaitingForPlayer()
        {
            PlayerDamageRecord.Clear();
            PlayerKillRecord.Clear();
            PlayerActualKills.Clear();
            PlayerScp079ExpRecord.Clear();
        }

        public void Verified(VerifiedEventArgs ev)
        {
            if (ev.Player != null)
            {
                Timing.CallDelayed(0.5f, delegate ()
                {
                    if (!PlayerDamageRecord.ContainsKey(ev.Player))
                    {
                        PlayerDamageRecord[ev.Player] = 0;
                    }
                    if (!PlayerKillRecord.ContainsKey(ev.Player))
                    {
                        PlayerKillRecord[ev.Player] = 0;
                    }
                    if (!PlayerActualKills.ContainsKey(ev.Player))
                    {
                        PlayerActualKills[ev.Player] = 0;
                    }
                    if (!PlayerScp079ExpRecord.ContainsKey(ev.Player))
                    {
                        PlayerScp079ExpRecord[ev.Player] = 0;
                    }

                    // 检查玩家是否有MVP音乐配置并显示提示
                    CheckAndShowMvpMusicHint(ev.Player);
                });
            }
        }

        // 新方法：检查并显示MVP音乐提示
        private void CheckAndShowMvpMusicHint(Player player)
        {
            if (player == null || _configManager == null)
            {
                return;
            }

            // 只在MVP系统启用时检查
            if (!SCP5K.Plugin.Instance.Config.IsEnableMVP)
            {
                return;
            }

            // 延迟一点时间确保玩家完全进入
            Timing.CallDelayed(1f, () =>
            {
                try
                {
                    // 获取玩家的音乐配置
                    var musicPaths = _configManager.GetMusicPathsForPlayer(player.UserId);

                    if (musicPaths != null && musicPaths.Any())
                    {
                        int musicCount = musicPaths.Count;

                        // 构建提示文本
                        string hintText = $"<color=#FFD700><size=30>🎵 {musicCount}个MVP音乐已成功加载</size></color>";

                        // 显示提示
                        ShowMvpMusicHint(player, hintText);

                        Log.Debug($"玩家 {player.Nickname} 已加载 {musicCount} 个MVP音乐");
                    }
                    else
                    {
                        Log.Debug($"玩家 {player.Nickname} 没有配置MVP音乐");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"检查MVP音乐配置时出错: {ex.Message}");
                }
            });
        }

        // 新方法：显示MVP音乐提示
        private void ShowMvpMusicHint(Player player, string hintText)
        {
            try
            {
                var display = PlayerDisplay.Get(player);
                if (display == null)
                {
                    Log.Error($"无法获取玩家 {player.Nickname} 的PlayerDisplay");
                    return;
                }

                var hint = new Hint
                {
                    Text = hintText,
                    FontSize = 30,
                    Alignment = HintAlignment.Right,
                    YCoordinate = 800 // 在屏幕上方显示
                };

                // 添加提示
                display.AddHint(hint);

                // 延迟5秒后隐藏提示
                Timing.CallDelayed(5f, () =>
                {
                    try
                    {

                        hint.Hide = true;
                            Log.Debug($"已隐藏玩家 {player.Nickname} 的MVP音乐提示");
                        
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"隐藏MVP音乐提示时出错: {ex.Message}");
                    }
                });

                Log.Debug($"已向玩家 {player.Nickname} 显示MVP音乐提示，将在5秒后隐藏");
            }
            catch (Exception ex)
            {
                Log.Error($"显示MVP音乐提示时出错: {ex.Message}");
            }
        }

        // 监听SCP079获得经验事件
        public void OnGainingExperience(GainingExperienceEventArgs ev)
        {
            if (ev.Player != null)
            {
                // 记录玩家获得的经验值
                PlayerScp079ExpRecord[ev.Player] = PlayerScp079ExpRecord.ContainsKey(ev.Player) ?
                    PlayerScp079ExpRecord[ev.Player] + ev.Amount :
                    ev.Amount;

                if (SCP5K.Plugin.Instance.Config.Debug)
                {
                    Log.Info($"SCP079 {ev.Player.Nickname} 获得经验: {ev.Amount}, 总经验: {PlayerScp079ExpRecord[ev.Player]}");
                }
            }
        }

        public void Dying(DyingEventArgs ev)
        {
            if (ev.Player != null && ev.Attacker != null && ev.Attacker != ev.Player)
            {
                // 记录实际击杀数（无论目标类型）
                PlayerActualKills[ev.Attacker] = PlayerActualKills.TryGetValue(ev.Attacker, out var actualKills) ? actualKills + 1 : 1;

                // 计算加权击杀分数
                int killPoints = 1;

                // 检查目标是否为SCP（且不是0492）
                if (ev.Player.IsScp && ev.Player.Role != RoleTypeId.Scp0492)
                {
                    killPoints = 3;
                }

                // 更新击杀记录
                PlayerKillRecord[ev.Attacker] = PlayerKillRecord.TryGetValue(ev.Attacker, out var currentKills) ?
                    currentKills + killPoints :
                    killPoints;
            }
        }

        public void Hurting(HurtingEventArgs ev)
        {
            if (ev.Player != null && ev.Attacker != null && ev.Attacker != ev.Player)
            {
                Timing.RunCoroutine(ProcessDamage(ev.Attacker, (int)ev.Amount));
            }
        }

        private IEnumerator<float> ProcessDamage(Player attacker, int damageAmount)
        {
            yield return Timing.WaitForOneFrame;

            if (attacker == null || !attacker.IsConnected) yield break;

            if (PlayerDamageRecord.ContainsKey(attacker))
            {
                PlayerDamageRecord[attacker] += damageAmount;
            }
            else
            {
                PlayerDamageRecord[attacker] = damageAmount;
            }
        }
    }
}