using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP5K.LCZRole
{
    public class D9341Role : CustomRole
    {
        public static D9341Role Instance { get; } = new D9341Role();
        public override uint Id { get; set; } = 9341;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override string Name { get; set; } = "D9341";
        public override string CustomInfo { get; set; } = "D9341";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "特殊D级人员 - D-9341\n\n<color=yellow>拥有保存与读取进度的能力</color>";

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            D9341EventHandler.InitializePlayer(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;

                player.RankColor = "orange";
                player.RankName = "LCZ-D-9341";
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.ClearInventory();
                foreach (var item in Plugin.Instance.Config.D9341InitialItems)
                    player.AddItem(item);

                player.ShowHint("\n\n\n你是D-9341\n丢弃手电筒/手提灯进行存档\n丢弃硬币后，丢弃对应标志物进行读档（30秒CD）\n去逃离吧,自由面前,死亡亦不足惜", 10f);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            D9341EventHandler.CleanupPlayer(player);
            base.RoleRemoved(player);
        }
    }

    public class D9341State
    {
        public List<SaveData> SavePoints { get; private set; } = new List<SaveData>();
        public int CurrentSaveIndex { get; set; } = -1;
        public int ReviveCount { get; set; } = 0;
        public float LastLoadTime { get; set; } = 0f;
        public bool IsEnhanced { get; set; } = false;

        public bool IsLoadOnCooldown => Time.time - LastLoadTime < 30f;
        public float LoadCooldownRemaining => 30f - (Time.time - LastLoadTime);

        public struct SaveData
        {
            public Vector3 Position;
            public List<ItemType> Items;
            public SaveData(Vector3 pos, List<ItemType> items) { Position = pos; Items = new List<ItemType>(items); }
        }
    }

    public class D9341EventHandler
    {
        private static Dictionary<Player, D9341State> d9341States = new Dictionary<Player, D9341State>();
        private static bool EscapeTriggered = false;
        private static bool hasAttemptedAutoSelectThisRound = false;

        public static bool IsD9341(Player player) => D9341Role.Instance.Check(player);
        public static void InitializePlayer(Player player) => d9341States[player] = new D9341State();
        public static void CleanupPlayer(Player player)
        {
            d9341States.Remove(player);
            player.DisableEffect(EffectType.MovementBoost);
            Map.ChangeLightsColor(Color.white);
        }

        public bool SetPlayerAsD9341(Player player)
        {
            D9341Role.Instance.AddRole(player);
            return true;
        }

        public void RegisterEvents() { Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem; Exiled.Events.Handlers.Player.Dying += OnDying; Exiled.Events.Handlers.Player.Escaping += OnEscaping; Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted; Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded; }
        public void UnregisterEvents() { Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem; Exiled.Events.Handlers.Player.Dying -= OnDying; Exiled.Events.Handlers.Player.Escaping -= OnEscaping; Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted; Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded; d9341States.Clear(); }

        private void OnRoundStarted()
        {
            EscapeTriggered = false;
            hasAttemptedAutoSelectThisRound = false;
            d9341States.Clear();

            if (!Plugin.Instance.Config.AutoSelectD9341) return;
            hasAttemptedAutoSelectThisRound = true;

            Timing.CallDelayed(2.5f, () =>
            {
                var eligiblePlayers = Player.Get(RoleTypeId.ClassD).Where(p =>
                    string.IsNullOrEmpty(p.RankName) &&
                    !D9341Role.Instance.Check(p) &&
                    !AthleteRole.Instance.Check(p) &&
                    !LiangziRole.Instance.Check(p)).ToList();

                if (eligiblePlayers.Count == 0) return;

                var random = new System.Random();
                D9341Role.Instance.AddRole(eligiblePlayers[random.Next(eligiblePlayers.Count)]);
            });
        }

        private void OnRoundEnded(RoundEndedEventArgs ev) { hasAttemptedAutoSelectThisRound = false; }

        private void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!IsD9341(ev.Player) || !d9341States.TryGetValue(ev.Player, out var state)) return;

            if (ev.Item.Type == ItemType.Flashlight && state.SavePoints.Count == 0)
            {
                ev.IsAllowed = false;
                ev.Player.RemoveItem(ev.Item);
                ev.Player.AddItem(ItemType.Lantern);
                var items = ev.Player.Items.Select(i => i.Type).ToList();
                state.SavePoints.Add(new D9341State.SaveData(ev.Player.Position, items));
                state.CurrentSaveIndex = 0;
                ev.Player.ShowHint(Plugin.Instance.Config.SaveHint, 5f);
                return;
            }

            if (ev.Item.Type == ItemType.Lantern && state.SavePoints.Count == 1)
            {
                ev.IsAllowed = false;
                ev.Player.RemoveItem(ev.Item);
                var items = ev.Player.Items.Select(i => i.Type).ToList();
                state.SavePoints.Add(new D9341State.SaveData(ev.Player.Position, items));
                state.CurrentSaveIndex = 1;
                ev.Player.ShowHint(Plugin.Instance.Config.SaveHint, 5f);
                return;
            }

            if (ev.Item.Type == ItemType.Coin)
            {
                if (state.IsLoadOnCooldown) { ev.IsAllowed = false; ev.Player.ShowHint($"冷却中，剩{state.LoadCooldownRemaining:F1}秒", 2f); return; }
                ev.IsAllowed = false;
                ev.Player.RemoveItem(ev.Item);

                if (state.SavePoints.Count == 0) { ev.Player.ShowHint("无可用存档", 3f); ev.Player.AddItem(ItemType.Coin); return; }

                ev.Player.ClearInventory();
                if (state.SavePoints.Count > 0) ev.Player.AddItem(ItemType.Flashlight);
                if (state.SavePoints.Count > 1) ev.Player.AddItem(ItemType.Lantern);
                ev.Player.ShowHint("已发存档读档器", 2f);
                return;
            }

            if ((ev.Item.Type == ItemType.Flashlight && state.SavePoints.Count > 0) || (ev.Item.Type == ItemType.Lantern && state.SavePoints.Count > 1))
            {
                if (state.IsLoadOnCooldown) { ev.IsAllowed = false; ev.Player.ShowHint($"读档冷却中: {state.LoadCooldownRemaining:F1}秒", 3f); return; }
                ev.IsAllowed = false;
                ev.Player.RemoveItem(ev.Item);

                int loadIndex = ev.Item.Type == ItemType.Flashlight ? 0 : 1;
                LoadSaveState(ev.Player, state, loadIndex, true);
                ev.Player.ShowHint($"回到第{loadIndex + 1}存档", 5f);
                return;
            }
        }

        private void LoadSaveState(Player player, D9341State state, int index, bool setCooldown)
        {
            var data = state.SavePoints[index];
            player.ClearInventory();
            player.Position = data.Position;
            foreach (var item in data.Items) player.AddItem(item);

            foreach (var marker in new[] { ItemType.Flashlight, ItemType.Lantern })
                foreach (var i in player.Items.Where(it => it.Type == marker).ToList()) player.RemoveItem(i);

            if (state.SavePoints.Count == 0) player.AddItem(ItemType.Flashlight);
            else if (state.SavePoints.Count == 1) player.AddItem(ItemType.Lantern);

            if (setCooldown) state.LastLoadTime = Time.time;
            state.CurrentSaveIndex = index;
        }

        private void OnDying(DyingEventArgs ev)
        {
            if (!IsD9341(ev.Player) || !d9341States.TryGetValue(ev.Player, out var state)) return;

            if (state.IsEnhanced || state.SavePoints.Count == 0 || state.CurrentSaveIndex < 0)
            {
                D9341Role.Instance.RemoveRole(ev.Player);
                return;
            }

            ev.IsAllowed = false;
            Timing.CallDelayed(0.1f, () =>
            {
                if (state.ReviveCount >= 2)
                {
                    D9341Role.Instance.RemoveRole(ev.Player);
                    ev.Player.Kill("复活次数已用尽");
                    return;
                }
                LoadSaveState(ev.Player, state, state.CurrentSaveIndex, false);
                state.ReviveCount++;
                ev.Player.ShowHint($"死亡读档! (复活 {state.ReviveCount}/2 次)", 5f);
            });
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            if (IsD9341(ev.Player) && !EscapeTriggered)
            {
                EscapeTriggered = true;
                ev.IsAllowed = false;

                if (d9341States.TryGetValue(ev.Player, out var state))
                {
                    state.SavePoints.Clear();
                    state.CurrentSaveIndex = -1;
                    state.ReviveCount = 0;
                    state.IsEnhanced = true;

                    ev.Player.MaxHealth = 1000f;
                    ev.Player.Health = 1000f;
                    ev.Player.ClearInventory();

                    ev.Player.AddItem(ItemType.Jailbird);
                    ev.Player.AddItem(ItemType.SCP500);
                    ev.Player.AddItem(ItemType.KeycardO5);
                    ev.Player.AddItem(ItemType.Adrenaline);
                    ev.Player.AddItem(ItemType.Coin);
                    ev.Player.AddItem(ItemType.Flashlight);

                    ev.Player.ShowHint("\n\n\n逃脱失败了,这真的是终点吗?\n下一次死亡,将是真正的解脱", 10f);
                }
            }
        }
    }
}