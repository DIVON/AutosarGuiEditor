using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Autosar.OsTasks
{
    public class OsTasksList:IGuidList<OsTask>
    {
        public void Reenumerate()
        {
            for(int i = 0; i < this.Count; i++)
            {
                this[i].Number = (i + 1);
            }
        }

        public override void DoSort()
        {
            Sort(delegate(OsTask x, OsTask y)
            {
                object namePropertyX = GetProperty(x, "Priority");
                object namePropertyY = GetProperty(y, "Priority");
                if ((namePropertyX != null) && (namePropertyY != null))
                {
                    string xName = namePropertyX.ToString();
                    string yName = namePropertyY.ToString();

                    if (xName == null && yName == null) return 0;
                    else if (xName == null) return -1;
                    else if (yName == null) return 1;
                    else return -xName.CompareTo(yName);
                }

                throw new Exception("Sort exception! Properties not exists!");
            });
        }

        public override String GetName()
        {
            return "OsTasks";
        }

        public int GetSchedulerNecessaryStepsCount()
        {
            double minPeriod = double.MaxValue;
            double maxPeriod = 0;

            foreach(OsTask task in this)
            {
                if (minPeriod > task.PeriodMs)
                {
                    minPeriod = task.PeriodMs;
                }
                if (maxPeriod < task.PeriodMs)
                {
                    maxPeriod = task.PeriodMs;
                }
            }

            int count = 0;
            if ((maxPeriod != 0) && (minPeriod != 0))
            {
                count = (int)(maxPeriod / minPeriod);
            }
            return count;
        }
    }
}
