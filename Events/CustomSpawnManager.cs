using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using NetworkManagerUtils.Dummies;
using SCP5K.SCPFouRole;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP5K
{
    public static class CustomSpawnManager
    {
        private static CoroutineHandle spawnTimerCoroutine;
        private static CoroutineHandle waitForPlayersCoroutine;
        private static CoroutineHandle gocCheckCoroutine;
        private static bool isSpawningActive = false;
        private static int currentSpawnIndex = 0;

        // 配置
        public static float SpawnInterval { get; set; } = 200f; // 默认200秒
        public static float CheckInterval { get; set; } = 2f;   // 每2秒检测一次观察者
        public static float GOCFixedTime { get; set; } = 900f;  // GOCARC固定15分钟检测
        public static List<string> AvailableSquads { get; set; } = new List<string> { "NU7A", "NU7B", "GOC" }; // 更新A连与B连

        private static DateTime roundStartTime;

        public static void Init()
        {
            Log.Info("自定义刷新管理器已初始化");
            ResetState();
        }

        public static void OnRoundStarted()
        {
            ResetState();
            roundStartTime = DateTime.Now;

            Timing.CallDelayed(5f, () =>
            {
                isSpawningActive = true;
                StartSpawnTimer();
                StartGOCFixedTimer();
                Log.Info($"自定义刷新系统已启动，首次刷新将在{SpawnInterval}秒后开始，GOC将在{GOCFixedTime}秒后检测");
            });
        }

        public static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            ResetState();
            Log.Info("自定义刷新系统已重置");
        }

        private static void StartSpawnTimer()
        {
            if (!isSpawningActive) return;

            spawnTimerCoroutine = Timing.CallDelayed(SpawnInterval, () =>
            {
                TrySpawnRandomSquad();
            });
            Log.Debug($"已启动刷新计时器，{SpawnInterval}秒后尝试刷新小队");
        }

        private static void StartGOCFixedTimer()
        {
            if (!isSpawningActive) return;

            gocCheckCoroutine = Timing.CallDelayed(GOCFixedTime, () =>
            {
                TrySpawnGOC();
            });
            Log.Debug($"已启动GOC固定时间检测，{GOCFixedTime}秒后尝试刷新GOC小队");
        }

        private static void TrySpawnRandomSquad()
        {
            if (!isSpawningActive || AvailableSquads.Count == 0)
            {
                Log.Warn("无法刷新小队：刷新未激活或无可用的阵容");
                return;
            }

            string selectedSquad = GetRandomSquad();
            if (string.IsNullOrEmpty(selectedSquad))
            {
                Log.Error("随机选择阵容失败");
                StartSpawnTimer();
                return;
            }

            Log.Info($"尝试刷新 {selectedSquad} 小队...");

            var spectators = GetEligibleSpectators();
            int requiredPlayers = GetRequiredPlayerCount(selectedSquad);

            if (spectators.Count >= requiredPlayers)
            {
                ExecuteSpawn(selectedSquad, spectators.Take(requiredPlayers).ToList());
                StartSpawnTimer();
            }
            else
            {
                Log.Info($"{selectedSquad} 需要 {requiredPlayers} 名玩家，但只有 {spectators.Count} 名观察者，开始等待...");
                waitForPlayersCoroutine = Timing.RunCoroutine(WaitForPlayersAndSpawn(selectedSquad, requiredPlayers));
            }
        }

        private static void TrySpawnGOC()
        {
            if (!isSpawningActive) return;

            Log.Info("开始检测GOC刷新条件...");
            int scpCount = GetSCPCount();
            Log.Info($"当前SCP数量: {scpCount}");

            if (scpCount >= 2)
            {
                var spectators = GetEligibleSpectators();
                int requiredPlayers = GetRequiredPlayerCount("GOCARC");

                if (spectators.Count >= requiredPlayers)
                {
                    ExecuteSpawn("GOCARC", spectators.Take(requiredPlayers).ToList());

                    if (spawnTimerCoroutine.IsRunning) Timing.KillCoroutines(spawnTimerCoroutine);
                    if (waitForPlayersCoroutine.IsRunning) Timing.KillCoroutines(waitForPlayersCoroutine);

                    StartSpawnTimer();
                    Log.Info("GOC刷新成功，已重置阵营刷新计时器");
                }
                else
                {
                    Log.Info($"GOC需要 {requiredPlayers} 名玩家，但只有 {spectators.Count} 名观察者，本次跳过GOC刷新");
                }
            }
            else
            {
                Log.Info($"SCP数量不足（需要2个，当前{scpCount}个），跳过GOC刷新");
            }
        }

        private static int GetSCPCount() => Player.List.Count(p => p.Role.Team == PlayerRoles.Team.SCPs && p.IsAlive);

        private static IEnumerator<float> WaitForPlayersAndSpawn(string squadName, int requiredPlayers)
        {
            bool spawned = false;
            float waitTime = 0f;
            float maxWaitTime = 300f;

            while (!spawned && waitTime < maxWaitTime && isSpawningActive)
            {
                yield return Timing.WaitForSeconds(CheckInterval);
                waitTime += CheckInterval;

                var spectators = GetEligibleSpectators();
                if (spectators.Count >= requiredPlayers)
                {
                    ExecuteSpawn(squadName, spectators.Take(requiredPlayers).ToList());
                    spawned = true;
                    Log.Info($"等待 {waitTime} 秒后成功刷新 {squadName} 小队");
                    StartSpawnTimer();
                }
                else if (Mathf.Approximately(waitTime % 30f, 0f))
                {
                    Log.Info($"等待 {squadName} 刷新中... 需要 {requiredPlayers} 名玩家，当前有 {spectators.Count} 名观察者");
                }
            }

            if (!spawned)
            {
                if (waitTime >= maxWaitTime)
                {
                    Log.Warn($"等待 {squadName} 刷新超时（{maxWaitTime}秒），跳过本次刷新");
                    StartSpawnTimer();
                }
            }
        }
        
        private static void ExecuteSpawn(string squadName, List<Player> players)
        {
            try
            {
                bool spawnSuccess = false;

                switch (squadName.ToUpper())
                {
                    case "GOCARC": spawnSuccess = GOCArcaneStrike.SpawnGOCTeam(players); break;
                    case "NU7A": spawnSuccess = Nu7HammerDown.SpawnNu7ATeam(players); break;
                    case "NU7B": spawnSuccess = Nu7HammerDown.SpawnNu7BTeam(players); break;
                    case "GOC": spawnSuccess = GOCTeam.SpawnGOCTeam(players); break;
                    default: Log.Error($"未知的阵容名称: {squadName}"); break;
                }

                if (spawnSuccess)
                {
                    Log.Info($"成功刷新 {squadName} 小队，玩家: {string.Join(", ", players.Select(p => p.Nickname))}");
                    foreach (var player in Player.List) player.ShowHint($"<color=yellow>{squadName} 小队已入场！</color>", 5f);
                }
                else
                {
                    Log.Error($"刷新 {squadName} 小队失败");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"执行刷新 {squadName} 时发生异常: {ex}");
            }
        }

        private static List<Player> GetEligibleSpectators()
        {
            return Player.List
                .Where(p => p.Role.Type == PlayerRoles.RoleTypeId.Spectator && !p.IsOverwatchEnabled && p.IsConnected)
                .OrderBy(_ => UnityEngine.Random.value)
                .ToList();
        }

        private static string GetRandomSquad()
        {
            if (AvailableSquads.Count == 0) return null;
            return AvailableSquads[UnityEngine.Random.Range(0, AvailableSquads.Count)];
        }

        private static int GetRequiredPlayerCount(string squadName)
        {
            switch (squadName.ToUpper())
            {
                case "GOCARC": return 3;
                case "NU7A": return 3;
                case "NU7B": return 4;
                case "GOC": return 4;
                default: return 3;
            }
        }

        private static void ResetState()
        {
            isSpawningActive = false;
            currentSpawnIndex = 0;
            if (spawnTimerCoroutine.IsRunning) Timing.KillCoroutines(spawnTimerCoroutine);
            if (waitForPlayersCoroutine.IsRunning) Timing.KillCoroutines(waitForPlayersCoroutine);
            if (gocCheckCoroutine.IsRunning) Timing.KillCoroutines(gocCheckCoroutine);
        }

        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }

        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            ResetState();
        }
    }
}