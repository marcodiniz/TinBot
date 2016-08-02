using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinBot.Operations
{
    static class PhoneState
    {
        public static bool IsStandByOn { get; set; } = false;

        public static event EventHandler OnStandByOn;
        public static event EventHandler OnStandByOff;

        public static void SetStandByOn()
        {
            if(IsStandByOn) return;

            IsStandByOn = true;
            OnStandByOn?.Invoke(null,EventArgs.Empty);
        }

        public static void SetStandByOff()
        {
            if(!IsStandByOn) return;

            IsStandByOn = false;
            OnStandByOff?.Invoke(null, EventArgs.Empty);
        }
    }
}
