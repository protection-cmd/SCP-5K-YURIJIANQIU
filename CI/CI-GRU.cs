using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning.Waves;
using SCP5K.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP5K.SCPFouRole
{
    public class CICommanderRole : CustomRole
    {
        public static CICommanderRole Instance { get; } = new CICommanderRole();
        public override uint Id { get; set; } = 71;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "CI-GRU-指挥官";
        public override string CustomInfo { get; set; } = "CI-GRU-指挥官";
        public override int MaxHealth { get; set; } = 120;

        public override string Description { get; set; }

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunAK, ItemType.KeycardChaosInsurgency, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.CIGRU);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.EnableEffect(EffectType.NightVision);

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                var message = "<color=yellow>你成为了混沌分裂者GRU小组的指挥官\n执行反基金会任务，守护人类！</color>";

                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "CI-GRU-指挥官入场");
            });
        }
    }

    public class CIRaznovRole : CustomRole
    {
        public static CIRaznovRole Instance { get; } = new CIRaznovRole();
        public override uint Id { get; set; } = 72;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosMarauder;
        public override string Name { get; set; } = "CI-GRU-雷泽诺夫";
        public override string CustomInfo { get; set; } = "CI-GRU-雷泽诺夫";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } 

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunFRMG0, ItemType.KeycardChaosInsurgency, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio, ItemType.Coin
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.CIGRU);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.EnableEffect(EffectType.NightVision);

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                var message = "<color=yellow>你成为了混沌分裂者GRU小组的雷泽诺夫\n<color=yellow>特殊能力：</color>\n• 丢弃硬币可部署地雷\n• 按G键重新获得硬币（60秒冷却）\n• 前往广播室使用对讲机可呼叫支援</color>";

                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "CI-GRU-雷泽诺夫入场");
            });
            CIGRU.InitializeRaznov(player);
        }

        protected override void RoleRemoved(Player player)
        {
            CIGRU.CleanupRaznov(player);
            base.RoleRemoved(player);
        }
    }

    public class CIHeavyRole : CustomRole
    {
        public static CIHeavyRole Instance { get; } = new CIHeavyRole();
        public override uint Id { get; set; } = 73;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRepressor;
        public override string Name { get; set; } = "CI-GRU-重装";
        public override string CustomInfo { get; set; } = "CI-GRU-重装";
        public override int MaxHealth { get; set; } = 150;

        public override string Description { get; set; } 

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunLogicer, ItemType.KeycardChaosInsurgency, ItemType.Medkit,
            ItemType.ArmorHeavy, ItemType.Adrenaline, ItemType.Radio, ItemType.GrenadeHE
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.CIGRU);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.EnableEffect(EffectType.NightVision);

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                var message = "<color=yellow>你成为了混沌分裂者GRU小组的重装\n执行反基金会任务，守护人类！</color>";

                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "CI-GRU-重装入场");
            });
        }
    }

    public class CIRiflemanRole : CustomRole
    {
        public static CIRiflemanRole Instance { get; } = new CIRiflemanRole();
        public override uint Id { get; set; } = 74;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosRifleman;
        public override string Name { get; set; } = "CI-GRU-步枪手";
        public override string CustomInfo { get; set; } = "CI-GRU-步枪手";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } 

        public List<ItemType> CustomRoleItems { get; set; } = new List<ItemType>
        {
            ItemType.GunAK, ItemType.KeycardChaosInsurgency, ItemType.Medkit,
            ItemType.ArmorCombat, ItemType.Adrenaline, ItemType.Radio
        };

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.CIGRU);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.EnableEffect(EffectType.NightVision);

                player.ClearInventory();
                foreach (var item in CustomRoleItems) player.AddItem(item);
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                var message = "<color=yellow>你成为了混沌分裂者GRU小组的步枪手\n执行反基金会任务，守护人类！</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 15f, "CI-GRU-步枪手入场");
            });
        }
    }

    public static class CIGRU
    {
        private static Dictionary<Player, DateTime> mineAbilityCooldowns = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, List<Vector3>> raznovMines = new Dictionary<Player, List<Vector3>>();
        private static Dictionary<Player, CoroutineHandle> raznovSupportCoroutines = new Dictionary<Player, CoroutineHandle>();
        private static Dictionary<Player, bool> raznovCallingSupport = new Dictionary<Player, bool>();
        private static Dictionary<Player, bool> raznovHasCoin = new Dictionary<Player, bool>();
        private static Dictionary<Player, bool> raznovCanCallSupport = new Dictionary<Player, bool>();

        private static string originalIntercomText = string.Empty;
        private static bool isIntercomTextModified = false;
        private static bool memeWeaponSpawned = false;
        private static bool memeWeaponActivating = false;
        private static CoroutineHandle memeWeaponActivationCoroutine;
        private static bool memeWeaponCanBePickedByNonCI = false;
        private static ushort memeWeaponSerial = 0;
        private const float MemeWeaponWeight = 44.3142857f;
        private static bool eventsRegistered = false;

        public static bool IsCIMember(Player player) => CICommanderRole.Instance.Check(player) || CIRaznovRole.Instance.Check(player) || CIHeavyRole.Instance.Check(player) || CIRiflemanRole.Instance.Check(player);
        public static bool IsRaznov(Player player) => CIRaznovRole.Instance.Check(player);

        public static void InitializeRaznov(Player player)
        {
            raznovMines[player] = new List<Vector3>();
            raznovCallingSupport[player] = false;
            raznovHasCoin[player] = true;
            raznovCanCallSupport[player] = true;
            SetIntercomTextForRaznov();
        }

        public static void CleanupRaznov(Player player)
        {
            raznovMines.Remove(player);
            if (raznovSupportCoroutines.ContainsKey(player)) Timing.KillCoroutines(raznovSupportCoroutines[player]);
            raznovSupportCoroutines.Remove(player);
            raznovCallingSupport.Remove(player);
            raznovHasCoin.Remove(player);
            raznovCanCallSupport.Remove(player);
            mineAbilityCooldowns.Remove(player);
        }

        public static bool SpawnCITeam(List<Player> players)
        {
            if (players.Count < 3 || players.Count > 6) return false;

            ResetState();
            string[] roles = GetRolesByPlayerCount(players.Count);
            if (roles == null) return false;

            var chaoWave = new ChaosSpawnWave();
            Respawn.PlayEffect(chaoWave);

            Timing.CallDelayed(13f,() =>
            {
                for (int i = 0; i < roles.Length; i++)
                {
                    var p = players[i];
                    if (roles[i] == "指挥官") CICommanderRole.Instance.AddRole(p);
                    else if (roles[i] == "雷泽诺夫") CIRaznovRole.Instance.AddRole(p);
                    else if (roles[i] == "重装") CIHeavyRole.Instance.AddRole(p);
                    else if (roles[i] == "步枪手") CIRiflemanRole.Instance.AddRole(p);
                }

                Timing.CallDelayed(2f, () =>
                {

                    Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#228B22>-\r\n<split><b><color=#228B22>注意，注意<split><b><color=#228B22>检测到混沌分裂者活动<split><b><color=#228B22>所有非必要人员请立即撤离<split> $PITCH_0.5 $SLEEP_0.05 .G4 .G4 .G5 .G6 $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Attention . Attention $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Chaos Insurgency activity detected $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 All non-essential personnel evacuate immediately $SLEEP_0.5 .");
                    Timing.CallDelayed(1f, () => SpawnMemeWeapon());
                });
            });
            return true;
        }

        private static string[] GetRolesByPlayerCount(int count)
        {
            switch (count)
            {
                case 3: return new[] { "指挥官", "雷泽诺夫", "重装" };
                case 4: return new[] { "指挥官", "雷泽诺夫", "重装", "步枪手" };
                case 5: return new[] { "指挥官", "雷泽诺夫", "重装", "步枪手", "步枪手" };
                case 6: return new[] { "指挥官", "雷泽诺夫", "重装", "重装", "步枪手", "步枪手" };
                default: return null;
            }
        }

        public static void ExecuteRaznovCoinAbilityFromKeybind(Player raznov)
        {
            if (!IsRaznov(raznov)) return;

            if (mineAbilityCooldowns.ContainsKey(raznov) && (DateTime.Now - mineAbilityCooldowns[raznov]).TotalSeconds < 60)
            {
                var remaining = 60 - (DateTime.Now - mineAbilityCooldowns[raznov]).TotalSeconds;
                raznov.ShowHint($"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>", 3f);
                return;
            }

            if (raznovHasCoin.TryGetValue(raznov, out bool hasCoin) && hasCoin)
            {
                raznov.ShowHint($"<color=yellow>你已经拥有硬币！</color>", 3f);
                return;
            }

            raznovHasCoin[raznov] = true;
            raznov.AddItem(ItemType.Coin);
            mineAbilityCooldowns[raznov] = DateTime.Now;
            raznov.ShowHint($"<color=green>已重新获得硬币！60秒后可再次使用此能力</color>", 5f);
        }

        private static void OnRaznovDroppingItem(DroppingItemEventArgs ev)
        {
            if (!IsRaznov(ev.Player) || ev.Item.Type != ItemType.Coin) return;

            if (!raznovHasCoin.TryGetValue(ev.Player, out bool hasCoin) || !hasCoin)
            {
                ev.Player.ShowHint("<color=red>你没有地雷可部署！</color>", 3f);
                ev.IsAllowed = false;
                return;
            }

            Vector3 minePos = ev.Player.Position;
            if (!raznovMines.ContainsKey(ev.Player)) raznovMines[ev.Player] = new List<Vector3>();
            raznovMines[ev.Player].Add(minePos);
            raznovHasCoin[ev.Player] = false;

            ev.Player.ShowHint($"<color=yellow>已在地面部署地雷！</color>", 5f);
            Timing.RunCoroutine(CheckMineTrigger(ev.Player, minePos));
            ev.IsAllowed = true;
        }

        private static IEnumerator<float> CheckMineTrigger(Player raznov, Vector3 minePosition)
        {
            float checkInterval = 0.5f;
            float maxDuration = 300f;
            float elapsedTime = 0f;

            while (elapsedTime < maxDuration)
            {
                foreach (var player in Player.List.Where(p => p.IsAlive && p != raznov && !IsCIMember(p)))
                {
                    if (Vector3.Distance(player.Position, minePosition) <= 3f)
                    {
                        ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                        grenade.FuseTime = 0.1f;
                        grenade.SpawnActive(player.Position, player);
                        if (raznovMines.ContainsKey(raznov)) raznovMines[raznov].Remove(minePosition);
                        yield break;
                    }
                }
                elapsedTime += checkInterval;
                yield return Timing.WaitForSeconds(checkInterval);
            }
            if (raznovMines.ContainsKey(raznov)) raznovMines[raznov].Remove(minePosition);
        }

        private static void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (!IsRaznov(ev.Player)) return;

            if (raznovCanCallSupport.TryGetValue(ev.Player, out bool canCall) && canCall && isIntercomTextModified && Intercom.DisplayText == "申请CI支援")
            {
                ev.Player.ShowHint("<color=yellow>正在呼叫混沌分裂者支援... 等待观察者加入</color>", 5f);
                raznovCanCallSupport[ev.Player] = false;
                Timing.CallDelayed(0.1f, () => { CallForSupport(ev.Player); ResetIntercomText(); });
            }
        }

        private static void CallForSupport(Player raznov)
        {
            if (raznovCallingSupport.TryGetValue(raznov, out bool calling) && calling) return;
            raznovCallingSupport[raznov] = true;

            Exiled.API.Features.Cassie.MessageTranslated("Chaos Insurgency reinforcement requested. Standby for arrival.", "混沌分裂者已请求支援，待命到达。", false, true);
            raznovSupportCoroutines[raznov] = Timing.RunCoroutine(CheckObserversForSupport(raznov));
        }

        private static IEnumerator<float> CheckObserversForSupport(Player raznov)
        {
            float elapsedTime = 0f;
            List<Player> spawnedSupport = new List<Player>();

            while (elapsedTime < 60f)
            {
                var spectators = Player.Get(RoleTypeId.Spectator).Where(p => p.IsConnected).ToList();
                int needed = 3 - spawnedSupport.Count;
                int available = Math.Min(spectators.Count, needed);

                for (int i = 0; i < available; i++)
                {
                    var spectator = spectators[i];
                    CIRiflemanRole.Instance.AddRole(spectator);
                    Timing.CallDelayed(0.5f, () => spectator.Position = raznov.Position + new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f)));
                    spawnedSupport.Add(spectator);
                }

                if (spawnedSupport.Count >= 3)
                {
                    Exiled.API.Features.Cassie.MessageTranslated("Chaos Insurgency reinforcement arrived.", "混沌分裂者支援已到达。", false, true);
                    break;
                }
                elapsedTime += 2f;
                yield return Timing.WaitForSeconds(2f);
            }
            raznovCallingSupport[raznov] = false;
        }

        private static void SetIntercomTextForRaznov()
        {
            try { originalIntercomText = Intercom.DisplayText; Intercom.DisplayText = "申请CI支援"; isIntercomTextModified = true; } catch { }
        }

        private static void ResetIntercomText()
        {
            try { if (isIntercomTextModified) { Intercom.DisplayText = originalIntercomText; isIntercomTextModified = false; } } catch { }
        }

        private static void SpawnMemeWeapon()
        {
            if (memeWeaponSpawned) return;
            var room079 = Room.Get(RoomType.Hcz079);
            if (room079 == null) return;

            var startPosObj = new GameObject();
            startPosObj.transform.parent = room079.GameObject.transform;
            startPosObj.transform.localPosition = new Vector3(5.824f, -2.372f, -6.852f);
            var pickup = Pickup.CreateAndSpawn(ItemType.MicroHID, startPosObj.transform.position, Quaternion.identity);

            if (pickup != null)
            {
                memeWeaponSerial = pickup.Info.Serial;
                pickup.Weight = MemeWeaponWeight;
                memeWeaponSpawned = true;
                memeWeaponCanBePickedByNonCI = false;
            }
            UnityEngine.Object.Destroy(startPosObj);
        }

        private static void OnPlayerDroppedItem(DroppedItemEventArgs ev)
        {
            if (ev.Pickup != null && ev.Pickup.Info.Serial == memeWeaponSerial && memeWeaponSerial != 0 && IsCIMember(ev.Player))
            {
                memeWeaponSerial = ev.Pickup.Info.Serial;
                ev.Pickup.Weight = MemeWeaponWeight;

                if (ev.Player.CurrentRoom != null && ev.Player.CurrentRoom.Type == RoomType.HczHid)
                {
                    StartMemeWeaponActivation(ev.Player);
                }
                else ev.Player.ShowHint("<color=yellow>模因武器必须在HID存储间丢弃才能激活！</color>", 5f);
            }
        }

        private static void StartMemeWeaponActivation(Player player)
        {
            if (memeWeaponActivating) return;
            memeWeaponActivating = true;
            memeWeaponCanBePickedByNonCI = true;

            foreach (var p in Player.List.Where(p => p.IsConnected))
                p.ShowHint($"<color=red>警告！CI-GRU正在HID存储间激活模因武器！\n倒计时120秒后所有非CI成员将被消灭！</color>", 10f);

            Exiled.API.Features.Cassie.MessageTranslated("Warning . CI-GRU meme weapon activation detected in HID containment . 120 seconds until detonation .", "警告。检测到CI-GRU在HID收容间激活模因武器。120秒后引爆。", false, true);
            memeWeaponActivationCoroutine = Timing.RunCoroutine(MemeWeaponCountdown());
        }

        private static IEnumerator<float> MemeWeaponCountdown()
        {
            float countdown = 120f;
            while (countdown > 0 && memeWeaponActivating)
            {
                if (countdown % 10f < 1f || countdown <= 10f)
                {
                    foreach (var p in Player.List) p.ShowHint($"<color=red>模因武器激活倒计时: {countdown:F0}秒</color>", 5f);
                    if (countdown <= 10f) Exiled.API.Features.Cassie.MessageTranslated($". {countdown:F0}", $"倒计时 {countdown:F0} 秒", false, true);
                }
                countdown -= 1f;
                yield return Timing.WaitForSeconds(1f);
            }
            if (memeWeaponActivating) ExecuteMemeContamination();
        }

        private static void ExecuteMemeContamination()
        {
            Exiled.API.Features.Cassie.MessageTranslated("Meme contamination initiated . All non-Chaos personnel terminated .", "模因污染启动。所有非混沌人员已终止。", false, true);
            foreach (var player in Player.List.Where(p => p.IsAlive))
            {
                if (!IsCIMember(player)) player.Kill("CI-GRU-模因污染");
                else player.ShowHint("<color=green>你免疫了模因污染！</color>", 5f);
            }
            ResetMemeWeaponState();
        }

        private static void OnPlayerSearchingPickup(SearchingPickupEventArgs ev)
        {
            if (ev.Pickup != null && ev.Pickup.Info.Serial == memeWeaponSerial && memeWeaponSerial != 0)
            {
                bool isCI = IsCIMember(ev.Player);
                if (memeWeaponCanBePickedByNonCI)
                {
                    if (isCI) { ev.IsAllowed = false; ev.Player.ShowHint("<color=red>模因武器激活中，只有非CI成员可拾取终止！</color>", 5f); }
                }
                else
                {
                    if (!isCI) { ev.IsAllowed = false; ev.Player.ShowHint("<color=red>只有CI-GRU成员可以使用。</color>", 3f); }
                }
            }
        }

        private static void OnPlayerPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.Pickup != null && ev.Pickup.Info.Serial == memeWeaponSerial && memeWeaponSerial != 0)
            {
                ev.Pickup.Weight = 2.0f;
                if (memeWeaponCanBePickedByNonCI && !IsCIMember(ev.Player) && memeWeaponActivating)
                {
                    memeWeaponActivating = false;
                    if (memeWeaponActivationCoroutine.IsRunning) Timing.KillCoroutines(memeWeaponActivationCoroutine);
                    memeWeaponCanBePickedByNonCI = false;
                    foreach (var p in Player.List) p.ShowHint($"<color=green>模因武器被 {ev.Player.Nickname} 拾取，激活已终止！</color>", 5f);
                }
            }
        }

        private static void ResetMemeWeaponState()
        {
            memeWeaponSpawned = false;
            memeWeaponActivating = false;
            memeWeaponCanBePickedByNonCI = false;
            memeWeaponSerial = 0;
            if (memeWeaponActivationCoroutine.IsRunning) Timing.KillCoroutines(memeWeaponActivationCoroutine);
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev) => ResetState();

        private static void ResetState()
        {
            mineAbilityCooldowns.Clear();
            raznovMines.Clear();
            foreach (var cor in raznovSupportCoroutines.Values) Timing.KillCoroutines(cor);
            raznovSupportCoroutines.Clear();
            raznovCallingSupport.Clear();
            raznovHasCoin.Clear();
            raznovCanCallSupport.Clear();
            ResetMemeWeaponState();
            ResetIntercomText();
        }

        public static void RegisterEvents()
        {
            if (eventsRegistered) return;
            Exiled.Events.Handlers.Player.DroppingItem += OnRaznovDroppingItem;
            Exiled.Events.Handlers.Player.IntercomSpeaking += OnIntercomSpeaking;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.SearchingPickup += OnPlayerSearchingPickup;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPlayerPickingUpItem;
            Exiled.Events.Handlers.Player.DroppedItem += OnPlayerDroppedItem;
            eventsRegistered = true;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.DroppingItem -= OnRaznovDroppingItem;
            Exiled.Events.Handlers.Player.IntercomSpeaking -= OnIntercomSpeaking;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.SearchingPickup -= OnPlayerSearchingPickup;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPlayerPickingUpItem;
            Exiled.Events.Handlers.Player.DroppedItem -= OnPlayerDroppedItem;
            ResetState();
            eventsRegistered = false;
        }
    }
}