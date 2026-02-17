using AudioApi.Dummies;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using Respawning;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SCP5K
{
    public static class ArcaneStrike
    {
        private static CoroutineHandle arcaneCountdown;
        private static int musicBotId = 911; // 奇术打击专用音乐机器人ID
        private static bool isActive = false;
        private static bool hasUsing = false;
        private static Dictionary<Player, Vector3> playerPositions = new Dictionary<Player, Vector3>();
        private static bool wasWarheadInProgress = false; // 记录核弹是否已经启动

        // 配置参数
        public static float CountdownDuration { get; set; } = 30f;
        public static float MusicDelay { get; set; } = 5f;
        public static string MusicPath { get; set; } = Paths.Exiled + "\\GOCM.ogg";
        public static string SwordSchematicName { get; set; } = "Sword";
        public static Vector3 SchematicPosition { get; set; } = Vector3.zero; // 默认位置

        // 回合开始时调用此方法重置状态
        public static void OnRoundStarted()
        {
            wasWarheadInProgress = false;
            isActive = false;
            hasUsing = false;
            playerPositions.Clear();

            foreach (Player player in Player.List)
            {
                // 禁用雾效
                player.DisableEffect(EffectType.FogControl);
            }

            Log.Debug("回合开始，奇术打击状态已重置");
        }

        // 回合结束时调用此方法清理状态
        public static void OnRoundEnded()
        {
            ResetRoundState();
            Log.Debug("回合结束，奇术打击状态已清理");
        }

        public static void Activate(Player triggerPlayer)
        {
            // 如果已经激活，则不再激活
            if (isActive)
            {
                Log.Warn("奇术打击已经在激活状态，跳过本次激活");
                return;
            }

            // 设置状态标志
            isActive = true;
            hasUsing = true;

            // 停止现有的倒计时
            if (arcaneCountdown.IsRunning)
                Timing.KillCoroutines(arcaneCountdown);

            // 检查并处理阿尔法核弹
            HandleAlphaWarhead();

            // 锁定回合
            Round.IsLocked = true;
            Warhead.IsLocked = true;

            // 显示初始提示
            foreach (Player player in Player.List)
            {
                player.ShowHint("<color=red>奇术打击已激活！30秒后释放...</color>", 10);
            }


            // 新增：将所有玩家重置为教程人员、使用雾效并传送到固定点
            SetupPlayersForArcaneStrike();

            // 延迟播放音乐
            Timing.CallDelayed(MusicDelay, () =>
            {
                PlayArcaneMusic();
            });

            // 生成模型
            SpawnArcaneSchematic();
            Map.ChangeLightsColor(UnityEngine.Color.cyan);
            
            // 开始倒计时
            arcaneCountdown = Timing.CallDelayed(CountdownDuration, () =>
            {
                // 最终释放
                Map.ChangeLightsColor(UnityEngine.Color.cyan);
                ExecuteArcaneStrike();
            });

            // 设置倒计时提示
            Map.ChangeLightsColor(UnityEngine.Color.cyan);
            SetupCountdownWarnings();

            Log.Info($"奇术打击已由玩家 {triggerPlayer.Nickname} 激活，将于 {CountdownDuration} 秒后释放");
        }

        public static void Cancel()
        {
            if (isActive || arcaneCountdown.IsRunning)
            {
                Timing.KillCoroutines(arcaneCountdown);

                // 确保音乐完全停止
                VoiceDummy.Remove(musicBotId);

                // 恢复核弹状态（如果是被奇术打击停止的）
                if (wasWarheadInProgress)
                {
                    Warhead.IsLocked = false;
                    wasWarheadInProgress = false;
                }

                // 重置所有状态
                isActive = false;
                Round.IsLocked = false;
                Warhead.IsLocked = false;

                // 清除玩家位置记录
                playerPositions.Clear();

                // 显示取消提示
                foreach (Player player in Player.List)
                {
                    player.ShowHint("<color=green>奇术打击已取消</color>", 5);
                }

                Log.Info("奇术打击已取消，所有状态已重置");
            }
        }

        public static void ResetRoundState()
        {
            if (arcaneCountdown.IsRunning)
            {
                Timing.KillCoroutines(arcaneCountdown);
            }

            // 确保音乐完全停止
            VoiceDummy.Remove(musicBotId);

            // 重置所有状态
            isActive = false;
            Round.IsLocked = false;
            Warhead.IsLocked = false;
            wasWarheadInProgress = false;

            // 清除玩家位置记录
            playerPositions.Clear();

            Log.Debug("奇术打击状态已完全重置");
        }

        private static void SetupCountdownWarnings()
        {
            Timing.CallDelayed(CountdownDuration - 6f, () =>
            {
                foreach (Player player in Player.List)
                {
                    if (player.IsAlive)
                    {
                        player.EnableEffect(EffectType.Flashed, 4f);
                        player.ShowHint("<color=white>强烈的奇术能量正在聚集...</color>", 5f);
                    }
                }
            });
        }

        private static void ExecuteArcaneStrike()
        {
            // 杀死所有玩家
            foreach (Player player in Player.List)
            {
                if (player.IsAlive)
                {
                    player.Kill("奇术打击");
                }
            }

            Warhead.Detonate();

            // 确保音乐完全停止
            VoiceDummy.Remove(musicBotId);

            // 重置状态 - 确保所有状态都被正确重置
            isActive = false;
            Round.IsLocked = false;
            Warhead.IsLocked = false;
            wasWarheadInProgress = false;

            // 清除玩家位置记录
            playerPositions.Clear();

            Log.Info("奇术打击已释放，所有玩家已被清除，状态已重置");
        }

        // 播放奇术打击音乐 - 更新为使用完整文件路径
        private static void PlayArcaneMusic()
        {
            if (string.IsNullOrEmpty(MusicPath) || !File.Exists(MusicPath))
            {
                Log.Error($"奇术打击音乐文件不存在: {MusicPath}");
                return;
            }

            // 移除旧的音乐机器人
            VoiceDummy.Remove(musicBotId);

            try
            {
                string botName = "奇术打击";

                if (!VoiceDummy.Add(musicBotId, botName))
                {
                    Log.Error("创建奇术打击音乐机器人失败");
                    return;
                }

                // 播放音乐 - 使用完整文件路径
                VoiceDummy.Play(musicBotId, MusicPath, 100f, false);
                Log.Info($"正在播放奇术打击音乐: {Path.GetFileNameWithoutExtension(MusicPath)}");
            }
            catch (Exception e)
            {
                Log.Error($"播放奇术打击音乐时出错: {e.Message}");
            }
        }

        private static void SpawnArcaneSchematic()
        {
            try
            {
                ObjectSpawner.SpawnSchematic(SwordSchematicName, SchematicPosition, Quaternion.identity, Vector3.one);
                Log.Info($"奇术打击模型已生成在固定位置: {SchematicPosition}");
            }
            catch (Exception ex)
            {
                Log.Error($"生成奇术打击模型时出错: {ex.Message}");
            }
        }

        // 新增方法：处理阿尔法核弹
        private static void HandleAlphaWarhead()
        {
            // 检查核弹是否已经启动
            if (Warhead.IsInProgress)
            {
                wasWarheadInProgress = true;
                Warhead.Stop();
                Log.Info("阿尔法核弹已启动，已被奇术打击停止");
            }
            else if (Warhead.IsDetonated)
            {
                Log.Warn("阿尔法核弹已经引爆，无法停止");
            }
            else
            {
                // 确保核弹被锁定，防止在奇术打击期间启动
                Warhead.IsLocked = true;
                Log.Debug("阿尔法核弹已被锁定，防止在奇术打击期间启动");
            }
        }

        // 新增方法：设置玩家为奇术打击状态
        private static void SetupPlayersForArcaneStrike()
        {
            Map.ChangeLightsColor(UnityEngine.Color.cyan);
            // 先收集所有存活玩家
            List<Player> alivePlayers = new List<Player>();
            foreach (Player player in Player.List)
            {
                if (player.IsAlive)
                {
                    alivePlayers.Add(player);
                }
            }

            // 第一步：将所有存活玩家变为混沌分裂者征召兵
            foreach (Player player in alivePlayers)
            {
                player.Role.Set(RoleTypeId.ChaosConscript, SpawnReason.ForceClass, RoleSpawnFlags.All);
                Log.Debug($"玩家 {player.Nickname} 已设置为混沌分裂者征召兵");
            }

            // 等待一帧确保角色切换完成
            Timing.CallDelayed(0.1f, () =>
            {
                // 第二步：记录所有玩家的当前位置（混沌分裂者出生点）
                foreach (Player player in alivePlayers)
                {
                    if (player.IsAlive)
                    {
                        playerPositions[player] = player.Position;
                        Log.Debug($"记录玩家 {player.Nickname} 的位置: {player.Position}");
                    }
                }

                // 第三步：将所有玩家变为教程人员并传送到记录的位置
                foreach (Player player in alivePlayers)
                {
                    if (player.IsAlive && playerPositions.ContainsKey(player))
                    {
                        // 先保存位置
                        Vector3 savedPosition = playerPositions[player];

                        // 变为教程人员
                        player.Role.Set(RoleTypeId.Tutorial, SpawnReason.ForceClass, RoleSpawnFlags.All);

                        // 等待一帧确保角色切换完成，然后传送
                        Timing.CallDelayed(0.1f, () =>
                        {
                            if (player.IsAlive)
                            {

                                player.Position = savedPosition;
                                // 禁用雾效
                                player.EnableEffect(EffectType.FogControl);
                                Log.Debug($"玩家 {player.Nickname} 已设置为教程人员并传送到混沌分裂者出生点");
                            }
                        });
                    }
                }

                Log.Info("所有玩家已重置为教程人员并传送到混沌分裂者出生点，雾效已启用");
            });
        }
    }
}