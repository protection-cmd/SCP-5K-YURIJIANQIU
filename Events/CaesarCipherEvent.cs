using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using SCP5K.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP5K
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DecryptCommand : ICommand
    {
        public string Command { get; } = "goc";
        public string[] Aliases { get; } = new[] { "Ca", "Ci" };
        public string Description { get; } = "解密凯撒密码";

        // 密码单词库（来自图片文件）
        private static readonly List<string> passwordWords = new List<string>
        {
            "AWARE", "ALIVE", "ALPHA", "ARRAY", "AREA", "ASSIST", "ATTEND",
            "BETA", "BRAIN", "BRAVE", "BRIEF", "CHAIR", "CLEF", "CORE",
            "FLM", "HURT", "OMGEA", "PAINFUL", "PHOBIA", "SIT"
        };

        // 固定添加的单词
        private static readonly List<string> fixedWords = new List<string> { "SL", "YURI" };

        // 存储每个玩家的密码信息
        private static Dictionary<Player, CipherChallenge> playerChallenges = new Dictionary<Player, CipherChallenge>();

        // 随机数生成器
        private static Random random = new Random();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);

            if (!CheckPermission(player, out response))
                return false;

            if (arguments.Count == 0)
            {
                response = "使用方法: .goc <密码>";
                return false;
            }

            var playerInput = string.Join(" ", arguments).ToUpper().Trim();
            return ProcessDecryption(player, playerInput, out response);
        }

        public bool CheckPermission(Player player, out string response)
        {
            if (!playerChallenges.ContainsKey(player))
            {
                response = "您当前没有凯撒密码挑战";
                return false;
            }

            response = string.Empty;
            return true;
        }


        /// <summary>
        /// 处理解密逻辑
        /// </summary>
        private bool ProcessDecryption(Player player, string playerInput, out string response)
        {
            if (!playerChallenges.ContainsKey(player))
            {
                response = "挑战状态不存在";
                return false;
            }

            var challenge = playerChallenges[player];

            // 如果已经完成，不再处理
            if (challenge.IsCompleted)
            {
                response = "挑战已完成，请等待结果";
                return false;
            }

            string correctPassword = challenge.OriginalPassword.ToUpper();
            string normalizedInput = playerInput.Trim().ToUpper();
            bool isCorrect = normalizedInput == correctPassword;

            // 记录挑战结果但不移除状态
            challenge.IsCompleted = true;
            challenge.IsSuccess = isCorrect;

            if (isCorrect)
            {
                response = "true";
                var message = $"<color=green>✅ 密码正确！挑战成功！</color>\n输入: {normalizedInput}\n正确: {correctPassword}";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 10f, "凯撒密码挑战t");
                Log.Info($"玩家 {player.Nickname} 成功完成凯撒密码挑战，输入: '{normalizedInput}'，正确: '{correctPassword}'");
            }
            else
            {
                response = "false";
                var message = $"<color=red>❌ 密码错误！挑战失败！</color>\n输入: {normalizedInput}\n正确: {correctPassword}";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 10f, "凯撒密码挑战f");
                Log.Info($"玩家 {player.Nickname} 凯撒密码挑战失败，输入: '{normalizedInput}'，正确: '{correctPassword}'");
            }

            return true;
        }

        /// <summary>
        /// 凯撒密码加密（向前移动3位）
        /// </summary>
        public static string CaesarEncrypt(string text, int shift = 3)
        {
            char[] buffer = text.ToUpper().ToCharArray();

            for (int i = 0; i < buffer.Length; i++)
            {
                char letter = buffer[i];
                if (char.IsLetter(letter))
                {
                    char offset = char.IsUpper(letter) ? 'A' : 'a';
                    letter = (char)((letter + shift - offset) % 26 + offset);
                }
                buffer[i] = letter;
            }
            return new string(buffer);
        }

        /// <summary>
        /// 凯撒密码解密（向后移动3位）
        /// </summary>
        public static string CaesarDecrypt(string text, int shift = 3)
        {
            return CaesarEncrypt(text, 26 - shift); // 26 - shift 相当于向后移动
        }

        /// <summary>
        /// 触发凯撒密码事件
        /// </summary>
        public static void StartCaesarChallenge(Player player)
        {
            try
            {
                // 如果玩家已有挑战，先重置
                ResetPlayerChallenge(player);

                // 将玩家设置为教程人员
                player.Role.Set(RoleTypeId.Tutorial, SpawnReason.ForceClass);

                // 生成随机密码（从单词库中随机选择3个单词）
                var allWords = new List<string>(passwordWords);
                allWords.AddRange(fixedWords);

                var selectedWords = allWords.OrderBy(x => random.Next()).Take(3).ToList();
                string originalPassword = string.Join(" ", selectedWords);
                string encryptedPassword = CaesarEncrypt(originalPassword);

                // 存储玩家挑战信息
                playerChallenges[player] = new CipherChallenge
                {
                    OriginalPassword = originalPassword,
                    EncryptedPassword = encryptedPassword,
                    IsCompleted = false,
                    IsSuccess = false
                };

                // 显示加密后的密码提示
                var message = $"<color=yellow>🔐 凯撒密码挑战 🔐</color>\n" +
                               $"加密密码: <color=cyan>{encryptedPassword}</color>\n" +
                               $"提示: 使用凯撒密码解密（向前移动3位）\n" +
                               $"在控制台输入: .goc [密码]\n" +
                               $"你只有一次机会！";
                HSMShowhint.HsmShowHint(player, message, 600, 0, 30f, "凯撒密码挑战");

                Log.Info($"玩家 {player.Nickname} 触发了凯撒密码事件");
                Log.Info($"原始密码: {originalPassword}");
                Log.Info($"加密密码: {encryptedPassword}");
            }
            catch (Exception ex)
            {
                Log.Error($"触发凯撒密码事件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 挑战成功后的处理（空实现，由调用方处理）
        /// </summary>
        private static void OnChallengeSuccess(Player player)
        {
            // 空实现，奖励逻辑由调用方处理
            Log.Info($"玩家 {player.Nickname} 成功完成凯撒密码挑战，等待调用方处理奖励");
        }

        /// <summary>
        /// 挑战失败后的处理（空实现，由调用方处理重试逻辑）
        /// </summary>
        private static void OnChallengeFailed(Player player)
        {
            // 空实现，重试逻辑由调用方处理
            Log.Info($"玩家 {player.Nickname} 凯撒密码挑战失败，等待调用方处理重试逻辑");
        }

        /// <summary>
        /// 重置玩家挑战状态
        /// </summary>
        public static void ResetPlayerChallenge(Player player)
        {
            if (playerChallenges.ContainsKey(player))
            {
                playerChallenges.Remove(player);
                Log.Debug($"已重置玩家 {player.Nickname} 的凯撒密码挑战状态");
            }
        }

        /// <summary>
        /// 回合结束时清理所有挑战状态
        /// </summary>
        public static void OnRoundEnded()
        {
            playerChallenges.Clear();
            Log.Debug("回合结束，已清理所有凯撒密码挑战状态");
        }

        /// <summary>
        /// 玩家退出游戏时清理挑战状态
        /// </summary>
        public static void OnPlayerLeft(Player player)
        {
            ResetPlayerChallenge(player);
            Log.Debug($"玩家 {player.Nickname} 退出游戏，已清理凯撒密码挑战状态");
        }

        /// <summary>
        /// 挑战信息类
        /// </summary>
        private class CipherChallenge
        {
            public string OriginalPassword { get; set; }
            public string EncryptedPassword { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsSuccess { get; set; }
        }

        // API 方法 - 供其他类调用

        // 检查玩家是否正在挑战中
        public static bool IsPlayerInChallenge(Player player)
        {
            return playerChallenges.ContainsKey(player) && !playerChallenges[player].IsCompleted;
        }

        // 获取玩家的加密密码
        public static string GetPlayerEncryptedPassword(Player player)
        {
            return playerChallenges.ContainsKey(player) ? playerChallenges[player].EncryptedPassword : null;
        }

        // 获取玩家的原始密码
        public static string GetPlayerOriginalPassword(Player player)
        {
            return playerChallenges.ContainsKey(player) ? playerChallenges[player].OriginalPassword : null;
        }

        // 获取玩家挑战结果
        public static bool? GetPlayerChallengeResult(Player player)
        {
            if (playerChallenges.ContainsKey(player) && playerChallenges[player].IsCompleted)
            {
                return playerChallenges[player].IsSuccess;
            }
            return null;
        }

        // 强制完成玩家挑战
        public static void ForceCompleteChallenge(Player player)
        {
            if (playerChallenges.ContainsKey(player))
            {
                playerChallenges[player].IsCompleted = true;
                playerChallenges[player].IsSuccess = true;
                OnChallengeSuccess(player);
            }
        }

        // 强制失败玩家挑战
        public static void ForceFailChallenge(Player player)
        {
            if (playerChallenges.ContainsKey(player))
            {
                playerChallenges[player].IsCompleted = true;
                playerChallenges[player].IsSuccess = false;
                OnChallengeFailed(player);
            }
        }

        // 获取所有正在进行挑战的玩家
        public static List<Player> GetPlayersInChallenge()
        {
            return playerChallenges.Keys.ToList();
        }

        // 获取所有已完成挑战的玩家
        public static List<Player> GetPlayersCompletedChallenge()
        {
            return playerChallenges.Where(x => x.Value.IsCompleted).Select(x => x.Key).ToList();
        }
    }
}
