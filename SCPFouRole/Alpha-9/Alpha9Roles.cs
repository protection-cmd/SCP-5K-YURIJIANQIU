using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using PlayerRoles;
using SCP5K.Events;
using System.Collections.Generic;

namespace SCP5K.SCPFouRole
{
    public class SCP105 : CustomRole
    {
        public override uint Id { get; set; } = 140;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override int MaxHealth { get; set; } = 300;
        public override string Name { get; set; } = "SCP-105-鸢尾";
        public override string Description { get; set; }
        public override string CustomInfo { get; set; } = "SCP-105-鸢尾";

        public virtual List<ItemType> inventory { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardO5, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.Medkit, ItemType.Radio
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            player.EnableEffect(EffectType.Slowness, 20);
            player.EnableEffect(EffectType.Scp1344);
        }
    }

    public class SCP076 : CustomRole
    {
        public override uint Id { get; set; } = 141;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override int MaxHealth { get; set; } = 200;
        public override string Name { get; set; } = "SCP-076-2-亚伯";
        public override string Description { get; set; }
        public override string CustomInfo { get; set; } = "SCP-076-2-亚伯";

        public virtual List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.SCP1509, ItemType.KeycardO5, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.SCP500, ItemType.Radio
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            player.HumeShield = 50; // 休谟护盾
        }
    }

    public class A9CombatAgent : CustomRole
    {
        public override uint Id { get; set; } = 142;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override int MaxHealth { get; set; } = 150;
        public override string Name { get; set; } = "Alpha-9 战斗特工";
        public override string Description { get; set; }
        public override string CustomInfo { get; set; } = "Alpha-9 战斗特工";

        public virtual List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardO5, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.Medkit, ItemType.Radio
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            // 被动：防御 永久获得50%伤害抗性
            player.EnableEffect(EffectType.DamageReduction, 100);
        }
    }

    public class A9Soldier : CustomRole
    {
        public override uint Id { get; set; } = 143;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override int MaxHealth { get; set; } = 120;
        public override string Name { get; set; } = "Alpha-9 士兵";
        public override string Description { get; set; }
        public override string CustomInfo { get; set; } = "Alpha-9 士兵";

        public virtual List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.KeycardO5, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.Medkit, ItemType.Radio
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            // 被动：防御 永久获得25%伤害抗性
            player.EnableEffect(EffectType.DamageReduction, 50);
        }
    }
}