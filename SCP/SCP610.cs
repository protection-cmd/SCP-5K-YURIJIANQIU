using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;

namespace SCP5K.SCPFouRole
{
    public class SCP610MotherRole : CustomRole
    {
        public static SCP610MotherRole Instance { get; } = new SCP610MotherRole();
        public override uint Id { get; set; } = 611;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override string Name { get; set; } = "SCP-610-母体";
        public override string CustomInfo { get; set; } = "SCP-610-母体";
        public override int MaxHealth { get; set; } = 1000;

        public override string Description { get; set; } = "SCP-610 - 母体\n\n<color=red>生命值: 1000</color>\n能力: 附近所有SCP-610成员获得增强";

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                var spawn610 = Room.Get(RoomType.Hcz049);
                player.Position = spawn610 != null ? spawn610.Position + Vector3.up : new Vector3(59.3f, 992.9f, -42.3f);
                player.ShowHint("你已成为SCP-610母体\n\n生命值: 1000\n能力: 附近所有SCP-610成员获得增强\n你的存在会加速血肉的传播！", 10f);
            });
            SCP610.SetMother(player);
        }

        protected override void RoleRemoved(Player player)
        {
            SCP610.ClearMother(player);
            base.RoleRemoved(player);
        }
    }

    public class SCP610SprayerRole : CustomRole
    {
        public static SCP610SprayerRole Instance { get; } = new SCP610SprayerRole();
        public override uint Id { get; set; } = 612;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override string Name { get; set; } = "SCP-610-喷射体";
        public override string CustomInfo { get; set; } = "SCP-610-喷射体";
        public override int MaxHealth { get; set; } = 600;

        public override string Description { get; set; } = "SCP-610 - 喷射体\n\n<color=red>生命值: 600</color>\n配有COM15手枪";

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType> { ItemType.GunCOM15 };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                var spawn610 = Room.Get(RoomType.Hcz049);
                player.Position = spawn610 != null ? spawn610.Position + Vector3.up : new Vector3(59.3f, 992.9f, -42.3f);
                player.ShowHint("你已成为SCP-610喷射体\n\n生命值: 600\n武器: COM15手枪\n能力: 使用武器快速传播感染", 10f);
            });
        }
    }

    public class SCP610ChildRole : CustomRole
    {
        public static SCP610ChildRole Instance { get; } = new SCP610ChildRole();
        public override uint Id { get; set; } = 613;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp0492;
        public override string Name { get; set; } = "SCP-610-子个体";
        public override string CustomInfo { get; set; } = "SCP-610-子个体";
        public override int MaxHealth { get; set; } = 400;

        public override string Description { get; set; } = "SCP-610 - 子个体\n\n<color=red>生命值: 400</color>";

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.EnableEffect(EffectType.Bleeding, 1, 3f);
                player.EnableEffect(EffectType.Hemorrhage, 1, 3f);
                player.ShowHint("你已被SCP-610感染！\n\n生命值: 400\n你现在是血肉瘟疫的一部分\n杀死更多人类来传播感染！", 10f);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.Bleeding);
            player.DisableEffect(EffectType.Hemorrhage);
            base.RoleRemoved(player);
        }
    }

    public static class SCP610
    {
        private static Player motherEntity = null;

        public static void SetMother(Player player) => motherEntity = player;
        public static void ClearMother(Player player) { if (motherEntity == player) motherEntity = null; }

        public static bool IsSCP610(Player player)
        {
            return SCP610MotherRole.Instance.Check(player) || SCP610SprayerRole.Instance.Check(player) || SCP610ChildRole.Instance.Check(player);
        }

        public static bool SpawnSCP610Team(List<Player> players)
        {
            if (players.Count != 2) return false;
            SCP610MotherRole.Instance.AddRole(players[0]);
            SCP610SprayerRole.Instance.AddRole(players[1]);
            return true;
        }

        public static bool SpawnMotherEntity(Player player) { SCP610MotherRole.Instance.AddRole(player); return true; }
        public static bool SpawnSprayer(Player player) { SCP610SprayerRole.Instance.AddRole(player); return true; }

        public static bool ConvertToChild(Player player)
        {
            if (IsSCP610(player)) return true;
            SCP610ChildRole.Instance.AddRole(player);
            return true;
        }

        private static void OnPlayerDied(DyingEventArgs ev)
        {
            if (ev.Attacker != null && IsSCP610(ev.Attacker) && ev.Player != null)
            {
                Vector3 deathPosition = ev.Player.Position;
                Timing.CallDelayed(0.1f, () =>
                {
                    if (ev.Player != null && ev.Player.IsConnected)
                    {
                        ConvertToChild(ev.Player);
                        Timing.CallDelayed(0.6f, () => { if (ev.Player.IsConnected) ev.Player.Position = deathPosition; });
                    }
                });
            }
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev) => motherEntity = null;

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Dying += OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDied;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            motherEntity = null;
        }
    }
}