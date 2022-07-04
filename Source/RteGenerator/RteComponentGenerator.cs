using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters;
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
            foreach (ApplicationSwComponentType component in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateComponent(component);
            }
        }

        void GenerateComponent(ApplicationSwComponentType component)
        {
            /* Generate component Folder */
            String componentFolder = RteFunctionsGenerator.GetComponentsFolder() + "\\" + component.Name + "\\contracts\\skeleton";
            String rteDir = RteFunctionsGenerator.GetRteFolder() + "\\";
            String incDir = componentFolder + "\\include\\";
            String srcDir = componentFolder + "\\src\\";
            Directory.CreateDirectory(incDir);
            Directory.CreateDirectory(srcDir);

            /* Fill sections of code */


            /* Each component runnable shall be in its own file */
            foreach (RunnableDefenition runnable in component.Runnables)
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
            ComponentRteHeaderGenerator.GenerateHeader(rteDir, component);

            CreateComponentIncludes(incDir, component);
        }

        void CreateComponentIncludes(String dir, ApplicationSwComponentType componentDefenition)
        {
            String filename = dir + componentDefenition.Name + ".h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            writer.WriteLine("");
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
            writer.Close();
        }

        void CreateRunnable(String dir, RunnableDefenition runnable)
        {
            ApplicationSwComponentType compDefenition = AutosarApplication.GetInstance().FindComponentDefenitionByRunnnable(runnable);
            String filename = dir + compDefenition.Name + "_" + runnable.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            writer.WriteLine("");
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

        public static void WriteAllFunctionWhichComponentCouldUse(ApplicationSwComponentType compDefenition, StreamWriter writer)
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
                        lines.Add(" *  " + RteFunctionsGenerator.Generate_InternalRteCall_FunctionName(port, operation));
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

        void CreateServerCalls(String dir, ApplicationSwComponentType compDefenition, PortDefenition portDefenition)
        {
            String filename = dir + compDefenition.Name + "_" + portDefenition.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + " " + portDefenition.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator.IncludesLine);
            writer.WriteLine("");
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

                    String funcName = RteFunctionsGenerator.Generate_RteCall_FunctionName(compDefenition, portDefenition, operation);
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
    }
}
