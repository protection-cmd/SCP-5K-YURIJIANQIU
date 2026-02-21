using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using SCP5K.Events;
using System.Collections.Generic;

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

        public override string Description { get; set; } 

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.ClassD);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.EnableEffect(EffectType.MovementBoost);
                player.ChangeEffectIntensity(EffectType.MovementBoost, 10);
                DDRunning.InitializeAthlete(player);
                var message = $"<color=orange>你被选为运动员！\n移动速度提升至1.5倍\n按下爆发极限按键可临时提升速度</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 10f, "运动员");
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

        public static bool SetPlayerAsAthlete(Player player)
        {
            AthleteRole.Instance.AddRole(player);
            return true;
        }

        public static void ActivateAthleteAbility(Player player)
        {
            if (!IsAthlete(player) || !athleteStates.TryGetValue(player, out var state)) return;

            if (state.IsAbilityCooldown) 
            { 
                var message1 = $"<color=red>爆发极限技能冷却中！</color>";
                HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "技能冷却");
                return; 
            }
            if (state.IsAbilityActive) return;

            state.IsAbilityActive = true;
            player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)BoostedSpeedIntensity);
            var message = $"<color=yellow>爆发极限激活！持续40秒</color>";
            HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "爆发极限激活");

            state.DurationCoroutine = Timing.CallDelayed(40f, () =>
            {
                if (IsAthlete(player))
                {
                    player.ChangeEffectIntensity(EffectType.MovementBoost, 10);
                    state.IsAbilityActive = false;
                    var message2 = $"<color=orange>爆发极限效果结束</color>";
                    HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "技能冷却");
                }
            });

            state.IsAbilityCooldown = true;
            state.CooldownCoroutine = Timing.CallDelayed(220f, () =>
            {
                if (IsAthlete(player))
                {
                    state.IsAbilityCooldown = false;
                    var message3 = $"<color=green>爆发极限技能已冷却完成！</color>";
                    HSMShowhint.HsmShowHint(player, message3, 600, 0, 5f, "技能冷却");
                }
            });
        }

        // 移除了 OnRoundStarted 绑定
        public static void RegisterEvents() { }
        public static void UnregisterEvents() { athleteStates.Clear(); }
    }
}