using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Utility;
using System;
using System.IO;
using System.Windows;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteSchedulerGenerator_Cpp
    {
        public void GenerateShedulerFiles(String dir)
        {
            Generate_RteExternalHeader_File(dir);
            Generate_RunTimeEnvironment_Header_File(dir);
            Generate_RunTimeEnvironment_Source_File(dir);
            Generate_RteTaskScheduler_Header_File(dir);
            Generate_RteTaskScheduler_Source_File(dir);
        }

        public RteSchedulerGenerator_Cpp()
        {
        }

        void Generate_RunTimeEnvironment_Source_File(String dir)
        {
            String FileName = dir + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME);
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_EXTERNALS_FILENAME);

            writer.WriteLine();

            writer.WriteLine("/* Scheduler variables  */");

            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                if (osTask.PeriodMs > 0)
                {
                    String taskCounter = RteFunctionsGenerator_Cpp.CreateTaskCounter(osTask.Name);
                    String defString = "STATIC uint32 " + taskCounter + ";";
                    writer.WriteLine(defString);
                }
            }
            writer.WriteLine();

            WriteAllExternComponentInstances(writer);
            /* End declare variables */

            WriteAllOsTasks(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteAllExternComponentInstances(StreamWriter writer)
        {
            return;

            //writer.WriteLine("/* Extern component instances  */");
            //foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            //{
            //    foreach(ComponentInstance compInstance in composition.ComponentInstances)
            //    {
            //        writer.WriteLine("extern " + compInstance.ComponentDefenition.Name + " cin" + compInstance.Name + ";");
            //    }
            //}
            //writer.WriteLine();
        }

#if false
        String GenerateRteOsTaskName(OsTask task)
        {
            return "Rte_osTask_" + task.Name;
        }


        void WriteInitOsTasks_FREERTOS(StreamWriter writer, RteOsInterfaceGenerator osGenerator)
        {
            writer.WriteLine("void " + InitOsFunctionName() + "(void)");
            writer.WriteLine("{");
            foreach(OsTask task in AutosarApplication.GetInstance().OsTasks)
            {
                writeInitOsTask(writer, task, osGenerator);
            }
            writer.WriteLine("}");
        }

        void writeInitOsTask(StreamWriter writer, OsTask task, RteOsInterfaceGenerator osGenerator)
        {            
            String rteTaskName = GenerateRteOsTaskName(task);
            String funcName = RteFunctionsGenerator.GenerateRteOsTaskFunctionName(task);
            String stackSize = (task.StackSizeInBytes / 2).ToString();
            writer.WriteLine("    " + osGenerator.CreateTask() + "(");
            writer.WriteLine("        " + funcName + ", /* Pointer to the function that implements the task. */");
            writer.WriteLine("        \"" + task.Name + "\", /* Text name given to the task. */");
            writer.WriteLine("        " + stackSize + ", /* The size of the stack in words */");
            writer.WriteLine("        (void*)0, /* A reference to xParameters is used as the task parameter. */");
            writer.WriteLine("        " + task.Priority + ", /* Task priority */");
            writer.WriteLine("        &" + rteTaskName + "); /* The handle to the task being created will be placed there */");
            writer.WriteLine();
        }

        String InitOsFunctionName()
        {
            return "Rte_InitOs";
        }

        void WriteAllOsTasks_FREERTOS(StreamWriter writer, RteOsInterfaceGenerator osGenerator)
        {
            foreach(OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                GenerateOsTaskFunctionCallOfRunnables_FREERTOS(writer, osTask, osGenerator);
            }
        }

        void GenerateOsTaskFunctionCallOfRunnables_FREERTOS(StreamWriter writer, OsTask osTask, RteOsInterfaceGenerator osGenerator)
        {
            String osTaskName = RteFunctionsGenerator.GenerateRteOsTaskFunctionName(osTask);
            writer.WriteLine("static void " + osTaskName + "(void * pvParameters)");
            writer.WriteLine("{");
            String lastTickVariableName = "xLastWakeTime";
            String xPeriodVariableName = "xPeriod";
            writer.WriteLine("    " + osGenerator.TickDataType() + " " + lastTickVariableName + ";");
            double sysTickFreq = AutosarApplication.GetInstance().SystickFrequencyHz;
            double delayValue = (double)(osTask.PeriodMs * sysTickFreq / 1000.0d);
            writer.WriteLine("    const " + osGenerator.TickDataType() + " " + xPeriodVariableName + " = " + ((int)(osTask.PeriodMs * 1000.0f)).ToString() + "u * " + ((int)sysTickFreq).ToString() + "u / 1000000u;");
            writer.WriteLine("    " + lastTickVariableName + " = " + osGenerator.GetTickCount() + ";");
            writer.WriteLine("    while(1)");
            writer.WriteLine("    {");
            WriteCallOfOsRunnables(writer, osTask);
            writer.WriteLine("        " + osGenerator.DelayUntil() + "(&" + lastTickVariableName + ", " + xPeriodVariableName + ");");
            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.WriteLine("");
        }
#endif
        void WriteAllOsTasks(StreamWriter writer)
        {
            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                GenerateOsTaskFunctionCallOfRunnables(writer, osTask);
            }
        }

        void GenerateOsTaskFunctionCallOfRunnables(StreamWriter writer, OsTask osTask)
        {
            String osTaskName = RteFunctionsGenerator_Cpp.GenerateRteOsTaskFunctionName(osTask);
            writer.WriteLine("void " + osTaskName + "(void)");
            writer.WriteLine("{");
            WriteCallOfOsRunnables(writer, osTask);
            if (osTask.PeriodMs > 0)
            {
                int maxPeriod = LowestCommonPeriodOfTaskssRunnables(osTask);
                if (maxPeriod != 0)
                {
                    String taskCounter = RteFunctionsGenerator_Cpp.CreateTaskCounter(osTask.Name);
                    writer.WriteLine("    " + taskCounter + " = (" + taskCounter + " + 1U) % (" + maxPeriod.ToString() + " / " + Convert.ToInt32(osTask.PeriodMs * 1000) + ");");
                }
            }
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        int LowestCommonPeriodOfTaskssRunnables(OsTask task)
        {
            if (task.Events.Count > 0)
            {
                int naimensheeObheeKratnoe = 0;

                /* Find first periodical event */
                int startIndex;
                for (startIndex = 0; startIndex < task.Events.Count; startIndex++)
                {
                    if (task.Events[startIndex].Defenition is TimingEvent)
                    {
                        TimingEvent timingEvent = task.Events[startIndex].Defenition as TimingEvent;
                        naimensheeObheeKratnoe = Convert.ToInt32(timingEvent.PeriodMs * 1000);
                        break;
                    }
                }

                for (int i = startIndex; i < task.Events.Count; i++)
                {
                    if (task.Events[i].Defenition is TimingEvent)
                    {
                        TimingEvent timingEvent = task.Events[i].Defenition as TimingEvent;
                        naimensheeObheeKratnoe = MathUtility.NaimensheeObsheeKratnoe(naimensheeObheeKratnoe, Convert.ToInt32(timingEvent.PeriodMs * 1000));
                        if (naimensheeObheeKratnoe == 0)
                        {
                            MessageBox.Show("There is no common frequency for runnables");
                            break;
                        }
                    }
                }
                return naimensheeObheeKratnoe;
            }
            else
            {
                return 0;
            }
        }

        void WriteCallOfOsRunnables(StreamWriter writer, OsTask osTask)
        {
            if (osTask.Events.Count > 0)
            {
                double lastPeriod = -1;
                bool wasBracersOpen = false;
                for (int eventIndex = 0; eventIndex < osTask.Events.Count; eventIndex++)
                {            

                    AutosarEvent autosarEventDefenition = osTask.Events[eventIndex].Defenition;
                    if (autosarEventDefenition is TimingEvent)
                    {
                        WritePeriodicalEvent(writer, ref osTask, osTask.Events[eventIndex], ref wasBracersOpen, ref lastPeriod);
                    }
                    if (autosarEventDefenition is OneTimeEvent)
                    {
                        WriteOneTimeEvent(writer, osTask.Events[eventIndex]);
                    }
                    if (autosarEventDefenition is ClientServerEvent)
                    {
                        if (wasBracersOpen)
                        {
                            writer.WriteLine("    }");
                            wasBracersOpen = false;
                        }

                        WriteAsyncClientServerEvent(writer, osTask.Events[eventIndex], autosarEventDefenition as ClientServerEvent);                        

                        lastPeriod = -1;
                    }
                }

                if (wasBracersOpen)
                {
                    writer.WriteLine("    }");
                    wasBracersOpen = false;
                }
            }
        }


        void WritePeriodicalEvent(StreamWriter writer, ref OsTask osTask, AutosarEventInstance eventInstance, ref bool wasBracersOpen, ref double lastPeriod)
        {
            String taskCounter = RteFunctionsGenerator_Cpp.CreateTaskCounter(osTask.Name);

            TimingEvent timingEvent = eventInstance.Defenition as TimingEvent;

            RunnableDefenition runnableDefenition = timingEvent.Runnable;

            int runnablePeriod = Convert.ToInt32(timingEvent.PeriodMs * 1000);
            int taskPeriod = Convert.ToInt32(osTask.PeriodMs * 1000);

            if ((runnableDefenition != null) && (osTask.PeriodMs != timingEvent.PeriodMs))
            {
                /* Add open bracers */
                if (lastPeriod != timingEvent.PeriodMs)
                {
                    /* Close previous period */
                    if (wasBracersOpen)
                    {
                        writer.WriteLine("    }");
                    }
                    lastPeriod = timingEvent.PeriodMs;

                    writer.WriteLine("    if (" + taskCounter + " % (" + runnablePeriod.ToString() + "U / " + taskPeriod.ToString() + "U) == (0U / " + taskPeriod.ToString() + "))");
                    writer.WriteLine("    {");
                }
                       
                wasBracersOpen = true;
                writer.WriteLine("        " + RteFunctionsGenerator_Cpp.Generate_CallOfEvent(eventInstance));
            }
            else
            {
                if (wasBracersOpen)
                {
                    wasBracersOpen = false;
                    writer.WriteLine("    }");
                }
                lastPeriod = osTask.PeriodMs;
                writer.WriteLine("    " + RteFunctionsGenerator_Cpp.Generate_CallOfEvent(eventInstance));
            }
        }

        void WriteAsyncClientServerEvent(StreamWriter writer, AutosarEventInstance eventInstance, ClientServerEvent eventDefenition)
        {
            ComponentInstance compInstance = AutosarApplication.GetInstance().FindComponentInstanceByEventId(eventInstance.GUID);
            

            if (compInstance != null)
            {
                String asyncField = "Rte_AsyncCall_" + compInstance.Name + "_" + eventDefenition.SourcePort.Name + "_" + eventDefenition.SourceOperation.Name;
                writer.WriteLine("    if (TRUE == " + asyncField + ")");
                writer.WriteLine("    {");
                writer.WriteLine("        " + RteFunctionsGenerator_Cpp.Generate_CallOfEvent(eventInstance));
                writer.WriteLine("        " + asyncField + " = FALSE;");
                writer.WriteLine("    }");
            }
            else
            {
                writer.WriteLine("    DEBUG is needed! Compo");
            }
        }

        void WriteOneTimeEvent(StreamWriter writer, AutosarEventInstance eventInstance)
        {
            writer.WriteLine("    " + RteFunctionsGenerator_Cpp.Generate_CallOfEvent(eventInstance));
        }

        void Generate_RunTimeEnvironment_Header_File(String dir)
        {
            String FileName = dir + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            writer.WriteLine();
            writer.WriteLine("#define RTE_C");
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + compDef.Name + ".h");
            }
            writer.WriteLine("#undef RTE_C");

            writer.WriteLine();

            writer.WriteLine("/* Time periods */");
            writer.WriteLine(RteFunctionsGenerator_Cpp.CreateDefine("SYSTICK_FREQUENCY", AutosarApplication.GetInstance().SystickFrequencyHz.ToString()));
            writer.WriteLine();


            /* Create defines for each frequency */
            /* Does it really needed ? */
            /*List<double> frequencys = GetDifferentFrequences();
            foreach (double frequency in frequencys)
            {
                String freqName = RteFunctionsGenerator.CreateFrequencyDefineName(frequency);

                String freqValue = "";

                if (frequency != 0u)
                {
                    int freq = (int)Math.Floor((double)AutosarApplication.GetInstance().SystickFrequencyHz / frequency);
                    freqValue = freq.ToString();
                    writer.WriteLine(RteFunctionsGenerator.CreateDefine(freqName, freqValue.ToString()));
                }
            }
            writer.WriteLine(); */

            writer.WriteLine("/*");
            writer.WriteLine(" * Rte tasks ");
            writer.WriteLine(" */");

            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                String osTaskName = RteFunctionsGenerator_Cpp.GenerateRteOsTaskFunctionName(osTask);
                writer.WriteLine("void " + osTaskName + "(void);");
            }

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        public void Generate_ExternalRunnables_File(string folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_EXTERNAL_RUNNABLES_FILE_DESCRIPTION);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);

            writer.WriteLine();
            writer.WriteLine("/* Declaration of all component's runnables */");
            writer.WriteLine();
            foreach (ApplicationSwComponentType compDefinition in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                foreach (RunnableDefenition runnable in compDefinition.Runnables)
                {
                    writer.WriteLine(RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(compDefinition, runnable, true) + ";");
                }
            }

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();

        }

        public void Generate_RteExternalHeader_File(string folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_EXTERNALS_FILENAME;
             StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, "This file contains all externals required for scheduling");
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);

            writer.WriteLine();
            writer.WriteLine("/* Declaration of all async events  */");
            writer.WriteLine();

            RteConnectionGenerator_Cpp.GenerateAllAsyncServerNotificators(writer, true);

            writer.WriteLine("/* Declaration of all async events  */");
            writer.WriteLine();

            RteConnectionGenerator_Cpp.GenerateExternComponentInstances(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();

        }

        void Generate_RteTaskScheduler_Header_File(String dir)
        {
            String FileName = dir + "\\" + Properties.Resources.RTE_TASK_SCHEDULER_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_TASK_SCHEDULER_FILE_DESCRIPTION);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);

            writer.WriteLine();

            writer.WriteLine("extern volatile boolean timeEventOccured;");

            writer.WriteLine();

            writer.WriteLine("void DoScheduling(void);");

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        void Generate_RteTaskScheduler_Source_File(String dir)
        {
            double systickfreq = AutosarApplication.GetInstance().SystickFrequencyHz;
            int schedulerStepMicrosec = (int)((1000.0d / systickfreq) * 1000.0);

            String FileName = dir + "\\" + Properties.Resources.RTE_TASK_SCHEDULER_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);

            writer.WriteLine();
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME);
            writer.WriteLine();

            
            OsTask initTask = AutosarApplication.GetInstance().OsTasks.GetInitTask();

            if (initTask != null)
            {
                AutosarApplication.GetInstance().OsTasks.Remove(initTask);
            }

            int tasksCount = AutosarApplication.GetInstance().OsTasks.Count;

            writer.WriteLine(RteFunctionsGenerator_Cpp.CreateDefine("RTE_TASKS_COUNT", tasksCount.ToString(), false));

            int stepsCount = AutosarApplication.GetInstance().OsTasks.GetSchedulerNecessaryStepsCount(schedulerStepMicrosec);

            writer.WriteLine();
            writer.WriteLine(RteFunctionsGenerator_Cpp.CreateDefine("RTE_SCHEDULER_STEPS", stepsCount.ToString(), false));

            writer.WriteLine();
            writer.WriteLine("/* One Rte Task pointer */");
            writer.WriteLine("typedef void (*Rte_Scheduler_Task)();");

            writer.WriteLine();
            writer.WriteLine("/* One step of scheduler */");
            writer.WriteLine("typedef  Rte_Scheduler_Task Rte_Scheduler_Step[RTE_TASKS_COUNT];");

            writer.WriteLine();
            writer.WriteLine("/* All possible steps in scheduler */");
            writer.WriteLine("typedef  Rte_Scheduler_Step Rte_Scheduler_Sequence[RTE_SCHEDULER_STEPS];");

            writer.WriteLine();
            writer.WriteLine("static uint32 schedulingCounter = 0u;");

            writer.WriteLine();

            writer.WriteLine();
            writer.WriteLine("static const Rte_Scheduler_Sequence  taskScheduling =");
            writer.WriteLine("{");
            /* Sort tasks by priority is necessary */
            AutosarApplication.GetInstance().OsTasks.DoSort();
            
            
            
            for (int i = 0; i < stepsCount; i++)
            {
                writer.WriteLine("    {");
                int writtenFunctions = 0;
                for (int j = 0; j < tasksCount; j++)
                {
                    OsTask task = AutosarApplication.GetInstance().OsTasks[j];
                    if (task.Name != "Init")
                    {
                        bool includeCondition = true;

                        int TaskMicrosec = (int)(task.PeriodMs * 1000);

                        int ost = schedulerStepMicrosec * i % TaskMicrosec;
                        includeCondition = (ost == 0);
                        if (includeCondition)
                        {
                            String osTaskName = RteFunctionsGenerator_Cpp.GenerateRteOsTaskFunctionName(task);
                            writer.Write("        " + osTaskName);
                            if (writtenFunctions < tasksCount - 1)
                            {
                                writer.WriteLine(",");
                            }
                            else
                            {
                                writer.WriteLine();
                            }
                            writtenFunctions++;
                        }
                    }
                }

                /* Write necessary NULLs */
                for (int j = writtenFunctions; j < tasksCount; j++)
                {
                    writer.Write("        NULL");
                    if (j < tasksCount - 1)
                    {
                        writer.WriteLine(",");
                    }
                    else
                    {
                        writer.WriteLine();
                    }
                }
                writer.Write("    }");
                if (i < stepsCount - 1)
                {
                    writer.WriteLine(",");
                }
                else
                {
                    writer.WriteLine();
                }
            }
            writer.WriteLine("};");
            writer.WriteLine();

            /* Writing DoScheduling function */
            writer.WriteLine("void DoScheduling(void)");
            writer.WriteLine("{");
            writer.WriteLine("    uint32 index = schedulingCounter % RTE_SCHEDULER_STEPS;");
            writer.WriteLine("    for (uint32 i = 0; i < RTE_TASKS_COUNT; i++)");
            writer.WriteLine("    {");
            writer.WriteLine("        if (((void *)0) != taskScheduling[index][i])");
            writer.WriteLine("        {");
            writer.WriteLine("            taskScheduling[index][i]();");
            writer.WriteLine("        }");
            writer.WriteLine("        else");
            writer.WriteLine("        {");
            writer.WriteLine("            break;");
            writer.WriteLine("        }");
            writer.WriteLine("    }");
            writer.WriteLine("    schedulingCounter++;");
            writer.WriteLine("}");


            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();

            if (initTask != null)
            {
                AutosarApplication.GetInstance().OsTasks.Add(initTask);
            }
        }
    }
}
