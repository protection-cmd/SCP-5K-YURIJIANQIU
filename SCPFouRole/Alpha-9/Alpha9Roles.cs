using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
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
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;
        public override List<string> Inventory { get; set; } = new List<string>
    {
        ItemType.GunFRMG0.ToString(),
        ItemType.KeycardO5.ToString(),
        ItemType.ArmorHeavy.ToString(),
        ItemType.Adrenaline.ToString(),
        ItemType.Medkit.ToString(),
        ItemType.Radio.ToString()
    };



        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            Alpha9Manager.Player105 = player;
            if (!Alpha9Manager.A9TeamMembers.Contains(player))
                Alpha9Manager.A9TeamMembers.Add(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            Timing.CallDelayed(0.6f, () =>
            {
                player.EnableEffect(EffectType.Slowness, 20);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);
            if (Alpha9Manager.Player105 == player)
                Alpha9Manager.Player105 = null;
            Alpha9Manager.A9TeamMembers.Remove(player);
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
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override List<string> Inventory { get; set; } = new List<string>
        { 
            ItemType.SCP1509.ToString(),
            ItemType.KeycardO5.ToString(),
            ItemType.ArmorHeavy.ToString(),
            ItemType.Adrenaline.ToString(),
            ItemType.SCP500.ToString(), 
            ItemType.Radio.ToString() 
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            Alpha9Manager.Player076 = player;
            if (!Alpha9Manager.A9TeamMembers.Contains(player))
                Alpha9Manager.A9TeamMembers.Add(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            Timing.CallDelayed(0.6f, () =>
            {
                player.HumeShield = 50; 
            });
        }

        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);
            if (Alpha9Manager.Player076 == player)
                Alpha9Manager.Player076 = null;
            Alpha9Manager.A9TeamMembers.Remove(player);
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
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override List<string> Inventory { get; set; } = new List<string>
        { ItemType.GunFRMG0.ToString(), 
          ItemType.KeycardO5.ToString(), 
          ItemType.ArmorHeavy.ToString(),
          ItemType.Adrenaline.ToString(), 
          ItemType.Medkit.ToString(),
          ItemType.Radio.ToString(),
          ItemType.SCP500.ToString() 
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            if (!Alpha9Manager.A9TeamMembers.Contains(player))
                Alpha9Manager.A9TeamMembers.Add(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            Timing.CallDelayed(0.6f, () =>
            {
                player.EnableEffect(EffectType.DamageReduction, 100);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);
            Alpha9Manager.A9TeamMembers.Remove(player);
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
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override List<string> Inventory { get; set; } = new List<string>
        { 
          ItemType.GunE11SR.ToString(), 
          ItemType.KeycardO5.ToString(),
          ItemType.ArmorHeavy.ToString(),
          ItemType.Adrenaline.ToString(),   
          ItemType.Medkit.ToString(), 
          ItemType.Radio.ToString() 
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            if (!Alpha9Manager.A9TeamMembers.Contains(player))
                Alpha9Manager.A9TeamMembers.Add(player);
            FactionManager.AddPlayer(player, FactionType.Alpha9);
            Timing.CallDelayed(0.6f, () =>
            {
                player.EnableEffect(EffectType.DamageReduction, 50);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);
            Alpha9Manager.A9TeamMembers.Remove(player);
        }
    }
}