using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using NorthwoodLib;
using PlayerRoles;
using ProjectMER.Features;
using SCP5K.LCZRole;
using SCP5K.SCPFouRole;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YuRiLS;
using YuRiLS.PlayerDataSystem;

namespace SCP5K.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RoleAssignCommand : ICommand
    {
        public string Command { get; } = "5K";
        public string[] Aliases { get; } = new[] { "scp5k" };
        public string Description { get; } = "设置玩家为指定的SCP-5K角色或队伍，或生成模型";

        private static readonly Dictionary<string, string> AvailableRoles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // 单个角色
            { "D9341", "D9341 - 存档读档能力者" },
            { "良子", "特殊D级人员 - 血量加成" },
            { "运动员", "运动员 - 移动速度加成" },
            { "682", "SCP-682 - 不灭孽蜥" },
            { "610MOTHER", "SCP-610母体 (1000血)" },
            { "610SPRAYER", "SCP-610喷射体 (600血+COM15)" },
            { "610CHILD", "SCP-610子个体 (400血)" },
            
            // 队伍
            { "GOC", "GOC奇术打击小组 (需要3名)" },
            { "NU7A", "Nu-7-A落锤特遣队 (需要3-5名)" },
            { "NU7B", "Nu-7-B落锤特遣队 (需要4-7名)" },
            { "GOCTEAM", "GOC打击小组 (需要4-8名)" },
            { "610", "SCP-610血肉瘟疫 (需要2名，第一个为母体，第二个为喷射体)" },
            { "CI", "混沌分裂者GRU小组 (需要3-6名)" },
            { "GRUCI", "GRU-CI 特遣队 (需要1-13名)" },
            
            // 模型生成
            { "SPAWN", "生成GOC模型" }
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;

            if (!sender.CheckPermission("5k.setrole"))
            {
                response = "权限不足，需要 5k.setrole 权限";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = GetUsageText();
                return false;
            }

            string commandType = arguments.At(0).ToUpper();

            if (commandType == "LIST")
            {
                response = GetAvailableRoles();
                return true;
            }

            if (!AvailableRoles.ContainsKey(commandType))
            {
                response = $"未知的命令: {commandType}\n\n{GetAvailableRoles()}";
                return false;
            }

            try
            {
                switch (commandType)
                {
                    case "D9341":
                    case "良子":
                    case "运动员":
                    case "682":
                    case "610MOTHER":
                    case "610SPRAYER":
                    case "610CHILD":
                        return HandleSingleRole(commandType, arguments, out response);

                    case "GOC":
                    case "NU7A":
                    case "NU7B":
                    case "GOCTEAM":
                    case "610":
                    case "CI":
                    case "GRUCI":
                        return HandleTeamRole(commandType, arguments, out response);

                    case "SPAWN":
                        return HandleSpawnModel(arguments, out response);

                    default:
                        response = $"未实现的命令类型: {commandType}";
                        return false;
                }
            }
            catch (Exception ex)
            {
                response = $"执行命令时出错: {ex.Message}";
                Log.Error($"5K命令执行错误: {ex}");
                return false;
            }
        }

        private bool HandleSingleRole(string roleType, ArraySegment<string> arguments, out string response)
        {
            response = string.Empty;

            if (arguments.Count < 2)
            {
                response = $"请指定玩家ID。用法: 5K {roleType} <玩家ID>";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int playerId))
            {
                response = "无效的玩家ID";
                return false;
            }

            Player player = Player.Get(playerId);
            if (player == null || !player.IsConnected)
            {
                response = $"未找到ID为 {playerId} 的在线玩家";
                return false;
            }

            bool success = false;
            string roleName = "";

            switch (roleType.ToUpper())
            {
                case "D9341": success = SetPlayerAsD9341(player); roleName = "D9341"; break;
                case "良子": success = DDpig.SetPlayerAsSpecialDClass(player); roleName = "良子"; break;
                case "运动员": success = DDRunning.SetPlayerAsAthlete(player); roleName = "运动员"; break;
                case "682": success = SCP682.SpawnSCP682(player); roleName = "SCP-682"; break;
                case "610MOTHER": success = SCP610.SpawnMotherEntity(player); roleName = "SCP-610母体"; break;
                case "610SPRAYER": success = SCP610.SpawnSprayer(player); roleName = "SCP-610喷射体"; break;
                case "610CHILD": success = SCP610.ConvertToChild(player); roleName = "SCP-610子个体"; break;
            }

            if (success)
            {
                response = $"已成功将玩家 {player.Nickname} ({player.Id}) 设置为 {roleName}";
                Log.Info($"管理员通过RA命令将玩家 {player.Nickname} 设置为 {roleName}");
                return true;
            }
            else
            {
                response = $"设置玩家 {player.Nickname} 为 {roleName} 失败";
                return false;
            }
        }

        private bool HandleTeamRole(string roleType, ArraySegment<string> arguments, out string response)
        {
            response = string.Empty;

            List<Player> targetPlayers = new List<Player>();

            if (arguments.Count < 2)
            {
                response = $"请指定玩家ID。用法: 5K {roleType} <玩家ID1> <玩家ID2> [玩家ID3...]";
                return false;
            }

            for (int i = 1; i < arguments.Count; i++)
            {
                if (int.TryParse(arguments.At(i), out int playerId))
                {
                    Player player = Player.Get(playerId);
                    if (player != null && player.IsConnected) targetPlayers.Add(player);
                }
            }

            if (roleType.ToUpper() == "GOCTEAM")
            {
                if (targetPlayers.Count < 4 || targetPlayers.Count > 8)
                {
                    response = $"生成GOCTEAM队伍需要4-8名玩家，但指定了{targetPlayers.Count}名"; return false;
                }
            }
            else if (roleType.ToUpper() == "CI")
            {
                if (targetPlayers.Count < 3 || targetPlayers.Count > 6)
                {
                    response = $"生成CI队伍需要3-6名玩家，但指定了{targetPlayers.Count}名"; return false;
                }
            }
            else if (roleType.ToUpper() == "NU7A")
            {
                if (targetPlayers.Count < 3 || targetPlayers.Count > 5)
                {
                    response = $"生成NU7A队伍需要3-5名玩家，但指定了{targetPlayers.Count}名"; return false;
                }
            }
            else if (roleType.ToUpper() == "NU7B")
            {
                if (targetPlayers.Count < 4 || targetPlayers.Count > 7)
                {
                    response = $"生成NU7B队伍需要4-7名玩家，但指定了{targetPlayers.Count}名"; return false;
                }
            }
            else if (roleType.ToUpper() == "GRUCI")
            {
                if (targetPlayers.Count < 1 || targetPlayers.Count > 13)
                {
                    response = $"生成GRUCI队伍需要1-13名玩家，但指定了{targetPlayers.Count}名"; return false;
                }
            }
            else
            {
                int requiredPlayers = GetRequiredPlayerCount(roleType);
                if (targetPlayers.Count != requiredPlayers)
                {
                    response = $"生成{roleType}队伍需要{requiredPlayers}名玩家，但指定了{targetPlayers.Count}名";
                    return false;
                }
            }

            bool success = false;
            string teamName = "";
            string playerNames = "";

            switch (roleType.ToUpper())
            {
                case "GOC":
                    success = GOCArcaneStrike.SpawnGOCTeam(targetPlayers);
                    teamName = "GOC奇术打击小组";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
                case "NU7A":
                    success = Nu7HammerDown.SpawnNu7ATeam(targetPlayers);
                    teamName = "Nu-7-A落锤特遣队";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
                case "NU7B":
                    success = Nu7HammerDown.SpawnNu7BTeam(targetPlayers);
                    teamName = "Nu-7-B落锤特遣队";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
                case "GOCTEAM":
                    success = GOCTeam.SpawnGOCTeam(targetPlayers);
                    teamName = "GOC打击小组";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
                case "610":
                    success = SCP610.SpawnSCP610Team(targetPlayers);
                    teamName = "SCP-610血肉瘟疫";
                    playerNames = string.Join(", ", targetPlayers.Select((p, index) =>
                        $"{p.Nickname}({p.Id})[{(index == 0 ? "母体" : "喷射体")}]"));
                    break;
                case "CI":
                    success = CIGRU.SpawnCITeam(targetPlayers);
                    teamName = "混沌分裂者GRU小组";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
                case "GRUCI":
                    success = GRUCIManager.SpawnTeam(targetPlayers);
                    teamName = "GRU-CI 特遣队";
                    playerNames = string.Join(", ", targetPlayers.Select(p => $"{p.Nickname}({p.Id})"));
                    break;
            }

            if (success)
            {
                response = $"已成功生成{teamName}，成员: {playerNames}";
                Log.Info($"管理员通过RA命令生成{teamName}，成员: {playerNames}");
                return true;
            }
            else
            {
                response = $"生成{teamName}失败";
                return false;
            }
        }

        private bool HandleSpawnModel(ArraySegment<string> arguments, out string response)
        {
            response = string.Empty;

            if (arguments.Count < 5)
            {
                response = "用法: 5K spawn GOCRGM/GOCSWORD <x> <y> <z>";
                return false;
            }

            string modelType = arguments.At(1).ToUpper();

            if (modelType != "GOCRGM" && modelType != "GOCSWORD")
            {
                response = "模型类型错误，可用类型: GOCRGM, GOCSWORD";
                return false;
            }

            if (!float.TryParse(arguments.At(2), out float x) ||
                !float.TryParse(arguments.At(3), out float y) ||
                !float.TryParse(arguments.At(4), out float z))
            {
                response = "坐标格式错误，请提供有效的浮点数坐标";
                return false;
            }

            Vector3 position = new Vector3(x, y, z);

            try
            {
                bool success = false;

                switch (modelType)
                {
                    case "GOCRGM":
                        success = SpawnGOCRGM(position);
                        response = success ? $"✓ 已成功在位置 {position} 生成GOCRGM模型" : "✗ 生成GOCRGM模型失败";
                        break;
                    case "GOCSWORD":
                        success = SpawnGOCSword(position);
                        response = success ? $"✓ 已成功在位置 {position} 生成GOCSWORD模型" : "✗ 生成GOCSWORD模型失败";
                        break;
                    default:
                        response = "未知的模型类型";
                        break;
                }

                if (success)
                {
                    Log.Info($"管理员通过RA命令在位置 {position} 生成{modelType}模型");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                response = $"生成模型时出错: {ex.Message}";
                Log.Error($"生成模型错误: {ex}");
                return false;
            }
        }

        private bool SpawnGOCRGM(Vector3 position)
        {
            try
            {
                string schematicName = GOCArcaneStrike.RGMSchematicName;
                if (string.IsNullOrEmpty(schematicName)) schematicName = "RGM";
                ObjectSpawner.SpawnSchematic(schematicName, position, Quaternion.identity, Vector3.one);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"生成GOCRGM模型失败: {ex.Message}");
                return false;
            }
        }

        private bool SpawnGOCSword(Vector3 position)
        {
            try
            {
                string schematicName = ArcaneStrike.SwordSchematicName;
                if (string.IsNullOrEmpty(schematicName)) schematicName = "Sword";
                ObjectSpawner.SpawnSchematic(schematicName, position, Quaternion.identity, Vector3.one);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"生成GOCSWORD模型失败: {ex.Message}");
                return false;
            }
        }

        private int GetRequiredPlayerCount(string roleType)
        {
            switch (roleType.ToUpper())
            {
                case "GOC": return 3;
                case "NU7A": return 3;
                case "NU7B": return 4;
                case "GOCTEAM": return 4;
                case "610": return 2;
                case "CI": return 3;
                default: return 3;
            }
        }

        private bool SetPlayerAsD9341(Player player)
        {
            try
            {
                var plugin = Plugin.Instance;
                if (plugin == null) return false;

                var handlerField = typeof(Plugin).GetField("d9341Handler",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (handlerField == null) return false;

                var handler = handlerField.GetValue(plugin) as D9341EventHandler;
                if (handler == null) return false;

                return handler.SetPlayerAsD9341(player);
            }
            catch (Exception ex)
            {
                Log.Error($"设置D9341时出错: {ex}");
                return false;
            }
        }

        private string GetUsageText()
        {
            return @"🏆 SCP-5K 角色设置命令

使用方法: 
5K <角色/队伍> [玩家ID] [玩家ID2] [玩家ID3]...
5K spawn <模型类型> <x> <y> <z>

🔹 单个角色 (需要指定1名玩家):
  5K D9341 <玩家ID>       - 设置为D9341
  5K 良子 <玩家ID>        - 设置为特殊D级(良子)
  5K 运动员 <玩家ID>      - 设置为运动员
  5K 682 <玩家ID>         - 设置为SCP-682 (不灭孽蜥)

🔹 队伍 (必须指定玩家ID):
  5K GOC <玩家ID1...3>                 - 生成GOC奇术打击小组 (需要3名)
  5K NU7A <玩家ID1...5>                - 生成Nu-7-A连 (需要3-5名)
  5K NU7B <玩家ID1...7>                - 生成Nu-7-B连 (需要4-7名)
  5K GRUCI <玩家ID1...13>              - 生成GRU-CI特遣队 (需要1-13名)
  5K CI <玩家ID1...6>                  - 生成混沌分裂者GRU小组 (需要3-6名)
  5K 610 <玩家ID1> <玩家ID2>           - 生成SCP-610血肉瘟疫 (母体+喷射体)";
        }

        private string GetAvailableRoles()
        {
            string result = "🏆 SCP-5K 可用命令 🏆\n\n";

            result += "🔹 单个角色 🔹\n";
            result += "═══════════════\n";
            foreach (var role in AvailableRoles.Where(x =>
                x.Key == "D9341" || x.Key == "良子" || x.Key == "运动员" ||
                x.Key == "682" || x.Key.StartsWith("610")))
            {
                result += $"• {role.Key}: {role.Value}\n";
            }

            result += "\n🔹 队伍 🔹\n";
            result += "══════════\n";
            foreach (var role in AvailableRoles.Where(x =>
                x.Key == "GOC" || x.Key == "NU7A" || x.Key == "NU7B" || x.Key == "GOCTEAM" || x.Key == "610" || x.Key == "CI" || x.Key == "GRUCI"))
            {
                result += $"• {role.Key}: {role.Value}\n";
            }

            return result;
        }
    }
}