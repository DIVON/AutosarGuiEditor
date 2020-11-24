using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.RteOs
{
    public class RteFreeRtosGenerator: RteOsInterfaceGenerator
    {
        string RteOsInterfaceGenerator.DelayUntil()
        {
            return "vTaskDelayUntil";
        }


        string RteOsInterfaceGenerator.TickDataType()
        {
            return "TickType_t";
        }


        string RteOsInterfaceGenerator.MillisecondsToTicks()
        {
            return "pdMS_TO_TICKS";
        }


        string RteOsInterfaceGenerator.GetTickCount()
        {
            return "xTaskGetTickCount()";
        }


        string RteOsInterfaceGenerator.CreateTask()
        {
            return "xTaskCreate";
        }


        string RteOsInterfaceGenerator.TaskHandleDataType()
        {
            return "TaskHandle_t";
        }
    }
}
