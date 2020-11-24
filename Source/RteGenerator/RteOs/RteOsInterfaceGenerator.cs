using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    interface RteOsInterfaceGenerator
    {
        String DelayUntil();
        String TickDataType();
        String MillisecondsToTicks();
        String GetTickCount();
        String CreateTask();
        String TaskHandleDataType();
    }
}
