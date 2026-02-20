using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;

namespace SCP5K.Events
{
    /// <summary>
    /// 全局阵营分类枚举
    /// </summary>
    public enum FactionType
    {
        None,
        ClassD,      // 所有D级 (普通D级, D9341, 良子, 运动员)
        Nu7A,        // 落锤 A连
        Nu7B,        // 落锤 B连
        GOCTeam,     // GOC 打击小组
        GOCArcane,   // GOC 奇术小组
        CIGRU,       // CI-GRU (雷泽诺夫等)
        GRUCI,       // GRU-CI 特遣队
        SCP610,      // 血肉瘟疫
        SCP682,      // 不灭孽蜥
        Alpha9       // Alpha-9 最后的希望
    }

    /// <summary>
    /// 全局阵营管理器 - 负责自动调度玩家的入表和出表
    /// </summary>
    public static class FactionManager
    {
        // 核心字典，用 HashSet 保证性能和防重复
        private static readonly Dictionary<FactionType, HashSet<Player>> Factions = new Dictionary<FactionType, HashSet<Player>>();

        static FactionManager()
        {
            foreach (FactionType type in Enum.GetValues(typeof(FactionType)))
            {
                Factions[type] = new HashSet<Player>();
            }
        }

        // 快捷获取各个阵营的只读列表（供你其他代码调用）
        public static HashSet<Player> ClassDTeam => Factions[FactionType.ClassD];
        public static HashSet<Player> Nu7ATeam => Factions[FactionType.Nu7A];
        public static HashSet<Player> Nu7BTeam => Factions[FactionType.Nu7B];
        public static HashSet<Player> GOCTeam => Factions[FactionType.GOCTeam];
        public static HashSet<Player> GOCArcaneTeam => Factions[FactionType.GOCArcane];
        public static HashSet<Player> CIGRUTeam => Factions[FactionType.CIGRU];
        public static HashSet<Player> GRUCITaskForceTeam => Factions[FactionType.GRUCI];
        public static HashSet<Player> SCP610Team => Factions[FactionType.SCP610];
        public static HashSet<Player> SCP682Team => Factions[FactionType.SCP682];
        public static HashSet<Player> Alpha9Team => Factions[FactionType.Alpha9];

        /// <summary>
        /// 核心：将玩家加入某个阵营
        /// </summary>
        public static void AddPlayer(Player player, FactionType type)
        {
            if (player == null) return;
            RemovePlayer(player); // 确保不会同时处于两个阵营

            if (type != FactionType.None)
            {
                Factions[type].Add(player);
                Log.Debug($"已将玩家 {player.Nickname} 添加入 {type} 阵营表。");
            }
        }

        /// <summary>
        /// 核心：将玩家从所有阵营中移除 (死亡、离线、变旁观者时调用)
        /// </summary>
        public static void RemovePlayer(Player player)
        {
            if (player == null) return;
            foreach (var list in Factions.Values)
            {
                list.Remove(player);
            }
        }

        // ================= 事件注册 =================
        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        }

        private static void OnWaitingForPlayers()
        {
            foreach (var list in Factions.Values) list.Clear();
            Log.Info("[FactionManager] 回合重置：所有阵营表已清空。");
        }

        private static void OnPlayerDied(DiedEventArgs ev) => RemovePlayer(ev.Player);
        private static void OnPlayerLeft(LeftEventArgs ev) => RemovePlayer(ev.Player);
        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            RemovePlayer(ev.Player);
            // 如果玩家变成了原版的 D级人员，自动归入 D级阵营
            if (ev.NewRole == RoleTypeId.ClassD)
            {
                AddPlayer(ev.Player, FactionType.ClassD);
            }
        }
    }
}