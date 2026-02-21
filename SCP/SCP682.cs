using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Hints;
using MEC;
using PlayerRoles;
using SCP5K.Events;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static Subtitles.SubtitleCategory;

namespace SCP5K.SCPFouRole
{
    public class SCP682Role : CustomRole
    {
        public static SCP682Role Instance { get; } = new SCP682Role();
        public override uint Id { get; set; } = 682;
        public override RoleTypeId Role { get; set; } = RoleTypeId.Scp939;
        public override string Name { get; set; } = "SCP-682";
        public override string CustomInfo { get; set; } = "SCP-682";
        public override int MaxHealth { get; set; } = 6000;

        public override string Description { get; set; } 

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            FactionManager.AddPlayer(player, FactionType.SCP682);
            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;

                player.EnableEffect(EffectType.DamageReduction, 30);
                string hintMessage = "\n\n\n\n你已变为SCP-682 - 不灭孽蜥\n" +
                                     "<color=red>憎恶: 使普攻附带心脏骤停效果20秒，CD50秒</color>\n" +
                                     "<color=yellow>按下绑定键触发</color>\n" +
                                     "<color=green>可悲: 使自身获得缓慢回复效果，每秒回复10HP，持续10秒，CD50秒</color>\n" +
                                     "<color=yellow>按下绑定键触发</color>";
                HSMShowhint.HsmShowHint(player, hintMessage, 600, 0, 10f, "SCP-682");
            });
            SCP682.Initialize682(player);
        }

        protected override void RoleRemoved(Player player)
        {
            player.DisableEffect(EffectType.DamageReduction);
            SCP682.Cleanup682(player);
            base.RoleRemoved(player);
        }
    }

    public static class SCP682
    {
        private static Dictionary<Player, DateTime> abhorrenceCooldowns = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, DateTime> pitifulCooldowns = new Dictionary<Player, DateTime>();
        private static Dictionary<Player, CoroutineHandle> regenerationCoroutines = new Dictionary<Player, CoroutineHandle>();

        private static Dictionary<Player, bool> abhorrenceActive = new Dictionary<Player, bool>();
        private static Dictionary<Player, DateTime> abhorrenceActivatedTime = new Dictionary<Player, DateTime>();

        public static bool IsSCP682(Player player) => SCP682Role.Instance.Check(player);

        public static void Initialize682(Player player)
        {
            abhorrenceActive[player] = false;
        }

        public static void Cleanup682(Player player)
        {
            abhorrenceCooldowns.Remove(player);
            pitifulCooldowns.Remove(player);
            abhorrenceActive.Remove(player);
            abhorrenceActivatedTime.Remove(player);

            if (regenerationCoroutines.ContainsKey(player) && regenerationCoroutines[player].IsRunning)
                Timing.KillCoroutines(regenerationCoroutines[player]);
            regenerationCoroutines.Remove(player);

            player.DisableEffect(EffectType.Vitality);
            player.DisableEffect(EffectType.CardiacArrest);
        }

        public static bool SpawnSCP682(Player player)
        {
            SCP682Role.Instance.AddRole(player);
            return true;
        }

        public static void ExecuteAbhorrenceAbilityFromKeybind(Player player)
        {
            if (!IsSCP682(player)) return;
            if (abhorrenceCooldowns.ContainsKey(player) && (DateTime.Now - abhorrenceCooldowns[player]).TotalSeconds < 50)
            {
                var remaining = 50 - (DateTime.Now - abhorrenceCooldowns[player]).TotalSeconds;
                var message = $"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "冷却");
                return;
            }

            abhorrenceActive[player] = true;
            abhorrenceActivatedTime[player] = DateTime.Now;
            abhorrenceCooldowns[player] = DateTime.Now;
            var message2 = $"<color=red>憎恶已激活！</color>\n普通攻击将附带心脏骤停效果，持续20秒";
            HSMShowhint.HsmShowHint(player, message2, 600, 0, 5f, "心脏骤停效果");

            Timing.CallDelayed(20f, () =>
            {
                if (IsSCP682(player))
                {
                    abhorrenceActive[player] = false;
                    player.DisableEffect(EffectType.CardiacArrest);
                    var message3 = $"<color=yellow>憎恶效果已结束</color>";
                    HSMShowhint.HsmShowHint(player, message3, 600, 0, 5f, "心脏骤停效果");
                }
            });
        }

        public static void ExecutePitifulAbilityFromKeybind(Player player)
        {
            if (!IsSCP682(player)) return;
            if (pitifulCooldowns.ContainsKey(player) && (DateTime.Now - pitifulCooldowns[player]).TotalSeconds < 50)
            {
                var remaining = 50 - (DateTime.Now - pitifulCooldowns[player]).TotalSeconds;
                var message = $"<color=red>能力冷却中！剩余 {remaining:F1} 秒</color>";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "冷却");
                return;
            }

            if (regenerationCoroutines.ContainsKey(player) && regenerationCoroutines[player].IsRunning)
                Timing.KillCoroutines(regenerationCoroutines[player]);

            pitifulCooldowns[player] = DateTime.Now;
            var message1 = $"<color=green>可悲已激活！</color>\n每秒回复10HP，持续10秒";
            HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "可悲");
            regenerationCoroutines[player] = Timing.RunCoroutine(RegenerateHealth(player, 10f, 10));
        }

        private static IEnumerator<float> RegenerateHealth(Player player, float amount, int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                yield return Timing.WaitForSeconds(1f);
                if (player == null || !player.IsAlive || !IsSCP682(player)) yield break;

                player.Health = Mathf.Min(player.Health + amount, player.MaxHealth);
                var message = $"<color=green>生命恢复中...</color> +{amount}HP";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 5f, "生命恢复");
            }
            if (player != null && IsSCP682(player))
            {
                var message1  = $"<color=yellow>生命恢复已完成</color>";
                HSMShowhint.HsmShowHint(player, message1, 600, 0, 5f, "生命恢复right");
            }
        }

        private static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker != null && IsSCP682(ev.Attacker) && abhorrenceActive.TryGetValue(ev.Attacker, out bool active) && active)
            {
                if ((DateTime.Now - abhorrenceActivatedTime[ev.Attacker]).TotalSeconds <= 20)
                {
                    ev.Player.EnableEffect(EffectType.CardiacArrest, 255, 3f);
                    var message = $"<color=red>你被SCP-682攻击了！</color>\n附带心脏骤停效果，持续3秒";
                    HSMShowhint.HsmShowHint(ev.Player, message, 600, 0, 5f, "心脏骤停效果");
                    var message2 = $"<color=red>你触发了憎恶攻击！</color>\n目标获得心脏骤停效果，持续3秒";
                    HSMShowhint.HsmShowHint(ev.Attacker, message2, 600, 0, 5f, "心脏骤停效果");
                }
                else
                {
                    abhorrenceActive[ev.Attacker] = false;
                    ev.Attacker.DisableEffect(EffectType.CardiacArrest);
                }
            }
        }

        private static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            abhorrenceCooldowns.Clear();
            pitifulCooldowns.Clear();
            abhorrenceActive.Clear();
            abhorrenceActivatedTime.Clear();
            foreach (var cor in regenerationCoroutines.Values) Timing.KillCoroutines(cor);
            regenerationCoroutines.Clear();
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
    }
}