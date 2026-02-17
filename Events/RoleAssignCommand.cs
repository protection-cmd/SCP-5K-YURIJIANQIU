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

        // 可用的角色和队伍列表
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
            { "NU7", "Nu-7落锤特遣队 (需要3名)" },
            { "GOCTEAM", "GOC打击小组 (需要4-8名)" },
            { "610", "SCP-610血肉瘟疫 (需要2名，第一个为母体，第二个为喷射体)" },
            { "CI", "混沌分裂者GRU小组 (需要3-6名)" },
            
            // 模型生成
            { "SPAWN", "生成GOC模型" }
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // 权限检查
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

            // 显示可用角色列表
            if (commandType == "LIST")
            {
                response = GetAvailableRoles();
                return true;
            }

            // 检查命令是否可用
            if (!AvailableRoles.ContainsKey(commandType))
            {
                response = $"未知的命令: {commandType}\n\n{GetAvailableRoles()}";
                return false;
            }

            try
            {
                // 处理不同类型的命令
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
                    case "NU7":
                    case "GOCTEAM":
                    case "610":
                    case "CI":
                        return HandleTeamRole(commandType, arguments, out response);

                    case "SPAWN": // 模型生成
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
            if (arguments.Count < 2)
            {
                response = $"请指定玩家ID。用法: 5K {roleType} <玩家ID>";
                return false;
            }

            // 解析玩家ID
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

            // 设置角色
            bool success = false;
            string roleName = "";

            switch (roleType.ToUpper())
            {
                case "D9341":
                    success = SetPlayerAsD9341(player);
                    roleName = "D9341";
                    break;

                case "良子":
                    success = DDpig.SetPlayerAsSpecialDClass(player);
                    roleName = "良子";
                    break;

                case "运动员":
                    success = DDRunning.SetPlayerAsAthlete(player);
                    roleName = "运动员";
                    break;

                case "682":
                    success = SCP682.SpawnSCP682(player);
                    roleName = "SCP-682";
                    break;

                case "610MOTHER":
                    success = SCP610.SpawnMotherEntity(player);
                    roleName = "SCP-610母体";
                    break;

                case "610SPRAYER":
                    success = SCP610.SpawnSprayer(player);
                    roleName = "SCP-610喷射体";
                    break;

                case "610CHILD":
                    success = SCP610.ConvertToChild(player);
                    roleName = "SCP-610子个体";
                    break;
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
            List<Player> targetPlayers = new List<Player>();

            // 必须指定玩家ID
            if (arguments.Count < 2)
            {
                response = $"请指定玩家ID。用法: 5K {roleType} <玩家ID1> <玩家ID2> [玩家ID3...]";
                return false;
            }

            // 收集所有玩家ID
            for (int i = 1; i < arguments.Count; i++)
            {
                if (int.TryParse(arguments.At(i), out int playerId))
                {
                    Player player = Player.Get(playerId);
                    if (player != null && player.IsConnected)
                    {
                        targetPlayers.Add(player);
                    }
                }
            }

            // 检查玩家数量
            if (roleType.ToUpper() == "GOCTEAM")
            {
                // GOCTEAM需要4-8名玩家
                if (targetPlayers.Count < 4 || targetPlayers.Count > 8)
                {
                    response = $"生成GOCTEAM队伍需要4-8名玩家，但指定了{targetPlayers.Count}名";
                    return false;
                }
            }
            else if (roleType.ToUpper() == "CI")
            {
                // CI需要3-6名玩家
                if (targetPlayers.Count < 3 || targetPlayers.Count > 6)
                {
                    response = $"生成CI队伍需要3-6名玩家，但指定了{targetPlayers.Count}名";
                    return false;
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

            // 生成队伍
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

                case "NU7":
                    success = Nu7HammerDown.SpawnNu7Team(targetPlayers);
                    teamName = "Nu-7落锤特遣队";
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

            // 解析坐标
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
                        response = success ?
                            $"✓ 已成功在位置 {position} 生成GOCRGM模型" :
                            "✗ 生成GOCRGM模型失败";
                        break;

                    case "GOCSWORD":
                        success = SpawnGOCSword(position);
                        response = success ?
                            $"✓ 已成功在位置 {position} 生成GOCSWORD模型" :
                            "✗ 生成GOCSWORD模型失败";
                        break;

                    default:
                        response = $"未知的模型类型: {modelType}";
                        return false;
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
                if (string.IsNullOrEmpty(schematicName))
                {
                    schematicName = "RGM";
                }

                ObjectSpawner.SpawnSchematic(
                    schematicName,
                    position,
                    Quaternion.identity,
                    Vector3.one
                );

                Log.Debug($"生成GOCRGM模型成功: {schematicName} 在 {position}");
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
                if (string.IsNullOrEmpty(schematicName))
                {
                    schematicName = "Sword";
                }

                ObjectSpawner.SpawnSchematic(
                    schematicName,
                    position,
                    Quaternion.identity,
                    Vector3.one
                );

                Log.Debug($"生成GOCSWORD模型成功: {schematicName} 在 {position}");
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
                case "GOC":
                    return 3;
                case "NU7":
                    return 3;
                case "GOCTEAM":
                    return 4;
                case "610":
                    return 2;
                case "CI":
                    return 3;
                default:
                    return 3;
            }
        }

        private bool SetPlayerAsD9341(Player player)
        {
            try
            {
                var plugin = Plugin.Instance;
                if (plugin == null)
                {
                    Log.Error("无法获取Plugin实例");
                    return false;
                }

                var handlerField = typeof(Plugin).GetField("d9341Handler",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (handlerField == null)
                {
                    Log.Error("无法找到d9341Handler字段");
                    return false;
                }

                var handler = handlerField.GetValue(plugin) as D9341EventHandler;
                if (handler == null)
                {
                    Log.Error("d9341Handler为null");
                    return false;
                }

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
  5K 610MOTHER <玩家ID>   - 设置为SCP-610母体 (固定1000血)
  5K 610SPRAYER <玩家ID>  - 设置为SCP-610喷射体 (固定600血+COM15)
  5K 610CHILD <玩家ID>    - 设置为SCP-610子个体 (固定400血)

🔹 队伍 (必须指定玩家ID):
  5K GOC <玩家ID> <玩家ID2> <玩家ID3>                - 生成GOC奇术打击小组 (需要3名)
  5K NU7 <玩家ID> <玩家ID2> <玩家ID3>                - 生成Nu-7落锤特遣队 (需要3名)
  5K GOCTeam <玩家ID> <玩家ID2> <玩家ID3> <玩家ID4> [玩家ID5...玩家ID8] - 生成GOC打击小组 (需要4-8名)
  5K 610 <玩家ID> <玩家ID2>                          - 生成SCP-610血肉瘟疫 (需要2名：第一个为母体，第二个为喷射体)
  5K CI <玩家ID> <玩家ID2> <玩家ID3> [玩家ID4...玩家ID6] - 生成混沌分裂者GRU小组 (需要3-6名)

🔹 模型生成:
  5K spawn GOCRGM <x> <y> <z>    - 在指定位置生成GOCRGM模型
  5K spawn GOCSWORD <x> <y> <z>  - 在指定位置生成GOCSWORD模型

📌 注意: 
  • 玩家数据管理已迁移到 LS 命令，请使用 ls 命令
  • 队伍生成必须指定玩家ID
  • 坐标x,y,z为浮点数，例如: 10.5 20.0 -5.3

📝 示例:
  5K 610 5 6             (玩家5为母体，玩家6为喷射体)
  5K GOC 2 4 6
  5K NU7 1 3 5
  5K CI 1 2 3 4          (4人CI小组)
  5K spawn GOCRGM 100.5 0.0 -50.0";
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
                x.Key == "GOC" || x.Key == "NU7" || x.Key == "GOCTEAM" || x.Key == "610" || x.Key == "CI"))
            {
                result += $"• {role.Key}: {role.Value}\n";
            }

            result += "\n🔹 模型生成 🔹\n";
            result += "═══════════════\n";
            result += "• GOCRGM: 在指定位置生成GOCRGM模型\n";
            result += "• GOCSWORD: 在指定位置生成GOCSWORD模型\n";

            result += "\n📌 提示: 玩家数据管理已迁移到 LS 命令，请使用 ls 命令\n";

            return result;
        }
    }
}