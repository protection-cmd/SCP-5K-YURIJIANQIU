using System;
using System.IO;
using System.Text.Json;
using Exiled.API.Features;

namespace SCP5K.MVPSystem
{
    public class MVPRoundRecord
    {
        private static MVPRoundRecord _instance;
        public static MVPRoundRecord Instance => _instance ??= new MVPRoundRecord();

        private string _recordFilePath;

        public class MVPRecordData
        {
            public string Steam64 { get; set; }
            public string PlayerName { get; set; }
            public string RoundId { get; set; }
            public DateTime Timestamp { get; set; }
            public int Score { get; set; }
            public int Kills { get; set; }
            public int Damage { get; set; }
        }

        private MVPRoundRecord()
        {
            // 获取配置文件路径
            string configPath = Plugin.Instance.Config.MVPConfigFilePath;
            string directory = Path.GetDirectoryName(configPath);

            // 构建记录文件路径
            _recordFilePath = Path.Combine(directory, "MVP_Record.json");

            Log.Info($"MVP记录文件路径: {_recordFilePath}");
        }

        // 写入MVP记录
        public bool WriteMVPRecord(Player mvpPlayer, int score, int kills, int damage)
        {
            if (mvpPlayer == null || string.IsNullOrEmpty(mvpPlayer.UserId))
            {
                Log.Error("写入MVP记录失败：玩家为空或UserId为空");
                return false;
            }

            try
            {
                // 提取Steam64
                string steam64 = ExtractSteam64(mvpPlayer.UserId);
                if (string.IsNullOrEmpty(steam64))
                {
                    Log.Error($"无法从UserId中提取Steam64: {mvpPlayer.UserId}");
                    return false;
                }

                var record = new MVPRecordData
                {
                    Steam64 = steam64,
                    PlayerName = mvpPlayer.Nickname,
                    RoundId = GenerateRoundId(),
                    Timestamp = DateTime.Now,
                    Score = score,
                    Kills = kills,
                    Damage = damage
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonContent = JsonSerializer.Serialize(record, options);
                File.WriteAllText(_recordFilePath, jsonContent);

                Log.Info($"MVP记录已写入: {mvpPlayer.Nickname} ({steam64}), 得分: {score}");

                if (Plugin.Instance.Config.Debug)
                {
                    Log.Debug($"写入的MVP记录: {jsonContent}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"写入MVP记录时出错: {ex.Message}");
                return false;
            }
        }

        // 清除MVP记录（由LS插件调用）
        public bool ClearMVPRecord()
        {
            try
            {
                if (File.Exists(_recordFilePath))
                {
                    File.Delete(_recordFilePath);
                    Log.Debug("MVP记录文件已删除");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"删除MVP记录文件时出错: {ex.Message}");
                return false;
            }
        }

        // 检查是否有MVP记录
        public bool HasMVPRecord()
        {
            return File.Exists(_recordFilePath);
        }

        // 获取MVP记录文件路径（提供给外部插件使用）
        public string GetRecordFilePath()
        {
            return _recordFilePath;
        }

        // 提取Steam64
        private string ExtractSteam64(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            int atIndex = userId.IndexOf('@');
            if (atIndex > 0)
                return userId.Substring(0, atIndex);

            return userId;
        }

        // 生成回合ID
        private string GenerateRoundId()
        {
            return $"Round_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}