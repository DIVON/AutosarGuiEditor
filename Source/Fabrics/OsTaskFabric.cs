using AutosarGuiEditor.Source.Autosar.OsTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Fabrics
{
    public static class OsTaskFabric
    {
        public static OsTask CreateOsTask(String Name)
        {
            OsTask task = new OsTask();
            task.Name = Name;
            return task;
        }
    }
}
