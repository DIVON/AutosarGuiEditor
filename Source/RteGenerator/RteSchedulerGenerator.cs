using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.RteGenerator.RteOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteSchedulerGenerator
    {
        public void GenerateShedulerFiles()
        {
            Generate_ExternalRunnables_File();
            Generate_RunTimeEnvironment_Header_File();
            Generate_RunTimeEnvironment_Source_File();
            Generate_RteTaskScheduler_Header_File();
            Generate_RteTaskScheduler_Source_File();
        }

        public RteSchedulerGenerator()
        {
        }

        void Generate_RunTimeEnvironment_Source_File()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME);
            writer.WriteLine();

            writer.WriteLine("/* Scheduler variables  */");

            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                List<double> frequences = PeriodChangeTimes(osTask);
                List<String> freqVariableNames = new List<string>();

                for (int i = 0; i < frequences.Count; i++)
                {
                    String freqName = osTask.Name + "_" + RteFunctionsGenerator.CreateFrequencyDefineName(frequences[i]) + "_" + i.ToString();
                    freqVariableNames.Add(freqName);
                    String variableDeclaration = RteFunctionsGenerator.GenerateVariable(freqName, "uint32", true, 0, "0");
                    writer.WriteLine(variableDeclaration);
                }
                writer.WriteLine();
            }

            WriteAllExternComponentInstances(writer);
            /* End declare variables */

            WriteAllOsTasks(writer);

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }
#if false
        void Generate_RunTimeEnvironment_Source_File_FREERTOS()
        {
            RteOsInterfaceGenerator osGenerator = new RteFreeRtosGenerator();

            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME); 
            writer.WriteLine();

            writer.WriteLine("/* Scheduler variables  */");

            foreach(OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                List<double> frequences = PeriodChangeTimes(osTask);
                List<String> freqVariableNames = new List<string>();

                for (int i = 0; i < frequences.Count; i++)
                {
                    String freqName = osTask.Name + "_" + RteFunctionsGenerator.CreateFrequencyDefineName(frequences[i]) + "_" + i.ToString();
                    freqVariableNames.Add(freqName);
                    String variableDeclaration = RteFunctionsGenerator.GenerateVariable(freqName, "uint32", true, 0, "0");
                    writer.WriteLine(variableDeclaration);
                }
                writer.WriteLine();
            }

            /* Declare variables for scheduling */
            WriteAllTasksVariables(writer, osGenerator);

            WriteAllExternComponentInstances(writer);
            /* End declare variables */

            WriteAllOsTasks_FREERTOS(writer, osGenerator);

            WriteInitOsTasks(writer, osGenerator);

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteAllTasksVariables(StreamWriter writer, RteOsInterfaceGenerator osGenerator)
        {
            writer.WriteLine("/* Os tasks */");
            foreach (OsTask task in AutosarApplication.GetInstance().OsTasks)
            {
                String datatype = osGenerator.TaskHandleDataType();
                String taskName = GenerateRteOsTaskName(task);
                writer.WriteLine("static " + datatype + " " + taskName + ";");
            }
            writer.WriteLine();
        }
#endif
        void WriteAllExternComponentInstances(StreamWriter writer)
        {
            writer.WriteLine("/* Extern component instances  */");
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach(ComponentInstance compInstance in composition.ComponentInstances)
                {
                    writer.WriteLine("extern " + compInstance.ComponentDefenition.Name + " cin" + compInstance.Name + ";");
                }
            }
            writer.WriteLine();
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
            String osTaskName = RteFunctionsGenerator.GenerateRteOsTaskFunctionName(osTask);
            writer.WriteLine("void " + osTaskName + "(void)");
            writer.WriteLine("{");
            WriteCallOfOsRunnables(writer, osTask);
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void WriteCallOfOsRunnables(StreamWriter writer, OsTask osTask)
        {
            if (osTask.Runnables.Count > 0)
            {
                double lastPeriod = -1;
                int changeIndex = 0;
                bool wasBracersOpen = false;
                for (int runnableIndex = 0; runnableIndex < osTask.Runnables.Count; runnableIndex++)
                {                
                    PeriodicRunnableDefenition runnableDefenition = AutosarApplication.GetInstance().FindRunnableDefenition(osTask.Runnables[runnableIndex].DefenitionGuid);
                    if ((runnableDefenition != null) && (osTask.PeriodMs != runnableDefenition.PeriodMs))
                    {
                        /* Close previous period */
                        if ((lastPeriod != runnableDefenition.PeriodMs) && wasBracersOpen)
                        {
                            writer.WriteLine("    }");
                        }

                        /* Add open bracers */
                        if  (lastPeriod != runnableDefenition.PeriodMs)
                        {
                            lastPeriod = runnableDefenition.PeriodMs;
                            String periodVariableName = osTask.Name + "_" + RteFunctionsGenerator.CreateFrequencyDefineName(runnableDefenition.PeriodMs) + "_" + changeIndex.ToString();
                            int ostatok = (int)(runnableDefenition.PeriodMs / osTask.PeriodMs);
                            changeIndex++;
                            writer.WriteLine("    if (++" + periodVariableName + " >= " + ostatok + ")");
                            writer.WriteLine("    {");
                            writer.WriteLine("        " + periodVariableName + " = 0u;" );
                        }
                       
                        wasBracersOpen = true;                       
                        writer.WriteLine("        " + RteFunctionsGenerator.Generate_CallOfRunnable(osTask.Runnables[runnableIndex]));
                    }
                    else
                    {
                        if (wasBracersOpen)
                        {
                            wasBracersOpen = false;
                            writer.WriteLine("    }");
                        }
                        lastPeriod = osTask.PeriodMs;
                        writer.WriteLine("    " + RteFunctionsGenerator.Generate_CallOfRunnable(osTask.Runnables[runnableIndex]));
                    }

                }

                if (wasBracersOpen)
                {
                    writer.WriteLine("    }");
                }
            }
        }

