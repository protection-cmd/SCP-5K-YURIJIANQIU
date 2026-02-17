using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;

namespace SCP5K
{
    public static class VanillaSpawnDisabler
    {
        private static bool isVanillaSpawnDisabled = false;
        private static CoroutineHandle disableCoroutine;

        // 配置
        public static bool DisableVanillaRespawns { get; set; } = true;

        // 初始化
        public static void Init()
        {
            Log.Info("原版刷新禁用系统已初始化");
            ResetState();
        }

        // 回合开始
        public static void OnRoundStarted()
        {
            if (!DisableVanillaRespawns) return;

            ResetState();
            isVanillaSpawnDisabled = true;


            Log.Info("原版刷新已被禁用，使用自定义刷新系统");
        }

        // 回合结束
        public static void OnRoundEnded(RoundEndedEventArgs ev)
        {
            ResetState();
            Log.Info("原版刷新禁用系统已重置");
        }

        

       

        public static void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (!isVanillaSpawnDisabled) return;


            ev.IsAllowed = false;

            Log.Info($"已阻止 {ev.NextKnownTeam} 团队的原版刷新");

           
        }

       

        // 重置状态
        private static void ResetState()
        {
            isVanillaSpawnDisabled = false;

        }

        // 注册事件
        public static void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;

        }

        // 注销事件
        public static void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;


            ResetState();
        }

        // 获取当前状态
        public static string GetStatus()
        {
            return $"原版刷新状态: {(isVanillaSpawnDisabled ? "已禁用" : "已启用")}";
        }
    }
}