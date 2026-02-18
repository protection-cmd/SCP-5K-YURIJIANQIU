using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K.LCZRole
{
    public class LiangziRole : CustomRole
    {
        public static LiangziRole Instance { get; } = new LiangziRole();
        public override uint Id { get; set; } = 901;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override string Name { get; set; } = "良子";
        public override string CustomInfo { get; set; } = "良子";
        public override int MaxHealth { get; set; } = 250;

        public override string Description { get; set; } 

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                player.ShowHint($"<color=orange>你被选为良子！\n血量提升至250</color>", 10f);
            });
        }
    }

    public static class DDpig
    {
        // 这里的自动刷新逻辑已移除，转由 ClassDSpawnManager 统一管理
        // 仅保留工具方法供管理器调用

        public static bool IsSpecialDClass(Player player) => LiangziRole.Instance.Check(player);

        public static bool SetPlayerAsSpecialDClass(Player player)
        {
            LiangziRole.Instance.AddRole(player);
            return true;
        }

        public static void RegisterEvents()
        {
            // 不再需要注册回合开始事件
        }

        public static void UnregisterEvents()
        {
            // 不再需要注销回合开始事件
        }
    }
}