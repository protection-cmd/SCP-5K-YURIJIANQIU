using CommandSystem.Commands.RemoteAdmin;
using Exiled.API.Features;
using MEC;
using Mirror;
using ProjectMER.Features;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP5K.Events
{
    public static class CASSIE
    {
        public static void OnRoundStarted()
        {
            Timing.CallDelayed(0.1f, () =>
            {
                Server.ExecuteCommand("/cassieadvanced custom False 1 <b><color=#FF0000>Warning\r\n<split><b><color=#FF0000>警告，O5议会已发布最高命令消灭人类\r\n<split><b><color=#FF0000>轻收容区域的所有人\r\n<split><b><color=#FF0000>所有人不得进行抵抗或者试图升级异常状况\r\n<split><b><color=#FF0000>这将视为SCP基金会的叛徒\r\n<split><b><color=#FF0000>知道情况的SCP将在不久后被释放以来辅助任务\r\n<split><b><color=#FF0000>所有人，请务必保持冷静\r\n<split> $PITCH_0.2 .G4 .G4 \r\n<split> $PITCH_1.0 $SLEEP_0.05 Warning . the O5 Council has issued the highest order to eliminate human $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 This includes everyone currently in the Light Containment Zone $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 No one is allowed to resist or attempt to escalate abnormal situations $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 Such actions will be considered treason against the SCP Foundation $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 SCP aware of the situation will soon be released to assist in the mission $SLEEP_0.5 .\r\n<split> $PITCH_1.0 $SLEEP_0.05 All personnel . please remain calm $SLEEP_0.5 .\r\n");
            });

            Log.Debug("CASSIE处理");
        }
    }
}