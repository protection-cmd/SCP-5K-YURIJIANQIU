using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Exiled.API.Features;
using MEC;

namespace SCP5K.MVPSystem
{
    public class MVPJsonDatabase
    {
        private Dictionary<string, MVPPlayerConfig> _mvpConfigs = new Dictionary<string, MVPPlayerConfig>();
        private string _databasePath;
        private CoroutineHandle _reloadCoroutine;
        private DateTime _lastFileModifiedTime = DateTime.MinValue;

        public MVPJsonDatabase()
        {
            _databasePath = SCP5K.Plugin.Instance.Config.MVPConfigFilePath;

            // 如果是旧的.txt文件，转换为.json
            if (_databasePath.EndsWith(".txt"))
            {
                _databasePath = _databasePath.Replace(".txt", ".json");
                Log.Info($"已将MVP配置文件路径从.txt改为.json: {_databasePath}");
            }
            else if (!_databasePath.EndsWith(".json"))
            {
                _databasePath = Path.Combine(Path.GetDirectoryName(_databasePath), "mvp_database.json");
            }
        }

        public void LoadDatabase()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_databasePath);

                // 确保目录存在
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log.Debug($"创建目录: {directoryPath}");
                }

                // 如果JSON文件不存在，检查是否有旧版.txt文件
                if (!File.Exists(_databasePath))
                {
                    // 检查是否有旧版.txt文件
                    string legacyTxtPath = _databasePath.Replace(".json", ".txt");
                    if (File.Exists(legacyTxtPath))
                    {
                        Log.Info("检测到旧版.txt配置文件，正在转换为JSON格式...");
                        ConvertLegacyTxtToJson(legacyTxtPath);
                    }
                    else
                    {
                        // 都没有，创建示例JSON数据库
                        CreateExampleDatabase();
                    }

                    // 记录文件修改时间
                    if (File.Exists(_databasePath))
                    {
                        _lastFileModifiedTime = File.GetLastWriteTime(_databasePath);
                    }

                    return;
                }

                // 检查文件是否被修改过
                DateTime currentModifiedTime = File.GetLastWriteTime(_databasePath);
                if (currentModifiedTime <= _lastFileModifiedTime)
                {
                    // 文件没有被修改，不需要重新加载
                    Log.Debug($"配置文件未修改，跳过重载 (上次修改: {_lastFileModifiedTime}, 当前: {currentModifiedTime})");
                    return;
                }

                Log.Debug($"配置文件已修改，重新加载 (上次修改: {_lastFileModifiedTime}, 当前: {currentModifiedTime})");

                // 读取JSON文件
                string jsonContent = File.ReadAllText(_databasePath);

                // 反序列化JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var newConfigs = JsonSerializer.Deserialize<Dictionary<string, MVPPlayerConfig>>(jsonContent, options)
                    ?? new Dictionary<string, MVPPlayerConfig>();

                // 更新配置
                _mvpConfigs = newConfigs;
                _lastFileModifiedTime = currentModifiedTime;

                Log.Info($"成功从JSON文件加载 {_mvpConfigs.Count} 个MVP配置");

                // 调试输出
                if (SCP5K.Plugin.Instance.Config.Debug)
                {
                    foreach (var kvp in _mvpConfigs)
                    {
                        Log.Debug($"加载配置: 玩家ID={kvp.Key}, 音乐数量={kvp.Value.MusicPaths?.Count ?? 0}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"加载MVP JSON数据库时发生错误: {ex.Message}");
                Log.Debug($"详细错误: {ex}");

                // 出错时创建新的数据库
                _mvpConfigs = new Dictionary<string, MVPPlayerConfig>();
                CreateExampleDatabase();

                // 记录文件修改时间
                if (File.Exists(_databasePath))
                {
                    _lastFileModifiedTime = File.GetLastWriteTime(_databasePath);
                }
            }
        }

        // 启动自动重载协程（每5分钟检查一次）
        public void StartAutoReload()
        {
            Log.Debug("启动MVP配置自动重载协程");

            // 先记录当前文件修改时间
            if (File.Exists(_databasePath))
            {
                _lastFileModifiedTime = File.GetLastWriteTime(_databasePath);
            }

            // 每5分钟检查一次文件修改
            _reloadCoroutine = Timing.RunCoroutine(AutoReloadCoroutine());
        }

        // 停止自动重载协程
        public void StopAutoReload()
        {
            if (_reloadCoroutine.IsRunning)
            {
                Timing.KillCoroutines(_reloadCoroutine);
                Log.Debug("停止MVP配置自动重载协程");
            }
        }

        // 立即重载配置（用于回合开始/结束时调用）
        public void ForceReload()
        {
            Log.Debug("强制重载MVP配置");
            LoadDatabase();
        }

        private IEnumerator<float> AutoReloadCoroutine()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(300f); // 每5分钟

                try
                {
                    // 检查文件是否存在
                    if (!File.Exists(_databasePath))
                    {
                        Log.Debug("MVP配置文件不存在，跳过本次检查");
                        continue;
                    }

                    // 检查文件是否被修改
                    DateTime currentModifiedTime = File.GetLastWriteTime(_databasePath);
                    if (currentModifiedTime > _lastFileModifiedTime)
                    {
                        Log.Info($"检测到MVP配置文件已修改，自动重载配置 (修改时间: {currentModifiedTime})");
                        LoadDatabase();
                    }
                    else
                    {
                        Log.Debug("MVP配置文件未修改，跳过重载");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"自动重载MVP配置时出错: {ex.Message}");
                }
            }
        }

        public void SaveDatabase()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonContent = JsonSerializer.Serialize(_mvpConfigs, options);
                File.WriteAllText(_databasePath, jsonContent);

                // 更新文件修改时间
                _lastFileModifiedTime = File.GetLastWriteTime(_databasePath);

                Log.Debug("MVP JSON数据库已保存");
            }
            catch (Exception ex)
            {
                Log.Error($"保存MVP JSON数据库时发生错误: {ex.Message}");
            }
        }

        public List<string> GetMusicPathsForPlayer(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Log.Debug("GetMusicPathsForPlayer: userId 为空");
                return null;
            }

            Log.Debug($"正在查找玩家ID: {userId}");

            // 直接匹配完整的userId
            if (_mvpConfigs.ContainsKey(userId))
            {
                var config = _mvpConfigs[userId];
                if (config.MusicPaths != null && config.MusicPaths.Any())
                {
                    Log.Debug($"直接匹配成功: {userId}, 找到 {config.MusicPaths.Count} 首音乐");
                    return config.MusicPaths;
                }
            }

            // 尝试匹配SteamID部分
            string steamIdOnly = ExtractSteamId(userId);
            if (!string.IsNullOrEmpty(steamIdOnly))
            {
                Log.Debug($"提取的SteamID: {steamIdOnly}");

                // 查找配置
                foreach (var kvp in _mvpConfigs)
                {
                    if (kvp.Value.SteamId == steamIdOnly)
                    {
                        Log.Debug($"通过SteamID匹配成功: {steamIdOnly}");
                        return kvp.Value.MusicPaths;
                    }
                }
            }

            Log.Debug($"未找到玩家 {userId} 的MVP音乐配置");
            return null;
        }

        public bool AddOrUpdatePlayerConfig(string userId, string steamId, string platform, List<string> musicPaths)
        {
            try
            {
                var config = new MVPPlayerConfig
                {
                    SteamId = steamId,
                    Platform = platform,
                    MusicPaths = musicPaths.Take(3).ToList(), // 限制最多3首
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                _mvpConfigs[userId] = config;
                SaveDatabase();

                Log.Info($"已更新玩家 {userId} 的MVP配置，添加了 {config.MusicPaths.Count} 首音乐");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"添加/更新玩家配置时出错: {ex.Message}");
                return false;
            }
        }

        public bool RemovePlayerConfig(string userId)
        {
            if (_mvpConfigs.Remove(userId))
            {
                SaveDatabase();
                Log.Info($"已移除玩家 {userId} 的MVP配置");
                return true;
            }

            return false;
        }

        public Dictionary<string, MVPPlayerConfig> GetAllConfigs()
        {
            return new Dictionary<string, MVPPlayerConfig>(_mvpConfigs);
        }

        public bool PlayerHasConfig(string userId)
        {
            return _mvpConfigs.ContainsKey(userId);
        }

        private string ExtractSteamId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            int atIndex = userId.IndexOf('@');
            if (atIndex > 0)
            {
                return userId.Substring(0, atIndex);
            }

            return userId;
        }

        private void CreateExampleDatabase()
        {
            try
            {
                // 创建示例配置
                _mvpConfigs = new Dictionary<string, MVPPlayerConfig>
                {
                    {
                        "76561199486004699@steam",
                        new MVPPlayerConfig
                        {
                            SteamId = "76561199486004699",
                            Platform = "steam",
                            MusicPaths = new List<string>
                            {
                                "C:/Music/mvp1.ogg",
                                "C:/Music/mvp2.ogg",
                                "C:/Music/mvp3.ogg"
                            },
                            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    },
                    {
                        "123456789@discord",
                        new MVPPlayerConfig
                        {
                            SteamId = "123456789",
                            Platform = "discord",
                            MusicPaths = new List<string>
                            {
                                "C:/Music/vip1.ogg",
                                "C:/Music/vip2.ogg"
                            },
                            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                };

                SaveDatabase();
                Log.Info("已创建示例MVP JSON数据库文件");

                // 输出文件路径，方便用户找到
                Log.Info($"MVP JSON配置文件路径: {_databasePath}");
                Log.Info($"你可以手动编辑此文件来配置MVP音乐");
            }
            catch (Exception ex)
            {
                Log.Error($"创建示例数据库时出错: {ex.Message}");
            }
        }

        private void ConvertLegacyTxtToJson(string legacyTxtPath)
        {
            try
            {
                Log.Info("正在转换旧版.txt配置文件到JSON格式...");

                string[] lines = File.ReadAllLines(legacyTxtPath);
                int convertedCount = 0;

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    try
                    {
                        string[] parts = line.Split(new char[] { ':' }, 2);
                        if (parts.Length < 2)
                            continue;

                        string playerInfo = parts[0];
                        int atIndex = playerInfo.LastIndexOf('@');
                        if (atIndex == -1)
                            continue;

                        string steamId = playerInfo.Substring(0, atIndex);
                        string platform = playerInfo.Substring(atIndex + 1);
                        string musicPathsStr = parts[1];

                        // 解析音乐路径
                        List<string> musicPaths = musicPathsStr
                            .Split(',')
                            .Select(path => path.Trim())
                            .Where(path => !string.IsNullOrEmpty(path))
                            .Take(3)
                            .ToList();

                        if (musicPaths.Any())
                        {
                            var config = new MVPPlayerConfig
                            {
                                SteamId = steamId,
                                Platform = platform,
                                MusicPaths = musicPaths,
                                LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            };

                            _mvpConfigs[playerInfo] = config;
                            convertedCount++;

                            Log.Debug($"转换配置: {playerInfo} -> {musicPaths.Count} 首音乐");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"转换行时出错 '{line}': {ex.Message}");
                    }
                }

                if (convertedCount > 0)
                {
                    SaveDatabase();
                    Log.Info($"已成功转换 {convertedCount} 个旧版配置到JSON格式");

                    // 备份旧文件
                    string backupPath = legacyTxtPath + ".backup";
                    File.Copy(legacyTxtPath, backupPath, true);
                    Log.Info($"旧版配置文件已备份到: {backupPath}");
                }
                else
                {
                    // 没有成功转换，创建示例数据库
                    CreateExampleDatabase();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"转换旧版配置时出错: {ex.Message}");
                // 出错时创建新的数据库
                CreateExampleDatabase();
            }
        }
    }

    public class MVPPlayerConfig
    {
        public string SteamId { get; set; }
        public string Platform { get; set; }
        public List<string> MusicPaths { get; set; } = new List<string>();
        public string LastUpdated { get; set; }
    }
}