using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;

namespace SCP5K.Events
{
    public static class SkillCleanupManager
    {
        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.Died += OnDied;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.Died -= OnDied;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        }

        private static void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player == null) return;

            // 当通过管理员(RA面板)、强制切换类或自然重生改变角色时，清空之前所有的CustomRole和技能状态
            if (ev.Reason == Exiled.API.Enums.SpawnReason.ForceClass || 
                ev.Reason == Exiled.API.Enums.SpawnReason.LateJoin ||
                ev.Reason == Exiled.API.Enums.SpawnReason.Respawn)
            {
                // 强制触发脱离原有的自定义角色
                foreach (var role in ev.Player.GetCustomRoles())
                {
                    role.RemoveRole(ev.Player);
                }
                
                ev.Player.CustomInfo = string.Empty;
                
                CleanUpPlayerStates(ev.Player);
            }
        }

        private static void OnDied(DiedEventArgs ev)
        {
            if (ev.Player != null)
            {
                ev.Player.RankName = string.Empty;
                ev.Player.RankColor = string.Empty;
                ev.Player.CustomInfo = string.Empty;
                CleanUpPlayerStates(ev.Player);
            }
        }

        private static void OnDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player != null) CleanUpPlayerStates(ev.Player);
        }

        public static void CleanUpPlayerStates(Player player)
        {

            player.DisableEffect(Exiled.API.Enums.EffectType.MovementBoost);
            player.DisableEffect(Exiled.API.Enums.EffectType.DamageReduction);
            player.DisableEffect(Exiled.API.Enums.EffectType.Ensnared);
            player.DisableEffect(Exiled.API.Enums.EffectType.NightVision);

            SCP5K.SCPFouRole.Nu7HammerDown.CleanUpPlayer(player);
        }
    }
}