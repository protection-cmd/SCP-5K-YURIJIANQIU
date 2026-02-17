using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using SCP5K.Events;
using SCP5K.SCPFouRole;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP5K
{
    public class GOCArcaneCommander : SCP5KRole
    {
        public static GOCArcaneCommander Instance { get; } = new GOCArcaneCommander();
        public override uint Id { get; set; } = 65;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 奇术打击-指挥官";
        public override string CustomInfo { get; set; } = "GOC奇术打击小组-指挥官";
        public override int MaxHealth { get; set; } = 150;

        public override string Description { get; set; } = "GOC奇术打击小组-指挥官\n\n<color=cyan>血量提升至150</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.Medkit, ItemType.ArmorCombat, ItemType.KeycardMTFCaptain,
            ItemType.Adrenaline, ItemType.GrenadeHE, ItemType.Flashlight, ItemType.GunFRMG0
        };

        // 【安全修复】：抬高 2.5 米防卡
        public override Vector3 CinematicPosition => new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        public override string SpawnHint => "你已被选为GOC奇术打击小组指挥官！\n正在初始化...30秒后传送到地表A门";

        protected override void TeleportToFinalPosition(Player player)
        {
            var surfaceGateA = Room.Get(RoomType.EzGateA);
            player.Position = surfaceGateA != null ? surfaceGateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
            player.RankName = "GOC奇术打击小组-指挥官";
            player.RankColor = "cyan";
        }

        protected override void RoleRemoved(Player player)
        {
            GOCArcaneStrike.HandleMemberRemoved(player);
            base.RoleRemoved(player);
        }
    }

    public class GOCArcaneSergeant : SCP5KRole
    {
        public static GOCArcaneSergeant Instance { get; } = new GOCArcaneSergeant();
        public override uint Id { get; set; } = 66;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Tutorial;
        public override string Name { get; set; } = "GOC 奇术打击-中士";
        public override string CustomInfo { get; set; } = "GOC奇术打击小组-中士";
        public override int MaxHealth { get; set; } = 120;

        public override string Description { get; set; } = "GOC奇术打击小组-中士\n\n<color=orange>血量提升至120</color>";

        public override List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.Medkit, ItemType.ArmorCombat, ItemType.KeycardMTFCaptain,
            ItemType.Adrenaline, ItemType.GrenadeHE, ItemType.Flashlight, ItemType.GunE11SR
        };

        public override Vector3 CinematicPosition => new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
        public override string SpawnHint => "你已被选为GOC奇术打击小组中士！\n正在初始化...30秒后传送到地表A门";

        protected override void TeleportToFinalPosition(Player player)
        {
            var surfaceGateA = Room.Get(RoomType.EzGateA);
            player.Position = surfaceGateA != null ? surfaceGateA.Position + Vector3.up * 2.5f : new Vector3(69.939f, 320.33f, -44.94f) + Vector3.up * 2.5f;
            player.RankName = "GOC奇术打击小组-中士";
            player.RankColor = "cyan";
        }

        protected override void RoleRemoved(Player player)
        {
            GOCArcaneStrike.HandleMemberRemoved(player);
            base.RoleRemoved(player);
        }
    }

    public static class GOCArcaneStrike
    {
        private static CoroutineHandle countdownCoroutine;
        private static CoroutineHandle spawnMusicCoroutine;

        public static bool isGOCActive = false;
        private static bool isCountdownActive = false;
        private static bool isCaesarChallengeActive = false;
        private static bool hasPlayedSpawnAnnouncement = false;
        private static SchematicObject GocArcSchematicInstance;

        public class CaesarState
        {
            public Vector3 OriginalPosition { get; set; }
            public List<ItemType> OriginalItems { get; set; }
            public bool CaesarCompleted { get; set; } = false;
            public bool CaesarSuccess { get; set; } = false;
        }
        private static Dictionary<Player, CaesarState> caesarStates = new Dictionary<Player, CaesarState>();
        private static Dictionary<Player, List<ItemType>> playerDropSequence = new Dictionary<Player, List<ItemType>>();
        private static readonly ItemType[] requiredSequence = { ItemType.Lantern, ItemType.Flashlight, ItemType.Coin };

        private static int musicBotId = new System.Random().Next(1, 500);
        private static int spawnMusicBotId;

        public static string MusicPath { get; set; }
        public static string SpawnMusicPath { get; set; }
        public static float CountdownDuration { get; set; } = 120f;
        public static Vector3 SchematicPosition { get; set; }
        public static string SchematicName { get; set; } = "goc_strike_team";
        public static string RGMSchematicName { get; set; } = "RGM";
        public static Vector3 RGMSchematicPosition { get; set; } = Vector3.zero;

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

        public static bool IsGOCMember(Player player) => GOCArcaneCommander.Instance.Check(player) || GOCArcaneSergeant.Instance.Check(player);

        public static List<Player> GetAliveGOCMembers()
        {
            var members = new List<Player>();
            members.AddRange(GOCArcaneCommander.Instance.TrackedPlayers.Where(p => p.IsAlive));
            members.AddRange(GOCArcaneSergeant.Instance.TrackedPlayers.Where(p => p.IsAlive));
            return members;
        }

        public static bool SpawnGOCTeam(List<Player> players)
        {
            if (players.Count < 3) return false;

            ResetState();
            isGOCActive = true;
            hasPlayedSpawnAnnouncement = false;

            SpawnGOCSchematic();
            PlayGOCSpawnMusic();

            Timing.CallDelayed(2.0f, () =>
            {
                if (players.Count >= 1 && players[0] != null && players[0].IsConnected) GOCArcaneCommander.Instance.AddRole(players[0]);
                if (players.Count >= 2 && players[1] != null && players[1].IsConnected) GOCArcaneSergeant.Instance.AddRole(players[1]);
                if (players.Count >= 3 && players[2] != null && players[2].IsConnected) GOCArcaneSergeant.Instance.AddRole(players[2]);

                Timing.CallDelayed(1f, () =>
                {
                    if (!hasPlayedSpawnAnnouncement)
                    {
                        PlayGOCSpawnAnnouncement();
                        hasPlayedSpawnAnnouncement = true;
                    }
                });
            });

            return true;
        }

        public static void HandleMemberRemoved(Player player)
        {
            if (playerDropSequence.ContainsKey(player)) playerDropSequence.Remove(player);

            if (caesarStates.ContainsKey(player))
            {
                if (DecryptCommand.IsPlayerInChallenge(player))
                    DecryptCommand.ResetPlayerChallenge(player);
                caesarStates.Remove(player);
            }

            Timing.CallDelayed(0.2f, () =>
            {
                if (isCaesarChallengeActive) CheckAllCaesarChallengesCompleted();
                if (isCountdownActive) CheckGOCMembersStatus();
            });
        }

        private static void PlayGOCSpawnAnnouncement()
        {
            Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#00FFFF>-\r\n<split><b><color=#00FFFF>我们的存在，是为了保护人类免受一切违背自然秩序的威胁\r\n<split><b><color=#00FFFF>这就是我们的任务\r\n<split><b><color=#00FFFF>生存，隐蔽，保护，毁灭，教育\r\n<split><b><color=#00FFFF>设施内所有人员注意\r\n<split>警告，探测到未知武装人员\r\n<split>...\r\n<split>全球超自然部队被发现在地表入口\r\n<split>全站切换至防御模式\r\n<split>授权致命武力\r\n<split>...\r\n<split><b><color=#FF0000>“以奇术为剑，斩断血肉滋生”\r\n<split> $PITCH_0.5 $SLEEP_0.05 .G4 .G4 .G5 .G6 $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 We exist to protect human from all that defies the natural order $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 This is our mission $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 survival concealment protection destruction and education $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Attention all personnel $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Alert . Unknown forces detected $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Global Occult Coalition forces detected at Surface Gate $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 All site security defense model $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Lethal force authorize $SLEEP_0.5 .");
        }

        private static void SpawnGOCSchematic()
        {
            try { GocArcSchematicInstance = ObjectSpawner.SpawnSchematic(SchematicName, SchematicPosition); Timing.CallDelayed(40f, () => GocArcSchematicInstance?.Destroy()); } catch { }
        }

        private static void PlayGOCSpawnMusic()
        {
            if (string.IsNullOrEmpty(SpawnMusicPath) || !System.IO.File.Exists(SpawnMusicPath)) return;
            try
            {
                SafeRemoveAudioBot(spawnMusicBotId);
                if (AudioApi.Dummies.VoiceDummy.Add(spawnMusicBotId, "GOC-奇袭小组"))
                {
                    AudioApi.Dummies.VoiceDummy.Play(spawnMusicBotId, SpawnMusicPath);
                    spawnMusicCoroutine = Timing.CallDelayed(300f, StopGOCSpawnMusic);
                }
            }
            catch { }
        }

        private static void StopGOCSpawnMusic() { SafeRemoveAudioBot(spawnMusicBotId); }

        private static void OnPlayerDroppingItem(DroppingItemEventArgs ev)
        {
            if (!isGOCActive || isCountdownActive || !IsGOCMember(ev.Player)) return;
            CheckDropSequence(ev.Player, ev.Item.Type);
        }

        private static void OnPlayerPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup.Type == ItemType.Lantern && !IsGOCMember(ev.Player))
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint("<color=red>只有GOC奇术打击小组成员才能拾取这个手提灯！</color>", 3f);
            }
        }

        private static void CheckDropSequence(Player player, ItemType droppedItem)
        {
            if (!playerDropSequence.ContainsKey(player)) playerDropSequence[player] = new List<ItemType>();
            var sequence = playerDropSequence[player];

            if (!player.Zone.HasFlag(ZoneType.Surface))
            {
                if (sequence.Count > 0) { sequence.Clear(); player.ShowHint("必须在地表区域才能激活序列！", 3f); }
                return;
            }

            int expectedIndex = sequence.Count;
            if (droppedItem == requiredSequence[0])
            {
                sequence.Clear(); sequence.Add(droppedItem);
                player.ShowHint($"✓ 开始激活序列: {GetItemName(droppedItem)}", 3f);
                return;
            }

            if (sequence.Count == 0 && droppedItem != requiredSequence[0]) return;

            if (expectedIndex < requiredSequence.Length && droppedItem == requiredSequence[expectedIndex])
            {
                sequence.Add(droppedItem);
                player.ShowHint($"✓ 序列进度: {sequence.Count}/{requiredSequence.Length}", 3f);
                if (sequence.SequenceEqual(requiredSequence)) OnSequenceCompleted(player);
            }
            else
            {
                sequence.Clear();
                string expectedItem = expectedIndex < requiredSequence.Length ? GetItemName(requiredSequence[expectedIndex]) : "未知物品";
                player.ShowHint($"✗ 顺序错误！应该丢弃: {expectedItem}\n请重新从{GetItemName(requiredSequence[0])}开始", 5f);
            }
        }

        private static string GetItemName(ItemType itemType) => itemType switch { ItemType.Lantern => "手提灯", ItemType.Flashlight => "手电筒", ItemType.Coin => "硬币", _ => itemType.ToString() };

        private static void OnSequenceCompleted(Player player)
        {
            if (isCountdownActive || isCaesarChallengeActive) return;
            isCaesarChallengeActive = true;
            Map.ChangeLightsColor(Color.cyan);

            Exiled.API.Features.Cassie.MessageTranslated("Arcane strike activation imminent. All personnel immediately terminate GOC members. Prevent the arcane strike at all costs!", "奇术打击即将被激活，所有人立即击杀GOC成员，阻止奇术打击的到来！！！", false, true);
            PlayGOCMusic();
            StartCaesarChallenges();
        }

        private static void StartCaesarChallenges()
        {
            var aliveMembers = GetAliveGOCMembers();
            if (aliveMembers.Count == 0) return;

            foreach (var member in aliveMembers)
            {
                caesarStates[member] = new CaesarState
                {
                    OriginalPosition = member.Position,
                    OriginalItems = member.Items.Select(item => item.Type).ToList()
                };
                DecryptCommand.StartCaesarChallenge(member);
            }
            Timing.RunCoroutine(CheckCaesarChallenges());
        }

        private static IEnumerator<float> CheckCaesarChallenges()
        {
            while (isCaesarChallengeActive)
            {
                var aliveMembers = GetAliveGOCMembers();
                var pendingMembers = aliveMembers.Where(m => caesarStates.ContainsKey(m) && !caesarStates[m].CaesarCompleted).ToList();

                foreach (var member in pendingMembers)
                {
                    if (!DecryptCommand.IsPlayerInChallenge(member))
                    {
                        var state = caesarStates[member];
                        state.CaesarCompleted = true;
                        var result = DecryptCommand.GetPlayerChallengeResult(member);

                        if (result.HasValue && result.Value)
                        {
                            state.CaesarSuccess = true;
                            member.ShowHint("<color=green>✅ 凯撒密码挑战成功！</color>", 5f);

                            Timing.CallDelayed(0.5f, () =>
                            {
                                foreach (var other in GetAliveGOCMembers().Where(m => caesarStates.ContainsKey(m) && !caesarStates[m].CaesarCompleted))
                                {
                                    caesarStates[other].CaesarCompleted = true;
                                    caesarStates[other].CaesarSuccess = true;
                                    if (DecryptCommand.IsPlayerInChallenge(other)) DecryptCommand.ResetPlayerChallenge(other);
                                    other.ShowHint("<color=green>其他成员已成功完成挑战，你也算作成功！</color>", 5f);
                                }
                                OnCaesarChallengeSuccess();
                            });
                            yield break;
                        }
                        else
                        {
                            state.CaesarSuccess = false;
                            GOCArcaneCommander.Instance.RemoveRole(member);
                            GOCArcaneSergeant.Instance.RemoveRole(member);
                            member.Explode();
                            member.ShowHint("<color=red>凯撒密码错误，学术造假的代价是自爆！</color>", 3f);
                        }
                    }
                }
                CheckAllCaesarChallengesCompleted();
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private static void CheckAllCaesarChallengesCompleted()
        {
            var trackedStates = caesarStates.Values;
            if (trackedStates.Count > 0 && trackedStates.All(s => s.CaesarCompleted))
            {
                if (!trackedStates.Any(s => s.CaesarSuccess)) OnAllCaesarChallengesFailed();
            }
        }

        private static void OnCaesarChallengeSuccess()
        {
            isCaesarChallengeActive = false;
            isCountdownActive = true;

            var successMembers = GetAliveGOCMembers().Where(m => caesarStates.ContainsKey(m) && caesarStates[m].CaesarSuccess).ToList();
            if (successMembers.Count == 0) { OnAllCaesarChallengesFailed(); return; }

            foreach (var member in successMembers)
            {
                var state = caesarStates[member];
                DecryptCommand.ResetPlayerChallenge(member);
                // 【安全修复】：原位置恢复时同样给予轻微抬升，避免卡住
                member.Position = state.OriginalPosition + Vector3.up * 1.5f;
                member.ClearInventory();
                foreach (var itemType in state.OriginalItems) member.AddItem(itemType);
                member.ShowHint("<color=green>已恢复装备并传送回原位置！</color>", 5f);
            }

            countdownCoroutine = Timing.CallDelayed(CountdownDuration, CheckGOCMembersStatus);
            foreach (var p in Player.List) p.ShowHint($"<color=red>奇术打击激活倒计时开始！{CountdownDuration}秒后释放</color>", 10f);
            Exiled.API.Features.Cassie.MessageTranslated("Arcane strike activation imminent. All personnel immediately terminate GOC members. This is the final opportunity. Prevent the arcane strike at all costs!", "奇术打击即将被激活，所有人立即击杀GOC成员，这是最后一次机会！阻止奇术打击的到来！！！", false, true);
            ObjectSpawner.SpawnSchematic(RGMSchematicName, RGMSchematicPosition, Quaternion.identity, Vector3.one);
        }

        private static void OnAllCaesarChallengesFailed()
        {
            isCaesarChallengeActive = false;
            CancelGOCStrike();
        }

        private static void CheckGOCMembersStatus()
        {
            if (!isCountdownActive) return;

            var aliveMembers = GetAliveGOCMembers();
            if (aliveMembers.Count > 0)
            {
                ArcaneStrike.Activate(aliveMembers.First());
                ResetState();
            }
            else CancelGOCStrike();
        }

        private static void PlayGOCMusic()
        {
            if (string.IsNullOrEmpty(MusicPath) || !System.IO.File.Exists(MusicPath)) return;
            try { SafeRemoveAudioBot(musicBotId); if (AudioApi.Dummies.VoiceDummy.Add(musicBotId, "")) AudioApi.Dummies.VoiceDummy.Play(musicBotId, MusicPath); } catch { }
        }

        private static void StopGOCMusic() { SafeRemoveAudioBot(musicBotId); }

        private static void CancelGOCStrike()
        {
            isCountdownActive = false;
            isCaesarChallengeActive = false;

            foreach (var member in GetAliveGOCMembers().ToList())
            {
                GOCArcaneCommander.Instance.RemoveRole(member);
                GOCArcaneSergeant.Instance.RemoveRole(member);
            }

            StopGOCMusic();
            if (countdownCoroutine.IsRunning) Timing.KillCoroutines(countdownCoroutine);
            Map.ChangeLightsColor(Color.white);
            foreach (var player in Player.List) player.ShowHint("<color=green>奇术打击已被阻止！</color>", 10f);
            Exiled.API.Features.Cassie.MessageTranslated("GOC arcane strike protocol has been terminated. Facility status returning to normal.", "GOC奇术打击协议已被终止，设施已恢复正常运作状态", false, true);
        }

        private static void ResetState()
        {
            isGOCActive = false;
            isCountdownActive = false;
            isCaesarChallengeActive = false;
            hasPlayedSpawnAnnouncement = false;
            caesarStates.Clear();
            playerDropSequence.Clear();

            foreach (var player in Player.List)
            {
                if (IsGOCMember(player))
                {
                    GOCArcaneCommander.Instance.RemoveRole(player);
                    GOCArcaneSergeant.Instance.RemoveRole(player);
                }
            }

            if (countdownCoroutine.IsRunning) Timing.KillCoroutines(countdownCoroutine);
            if (spawnMusicCoroutine.IsRunning) Timing.KillCoroutines(spawnMusicCoroutine);
            StopGOCMusic();
            StopGOCSpawnMusic();
        }

        public static void RegisterEvents() { Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted; Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded; Exiled.Events.Handlers.Player.DroppingItem += OnPlayerDroppingItem; Exiled.Events.Handlers.Player.PickingUpItem += OnPlayerPickingUpItem; }
        public static void UnregisterEvents() { Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted; Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded; Exiled.Events.Handlers.Player.DroppingItem -= OnPlayerDroppingItem; Exiled.Events.Handlers.Player.PickingUpItem -= OnPlayerPickingUpItem; ResetState(); }
        private static void OnRoundStarted() => ResetState();
        private static void OnRoundEnded(RoundEndedEventArgs ev) => ResetState();
    }
}