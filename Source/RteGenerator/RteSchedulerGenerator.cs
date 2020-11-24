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
        }

        public RteSchedulerGenerator()
        {
        }

        void Generate_RunTimeEnvironment_Source_File()
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

            WriteAllOsTasks(writer, osGenerator);

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

        String GenerateRteOsTaskName(OsTask task)
        {
            return "Rte_osTask_" + task.Name;
        }

        void WriteInitOsTasks(StreamWriter writer, RteOsInterfaceGenerator osGenerator)
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

        void WriteAllOsTasks(StreamWriter writer, RteOsInterfaceGenerator osGenerator)
        {
            foreach(OsTask osTask in AutosarApplication.GetInstance().OsTasks)
            {
                GenerateOsTaskFunctionCallOfRunnables(writer, osTask, osGenerator);
            }
        }
        
        void GenerateOsTaskFunctionCallOfRunnables(StreamWriter writer, OsTask osTask, RteOsInterfaceGenerator osGenerator)
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
                    if (osTask.PeriodMs != runnableDefenition.PeriodMs)
                    {
                        /* Close previous period */
                        if ((lastPeriod != runnableDefenition.PeriodMs) && wasBracersOpen)
                        {
                            writer.WriteLine("        }");
                        }

                        /* Add open bracers */
                        if  (lastPeriod != runnableDefenition.PeriodMs)
                        {
                            lastPeriod = runnableDefenition.PeriodMs;
                            String periodVariableName = osTask.Name + "_" + RteFunctionsGenerator.CreateFrequencyDefineName(runnableDefenition.PeriodMs) + "_" + changeIndex.ToString();
                            int ostatok = (int)(runnableDefenition.PeriodMs / osTask.PeriodMs);
                            changeIndex++;
                            writer.WriteLine("        if (++" + periodVariableName + " >= " + ostatok + ")");
                            writer.WriteLine("        {");
                            writer.WriteLine("            " + periodVariableName + " = 0;" );
                        }

                        lastPeriod = runnableDefenition.PeriodMs;
                       
                        wasBracersOpen = true;                       
                        writer.WriteLine("            " + RteFunctionsGenerator.Generate_CallOfRunnable(osTask.Runnables[runnableIndex]));
                    }
                    else
                    {
                        if (wasBracersOpen)
                        {
                            wasBracersOpen = false;
                            writer.WriteLine("        }");
                        }
                        lastPeriod = osTask.PeriodMs;
                        writer.WriteLine("        " + RteFunctionsGenerator.Generate_CallOfRunnable(osTask.Runnables[runnableIndex]));
                    }

                }

                if (wasBracersOpen)
                {
                    wasBracersOpen = false;
                    writer.WriteLine("        }");
                }
            }
        }

        void Generate_RunTimeEnvironment_Header_File()
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
                        foreach (ClientServerOperation operation in csInterface.Operations)
                        {
                            String funcName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(componentDefenition, port, operation);
                            String funcArguments = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, componentDefenition.MultipleInstantiation);
                            writer.WriteLine(Properties.Resources.STD_RETURN_TYPE + funcName + funcArguments + ";");
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

        List<double> PeriodChangeTimes(OsTask task)
        {
            List<double> periods = new List<double>();
            RunnableInstancesList runnables = task.Runnables;
            double prevPeriod = -1;
            if (runnables.Count > 0)
            {
                for (int i = 0; i < runnables.Count; i++)
                {
                    if (runnables[i].Defenition.PeriodMs != task.PeriodMs)
                    {
                        if (runnables[i].Defenition.PeriodMs != prevPeriod)
                        {
                            periods.Add(runnables[i].Defenition.PeriodMs);
                        }
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
    }
}
