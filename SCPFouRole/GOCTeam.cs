using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCP5K.SCPFouRole
{
    public class GOCCommander : SCP5KRole
    {
        public static GOCCommander Instance { get; } = new GOCCommander();
        public override uint Id { get; set; } = 61;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 指挥官";
        public override string CustomInfo { get; set; } = "GOC打击小组-指挥官";
        public override int MaxHealth { get; set; } = 150;

        public override string Description { get; set; } 

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        // 【安全修复】：坐标统一抬高 2.5 米
        public override Vector3 CinematicPosition => GOCTeam.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了全球超自然联盟打击小组的指挥官\n<color=yellow>按G键使用静谧行动（60秒冷却）</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            var gateA = Room.Get(RoomType.EzGateA);
            player.Position = gateA != null ? gateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        }

        protected override void SubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip += OnAbilityKey; base.SubscribeEvents(); }
        protected override void UnsubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip -= OnAbilityKey; base.UnsubscribeEvents(); }

        private void OnAbilityKey(TogglingNoClipEventArgs ev)
        {
            if (!Check(ev.Player)) return;
            ev.IsAllowed = false;
            GOCTeam.ExecuteCommanderAbilityFromKeybind(ev.Player);
        }
    }

    public class GOCHeavy : SCP5KRole
    {
        public static GOCHeavy Instance { get; } = new GOCHeavy();
        public override uint Id { get; set; } = 62;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 重装";
        public override string CustomInfo { get; set; } = "GOC打击小组-重装";
        public override int MaxHealth { get; set; } = 200;

        public override string Description { get; set; } 

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => GOCTeam.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了全球超自然联盟打击小组的重装\n<color=yellow>按设定键使用凝神静气（30秒冷却）</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            var gateA = Room.Get(RoomType.EzGateA);
            player.Position = gateA != null ? gateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        }

        protected override void SubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip += OnAbilityKey; base.SubscribeEvents(); }
        protected override void UnsubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip -= OnAbilityKey; base.UnsubscribeEvents(); }

        private void OnAbilityKey(TogglingNoClipEventArgs ev)
        {
            if (!Check(ev.Player)) return;
            ev.IsAllowed = false;
            GOCTeam.ExecuteHeavyAbilityFromKeybind(ev.Player);
        }
    }

    public class GOCSergeant : SCP5KRole
    {
        public static GOCSergeant Instance { get; } = new GOCSergeant();
        public override uint Id { get; set; } = 63;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 中士";
        public override string CustomInfo { get; set; } = "GOC打击小组-中士";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } 

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunCrossvec, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => GOCTeam.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了全球超自然联盟打击小组的中士\n<color=yellow>按G键使用生命付之一炬（一次性）</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            var gateA = Room.Get(RoomType.EzGateA);
            player.Position = gateA != null ? gateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        }

        protected override void SubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip += OnAbilityKey; base.SubscribeEvents(); }
        protected override void UnsubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip -= OnAbilityKey; base.UnsubscribeEvents(); }

        private void OnAbilityKey(TogglingNoClipEventArgs ev)
        {
            if (!Check(ev.Player)) return;
            ev.IsAllowed = false;
            GOCTeam.ExecuteSergeantAbilityFromKeybind(ev.Player);
        }
    }

    public class GOCPrivate : SCP5KRole
    {
        public static GOCPrivate Instance { get; } = new GOCPrivate();
        public override uint Id { get; set; } = 64;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 列兵";
        public override string CustomInfo { get; set; } = "GOC打击小组-列兵";
        public override int MaxHealth { get; set; } = 120;

        public override string Description { get; set; } 

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunCrossvec, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio
        };

        public override Vector3 CinematicPosition => GOCTeam.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了全球超自然联盟打击小组的列兵";

        protected override void TeleportToFinalPosition(Player player)
        {
            var gateA = Room.Get(RoomType.EzGateA);
            player.Position = gateA != null ? gateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        }
    }

    public static class GOCTeam
    {
        private static Dictionary<Player, DateTime> commanderCooldowns = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> heavyCooldowns = new Dictionary<Player, DateTime>();
        private static HashSet<Player> sergeantBurnUsed = new HashSet<Player>();

        private static int schematicMusicBotId = new System.Random().Next(3000, 3500);
        private static CoroutineHandle musicCoroutine;
        private static SchematicObject gocSchematicInstance;

        public static string SchematicName { get; set; } = "GOCTeamRc";
        public static Vector3 SchematicPosition { get; set; } = Vector3.zero;
        public static string SpawnMusicPath { get; set; }

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
                        m.Invoke(null, new object[] { id });
                        break;
                    }
                }
            }
            catch { }
        }

        public static bool IsGOCMember(Player player) => GOCCommander.Instance.Check(player) || GOCHeavy.Instance.Check(player) || GOCSergeant.Instance.Check(player) || GOCPrivate.Instance.Check(player);
        public static bool IsCommander(Player player) => GOCCommander.Instance.Check(player);
        public static bool IsHeavy(Player player) => GOCHeavy.Instance.Check(player);
        public static bool IsSergeant(Player player) => GOCSergeant.Instance.Check(player);

        public static void ExecuteCommanderAbilityFromKeybind(Player commander)
        {
            if (commanderCooldowns.ContainsKey(commander) && (DateTime.Now - commanderCooldowns[commander]).TotalSeconds < 60)
            {
                var remaining = 60 - (DateTime.Now - commanderCooldowns[commander]).TotalSeconds;
                commander.ShowHint($"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>", 3f);
                return;
            }
            commander.EnableEffect(EffectType.Fade, 200, 10f);
            commander.ShowHint($"<color=green>静谧行动已激活！</color>", 5f);
            commanderCooldowns[commander] = DateTime.Now;
        }

        public static void ExecuteHeavyAbilityFromKeybind(Player heavy)
        {
            if (heavyCooldowns.ContainsKey(heavy) && (DateTime.Now - heavyCooldowns[heavy]).TotalSeconds < 30)
            {
                var remaining = 30 - (DateTime.Now - heavyCooldowns[heavy]).TotalSeconds;
                heavy.ShowHint($"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>", 3f);
                return;
            }
            heavy.EnableEffect(EffectType.DamageReduction, 100, 5f);
            heavy.ShowHint($"<color=green>凝神静气已激活！</color>", 5f);
            heavyCooldowns[heavy] = DateTime.Now;
        }

        public static void ExecuteSergeantAbilityFromKeybind(Player sgt)
        {
            if (sergeantBurnUsed.Contains(sgt)) { sgt.ShowHint($"<color=red>只能使用一次！</color>", 3f); return; }
            sgt.EnableEffect(EffectType.AntiScp207);
            sgt.MaxHealth = 75;
            if (sgt.Health > 75f) sgt.Health = 75f;
            sgt.ShowHint($"<color=orange>生命付之一炬！</color>", 5f);
            sergeantBurnUsed.Add(sgt);
        }

        public static bool SpawnGOCTeam(List<Player> players)
        {
            if (players.Count < 4 || players.Count > 8) return false;
            string[] roles = GetRolesByPlayerCount(players.Count);
            if (roles == null) return false;

            SpawnGOCSchematic();
            PlayGOCSpawnMusic();
            Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#00FFFF>-<split><b><color=#00FFFF>设施内所有人员注意<split><b><color=#00FFFF>全球超自然联盟已开始对设施展开突袭，并已突破至第一区\r\n<split> $PITCH_1.0 $SLEEP_0.05 Attention all personnel $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 The Global Occult Coalition has launched a raid on the facility and has breached into zone 1 $SLEEP_0.5 .\r\n");

            Timing.CallDelayed(2.0f, () =>
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (i >= players.Count || players[i] == null || !players[i].IsConnected) continue;

                    if (roles[i] == "指挥官") GOCCommander.Instance.AddRole(players[i]);
                    else if (roles[i] == "重装") GOCHeavy.Instance.AddRole(players[i]);
                    else if (roles[i] == "中士") GOCSergeant.Instance.AddRole(players[i]);
                    else if (roles[i] == "列兵") GOCPrivate.Instance.AddRole(players[i]);
                }
            });

            return true;
        }

        private static string[] GetRolesByPlayerCount(int c)
        {
            switch (c)
            {
                case 4: return new[] { "指挥官", "中士", "列兵", "列兵" };
                case 5: return new[] { "指挥官", "重装", "中士", "列兵", "列兵" };
                case 6: return new[] { "指挥官", "重装", "中士", "中士", "列兵", "列兵" };
                case 7: return new[] { "指挥官", "重装", "中士", "中士", "列兵", "列兵", "列兵" };
                case 8: return new[] { "指挥官", "重装", "中士", "中士", "列兵", "列兵", "列兵", "列兵" };
                default: return null;
            }
        }

        private static void SpawnGOCSchematic()
        {
            try { gocSchematicInstance = ObjectSpawner.SpawnSchematic(SchematicName, SchematicPosition); Timing.CallDelayed(40f, () => gocSchematicInstance?.Destroy()); } catch { }
        }

        public static void PlayGOCSpawnMusic()
        {
            if (string.IsNullOrEmpty(SpawnMusicPath) || !System.IO.File.Exists(SpawnMusicPath)) return;
            try
            {
                SafeRemoveAudioBot(schematicMusicBotId);
                if (AudioApi.Dummies.VoiceDummy.Add(schematicMusicBotId, "GOC打击小组"))
                {
                    AudioApi.Dummies.VoiceDummy.Play(schematicMusicBotId, SpawnMusicPath);
                    musicCoroutine = Timing.CallDelayed(300f, StopGOCSpawnMusic);
                }
            }
            catch { }
        }

        private static void StopGOCSpawnMusic() { SafeRemoveAudioBot(schematicMusicBotId); }
        private static void OnRoundEnded(RoundEndedEventArgs ev) { commanderCooldowns.Clear(); heavyCooldowns.Clear(); sergeantBurnUsed.Clear(); if (musicCoroutine.IsRunning) Timing.KillCoroutines(musicCoroutine); StopGOCSpawnMusic(); }

        public static void RegisterEvents() { Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded; }
        public static void UnregisterEvents() { Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded; }
    }
}