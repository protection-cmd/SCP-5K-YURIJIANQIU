using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;

namespace SCP5K.MVPSystem
{
    public class MVPConfigManager
    {
        private MVPJsonDatabase _jsonDatabase;

        public MVPConfigManager()
        {
            _jsonDatabase = new MVPJsonDatabase();
        }

        public void LoadMVPConfig()
        {
            try
            {
                _jsonDatabase.LoadDatabase();
                Log.Info("MVP JSON数据库加载完成");
            }
            catch (Exception ex)
            {
                Log.Error($"加载MVP配置时发生错误: {ex.Message}");
            }
        }

        public void StartAutoReload()
        {
            _jsonDatabase.StartAutoReload();
        }

        public void StopAutoReload()
        {
            _jsonDatabase.StopAutoReload();
        }

        public void ForceReload()
        {
            _jsonDatabase.ForceReload();
        }

        public List<string> GetMusicPathsForPlayer(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Log.Debug("GetMusicPathsForPlayer: userId 为空");
                return null;
            }

            Log.Debug($"正在为玩家ID查找音乐: {userId}");
            var musicPaths = _jsonDatabase.GetMusicPathsForPlayer(userId);

            if (musicPaths == null || !musicPaths.Any())
            {
                Log.Debug($"未找到玩家 {userId} 的MVP音乐配置");
                return null;
            }

            Log.Debug($"为玩家 {userId} 找到 {musicPaths.Count} 首音乐");
            return musicPaths;
        }

        // 新增方法：获取玩家音乐数量
        public int GetMusicCountForPlayer(string userId)
        {
            var musicPaths = GetMusicPathsForPlayer(userId);
            return musicPaths?.Count ?? 0;
        }

        public bool AddOrUpdateConfig(string userId, string steamId, string platform, List<string> musicPaths)
        {
            return _jsonDatabase.AddOrUpdatePlayerConfig(userId, steamId, platform, musicPaths);
        }

        public bool RemoveConfig(string userId)
        {
            return _jsonDatabase.RemovePlayerConfig(userId);
        }

        public Dictionary<string, MVPPlayerConfig> GetAllConfigs()
        {
            return _jsonDatabase.GetAllConfigs();
        }

        public bool PlayerHasConfig(string userId)
        {
            return _jsonDatabase.PlayerHasConfig(userId);
        }

        public void ReloadConfig()
        {
            Log.Debug("正在重新加载MVP JSON数据库...");
            LoadMVPConfig();
        }

        public string GetJsonContent()
        {
            try
            {
                string filePath = SCP5K.Plugin.Instance.Config.MVPConfigFilePath;
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                return "{}";
            }
            catch (Exception ex)
            {
                Log.Error($"获取JSON内容时出错: {ex.Message}");
                return "{}";
            }
        }
    }
}