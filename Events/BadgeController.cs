using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K
{
    public static class BadgeController
    {
        // 存储玩家原始称号信息
        private static Dictionary<string, (string name, string color)> originalPlayerData = new Dictionary<string, (string, string)>();
        private static bool isEnabled = false;

        /// <summary>
        /// 初始化徽章控制器
        /// </summary>
        public static void Initialize()
        {
            isEnabled = Plugin.Instance.Config.RemoveBadgesOnRoundStart;

            if (isEnabled)
            {
                RegisterEvents();
                Log.Debug("徽章控制器已初始化并启用");
            }
            else
            {
                Log.Debug("徽章控制器已初始化但未启用");
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        public static void RegisterEvents()
        {
            if (!isEnabled) return;

            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoined;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;

            Log.Debug("徽章控制器事件已注册");
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoined;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;

            Log.Debug("徽章控制器事件已注销");
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void ReloadConfig()
        {
            bool newEnabled = Plugin.Instance.Config.RemoveBadgesOnRoundStart;

            if (newEnabled != isEnabled)
            {
                if (newEnabled)
                {
                    RegisterEvents();
                }
                else
                {
                    UnregisterEvents();
                }
                isEnabled = newEnabled;
            }

            Log.Debug($"徽章控制器配置已重新加载: {(isEnabled ? "启用" : "禁用")}");
        }

        private static void OnRoundStarted()
        {
            if (!isEnabled) return;

            Log.Info("回合开始，正在清除所有玩家称号...");

            // 清除所有现有玩家的称号
            foreach (Player player in Player.List)
            {
                RemovePlayerBadge(player);
            }

            Log.Info($"已清除 {Player.List.Count()} 名玩家的称号");
        }

        private static void OnPlayerJoined(JoinedEventArgs ev)
        {
            if (!isEnabled) return;

            // 新玩家加入时立即清除称号
            Timing.CallDelayed(0.01f, () =>
            {
                if (ev.Player != null && ev.Player.IsConnected)
                {
                    RemovePlayerBadge(ev.Player);
                    Log.Debug($"已清除新玩家 {ev.Player.Nickname} 的称号");
                }
            });
        }

        private static void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (!isEnabled) return;

            // 玩家验证通过后清除称号
            Timing.CallDelayed(0.01f, () =>
            {
                if (ev.Player != null && ev.Player.IsConnected)
                {
                    RemovePlayerBadge(ev.Player);
                    Log.Debug($"已清除已验证玩家 {ev.Player.Nickname} 的称号");
                }
            });
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!isEnabled) return;

            // 玩家切换角色时清除称号
            Timing.CallDelayed(0.01f, () =>
            {
                if (ev.Player != null && ev.Player.IsConnected)
                {
                    RemovePlayerBadge(ev.Player);
                    Log.Debug($"已清除切换角色玩家 {ev.Player.Nickname} 的称号");
                }
            });
        }

        /// <summary>
        /// 移除玩家称号
        /// </summary>
        private static void RemovePlayerBadge(Player player)
        {
            try
            {
                // 保存玩家原始数据（如果需要恢复）
                string playerKey = $"{player.UserId}_{player.Nickname}";
                if (!originalPlayerData.ContainsKey(playerKey) &&
                    (!string.IsNullOrEmpty(player.RankName) || !string.IsNullOrEmpty(player.RankColor)))
                {
                    originalPlayerData[playerKey] = (player.RankName, player.RankColor);
                }

                // 清除称号和颜色
                player.RankName = string.Empty;
                player.RankColor = string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error($"清除玩家 {player.Nickname} 称号时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 恢复玩家原始称号（API方法，供其他插件调用）
        /// </summary>
        public static void RestorePlayerBadge(Player player)
        {
            try
            {
                string playerKey = $"{player.UserId}_{player.Nickname}";
                if (originalPlayerData.ContainsKey(playerKey))
                {
                    var (name, color) = originalPlayerData[playerKey];
                    player.RankName = name;
                    player.RankColor = color;
                    originalPlayerData.Remove(playerKey);
                    Log.Debug($"已恢复玩家 {player.Nickname} 的原始称号");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"恢复玩家 {player.Nickname} 称号时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 强制清除所有玩家称号（API方法，供其他插件调用）
        /// </summary>
        public static void ForceRemoveAllBadges()
        {
            foreach (Player player in Player.List)
            {
                RemovePlayerBadge(player);
            }
            Log.Info($"已强制清除 {Player.List.Count()} 名玩家的称号");
        }

        /// <summary>
        /// 回合结束时清理数据
        /// </summary>
        public static void OnRoundEnded()
        {
            originalPlayerData.Clear();
            Log.Debug("回合结束，已清理徽章控制器数据");
        }
    }
}