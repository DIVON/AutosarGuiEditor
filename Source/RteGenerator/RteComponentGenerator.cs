using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteComponentGenerator
    {
        public RteComponentGenerator()
        {
        }

        public void GenerateComponentsFiles()
        {
            foreach (ComponentDefenition component in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateComponent(component);
            }
        }

        void GenerateComponent(ComponentDefenition component)
        {
            /* Generate component Folder */
            String componentFolder = RteFunctionsGenerator.GetComponentsFolder() + "\\" + component.Name;
            String rteDir = RteFunctionsGenerator.GetRteFolder() + "\\";
            String incDir = componentFolder + "\\include\\";
            String srcDir = componentFolder + "\\src\\";
            Directory.CreateDirectory(incDir);
            Directory.CreateDirectory(srcDir);

            /* Fill sections of code */


            /* Each component runnable shall be in its own file */
            foreach (PeriodicRunnableDefenition runnable in component.Runnables)
            {
                CreateRunnable(srcDir, runnable);
            }

            /* Each Server port functions shall be in its own file */
            foreach (PortDefenition port in component.Ports)
            {
                if (port.PortType == PortType.Server)
                {
                    CreateServerCalls(srcDir, component, port);
                }
            }

            /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
            CreateRteIncludes(rteDir, component);

            CreateComponentIncludes(incDir, component);
        }

        void CreateComponentIncludes(String dir, ComponentDefenition componentDefenition)
        {
            String filename = dir + componentDefenition.Name + ".h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            RteFunctionsGenerator.AddInclude(writer, "Rte_" + componentDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.MacrosLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.TypeDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.ExternalVariablesLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfExternalVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.GlobalFunctionsDeclarationLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfGlobalFunctionsDeclarationLine);
            writer.WriteLine("");

            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void CreateRunnable(String dir, PeriodicRunnableDefenition runnable)
        {
            ComponentDefenition compDefenition = AutosarApplication.GetInstance().FindComponentDefenitionByRunnnable(runnable);
            String filename = dir + compDefenition.Name + "_" + runnable.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            RteFunctionsGenerator.AddInclude(writer, compDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.MacrosLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.TypeDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.VariablesLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.LocalFunctionsDeclarationLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfLocalFunctionsDeclarationLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.LocalFunctionsDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfLocalFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.GlobalFunctionsDefenitionsLine);
            writer.WriteLine("");


            
            
            /* Fill all function names which component could use*/
            WriteAllFunctionWhichComponentCouldUse(compDefenition, writer);

            writer.WriteLine(RteFunctionsGenerator.Generate_RunnableFunction(compDefenition, runnable));
            writer.WriteLine("{");
            writer.WriteLine("    ");
            writer.WriteLine("}");
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator.EndOfGlobalFunctionsDefenitionsLine);
            writer.WriteLine("");

            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        private static void WriteAllFunctionWhichComponentCouldUse(ComponentDefenition compDefenition, StreamWriter writer)
        {
            List<String> lines = new List<string>();

            /* Write function name and its body */
            
            foreach (PimDefenition pimDefenition in compDefenition.PerInstanceMemoryList)
            {
                lines.Add(" *  " + RteFunctionsGenerator.GenerateShortPimFunctionName(pimDefenition));
            }
            foreach (CDataDefenition cdataDefenition in compDefenition.CDataDefenitions)
            {
                lines.Add(" *  " + RteFunctionsGenerator.GenerateShortCDataFunctionName(cdataDefenition));
            }

            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(port.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        lines.Add(" *  " + RteFunctionsGenerator.GenerateReadWriteFunctionName(port, field));
                    }
                }
            }

            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Sender)
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(port.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        lines.Add(" *  " + RteFunctionsGenerator.GenerateReadWriteFunctionName(port, field));
                    }
                }
            }

            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(port.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        lines.Add(" *  " + RteFunctionsGenerator.Generate_RteCall_FunctionName(port, operation));
                    }
                }
            }

            if (lines.Count > 0)
            {
                writer.WriteLine("/* ");
                writer.WriteLine(" *  This RTE functions could be used: ");
                foreach(String str in lines)
                {
                    writer.WriteLine(str);
                }
                writer.WriteLine(" */");
            }
        }

        void CreateServerCalls(String dir, ComponentDefenition compDefenition, PortDefenition portDefenition)
        {
            String filename = dir + compDefenition.Name + "_" + portDefenition.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + " " + portDefenition.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            RteFunctionsGenerator.AddInclude(writer, compDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.MacrosLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.TypeDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.VariablesLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.LocalFunctionsDeclarationLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfLocalFunctionsDeclarationLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.LocalFunctionsDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfLocalFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.GlobalFunctionsDefenitionsLine);
            writer.WriteLine("");
            ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
            if (csInterface != null)
            {
                foreach (ClientServerOperation operation in csInterface.Operations)
                {
                    /* Fill functions which component could use */
                    /* Fill all function names which component could use*/
                    WriteAllFunctionWhichComponentCouldUse(compDefenition, writer);

                    String funcName = RteFunctionsGenerator.Generate_RteCall_FunctionName(portDefenition, operation);
                    String funcArgument = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, compDefenition.MultipleInstantiation);
                    writer.WriteLine(Properties.Resources.STD_RETURN_TYPE + funcName + funcArgument);
                    writer.WriteLine("{");
                    writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                    writer.WriteLine("}");
                    writer.WriteLine("");
                }

                writer.WriteLine(RteFunctionsGenerator.EndOfGlobalFunctionsDefenitionsLine);
                writer.WriteLine("");

                RteFunctionsGenerator.WriteEndOfFile(writer);
                writer.Close();
            }
            else
            {
                System.Windows.MessageBox.Show(portDefenition.GUID.ToString("B") + " not found in interfaces!");
            }
        }

        /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
        public void CreateRteIncludes(String dir, ComponentDefenition componentDefenition)
        {
            String filename = dir + "\\" +"Rte_" + componentDefenition.Name + ".h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + componentDefenition.Name + " header file");
            string guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfIncludesLine);
            writer.WriteLine("");

            /* MACROS */
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.MacrosLine);
            
            /* Write all runnables frequences */
            writer.WriteLine("/* Runnables frequences */");
            foreach (PeriodicRunnableDefenition runnable in componentDefenition.Runnables)
            {
                String runnableFreqMacroName = "Rte_Period_" + componentDefenition.Name + "_ru" + runnable.Name;
                String define = RteFunctionsGenerator.CreateDefine(runnableFreqMacroName, (runnable.PeriodMs * 1000).ToString() + "UL");
                writer.WriteLine(define);
            }

            /* Write all pims */
            foreach (PimDefenition pim in componentDefenition.PerInstanceMemoryList)
            {
                String define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortPimFunctionName(pim), RteFunctionsGenerator.GenerateFullPimFunctionName(componentDefenition, pim), false);
                writer.WriteLine(define);
            }

            /* Write all cdata */
            foreach (CDataDefenition cdata in componentDefenition.CDataDefenitions)
            {
                String define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortCDataFunctionName(cdata), RteFunctionsGenerator.GenerateFullCDataFunctionName(componentDefenition, cdata), false);
                writer.WriteLine(define);
            }

            String externFunctions = "";

            /* Add defines for all ports */
            foreach (PortDefenition portDefenition in componentDefenition.Ports)
            {
                if ((portDefenition.PortType == PortType.Sender) || (portDefenition.PortType == PortType.Receiver))
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        String funcName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDefenition, field);
                        String RteFuncName = RteFunctionsGenerator.GenerateReadWriteConnectionFunctionName(portDefenition, field);
                        writer.WriteLine(RteFunctionsGenerator.CreateDefine(funcName, RteFuncName, false));
                        String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, portDefenition.PortType, componentDefenition.MultipleInstantiation);

                        externFunctions += Properties.Resources.STD_RETURN_TYPE + funcName + fieldVariable +";" +Environment.NewLine;
                    }
                }
                else if (portDefenition.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        String funcName = RteFunctionsGenerator.Generate_RteCall_FunctionName(portDefenition, operation);
                        String RteFuncName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(componentDefenition, portDefenition, operation);
                        String funcArgument = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, componentDefenition.MultipleInstantiation);
                        writer.WriteLine(RteFunctionsGenerator.CreateDefine(funcName, RteFuncName, false));
                        externFunctions += Properties.Resources.STD_RETURN_TYPE + funcName + funcArgument + ";" + Environment.NewLine;
                    }
                }
                else if (portDefenition.PortType == PortType.Server)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        String funcName = RteFunctionsGenerator.Generate_RteCall_FunctionName(portDefenition, operation);
                        String RteFuncName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(componentDefenition, portDefenition, operation);
                        String funcArgument = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, componentDefenition.MultipleInstantiation);
                        writer.WriteLine(RteFunctionsGenerator.CreateDefine(funcName, RteFuncName, false));
                    }
                }
            }

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.RteFunctionsDefenitionsLine);
            writer.WriteLine("");
            
            /* Add function */
            writer.Write(externFunctions);

            /* Add Pim's functions */
            foreach(PimDefenition pimDefenition in componentDefenition.PerInstanceMemoryList)
            {
                String datatype = pimDefenition.DataTypeName;
                String pimFuncName = RteFunctionsGenerator.GenerateShortPimFunctionName(pimDefenition);
                String arguments = "(";
                if (componentDefenition.MultipleInstantiation)
                {
                    arguments += RteFunctionsGenerator.ComponentInstancePointerDatatype + " instance";
                }
                arguments += ");";
                writer.WriteLine(datatype + " * const " + pimFuncName + arguments);
            }

            /* Add CData functions */
            foreach (CDataDefenition cdataDefenition in componentDefenition.CDataDefenitions)
            {
                String datatype = cdataDefenition.DataTypeName;
                String cdataFuncName = RteFunctionsGenerator.GenerateShortCDataFunctionName(cdataDefenition);
                String arguments = "(";
                if (componentDefenition.MultipleInstantiation)
                {
                    arguments += RteFunctionsGenerator.ComponentInstancePointerDatatype + " instance";
                }
                arguments += ");";
                writer.WriteLine(datatype + " " + cdataFuncName + arguments);
            }

            writer.WriteLine(RteFunctionsGenerator.EndOfRteFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            RteFunctionsGenerator.CloseGuardDefine(writer);
            writer.Close();
        }
    }
}
