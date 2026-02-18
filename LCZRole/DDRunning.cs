using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K.LCZRole
{
    public class AthleteRole : CustomRole
    {
        public static AthleteRole Instance { get; } = new AthleteRole();
        public override uint Id { get; set; } = 902;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override string Name { get; set; } = "运动员";
        public override string CustomInfo { get; set; } = "运动员";
        public override int MaxHealth { get; set; } = 100;

        public override string Description { get; set; } = "特殊D级人员 - 运动员\n\n<color=orange>移动速度提升，拥有爆发极限技能</color>";

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.EnableEffect(EffectType.MovementBoost);
                player.ChangeEffectIntensity(EffectType.MovementBoost, 10);
                DDRunning.InitializeAthlete(player);
                player.ShowHint($"<color=orange>你被选为运动员！\n移动速度提升至1.5倍\n按下爆发极限按键可临时提升速度</color>", 10f);
            });
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.MovementBoost);
            DDRunning.CleanupAthlete(player);
            base.RoleRemoved(player);
        }
    }

    public static class DDRunning
    {
        private class AthleteState
        {
            public bool IsAbilityActive = false;
            public bool IsAbilityCooldown = false;
            public CoroutineHandle? DurationCoroutine;
            public CoroutineHandle? CooldownCoroutine;
        }

        private static Dictionary<Player, AthleteState> athleteStates = new Dictionary<Player, AthleteState>();
        private const float BoostedSpeedIntensity = 60f;

        public static bool IsAthlete(Player player) => AthleteRole.Instance.Check(player);

        public static void InitializeAthlete(Player player) => athleteStates[player] = new AthleteState();
        public static void CleanupAthlete(Player player)
        {
            if (athleteStates.TryGetValue(player, out var state))
            {
                if (state.DurationCoroutine.HasValue) Timing.KillCoroutines(state.DurationCoroutine.Value);
                if (state.CooldownCoroutine.HasValue) Timing.KillCoroutines(state.CooldownCoroutine.Value);
            }
            athleteStates.Remove(player);
        }

        // 移除自动选择逻辑，仅保留设置方法
        public static bool SetPlayerAsAthlete(Player player)
        {
            AthleteRole.Instance.AddRole(player);
            return true;
        }

        public static void ActivateAthleteAbility(Player player)
        {
            if (!IsAthlete(player) || !athleteStates.TryGetValue(player, out var state)) return;

            if (state.IsAbilityCooldown) { player.ShowHint("<color=red>爆发极限技能正在冷却中！</color>", 3f); return; }
            if (state.IsAbilityActive) return;

            state.IsAbilityActive = true;
            player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)BoostedSpeedIntensity);
            player.ShowHint($"<color=yellow>爆发极限激活！持续40秒</color>", 5f);

            state.DurationCoroutine = Timing.CallDelayed(40f, () =>
            {
                if (IsAthlete(player))
                {
                    player.ChangeEffectIntensity(EffectType.MovementBoost, 10);
                    state.IsAbilityActive = false;
                    player.ShowHint("<color=orange>爆发极限效果结束</color>", 3f);
                }
            });

            state.IsAbilityCooldown = true;
            state.CooldownCoroutine = Timing.CallDelayed(220f, () =>
            {
                if (IsAthlete(player))
                {
                    state.IsAbilityCooldown = false;
                    player.ShowHint("<color=green>爆发极限技能已冷却完成！</color>", 3f);
                }
            });
        }

        // 移除了 OnRoundStarted 绑定
        public static void RegisterEvents() { }
        public static void UnregisterEvents() { athleteStates.Clear(); }
    }
}