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
            if (player.Role is FpcRole fpc) fpc.IsInvisible = true;

            // 坐标抬升已在各子类 CinematicPosition 属性中完成
            player.Position = CinematicPosition;
            player.ShowHint(SpawnHint, 10f);

            yield return Timing.WaitForSeconds(CinematicDuration);

            if (player == null || !player.IsAlive) yield break;

            if (player.Role is FpcRole fpc2) fpc2.IsInvisible = false;
            player.DisableEffect(EffectType.FogControl);
            player.DisableEffect(EffectType.Ensnared);

            TeleportToFinalPosition(player);
        }

        protected abstract void TeleportToFinalPosition(Player player);
    }

    public class Nu7Commander : SCP5KRole
    {
        public static Nu7Commander Instance { get; } = new Nu7Commander();
        public override uint Id { get; set; } = 51;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfCaptain;
        public override string Name { get; set; } = "Nu-7 指挥官";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-指挥官";
        public override int MaxHealth { get; set; } = 150;

        public override string Description { get; set; } = "Nu-7 指挥官 - 落锤之眼\n\n<color=orange>血量提升至150</color>\n<color=yellow>按G键使用战术协调能力（40秒冷却）</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorHeavy, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        // 【安全修复】：抬高 2.5 米防卡防虚空
        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7 代号'落锤'的指挥官\n<color=yellow>按G键使用战术协调能力（40秒冷却）</color>";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfCaptain.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
            player.RankName = "NU-7-指挥官";
            player.RankColor = "cyan";
        }

        protected override void SubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip += OnAbilityKey; base.SubscribeEvents(); }
        protected override void UnsubscribeEvents() { Exiled.Events.Handlers.Player.TogglingNoClip -= OnAbilityKey; base.UnsubscribeEvents(); }

        private void OnAbilityKey(TogglingNoClipEventArgs ev)
        {
            if (!Check(ev.Player)) return;
            ev.IsAllowed = false;
            Nu7HammerDown.ExecuteCommanderAbilityFromKeybind(ev.Player);
        }
    }

    public class Nu7Sergeant : SCP5KRole
    {
        public static Nu7Sergeant Instance { get; } = new Nu7Sergeant();
        public override uint Id { get; set; } = 52;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfSergeant;
        public override string Name { get; set; } = "Nu-7 中士";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-中士";
        public override int MaxHealth { get; set; } = 200;

        public override string Description { get; set; } = "Nu-7 中士 - 落锤之盾";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunE11SR, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorHeavy, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7 代号'落锤'的中士";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfSergeant.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
            player.RankName = "NU-7-中士";
            player.RankColor = "cyan";
        }
    }

    public class Nu7Private : SCP5KRole
    {
        public static Nu7Private Instance { get; } = new Nu7Private();
        public override uint Id { get; set; } = 53;
        public override RoleTypeId Role { get; set; } = RoleTypeId.NtfPrivate;
        public override string Name { get; set; } = "Nu-7 列兵";
        public override string CustomInfo { get; set; } = "Nu-7 落锤-列兵";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "Nu-7 列兵 - 落锤之锋";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunCrossvec, ItemType.KeycardMTFCaptain, ItemType.Medkit,
            ItemType.ArmorHeavy, ItemType.Adrenaline, ItemType.Radio
        };

        public override Vector3 CinematicPosition => Nu7HammerDown.SchematicPosition + Vector3.up * 2.5f;
        public override string SpawnHint => "你成为了机动特遣队Nu-7 代号'落锤'的列兵";

        protected override void TeleportToFinalPosition(Player player)
        {
            player.Position = RoleTypeId.NtfPrivate.GetRandomSpawnLocation().Position + Vector3.up * 1.5f;
            player.RankName = "NU-7-列兵";
            player.RankColor = "cyan";
        }
    }

    public static class Nu7HammerDown
    {
        private static Dictionary<Player, DateTime> commanderCooldowns = new Dictionary<Player, DateTime>();
        private static int schematicMusicBotId = new System.Random().Next(1000, 1500);
        private static CoroutineHandle musicCoroutine;
        private static SchematicObject Nu7SchematicInstance;

        public static string SchematicName { get; set; } = "Nu7Rc";
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

        public static bool IsNu7Member(Player player) => Nu7Commander.Instance.Check(player) || Nu7Sergeant.Instance.Check(player) || Nu7Private.Instance.Check(player);
        public static bool IsCommander(Player player) => Nu7Commander.Instance.Check(player);

        public static void ExecuteCommanderAbilityFromKeybind(Player commander)
        {
            if (commanderCooldowns.ContainsKey(commander) && (DateTime.Now - commanderCooldowns[commander]).TotalSeconds < 40)
            {
                var remaining = 40 - (DateTime.Now - commanderCooldowns[commander]).TotalSeconds;
                commander.ShowHint($"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>", 3f);
                return;
            }

            var allNu7 = new List<Player>();
            allNu7.AddRange(Nu7Commander.Instance.TrackedPlayers);
            allNu7.AddRange(Nu7Sergeant.Instance.TrackedPlayers);
            allNu7.AddRange(Nu7Private.Instance.TrackedPlayers);

            int affected = 0;
            foreach (var member in allNu7)
            {
                if (member == commander || IsCommander(member)) continue;
                if (member.IsAlive)
                {
                    member.Health = member.Health > 30 ? member.Health - 30f : 1f;
                    member.EnableEffect(EffectType.DamageReduction, 30, 30f);
                    member.ShowHint($"<color=orange>指挥官使用了战术协调！</color>", 5f);
                    affected++;
                }
            }
            commander.ShowHint($"<color=green>战术协调已生效！影响了 {affected} 名队员</color>", 5f);
            commanderCooldowns[commander] = DateTime.Now;
        }

        public static bool SpawnNu7Team(List<Player> players)
        {
            if (players.Count < 3) return false;
            string[] roles = GetRolesByPlayerCount(players.Count);
            if (roles == null) return false;

            SpawnNu7Schematic();
            PlayNu7SpawnMusic();
            Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#A9A9A9>机动特遣队Nu-7“落锤”已进入该设施\r\n<split><b><color=#A9A9A9>因为该站点正在经历大规模遏制故障和多次停电\r\n<split> $PITCH_1.0 $SLEEP_0.05 Mobile task force unit new 7 designated hammer down has entered the facility $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 mass containment failure and multiple power outages $SLEEP_0.5 .\r\n");

            Timing.CallDelayed(2.0f, () =>
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    if (i >= players.Count || players[i] == null || !players[i].IsConnected) continue;

                    if (roles[i] == "指挥官") Nu7Commander.Instance.AddRole(players[i]);
                    else if (roles[i] == "中士") Nu7Sergeant.Instance.AddRole(players[i]);
                    else if (roles[i] == "列兵") Nu7Private.Instance.AddRole(players[i]);
                }
            });

            return true;
        }

        private static string[] GetRolesByPlayerCount(int count)
        {
            switch (count)
            {
                case 6: return new[] { "指挥官", "中士", "中士", "列兵", "列兵", "列兵" };
                case 5: return new[] { "指挥官", "中士", "列兵", "列兵", "列兵" };
                case 4: return new[] { "指挥官", "中士", "列兵", "列兵" };
                case 3: return new[] { "指挥官", "中士", "列兵" };
                default: return null;
            }
        }

        private static void SpawnNu7Schematic() { try { Nu7SchematicInstance = ObjectSpawner.SpawnSchematic(SchematicName, SchematicPosition); Timing.CallDelayed(40f, () => Nu7SchematicInstance?.Destroy()); } catch { } }

        public static void PlayNu7SpawnMusic()
        {
            if (string.IsNullOrEmpty(SpawnMusicPath) || !System.IO.File.Exists(SpawnMusicPath)) return;
            try
            {
                SafeRemoveAudioBot(schematicMusicBotId);
                if (AudioApi.Dummies.VoiceDummy.Add(schematicMusicBotId, "Nu-7落锤"))
                {
                    AudioApi.Dummies.VoiceDummy.Play(schematicMusicBotId, SpawnMusicPath);
                    musicCoroutine = Timing.CallDelayed(300f, StopNu7SpawnMusic);
                }
            }
            catch { }
        }

        private static void StopNu7SpawnMusic() { SafeRemoveAudioBot(schematicMusicBotId); }
        private static void OnRoundEnded(RoundEndedEventArgs ev) { commanderCooldowns.Clear(); if (musicCoroutine.IsRunning) Timing.KillCoroutines(musicCoroutine); StopNu7SpawnMusic(); }
        public static void RegisterEvents() { Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded; }
        public static void UnregisterEvents() { Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded; }
    }
}