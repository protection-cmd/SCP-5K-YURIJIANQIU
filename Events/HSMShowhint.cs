using Exiled.API.Features;
using HintServiceMeow.Core.Enum;
using MEC;
using System;

namespace SCP5K.Events
{
    public class HSMShowhint
    {
        public static void HsmShowHint(Player player,String Message,int YCoordinate,int XCoordinate,float Duration,string Id, HintServiceMeow.Core.Enum.HintAlignment HintAlignment=HintAlignment.Center)
        {
            if (player == null || !player.IsConnected) return;

            var display = HintServiceMeow.Core.Utilities.PlayerDisplay.Get(player);
            if (display == null) return;

            display.RemoveHint(Id); 

            var hint = new HintServiceMeow.Core.Models.Hints.Hint
            {
                Text = Message,
                YCoordinate = YCoordinate,
                XCoordinate = XCoordinate,
                Alignment = HintAlignment,
                Id = Id
            };

            display.AddHint(hint);

            Timing.CallDelayed(Duration, () =>
            {
                try
                {
                    display.RemoveHint(hint);
                }
                catch (Exception ex)
                {
                    Log.Error($"清除hint时出错: {ex.Message}");
                }
            });


        }
    }
}
