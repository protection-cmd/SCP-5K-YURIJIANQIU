using AudioApi;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCP5K.MVPSystem
{
    public class MusicPlayer
    {
        private static int _mvpMusicBotId = new System.Random().Next(2000, 2500);
        private static CoroutineHandle _musicCoroutine;
        private MVPConfigManager _configManager;
        private bool _isMusicPlaying = false;

        public MusicPlayer(MVPConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void WaitingForPlayer()
        {
            Server.FriendlyFire = false;
            StopMVPMusic();
            // 回合开始时重载配置
            _configManager?.ForceReload();
        }
        
        // 回合结束时重载配置
        public void RoundEnded()
        {
            StopMVPMusic();
            _configManager?.ForceReload();
        }

        public string TryPlayMusic(Player p)
        {
            if (p == null)
            {
                Log.Debug("TryPlayMusic: 玩家为空");
                return null;
            }

            Log.Debug($"正在为玩家 {p.Nickname} 查找MVP音乐，UserID: {p.UserId}");

            // 先停止当前音乐
            StopMVPMusic();

            List<string> musicList = _configManager?.GetMusicPathsForPlayer(p.UserId);
            if (musicList == null || !musicList.Any())
            {
                Log.Debug($"玩家 {p.Nickname} 没有配置MVP音乐路径");
                return null;
            }

            if (musicList.Count > 3)
            {
                musicList = musicList.Take(3).ToList();
                Log.Debug($"玩家 {p.Nickname} 的音乐路径超过3个，已限制为前3个");
            }

            Random random = new Random();
            string selectedMusic = musicList[random.Next(musicList.Count)];
            
            if (string.IsNullOrEmpty(selectedMusic))
            {
                Log.Error($"选中的音乐路径为空");
                return null;
            }
            
            string musicName = Path.GetFileNameWithoutExtension(selectedMusic);
            musicName = string.IsNullOrEmpty(musicName) ? selectedMusic : musicName;

            bool success = PlayMVPMusic(selectedMusic, musicName, p.Nickname);

            if (success)
            {
                Log.Info($"MVP音乐已播放: {musicName}");
                return musicName;
            }
            else
            {
                Log.Error($"播放MVP音乐失败: {musicName}");
                return null;
            }
        }

        private bool PlayMVPMusic(string musicPath, string musicName, string playerName)
        {
            if (string.IsNullOrEmpty(musicPath))
            {
                Log.Error("PlayMVPMusic: MVP音乐路径为空");
                return false;
            }

            try
            {
                // 检查音乐文件是否存在
                if (!File.Exists(musicPath))
                {
                    Log.Error($"PlayMVPMusic: MVP音乐文件不存在: {musicPath}");
                    return false;
                }

                // 停止之前的音乐
                StopMVPMusic();

                // 移除旧的音乐机器人（如果存在）
                try
                {
                    AudioApi.Dummies.VoiceDummy.Clear();
                    Log.Info($"已移除旧的音乐机器人 ID: {_mvpMusicBotId}");
                }
                catch (Exception ex)
                {
                    Log.Debug($"移除旧音乐机器人时出错: {ex.Message}");
                }

                // 创建新的音乐机器人
                try
                {
                    AudioApi.Dummies.VoiceDummy.Add(_mvpMusicBotId, $"MVP:{playerName}");
                }
                catch (Exception ex)
                {
                    Log.Error($"创建MVP音乐机器人失败: {ex.Message}");
                }
                try
                {
                    AudioApi.Dummies.VoiceDummy.Play(_mvpMusicBotId, musicPath, 100f, false);
                    _isMusicPlaying = true;
                }
                catch (Exception ex)
                {                     
                    Log.Error($"播放MVP音乐失败: {ex.Message}");
                }



                
                Log.Info($"正在播放MVP音乐: {musicName} - 玩家: {playerName}");

                // 30秒后自动停止音乐
                _musicCoroutine = Timing.RunCoroutine(AutoStopMusicCoroutine());

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"播放MVP音乐时出错: {e.Message}");
                Log.Debug($"详细错误: {e}");
                _isMusicPlaying = false;
                return false;
            }
        }

        private IEnumerator<float> AutoStopMusicCoroutine()
        {
            yield return Timing.WaitForSeconds(30f);
            
            try
            {
                if (_isMusicPlaying)
                {
                    StopMVPMusic();
                    Log.Debug("MVP音乐已自动停止（30秒后）");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"自动停止音乐协程出错: {ex.Message}");
            }
        }

        public void StopMVPMusic()
        {
            try
            {
                // 停止协程
                if (_musicCoroutine.IsRunning)
                {
                    Timing.KillCoroutines(_musicCoroutine);
                    _musicCoroutine = default;
                }

                // 移除音乐机器人
                if (_isMusicPlaying)
                {
                    try
                    {
                        AudioApi.Dummies.VoiceDummy.Remove(_mvpMusicBotId);
                        Log.Debug($"已停止并移除音乐机器人 ID: {_mvpMusicBotId}");
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"移除音乐机器人时出错（可能已不存在）: {ex.Message}");
                    }
                    
                    _isMusicPlaying = false;
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"停止MVP音乐时出错（可能协程未运行）: {ex.Message}");
            }
        }
    }
}