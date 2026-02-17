using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
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
        public static float SpawnInterval { get; set; } = 200f; // 默认200秒（仅用于Nu-7）
        public static float CheckInterval { get; set; } = 2f;   // 每2秒检测一次观察者
        public static float GOCFixedTime { get; set; } = 900f;  // GOCARC固定15分钟（900秒）检测
        public static List<string> AvailableSquads { get; set; } = new List<string> { "NU7", "GOC" }; // 添加GOC到可用阵容

        // 回合开始时间
        private static DateTime roundStartTime;

        // 初始化
        public static void Init()
        {
            Log.Info("自定义刷新管理器已初始化");
            ResetState();
        }

        // 回合开始
        public static void OnRoundStarted()
        {
            ResetState();
            roundStartTime = DateTime.Now;

            // 延迟5秒后开始刷新计时（让游戏稳定）
            Timing.CallDelayed(5f, () =>
            {
                isSpawningActive = true;

                // 启动Nu-7的随机计时刷新
                StartSpawnTimer();

                // 启动GOC的固定时间检测
                StartGOCFixedTimer();

                Log.Info($"自定义刷新系统已启动，Nu-7首次刷新将在{SpawnInterval}秒后开始，GOC将在{GOCFixedTime}秒后检测");
            });
        }

        // 回合结束
        public static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            ResetState();
            Log.Info("自定义刷新系统已重置");
        }

        // 开始Nu-7刷新计时器
        private static void StartSpawnTimer()
        {
            if (!isSpawningActive) return;

            spawnTimerCoroutine = Timing.CallDelayed(SpawnInterval, () =>
            {
                TrySpawnRandomSquad();
            });

            Log.Debug($"已启动Nu-7刷新计时器，{SpawnInterval}秒后尝试刷新小队");
        }

        // 开始GOC固定时间检测
        private static void StartGOCFixedTimer()
        {
            if (!isSpawningActive) return;

            gocCheckCoroutine = Timing.CallDelayed(GOCFixedTime, () =>
            {
                TrySpawnGOC();
            });

            Log.Debug($"已启动GOC固定时间检测，{GOCFixedTime}秒后尝试刷新GOC小队");
        }

        // 尝试随机刷新一个小队（用于Nu-7和GOC）
        private static void TrySpawnRandomSquad()
        {
            if (!isSpawningActive || AvailableSquads.Count == 0)
            {
                Log.Warn("无法刷新小队：刷新未激活或无可用的阵容");
                return;
            }

            // 随机选择一个阵容
            string selectedSquad = GetRandomSquad();
            if (string.IsNullOrEmpty(selectedSquad))
            {
                Log.Error("随机选择阵容失败");
                StartSpawnTimer(); // 继续下一次计时
                return;
            }

            Log.Info($"尝试刷新 {selectedSquad} 小队...");

            // 检查观察者数量
            var spectators = GetEligibleSpectators();
            int requiredPlayers = GetRequiredPlayerCount(selectedSquad);

            if (spectators.Count >= requiredPlayers)
            {
                // 观察者足够，立即刷新
                ExecuteSpawn(selectedSquad, spectators.Take(requiredPlayers).ToList());
                StartSpawnTimer(); // 开始下一次计时
            }
            else
            {
                // 观察者不足，开始等待
                Log.Info($"{selectedSquad} 需要 {requiredPlayers} 名玩家，但只有 {spectators.Count} 名观察者，开始等待...");
                waitForPlayersCoroutine = Timing.RunCoroutine(WaitForPlayersAndSpawn(selectedSquad, requiredPlayers));
            }
        }

        // 尝试刷新GOC小队（固定时间检测）
        private static void TrySpawnGOC()
        {
            if (!isSpawningActive)
            {
                Log.Warn("无法刷新GOC：刷新未激活");
                return;
            }

            Log.Info("开始检测GOC刷新条件...");

            // 检测SCP数量
            int scpCount = GetSCPCount();
            Log.Info($"当前SCP数量: {scpCount}");

            if (scpCount >= 2)
            {
                // SCP数量足够，继续检查观察者
                var spectators = GetEligibleSpectators();
                int requiredPlayers = GetRequiredPlayerCount("GOCARC");

                if (spectators.Count >= requiredPlayers)
                {
                    // 观察者足够，立即刷新GOC
                    ExecuteSpawn("GOCARC", spectators.Take(requiredPlayers).ToList());

                    // GOC刷新后，重新开始Nu-7的计时
                    if (spawnTimerCoroutine.IsRunning)
                        Timing.KillCoroutines(spawnTimerCoroutine);

                    if (waitForPlayersCoroutine.IsRunning)
                        Timing.KillCoroutines(waitForPlayersCoroutine);

                    StartSpawnTimer();
                    Log.Info("GOC刷新成功，已重置Nu-7刷新计时器");
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

            // GOC固定时间检测只执行一次，不再重复
        }

        // 获取SCP数量
        private static int GetSCPCount()
        {
            return Player.List.Count(p => p.Role.Team == PlayerRoles.Team.SCPs && p.IsAlive);
        }

        // 等待玩家并刷新
        private static IEnumerator<float> WaitForPlayersAndSpawn(string squadName, int requiredPlayers)
        {
            bool spawned = false;
            float waitTime = 0f;
            float maxWaitTime = 300f; // 最多等待5分钟

            while (!spawned && waitTime < maxWaitTime && isSpawningActive)
            {
                yield return Timing.WaitForSeconds(CheckInterval);
                waitTime += CheckInterval;

                var spectators = GetEligibleSpectators();
                if (spectators.Count >= requiredPlayers)
                {
                    // 玩家足够，执行刷新
                    ExecuteSpawn(squadName, spectators.Take(requiredPlayers).ToList());
                    spawned = true;
                    Log.Info($"等待 {waitTime} 秒后成功刷新 {squadName} 小队");

                    // 只在成功生成后才开始下一次计时
                    StartSpawnTimer();
                }
                else if (Mathf.Approximately(waitTime % 30f, 0f)) // 每30秒输出一次等待状态
                {
                    Log.Info($"等待 {squadName} 刷新中... 需要 {requiredPlayers} 名玩家，当前有 {spectators.Count} 名观察者");
                }
            }

            if (!spawned)
            {
                if (waitTime >= maxWaitTime)
                {
                    Log.Warn($"等待 {squadName} 刷新超时（{maxWaitTime}秒），跳过本次刷新");
                    // 超时情况下也重新开始计时，避免系统卡死
                    StartSpawnTimer();
                }
                // 注意：如果因为 isSpawningActive 变为 false 而退出循环，不应该开始新的计时
            }
        }

        // 执行刷新
        private static void ExecuteSpawn(string squadName, List<Player> players)
        {
            try
            {
                bool spawnSuccess = false;

                switch (squadName.ToUpper())
                {
                    case "GOCARC":
                        spawnSuccess = GOCArcaneStrike.SpawnGOCTeam(players);
                        break;
                    case "NU7":
                        spawnSuccess = Nu7HammerDown.SpawnNu7Team(players);
                        break;
                    case "GOC":
                        spawnSuccess = GOCTeam.SpawnGOCTeam(players);
                        break;
                    default:
                        Log.Error($"未知的阵容名称: {squadName}");
                        break;
                }

                if (spawnSuccess)
                {
                    Log.Info($"成功刷新 {squadName} 小队，玩家: {string.Join(", ", players.Select(p => p.Nickname))}");

                    // 全局通告
                    foreach (var player in Player.List)
                    {
                        player.ShowHint($"<color=yellow>{squadName} 小队已入场！</color>", 5f);
                    }
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

        // 获取符合条件的观察者
        private static List<Player> GetEligibleSpectators()
        {
            return Player.List
                .Where(p => p.Role.Type == PlayerRoles.RoleTypeId.Spectator &&
                           !p.IsOverwatchEnabled &&
                           p.IsConnected)
                .OrderBy(_ => UnityEngine.Random.value) // 随机排序
                .ToList();
        }

        // 获取随机阵容
        private static string GetRandomSquad()
        {
            if (AvailableSquads.Count == 0) return null;

            int index = UnityEngine.Random.Range(0, AvailableSquads.Count);
            return AvailableSquads[index];
        }

        // 获取所需玩家数量
        private static int GetRequiredPlayerCount(string squadName)
        {
            switch (squadName.ToUpper())
            {
                case "GOCARC":
                    return 3; // GOC需要3名玩家
                case "NU7":
                    return 3; // Nu-7最少需要3名玩家
                case "GOC":
                    return 4; // GOCTeam最少需要4名玩家
                default:
                    return 3; // 默认3名
            }
        }

        // 重置状态
        private static void ResetState()
        {
            isSpawningActive = false;
            currentSpawnIndex = 0;

            if (spawnTimerCoroutine.IsRunning)
                Timing.KillCoroutines(spawnTimerCoroutine);

            if (waitForPlayersCoroutine.IsRunning)
                Timing.KillCoroutines(waitForPlayersCoroutine);

            if (gocCheckCoroutine.IsRunning)
                Timing.KillCoroutines(gocCheckCoroutine);
        }

        // 注册事件
        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }

        // 注销事件
        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            ResetState();
        }

        // 手动添加阵容（用于后续扩展）
        public static void AddSquad(string squadName)
        {
            if (!AvailableSquads.Contains(squadName))
            {
                AvailableSquads.Add(squadName);
                Log.Info($"已添加阵容: {squadName}");
            }
        }

        // 手动移除阵容
        public static void RemoveSquad(string squadName)
        {
            if (AvailableSquads.Contains(squadName))
            {
                AvailableSquads.Remove(squadName);
                Log.Info($"已移除阵容: {squadName}");
            }
        }

        // 获取当前状态信息（用于调试）
        public static string GetStatusInfo()
        {
            var spectators = GetEligibleSpectators();
            int scpCount = GetSCPCount();
            return $"刷新状态: {(isSpawningActive ? "活跃" : "未激活")}, " +
                   $"可用阵容: {string.Join(", ", AvailableSquads)}, " +
                   $"观察者数量: {spectators.Count}, " +
                   $"SCP数量: {scpCount}, " +
                   $"Nu-7下次刷新间隔: {SpawnInterval}秒, " +
                   $"GOC检测时间: {GOCFixedTime}秒后";
        }
    }
}