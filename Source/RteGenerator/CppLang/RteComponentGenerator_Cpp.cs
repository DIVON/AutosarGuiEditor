using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteComponentGenerator_Cpp
    {
        public RteComponentGenerator_Cpp()
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
            String componentFolder = RteFunctionsGenerator_Cpp.GetComponentsFolder() + "\\" + component.Name + "\\contracts\\skeleton";
            String rteDir = RteFunctionsGenerator_Cpp.GetRteFolder() + "\\";
            String incDir = componentFolder + "\\include\\";
            String srcDir = componentFolder + "\\src\\";
            Directory.CreateDirectory(incDir);
            Directory.CreateDirectory(srcDir);

            /* Fill sections of code */


            /* Each component's runnable shall be in its own file */
            foreach (RunnableDefenition runnable in component.Runnables)
            {
                string arguments = "";
                string returnType = "void";

                String runnableDefenitionLine = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(component, runnable);
                
                CreateRunnable(srcDir, component, runnable, runnableDefenitionLine, arguments, returnType);
            }


            /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
            ComponentRteHeaderGenerator_Cpp.GenerateHeader(rteDir, component);

            CreateComponentIncludes(incDir, component);
        }

        void CreateComponentIncludes(String dir, ApplicationSwComponentType componentDefenition)
        {
            String filename = dir + componentDefenition.Name + ".h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + componentDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.MacrosLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.TypeDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.ExternalVariablesLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfExternalVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.GlobalFunctionsDeclarationLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfGlobalFunctionsDeclarationLine);
            writer.WriteLine("");

            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);
            writer.Close();
        }

        void CreateRunnable(String dir, ApplicationSwComponentType compDefenition, RunnableDefenition runnable, String runnableDefenitionLine, String arguments, String returnType)
        {
            String filename = dir + compDefenition.Name + "_ru" + runnable.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.MacrosLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.TypeDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.VariablesLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.LocalFunctionsDeclarationLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfLocalFunctionsDeclarationLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.LocalFunctionsDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfLocalFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.GlobalFunctionsDefenitionsLine);
            writer.WriteLine("");


            
            
            /* Fill all function names which component could use*/
            WriteAllFunctionWhichComponentCouldUse(compDefenition, writer);

            writer.WriteLine(runnableDefenitionLine);
            writer.WriteLine("{");
            if (returnType != "void")
            {
                writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
            }
            else
            {
                writer.WriteLine("    ");
            }
            
            writer.WriteLine("}");
            writer.WriteLine("");

            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfGlobalFunctionsDefenitionsLine);
            writer.WriteLine("");

            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        public static void WriteAllFunctionWhichComponentCouldUse(ApplicationSwComponentType compDefenition, StreamWriter writer)
        {
            List<String> lines = new List<string>();

            /* Write function name and its body */
            
            foreach (PimDefenition pimDefenition in compDefenition.PerInstanceMemoryList)
            {
                lines.Add(" *  " + RteFunctionsGenerator_Cpp.GenerateShortPimFunctionName(pimDefenition));
            }
            foreach (CDataDefenition cdataDefenition in compDefenition.CDataDefenitions)
            {
                lines.Add(" *  " + RteFunctionsGenerator_Cpp.GenerateShortCDataFunctionName(cdataDefenition));
            }

            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(port.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        lines.Add(" *  " + RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field));
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
                        lines.Add(" *  " + RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field));
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
                        lines.Add(" *  " + RteFunctionsGenerator_Cpp.Generate_InternalRteCall_FunctionName(port, operation));
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
    }
}
