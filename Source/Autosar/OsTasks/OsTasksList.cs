﻿using AutosarGuiEditor.Source.Interfaces;
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

        int gcd(int a, int b)
        {
            if (b == 0)
            {
                return a;
            }
            else
            {
                return gcd(b, a % b);
            }
        }
 
        // Returns Least Common Multiply of array elements
        int findlcm(List<int> arr)
        {
            // Initialize result
            if (arr.Count > 0)
            {
                int ans = arr[0];

                // ans contains LCM of arr[0], ..arr[i]
                // after i'th iteration,
                for (int i = 1; i < arr.Count; i++)
                {
                    ans = (((arr[i] * ans)) / (gcd(arr[i], ans)));
                }

                return ans;
            }
            else
            {
                return 0;
            }
            
        }

        public int GetSchedulerNecessaryStepsCount(double schedulerPeriodUs)
        {
            List<int> schedulerTaskSteps = new List<int>();
            OsTask initTask = GetInitTask();
            foreach(OsTask task in this)
            {
                if (task != initTask)
                {
                    schedulerTaskSteps.Add(Convert.ToInt32(task.PeriodMs * 1000 / schedulerPeriodUs));
                }
            }

            int stepsPeriod = findlcm(schedulerTaskSteps);
            return stepsPeriod;
        }

        public OsTask GetInitTask()
        {
            return GetTaskByName("Init");
        }

        public OsTasksList GetZeroFrequencyTasks()
        {
            OsTasksList list = new OsTasksList();
            foreach (OsTask task in this)
            {
                if (task.PeriodMs == 0)
                {
                    list.Add(task);
                }
            }
            return list;
        }

        public OsTasksList GetTasksWithNonZeroFrequency()
        {
            OsTasksList list = new OsTasksList();
            foreach (OsTask task in this)
            {
                if (task.PeriodMs != 0)
                {
                    list.Add(task);
                }
            }
            return list;
        }

        public OsTask GetTaskByName(String taskName)
        {
            foreach (OsTask task in this)
            {
                if (task.Name == taskName)
                {
                    return task;
                }
            }
            return null;
        }
    }
}
