using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP5K.SCPFouRole
{
    public abstract class SCP5KRole : CustomRole
    {
        public virtual float CinematicDuration => 30f;
        public abstract Vector3 CinematicPosition { get; }
        public abstract string SpawnHint { get; }
        public virtual List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>();

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                if (CustomRoleItems != null && CustomRoleItems.Count > 0)
                {
                    player.ClearInventory();
                    foreach (var item in CustomRoleItems)
                    {
                        player.AddItem(item);
                    }
                }
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
            });

            Timing.RunCoroutine(SpawnCinematicSequence(player));
        }

        private IEnumerator<float> SpawnCinematicSequence(Player player)
        {
            player.EnableEffect(EffectType.Ensnared, 255, CinematicDuration);
            player.EnableEffect(EffectType.FogControl);
            // 入场开启夜视（强度1）
            player.EnableEffect(EffectType.NightVision, 1, CinematicDuration);
            if (player.Role is FpcRole fpc) fpc.IsInvisible = true;

            // 坐标抬升已在各子类 CinematicPosition 属性中完成
            player.Position = CinematicPosition;
            player.ShowHint(SpawnHint, 10f);

            yield return Timing.WaitForSeconds(CinematicDuration);

            if (player == null || !player.IsAlive) yield break;

            if (player.Role is FpcRole fpc2) fpc2.IsInvisible = false;
            player.DisableEffect(EffectType.FogControl);
            player.DisableEffect(EffectType.Ensnared);
            // 传送前关闭夜视
            player.DisableEffect(EffectType.NightVision);

            TeleportToFinalPosition(player);
        }

        protected abstract void TeleportToFinalPosition(Player player);
    }

    #region Nu-7-A连 (偏向机动与团队)
    public class Nu7ACommander : SCP5KRole
    {
        public static Nu7ACommander Instance { get; } = new Nu7ACommander();
        public override uint Id { get; set; } = 511;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfCaptain;
        public override string Name { get; set; } = "Nu-7-A 指挥官";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-A连指挥官";
        public override int MaxHealth { get; set; } = 150;

        public override string Description { get; set; } = "Nu-7-A连 指挥官\n<color=yellow>技能1: 先发制人(设置锚点延迟返回)\n技能2: 全军出击(A连群体加速)</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardMTFCaptain, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.Medkit, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-A连的指挥官\n<color=yellow>使用技能键触发 先发制人 / 全军出击</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfCaptain.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }

    public class Nu7AJiFeng : SCP5KRole
    {
        public static Nu7AJiFeng Instance { get; } = new Nu7AJiFeng();
        public override uint Id { get; set; } = 512;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfSergeant;
        public override string Name { get; set; } = "Nu-7-A-疾风";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-A连-疾风";
        public override int MaxHealth { get; set; } = 120;

        public override string Description { get; set; } = "Nu-7-A连 疾风\n<color=yellow>技能: 无畏，无惧(大幅度伤害减免与加速)</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorHeavy, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-A连的疾风\n<color=yellow>使用技能键触发 无畏，无惧</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfSergeant.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }

    public class Nu7APrivate : SCP5KRole
    {
        public static Nu7APrivate Instance { get; } = new Nu7APrivate();
        public override uint Id { get; set; } = 513;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfPrivate;
        public override string Name { get; set; } = "Nu-7-A 列兵";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-A连列兵";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "Nu-7-A连 列兵";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.ArmorCombat, ItemType.Medkit, ItemType.Radio, ItemType.Adrenaline
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-A连的列兵";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfPrivate.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }
    #endregion

    #region Nu-7-B连 (偏向控制与爆发)
    public class Nu7BCommander : SCP5KRole
    {
        public static Nu7BCommander Instance { get; } = new Nu7BCommander();
        public override uint Id { get; set; } = 521;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfCaptain;
        public override string Name { get; set; } = "Nu-7-B 指挥官";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-B连指挥官";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "Nu-7-B连 指挥官\n<color=yellow>技能1: 献祭过往(同归于尽领域)\n技能2: 画地为牢(群体禁锢敌方)</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardMTFCaptain, ItemType.ArmorHeavy,
            ItemType.Adrenaline, ItemType.Medkit, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-B连的指挥官\n<color=yellow>使用技能键触发 献祭过往 / 画地为牢</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfCaptain.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }

    public class Nu7BTieXue : SCP5KRole
    {
        public static Nu7BTieXue Instance { get; } = new Nu7BTieXue();
        public override uint Id { get; set; } = 522;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfSergeant;
        public override string Name { get; set; } = "Nu-7-B 铁血";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-B连铁血";
        public override int MaxHealth { get; set; } = 120;

        public override string Description { get; set; } = "Nu-7-B连 铁血\n<color=yellow>技能1: 再著诗篇(获得囚鸟)\n技能2: 冲，冲，冲！(移速提升，伤害翻倍)</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.KeycardO5, ItemType.Jailbird, ItemType.ArmorHeavy,
            ItemType.Medkit, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-B连的铁血\n<color=yellow>使用技能键触发 再著诗篇 / 冲，冲，冲！</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfSergeant.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }

    public class Nu7BPrivate : SCP5KRole
    {
        public static Nu7BPrivate Instance { get; } = new Nu7BPrivate();
        public override uint Id { get; set; } = 523;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfPrivate;
        public override string Name { get; set; } = "Nu-7-B 列兵";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-B连列兵";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "Nu-7-B连 列兵";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.ArmorCombat, ItemType.Medkit, ItemType.Radio, ItemType.Adrenaline
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7-B连的列兵";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfPrivate.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
        }
    }
    #endregion

    public static class Nu7HammerDown
    {
        // 技能冷却与状态跟踪
        private static Dictionary<Player, DateTime> aCmdrSkill1CD = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> aCmdrSkill2CD = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> aJiFengSkillCD = new Dictionary<Player, DateTime>();

        private static HashSet<Player> bCmdrSacrificeUsed = new HashSet<Player>();
        private static Dictionary<Player, DateTime> bCmdrSkill2CD = new Dictionary<Player, DateTime>();

        private static Dictionary<Player, DateTime> bTieXueSkill1CD = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> bTieXueSkill2CD = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> bTieXueDamageBoostEnd = new Dictionary<Player, DateTime>();

        private static int schematicMusicBotId = new System.Random().Next(1000, 1500);
        private static CoroutineHandle musicCoroutine;
        private static SchematicObject Nu7SchematicInstance;

        public static string SchematicName { get; set; } = "Nu7Rc";
        public static Vector3 SchematicPosition { get; set; } = Vector3.zero;

        // ★ 分别记录A连和B连的音乐路径
        public static string SpawnMusicPathA { get; set; }
        public static string SpawnMusicPathB { get; set; }

        public static void SafeRemoveAudioBot(int id)
        {
            try
            {
                var type = typeof(AudioApi.Dummies.VoiceDummy);
                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                foreach (var m in methods)
                {
                    if (m.Name == "Remove" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int))
                    {
                        m.Invoke(null, new object[] { id }); break;
                    }
                }
            }
            catch { }
        }

        public static void CleanUpPlayer(Player player)
        {
            aCmdrSkill1CD.Remove(player);
            aCmdrSkill2CD.Remove(player);
            aJiFengSkillCD.Remove(player);
            bCmdrSacrificeUsed.Remove(player);
            bCmdrSkill2CD.Remove(player);
            bTieXueSkill1CD.Remove(player);
            bTieXueSkill2CD.Remove(player);
            bTieXueDamageBoostEnd.Remove(player);
        }

        public static bool IsNu7AMember(Player player) => Nu7ACommander.Instance.Check(player) || Nu7AJiFeng.Instance.Check(player) || Nu7APrivate.Instance.Check(player);
        public static bool IsNu7BMember(Player player) => Nu7BCommander.Instance.Check(player) || Nu7BTieXue.Instance.Check(player) || Nu7BPrivate.Instance.Check(player);

        #region Nu-7-A 技能逻辑
        public static void ExecuteNu7ACmdrSkill1(Player player)
        {
            if (aCmdrSkill1CD.ContainsKey(player) && (DateTime.Now - aCmdrSkill1CD[player]).TotalSeconds < 60)
            {
                player.ShowHint($"<color=red>先发制人冷却中: {60 - (DateTime.Now - aCmdrSkill1CD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            aCmdrSkill1CD[player] = DateTime.Now;
            Vector3 anchorPos = player.Position;
            player.ShowHint("<color=green>先发制人: 锚点已设置，20秒后将强制返回！</color>", 4f);

            Timing.CallDelayed(20f, () =>
            {
                if (player != null && player.IsAlive && Nu7ACommander.Instance.Check(player))
                {
                    player.Position = anchorPos;
                    player.ShowHint("<color=cyan>已强制返回锚点！</color>", 3f);
                }
            });
        }

        public static void ExecuteNu7ACmdrSkill2(Player player)
        {
            if (aCmdrSkill2CD.ContainsKey(player) && (DateTime.Now - aCmdrSkill2CD[player]).TotalSeconds < 60)
            {
                player.ShowHint($"<color=red>全军出击冷却中: {60 - (DateTime.Now - aCmdrSkill2CD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            aCmdrSkill2CD[player] = DateTime.Now;
            int count = 0;
            foreach (var p in Player.List.Where(x => x.IsAlive && IsNu7AMember(x)))
            {
                p.EnableEffect(EffectType.MovementBoost, 20, 10f);
                count++;
            }
            player.ShowHint($"<color=green>全军出击已激活！影响了 {count} 名A连队员</color>", 4f);
        }

        public static void ExecuteNu7AJiFengSkill(Player player)
        {
            if (aJiFengSkillCD.ContainsKey(player) && (DateTime.Now - aJiFengSkillCD[player]).TotalSeconds < 40)
            {
                player.ShowHint($"<color=red>无畏无惧冷却中: {40 - (DateTime.Now - aJiFengSkillCD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            aJiFengSkillCD[player] = DateTime.Now;
            player.EnableEffect(EffectType.DamageReduction, 100, 10f);
            player.EnableEffect(EffectType.MovementBoost, 20, 10f);
            player.ShowHint("<color=green>无畏，无惧！50%减伤与加速激活 (持续10秒)</color>", 4f);
        }
        #endregion

        #region Nu-7-B 技能逻辑
        public static void ExecuteNu7BCmdrSkill1(Player player)
        {
            if (bCmdrSacrificeUsed.Contains(player))
            {
                player.ShowHint("<color=red>献祭过往，永垂不朽 只能使用一次！</color>", 3f); return;
            }
            bCmdrSacrificeUsed.Add(player);
            Vector3 anchor = player.Position;
            player.ShowHint("<color=red>献祭过往，永垂不朽：已记录坐标，3秒后大清算！</color>", 3f);

            Timing.CallDelayed(3f, () =>
            {
                if (player == null || !player.IsAlive || !Nu7BCommander.Instance.Check(player)) return;

                foreach (var p in Player.List.Where(x => x.IsAlive))
                {
                    if (Vector3.Distance(p.Position, anchor) <= 4f)
                    {
                        p.Kill("献祭过往，永垂不朽");
                    }
                }
            });
        }

        public static void ExecuteNu7BCmdrSkill2(Player player)
        {
            if (bCmdrSkill2CD.ContainsKey(player) && (DateTime.Now - bCmdrSkill2CD[player]).TotalSeconds < 60)
            {
                player.ShowHint($"<color=red>画地为牢冷却中: {60 - (DateTime.Now - bCmdrSkill2CD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            bCmdrSkill2CD[player] = DateTime.Now;
            Vector3 anchor = player.Position;
            int count = 0;
            foreach (var p in Player.List.Where(x => x.IsAlive))
            {
                if (Vector3.Distance(p.Position, anchor) <= 3f)
                {
                    if (p.Role.Side != Side.Mtf && p.Role.Team != Team.SCPs)
                    {
                        p.EnableEffect(EffectType.Ensnared, 255, 5f);
                        count++;
                    }
                }
            }
            player.ShowHint($"<color=green>画地为牢触发！成功禁锢了 {count} 个敌方目标</color>", 3f);
        }

        public static void ExecuteNu7BTieXueSkill1(Player player)
        {
            if (bTieXueSkill1CD.ContainsKey(player) && (DateTime.Now - bTieXueSkill1CD[player]).TotalSeconds < 120)
            {
                player.ShowHint($"<color=red>再著诗篇冷却中: {120 - (DateTime.Now - bTieXueSkill1CD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            if (player.IsInventoryFull)
            {
                player.ShowHint("<color=yellow>背包已满，未能获得囚鸟，保留CD！</color>", 3f);
                return;
            }
            player.AddItem(ItemType.Jailbird);
            bTieXueSkill1CD[player] = DateTime.Now;
            player.ShowHint("<color=green>再著诗篇：已获得囚鸟！</color>", 3f);
        }

        public static void ExecuteNu7BTieXueSkill2(Player player)
        {
            if (bTieXueSkill2CD.ContainsKey(player) && (DateTime.Now - bTieXueSkill2CD[player]).TotalSeconds < 60)
            {
                player.ShowHint($"<color=red>冲冲冲冷却中: {60 - (DateTime.Now - bTieXueSkill2CD[player]).TotalSeconds:F1}秒</color>", 3f); return;
            }
            bTieXueSkill2CD[player] = DateTime.Now;
            player.EnableEffect(EffectType.MovementBoost, 20, 5f);
            bTieXueDamageBoostEnd[player] = DateTime.Now.AddSeconds(5);
            player.ShowHint("<color=red>冲，冲，冲！5秒内伤害翻倍并加速！</color>", 3f);
        }
        #endregion

        #region 生成与核心事件
        public static bool SpawnNu7ATeam(List<Player> players)
        {
            if (players.Count < 3 || players.Count > 5) return false;

            SpawnNu7Schematic();
            PlayNu7SpawnMusic(true); // ★ A连传入 True
            PlayCassie();

            Timing.CallDelayed(2.0f, () =>
            {
                if (players.Count >= 1 && players[0] != null) Nu7ACommander.Instance.AddRole(players[0]);
                if (players.Count >= 2 && players[1] != null) Nu7AJiFeng.Instance.AddRole(players[1]);
                for (int i = 2; i < players.Count; i++)
                {
                    if (players[i] != null) Nu7APrivate.Instance.AddRole(players[i]);
                }
            });
            return true;
        }

        public static bool SpawnNu7BTeam(List<Player> players)
        {
            if (players.Count < 4 || players.Count > 7) return false;

            SpawnNu7Schematic();
            PlayNu7SpawnMusic(false); // ★ B连传入 False
            PlayCassie();

            Timing.CallDelayed(2.0f, () =>
            {
                if (players.Count >= 1 && players[0] != null) Nu7BCommander.Instance.AddRole(players[0]);
                if (players.Count >= 2 && players[1] != null) Nu7BTieXue.Instance.AddRole(players[1]);
                for (int i = 2; i < players.Count; i++)
                {
                    if (players[i] != null) Nu7BPrivate.Instance.AddRole(players[i]);
                }
            });
            return true;
        }

        private static void PlayCassie()
        {
            Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#A9A9A9>机动特遣队Nu-7“落锤”已进入该设施\r\n<split><b><color=#A9A9A9>因为该站点正在经历大规模遏制故障和多次停电\r\n<split> $PITCH_1.0 $SLEEP_0.05 Mobile task force unit new 7 designated hammer down has entered the facility $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 mass containment failure and multiple power outages $SLEEP_0.5 .\r\n");
        }

        private static void SpawnNu7Schematic()
        {
            try
            {
                Nu7SchematicInstance = ObjectSpawner.SpawnSchematic(SchematicName, SchematicPosition);
                Timing.CallDelayed(40f, () => Nu7SchematicInstance?.Destroy());
            }
            catch { }
        }

        // ★ 根据是否是A连来决定播放哪首音乐
        public static void PlayNu7SpawnMusic(bool isTeamA)
        {
            string path = isTeamA ? SpawnMusicPathA : SpawnMusicPathB;
            string botName = isTeamA ? "Nu-7-A连落锤" : "Nu-7-B连落锤";

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return;
            try
            {
                SafeRemoveAudioBot(schematicMusicBotId);
                if (AudioApi.Dummies.VoiceDummy.Add(schematicMusicBotId, botName))
                {
                    AudioApi.Dummies.VoiceDummy.Play(schematicMusicBotId, path);
                    musicCoroutine = Timing.CallDelayed(300f, StopNu7SpawnMusic);
                }
            }
            catch { }
        }

        private static void StopNu7SpawnMusic() { SafeRemoveAudioBot(schematicMusicBotId); }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker != null && Nu7BTieXue.Instance.Check(ev.Attacker))
            {
                if (bTieXueDamageBoostEnd.TryGetValue(ev.Attacker, out DateTime endTime) && DateTime.Now < endTime)
                {
                    ev.Amount *= 2f;
                }
            }
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            aCmdrSkill1CD.Clear(); aCmdrSkill2CD.Clear(); aJiFengSkillCD.Clear();
            bCmdrSacrificeUsed.Clear(); bCmdrSkill2CD.Clear();
            bTieXueSkill1CD.Clear(); bTieXueSkill2CD.Clear(); bTieXueDamageBoostEnd.Clear();
            if (musicCoroutine.IsRunning) Timing.KillCoroutines(musicCoroutine);
            StopNu7SpawnMusic();
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
        #endregion
    }
}