#if false
        void Generate_RunTimeEnvironment_Header_File_FREERTOS()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, "<FreeRTOS.h>");
            RteFunctionsGenerator.AddInclude(writer, "<task.h>");
            RteFunctionsGenerator.AddInclude(writer, "<stm32f4xx.h>");
            writer.WriteLine();

            writer.WriteLine("/* Time periods */");
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("SYSTICK_FREQUENCY", AutosarApplication.GetInstance().SystickFrequencyHz.ToString()));
            writer.WriteLine();

            



            /* Create defines for each frequency */
            List<double> frequencys = GetDifferentFrequences();
            foreach(double frequency in frequencys)
            {
                String freqName = RteFunctionsGenerator.CreateFrequencyDefineName(frequency);
                
                String freqValue = "";

                int freq = (int)Math.Floor((double)AutosarApplication.GetInstance().SystickFrequencyHz / frequency);
                freqValue = freq.ToString();
                writer.WriteLine(RteFunctionsGenerator.CreateDefine(freqName, freqValue.ToString()));
            }
            writer.WriteLine();

            writer.WriteLine("/*");
            writer.WriteLine(" * Init os tasks.");
            writer.WriteLine(" */");
            writer.WriteLine("void " + InitOsFunctionName() + "(void);");

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }
#endif
        void Generate_RunTimeEnvironment_Header_File()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME);
            switch (AutosarApplication.GetInstance().MCUType.Type)
            {
                case MCUTypeDef.STM32F1xx:
                {
                    RteFunctionsGenerator.AddInclude(writer, "<stm32f1xx.h>");
                    break;
                }
                case MCUTypeDef.STM32F4xx:
                {
                    RteFunctionsGenerator.AddInclude(writer, "<stm32f4xx.h>");
                    break;
                }
            }
            writer.WriteLine();

            writer.WriteLine("/* Time periods */");
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("SYSTICK_FREQUENCY", AutosarApplication.GetInstance().SystickFrequencyHz.ToString()));
            writer.WriteLine();


            /* Create defines for each frequency */
            List<double> frequencys = GetDifferentFrequences();
            foreach (double frequency in frequencys)
            {
                String freqName = RteFunctionsGenerator.CreateFrequencyDefineName(frequency);

                String freqValue = "";

                int freq = (int)Math.Floor((double)AutosarApplication.GetInstance().SystickFrequencyHz / frequency);
                freqValue = freq.ToString();
                writer.WriteLine(RteFunctionsGenerator.CreateDefine(freqName, freqValue.ToString()));
            }
            writer.WriteLine();

            writer.WriteLine("/*");
            writer.WriteLine(" * Rte tasks ");
            writer.WriteLine(" */");

            foreach (OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                String osTaskName = RteFunctionsGenerator.GenerateRteOsTaskFunctionName(osTask);
                writer.WriteLine("void " + osTaskName + "(void);");
            }

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void Generate_ExternalRunnables_File()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_EXTERNAL_RUNNABLES_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            writer.WriteLine("/* Declaration of all periodic runnables */");
            writer.WriteLine();
            foreach (ComponentDefenition compDefinition in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                foreach (PeriodicRunnableDefenition runnable in compDefinition.Runnables)
                {
                    writer.WriteLine(RteFunctionsGenerator.Generate_RunnableFunction(compDefinition, runnable) + ";");
                }
            }

            writer.WriteLine();
            writer.WriteLine("/* Declaration of all server call functions */");
            writer.WriteLine();
            foreach (ComponentDefenition componentDefenition in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                foreach (PortDefenition port in componentDefenition.Ports)
                {
                    if (port.PortType == PortType.Server)
                    {
                        ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(port.InterfaceGUID);
                        if (csInterface != null)
                        {
                            foreach (ClientServerOperation operation in csInterface.Operations)
                            {
                                String funcName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(componentDefenition, port, operation);
                                String funcArguments = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, componentDefenition.MultipleInstantiation);
                                writer.WriteLine(Properties.Resources.STD_RETURN_TYPE + funcName + funcArguments + ";");
                            }
                        }
                    }                    
                }
            }

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();

        }

        List<double>  PeriodChangeTimes(OsTask task)
        {
            List<double> periods = new List<double>();
            RunnableInstancesList runnables = task.Runnables;
            double prevPeriod = -1;

            for (int i = 0; i < runnables.Count; i++)
            {
                /* Check if period of runnable is other then the task's period */
                if (runnables[i].Defenition.PeriodMs != task.PeriodMs)
                {
                    if (runnables[i].Defenition.PeriodMs != prevPeriod)
                    {
                        prevPeriod = runnables[i].Defenition.PeriodMs;
                        periods.Add(runnables[i].Defenition.PeriodMs);
                    }
                }
            }

            return periods;
        }

        List<double> GetDifferentFrequences()
        {
            List<double> frequences = new List<double>();
            RunnableInstancesList runnables = AutosarApplication.GetInstance().GetAllRunnablesOrderedByStartup();
            
            foreach (PeriodicRunnableInstance runnable in runnables)
            {
                PeriodicRunnableDefenition runnableDefenition = AutosarApplication.GetInstance().FindRunnableDefenition(runnables[0].DefenitionGuid);
                if (!frequences.Exists(x => x == runnableDefenition.PeriodMs))
                {
                    frequences.Add(runnableDefenition.PeriodMs);
                }                    
            }
            
            return frequences;
        }

        void Generate_RteTaskScheduler_Header_File()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_TASK_SCHEDULER_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_TASK_SCHEDULER_FILE_DESCRIPTION);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);

            writer.WriteLine();

            writer.WriteLine("extern volatile boolean timeEventOccured;");

            writer.WriteLine();

            writer.WriteLine("void DoScheduling(void);");

            writer.WriteLine();
            RteFunctionsGenerator.CloseGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void Generate_RteTaskScheduler_Source_File()
        {
            String FileName = RteFunctionsGenerator.GetRteFolder() + "\\" + Properties.Resources.RTE_TASK_SCHEDULER_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);

            RteFunctionsGenerator.GenerateFileTitle(writer, FileName, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_FILE_DESCRIPTION);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_RUNTIME_ENVIRONMENT_H_FILENAME);
            writer.WriteLine();

            int tasksCount = AutosarApplication.GetInstance().OsTasks.Count;
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("RTE_TASKS_COUNT", tasksCount.ToString(), false));

            int stepsCount = AutosarApplication.GetInstance().OsTasks.GetSchedulerNecessaryStepsCount();

            writer.WriteLine();
            writer.WriteLine(RteFunctionsGenerator.CreateDefine("RTE_SCHEDULER_STEPS", stepsCount.ToString(), false));

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

            String htim = "Not defined!";
           
            switch (AutosarApplication.GetInstance().MCUType.Type)
            {
                case MCUTypeDef.STM32F1xx:
                {
                    htim = "htim4";
                    break;
                }
                case MCUTypeDef.STM32F4xx:
                {
                    htim = "htim13";
                    break;
                }
            }

            writer.WriteLine("extern TIM_HandleTypeDef " + htim + ";");


            writer.WriteLine();
            writer.WriteLine("static Rte_Scheduler_Sequence  taskScheduling =");
            writer.WriteLine("{");
            /* Sort tasks by priority is necessary */
            AutosarApplication.GetInstance().OsTasks.DoSort();
            double systickfreq = AutosarApplication.GetInstance().SystickFrequencyHz;
            int schedulerStepMicrosec = (int)((1000.0d / systickfreq) * 1000.0);
            
            for (int i = 0; i < stepsCount; i++)
            {
                writer.WriteLine("    {");
                int writtenFunctions = 0;
                for (int j = 0; j < tasksCount; j++)
                {
                    OsTask task = AutosarApplication.GetInstance().OsTasks[j];
                    bool includeCondition = true;

                    int TaskMicrosec = (int)(task.PeriodMs * 1000);

                    int ost = schedulerStepMicrosec * i % TaskMicrosec;
                    includeCondition = (ost == 0);
                    if (includeCondition)
                    {
                         String osTaskName = RteFunctionsGenerator.GenerateRteOsTaskFunctionName(task);
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
            writer.WriteLine("    if (__HAL_TIM_GET_FLAG(&" + htim + ", TIM_FLAG_UPDATE) != RESET)");
            writer.WriteLine("    {");
            writer.WriteLine("        __HAL_TIM_CLEAR_FLAG(&" + htim + ", TIM_FLAG_UPDATE);");
            writer.WriteLine("        uint32 index = schedulingCounter % RTE_SCHEDULER_STEPS;");
            writer.WriteLine("        for (uint32 i = 0; i < RTE_TASKS_COUNT; i++)");
            writer.WriteLine("        {");
            writer.WriteLine("            if (NULL != taskScheduling[index][i])");
            writer.WriteLine("            {");
            writer.WriteLine("                taskScheduling[index][i]();");
            writer.WriteLine("            }");
            writer.WriteLine("            else");
            writer.WriteLine("            {");
            writer.WriteLine("                break;");
            writer.WriteLine("            }");
            writer.WriteLine("        }");
            writer.WriteLine("        schedulingCounter++;");
            writer.WriteLine("    }");
            writer.WriteLine("}");


            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }
    }
}
