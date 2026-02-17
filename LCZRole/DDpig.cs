using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K.LCZRole
{
    public class LiangziRole : CustomRole
    {
        public static LiangziRole Instance { get; } = new LiangziRole();
        public override uint Id { get; set; } = 901;
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override string Name { get; set; } = "良子";
        public override string CustomInfo { get; set; } = "良子";
        public override int MaxHealth { get; set; } = 250;

        public override string Description { get; set; } = "特殊D级人员 - 味真足\n\n<color=orange>血量提升至250</color>";

        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);

            Timing.CallDelayed(0.6f, () =>
            {
                if (player == null || !player.IsConnected) return;
                player.RankColor = "orange";
                player.RankName = "LCZ-D-良子";
                player.MaxHealth = this.MaxHealth;
                player.Health = this.MaxHealth;
                player.ShowHint($"<color=orange>你被选为良子！\n血量提升至250</color>", 10f);
            });
        }
    }

    public static class DDpig
    {
        public static void OnRoundStarted()
        {
            Timing.CallDelayed(2.5f, () => SelectSpecialDClass());
        }

        private static void SelectSpecialDClass()
        {
            try
            {
                var eligiblePlayers = Player.Get(RoleTypeId.ClassD).Where(p =>
                    string.IsNullOrEmpty(p.RankName) &&
                    !D9341Role.Instance.Check(p) &&
                    !AthleteRole.Instance.Check(p) &&
                    !LiangziRole.Instance.Check(p)).ToList();

                if (eligiblePlayers.Count == 0) return;

                var random = new System.Random();
                SetPlayerAsSpecialDClass(eligiblePlayers[random.Next(eligiblePlayers.Count)]);
            }
            catch (Exception ex) { Log.Error($"选择良子时出错: {ex.Message}"); }
        }

        public static bool IsSpecialDClass(Player player) => LiangziRole.Instance.Check(player);

        public static bool SetPlayerAsSpecialDClass(Player player)
        {
            LiangziRole.Instance.AddRole(player);
            return true;
        }

        public static void RegisterEvents() { Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted; }
        public static void UnregisterEvents() { Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted; }
    }
}