using System;
using System.IO;
using Exiled.API.Features;
using MEC;
using AudioApi.Dummies;
namespace SCP5K
{
    public static class OmegaWarhead
    {
        private static CoroutineHandle omegaCountdown;
        private static int musicBotId = 997; // 使用不同的ID避免冲突

        // 新增：多重保险机制
        private static bool isDetonating = false;
        private static bool hasDetonatedThisRound = false;

        public static void Detonate()
        {
            // 保险1：检查是否正在倒计时中
            if (isDetonating)
            {
                Log.Warn("Omega核弹已经在倒计时中，无法重复启动");
                return;
            }

            // 保险2：检查本回合是否已经触发过Omega核弹
            if (hasDetonatedThisRound)
            {
                Log.Warn("本回合已经触发过Omega核弹，无法再次启动");
                return;
            }

            // 设置状态标志
            isDetonating = true;
            hasDetonatedThisRound = true;

            Round.IsLocked = true;

            // 播放CASSIE警告
            Exiled.API.Features.Cassie.MessageTranslated(
                "This is an announcement to all personnel in the facility. Omega nuclear warhead detonation procedure initiated. All areas will detonate in three minutes. The program cannot be terminated. All personnel evacuate now.",
                "这是对设施内所有人员的通告。欧米茄核弹头引爆程序已启动。所有区域将在三分钟后引爆。程序无法被终止。所有人员现在撤离。"
            );

            // 设置全设施蓝光
            Map.ChangeLightsColor(UnityEngine.Color.blue);


            // 播放Omega音乐
            PlayOmegaMusic();



            // 开始三分钟倒计时
            omegaCountdown = Timing.CallDelayed(222f, () =>
            {
                // 引爆Alpha核弹以达到屏幕震爆效果
                Warhead.Detonate();

                // 立即将灯光颜色调回蓝色（Alpha核弹爆炸会将灯光设为红色）
                Map.ChangeLightsColor(UnityEngine.Color.blue);

                // 杀死所有玩家
                foreach (Player player in Player.List)
                {
                    player.Kill("Omega核弹");
                }

                // 停止音乐
                StopOmegaMusic();

                // 重置状态标志
                isDetonating = false;

                Round.IsLocked = false;

                Log.Info("Omega核弹已引爆，所有玩家已被清除");
            });

            Log.Info("Omega核弹倒计时已启动，将于3分钟后引爆");
        }

        public static void CancelDetonation()
        {
            if (omegaCountdown.IsRunning)
            {
                Timing.KillCoroutines(omegaCountdown);
                StopOmegaMusic();
                Map.ChangeLightsColor(UnityEngine.Color.white);

                // 重置状态标志
                isDetonating = false;
                Round.IsLocked = false;

                Log.Info("Omega核弹倒计时已取消");
            }
        }

        // 新增：回合开始重置方法
        public static void OnRoundStart()
        {
            // 取消任何正在进行的倒计时
            if (omegaCountdown.IsRunning)
            {
                Timing.KillCoroutines(omegaCountdown);
            }

            // 停止音乐
            StopOmegaMusic();

            // 重置状态标志 - 回合开始时重置核弹状态
            isDetonating = false;
            hasDetonatedThisRound = false;
            Round.IsLocked = false;

            Log.Debug("Omega核弹状态已在回合开始时重置");
        }

        // 新增：回合结束清理方法（不重置hasDetonatedThisRound）
        public static void OnRoundEnd()
        {
            // 取消任何正在进行的倒计时
            if (omegaCountdown.IsRunning)
            {
                Timing.KillCoroutines(omegaCountdown);
            }

            // 停止音乐
            StopOmegaMusic();

            // 只重置部分状态，保持hasDetonatedThisRound不变
            isDetonating = false;
            Round.IsLocked = false;

            Log.Debug("Omega核弹已在回合结束时清理，保持本回合状态");
        }

        // 播放Omega音乐 - 更新为使用完整文件路径
        private static void PlayOmegaMusic()
        {
            string musicPath = Plugin.Instance.Config.OmegaMusicPath;

            if (string.IsNullOrEmpty(musicPath) || !File.Exists(musicPath))
            {
                Log.Error($"Omega音乐文件不存在: {musicPath}");
                return;
            }

            // 移除旧的音乐机器人
            VoiceDummy.Remove(musicBotId);

            try
            {
                // 创建音乐机器人
                string botName = $"Omega核弹";

                if (!VoiceDummy.Add(musicBotId, botName))
                {
                    Log.Error("创建Omega音乐机器人失败");
                    return;
                }

                // 播放音乐 - 使用完整文件路径
                VoiceDummy.Play(musicBotId, musicPath, 100f, false);
                Log.Debug($"正在播放Omega音乐: {Path.GetFileNameWithoutExtension(musicPath)}");
            }
            catch (Exception e)
            {
                Log.Error($"播放Omega音乐时出错: {e.Message}");
            }
        }

        // 停止Omega音乐
        private static void StopOmegaMusic()
        {
            try
            {
                VoiceDummy.Remove(musicBotId);
            }
            catch (Exception ex)
            {
                Log.Error($"停止Omega音乐时出错: {ex.Message}");
            }
        }
    }
}