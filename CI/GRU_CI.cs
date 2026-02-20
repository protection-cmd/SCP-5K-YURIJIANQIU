using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using SCP5K.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Subtitles.SubtitleCategory;

namespace SCP5K.SCPFouRole
{
    #region GRU-CI 新阵营 - 角色定义

    public class GRUCIHacker : CustomRole
    {
        public static GRUCIHacker Instance { get; } = new GRUCIHacker();
        public override uint Id { get; set; } = 801;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosMarauder;
        public override string Name { get; set; } = "GRU-CI-黑客";
        public override string CustomInfo { get; set; } = "GRU-CI-黑客";
        public override int MaxHealth { get; set; } = 100;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;
        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunShotgun, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency, ItemType.GunRevolver };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                player.IsBypassModeEnabled = true;
                var message = "你成为了 GRU-CI 黑客\n<color=yellow>你的便携破解设备允许你开启设施内的任何门！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-黑客入场");
            });
        }

        protected override void RoleRemoved(Player player)
        {
            player.IsBypassModeEnabled = false;
            base.RoleRemoved(player);
        }
    }

    public class GRUCIBreacher : CustomRole
    {
        public static GRUCIBreacher Instance { get; } = new GRUCIBreacher();
        public override uint Id { get; set; } = 802;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "GRU-CI-突破手";
        public override string CustomInfo { get; set; } = "GRU-CI-突破手";
        public override int MaxHealth { get; set; } = 120;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunAK, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                var message = "你成为了 GRU-CI 突破手\n<color=yellow>你的专属AK枪械经过特殊改装，80伤害！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-突破手入场");

                Timing.CallDelayed(0.5f, () =>
                {
                    if (player == null || !player.IsAlive) return;
                    var ak = player.Items.FirstOrDefault(x => x.Type == ItemType.GunAK);
                    if (ak != null)
                    {
                        GRUCIManager.RegisterBreacherAK(ak.Serial);
                        Log.Info($"[GRU-CI] 已成功绑定一把突破手专武，Serial: {ak.Serial}");
                    }
                });
            });
        }
    }

    public class GRUCIDemolitionist : CustomRole
    {
        public static GRUCIDemolitionist Instance { get; } = new GRUCIDemolitionist();
        public override uint Id { get; set; } = 803;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "GRU-CI-爆破手";
        public override string CustomInfo { get; set; } = "GRU-CI-爆破手";
        public override int MaxHealth { get; set; } = 110;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunAK, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency, ItemType.GrenadeHE };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                var message = "你成为了 GRU-CI 爆破手\n<color=yellow>使用技能键可不断补给高爆手雷！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-爆破手入场");
            });
        }
    }

    public class GRUCICommander : CustomRole
    {
        public static GRUCICommander Instance { get; } = new GRUCICommander();
        public override uint Id { get; set; } = 804;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRepressor;
        public override string Name { get; set; } = "GRU-CI-指挥官";
        public override string CustomInfo { get; set; } = "GRU-CI-指挥官";
        public override int MaxHealth { get; set; } = 150;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunLogicer, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                var message = "你成为了 GRU-CI 指挥官\n<color=yellow>使用技能键可获取一把充能电炮！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-指挥官入场");
            });
        }
    }

    public class GRUCIInvestigator : CustomRole
    {
        public static GRUCIInvestigator Instance { get; } = new GRUCIInvestigator();
        public override uint Id { get; set; } = 805;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosMarauder;
        public override string Name { get; set; } = "GRU-CI-考察员";
        public override string CustomInfo { get; set; } = "GRU-CI-考察员";
        public override int MaxHealth { get; set; } = 150;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunShotgun, ItemType.Medkit, ItemType.ArmorCombat, ItemType.KeycardChaosInsurgency, ItemType.Radio };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                var message = "你成为了 GRU-CI 考察员\n<color=yellow>使用技能键触发 团队加速 / 牵引敌人</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-考察员入场");
            });
        }
    }

    public class GRUCISoldier : CustomRole
    {
        public static GRUCISoldier Instance { get; } = new GRUCISoldier();
        public override uint Id { get; set; } = 806;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "GRU-CI-士兵";
        public override string CustomInfo { get; set; } = "GRU-CI-士兵";
        public override int MaxHealth { get; set; } = 110;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunAK, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                // 清空装甲值
                player.ArtificialHealth = 0;
                var message = "你成为了 GRU-CI 士兵\n<color=yellow>你是GRU-CI的普通士兵，协助队友完成目标！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-士兵入场");
            });
        }
    }

    public class GRUCIConscript : CustomRole
    {
        public static GRUCIConscript Instance { get; } = new GRUCIConscript();
        public override uint Id { get; set; } = 807;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "GRU-CI-征召兵";
        public override string CustomInfo { get; set; } = "GRU-CI-征召兵";
        public override int MaxHealth { get; set; } = 100;
        public override bool KeepRoleOnChangingRole { get; set; } = false;
        public override bool KeepRoleOnDeath { get; set; } = false;
        public override bool KeepInventoryOnSpawn { get; set; } = false;
        public override bool KeepPositionOnSpawn { get; set; } = false;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        { ItemType.GunAK, ItemType.Medkit, ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.KeycardChaosInsurgency };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            // ★ 加入阵营表
            FactionManager.AddPlayer(player, FactionType.GRUCI);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);

                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                // 清空装甲值
                player.ArtificialHealth = 0;
                var message = "你成为了 GRU-CI 征召兵\n<color=yellow>你是GRU-CI的普通士兵，协助队友完成目标！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "GRU-CI-征召兵入场");
            });
        }
    }
    #endregion

    #region GRU-CI 新阵营 - 管理器
    public static class GRUCIManager
    {
        // 突破手专武 Serial 记录表
        private static HashSet<ushort> breacherAKSerials = new HashSet<ushort>();

        // 技能CD记录
        private static Dictionary<Player, DateTime> demoSkillCD = new Dictionary<Player, DateTime>();
        private static HashSet<Player> cmdrSkillUsed = new HashSet<Player>();
        private static Dictionary<Player, DateTime> invSkill1CD = new Dictionary<Player, DateTime>();
        private static HashSet<Player> invSkill2Used = new HashSet<Player>();

        public static void RegisterBreacherAK(ushort serial) => breacherAKSerials.Add(serial);

        public static void CleanUpPlayer(Player p)
        {
            demoSkillCD.Remove(p);
            cmdrSkillUsed.Remove(p);
            invSkill1CD.Remove(p);
            invSkill2Used.Remove(p);
        }

        public static bool IsGRUCIMember(Player p) => GRUCIHacker.Instance.Check(p) || GRUCIBreacher.Instance.Check(p) || GRUCIDemolitionist.Instance.Check(p) || GRUCICommander.Instance.Check(p) || GRUCIInvestigator.Instance.Check(p) || GRUCISoldier.Instance.Check(p) || GRUCIConscript.Instance.Check(p);

        public static void ExecuteDemoSkill(Player player)
        {
            if (demoSkillCD.ContainsKey(player) && (DateTime.Now - demoSkillCD[player]).TotalSeconds < 35)
            {
                var message = $"<color=red>火力充足冷却中: {35 - (DateTime.Now - demoSkillCD[player]).TotalSeconds:F1}秒</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "火力充足冷却"); return;
            }
            if (player.IsInventoryFull)
            {
                var message = "<color=red>背包已满，无法获取手雷！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "火力充足背包已满"); return;
            }
            player.AddItem(ItemType.GrenadeHE);
            demoSkillCD[player] = DateTime.Now;
            var message2 = "<color=green>火力充足！已获得一枚高爆手雷！</color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 4f, "火力充足获得");
        }

        public static void ExecuteCmdrSkill(Player player)
        {
            if (cmdrSkillUsed.Contains(player))
            {
                var message = "<color=red>高斯放电装置只能使用一次！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "高斯放电装置只能使用一次"); return;
            }
            if (player.IsInventoryFull)
            {
                var message = "<color=red>背包已满，无法获取电炮！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "高斯放电装置背包已满"); return;
            }
            player.AddItem(ItemType.MicroHID);
            cmdrSkillUsed.Add(player);
            var message2 = "<color=green>高斯放电装置已启动，你获得了电炮！</color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 4f, "高斯放电装置已启动");
        }

        public static void ExecuteInvSkill1(Player player)
        {
            if (invSkill1CD.ContainsKey(player) && (DateTime.Now - invSkill1CD[player]).TotalSeconds < 50)
            {
                var message = $"<color=red>寻找真相冷却中: {50 - (DateTime.Now - invSkill1CD[player]).TotalSeconds:F1}秒</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "寻找真相冷却"); return;
            }
            invSkill1CD[player] = DateTime.Now;
            int count = 0;
            foreach (var p in Player.List.Where(x => x.IsAlive && IsGRUCIMember(x)))
            {
                p.EnableEffect(EffectType.MovementBoost, 20, 10f);
                count++;
            }
            var message2 = $"<color=green>寻找真相：已为 {count} 名存活队友提供移速增益！</color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 4f, "寻找真相");
        }

        public static void ExecuteInvSkill2(Player player)
        {
            if (invSkill2Used.Contains(player))
            {
                var message = "<color=red>零位能量场牵引器只能使用一次！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "零位能量场牵引器只能使用一次"); return;
            }

            // 搜索敌对阵营 (非自身非CI同阵营)
            var enemies = Player.List.Where(p => p.IsAlive && p.Role.Team != Team.ChaosInsurgency ).ToList();
            if (enemies.Count == 0)
            {
                var message = "<color=yellow>当前没有可牵引的敌方单位！(保留技能使用次数)</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 3f, "没有可牵引"); return;
            }

            var target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
            target.Position = player.Position + player.CameraTransform.forward * 1.5f;
            target.EnableEffect(EffectType.Ensnared, 255, 3f);

            var message2 = $"<color=red>你被零位能量场牵引了！禁锢3秒！</color>";
            HSMShowhint.HsmShowHint(target, message2, 600, 0, 3f, "被零位能量场牵引");

            var message3 = $"<color=green>牵引成功！已将 {target.Nickname} 拉至面前！</color>";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 3f, "牵引成功");

            invSkill2Used.Add(player);
        }

        public static void OnHurting(HurtingEventArgs ev)
        {
            // 1. 确保攻击者和受击者有效，且不是自残
            if (ev.Attacker == null || ev.Player == null || ev.Attacker == ev.Player) return;

            // 2. 防止无限循环：如果我们造成的 80 点 Custom 真伤再次触发这个事件，直接放行
            if (ev.DamageHandler.Type == DamageType.Custom) return;

            // 3. 检查攻击者手里是否拿着物品，且物品是 AK
            if (ev.Attacker.CurrentItem != null && ev.Attacker.CurrentItem.Type == ItemType.GunAK)
            {
                // 4. 判断这把枪的序列号，是否在我们的“真伤专武”记录表中！
                if (breacherAKSerials.Contains(ev.Attacker.CurrentItem.Serial))
                {
                    // 5. 确保是这把AK开火造成的伤害 
                    if (ev.DamageHandler.Type == DamageType.AK)
                    {
                        ev.Amount = 80;
                    }
                }
            }
        }

        public static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            breacherAKSerials.Clear();
            demoSkillCD.Clear();
            cmdrSkillUsed.Clear();
            invSkill1CD.Clear();
            invSkill2Used.Clear();
        }

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        }

        public static bool SpawnTeam(List<Player> players)
        {
            if (players.Count < 1 || players.Count > 13) return false;

            var rand = new System.Random();

            for (int i = 0; i < players.Count; i++)
            {
                Player p = players[i];
                if (p == null) continue;

                if (i == 0) GRUCIHacker.Instance.AddRole(p);
                else if (i == 1) GRUCIBreacher.Instance.AddRole(p);
                else if (i == 2) GRUCIDemolitionist.Instance.AddRole(p);
                else if (i == 3) GRUCICommander.Instance.AddRole(p);
                else if (i == 4) GRUCIInvestigator.Instance.AddRole(p);
                else
                {
                    if (rand.Next(0, 2) == 0) GRUCISoldier.Instance.AddRole(p);
                    else GRUCIConscript.Instance.AddRole(p);
                }
            }

            return true;
        }
    }
    #endregion
}