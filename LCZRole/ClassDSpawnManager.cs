using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K.LCZRole
{
    public static class ClassDSpawnManager
    {
        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        }

        private static void OnRoundStarted()
        {
            // 延迟2秒确保玩家已完全生成
            Timing.CallDelayed(2f, () =>
            {
                AssignClassDRoles();
            });
        }

        private static void AssignClassDRoles()
        {
            // 获取所有D级人员
            List<Player> classDPlayers = Player.Get(RoleTypeId.ClassD).ToList();

            // 随机打乱列表以实现随机分配
            classDPlayers = classDPlayers.OrderBy(x => UnityEngine.Random.value).ToList();

            int count = classDPlayers.Count;
            if (count == 0) return;

            Log.Info($"[ClassDSpawnManager] 统计到 {count} 名D级人员，开始分配特殊角色...");

            // 按照分配规则：
            // 1. 良子 (优先)
            // 2. 运动员
            // 3. D9341
            // 超过3人则不再分配其他特殊角色

            // 分配优先级 1: 良子
            if (count >= 1)
            {
                Player p = classDPlayers[0];
                DDpig.SetPlayerAsSpecialDClass(p);
                Log.Info($"[ClassDSpawnManager] 已分配 {p.Nickname} 为 良子");
            }

            // 分配优先级 2: 运动员
            if (count >= 2)
            {
                Player p = classDPlayers[1];
                DDRunning.SetPlayerAsAthlete(p);
                Log.Info($"[ClassDSpawnManager] 已分配 {p.Nickname} 为 运动员");
            }

            // 分配优先级 3: D9341
            if (count >= 3)
            {
                Player p = classDPlayers[2];
                D9341Role.Instance.AddRole(p);
                Log.Info($"[ClassDSpawnManager] 已分配 {p.Nickname} 为 D9341");
            }

            // 如果未来有新角色，继续在此处添加 count >= 4 的逻辑即可
        }
    }
}