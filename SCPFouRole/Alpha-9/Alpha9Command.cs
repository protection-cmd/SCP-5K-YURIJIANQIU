using CommandSystem;
using Exiled.API.Features;
using System;

namespace SCP5K.SCPFouRole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Alpha9Command : ICommand
    {
        public string Command { get; } = "a9";
        public string[] Aliases { get; } = new string[] { };
        public string Description { get; } = "Alpha-9 叛变投票 (yes/no)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "只有玩家才能使用此命令！";
                return false;
            }

            if (!Alpha9Manager.IsVotingActive)
            {
                response = "当前未在投票阶段！";
                return false;
            }

            if (!Alpha9Manager.A9TeamMembers.Contains(player))
            {
                response = "你不是Alpha-9阵营成员！";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "请使用 .a9 yes 或 .a9 no 进行投票";
                return false;
            }

            string vote = arguments.At(0).ToLower();
            if (vote == "yes")
            {
                Alpha9Manager.RegisterVote(player, true);
                response = "你已投票：叛离SCP基金会！";
                return true;
            }
            else if (vote == "no")
            {
                Alpha9Manager.RegisterVote(player, false);
                response = "你已投票：不叛变！";
                return true;
            }
            else
            {
                response = "无效选项。请使用 .a9 yes 或 .a9 no";
                return false;
            }
        }
    }
}