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

namespace AutosarGuiEditor.Source.RteGenerator.CLang
{
    public class RteComponentGenerator_C
    {
        public RteComponentGenerator_C()
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
            String componentFolder = RteFunctionsGenerator_C.GetComponentsFolder() + "\\" + component.Name + "\\contracts\\skeleton";
            String rteDir = RteFunctionsGenerator_C.GetRteFolder() + "\\";
            String incDir = componentFolder + "\\include\\";
            String srcDir = componentFolder + "\\src\\";
            Directory.CreateDirectory(incDir);
            Directory.CreateDirectory(srcDir);

            /* Fill sections of code */


            /* Each component's runnable shall be in its own file */
            foreach (RunnableDefenition runnable in component.Runnables)
            {
                string arguments = "";
                string returnType;

                String runnableDefenitionLine = RteFunctionsGenerator_C.Generate_RunnableDeclaration(component, runnable, out returnType);
                
                CreateRunnable(srcDir, component, runnable, runnableDefenitionLine, arguments, returnType);
            }


            /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
            ComponentRteHeaderGenerator_C.GenerateHeader(rteDir, component);

            CreateComponentIncludes(incDir, component);
        }

        void CreateComponentIncludes(String dir, ApplicationSwComponentType componentDefenition)
        {
            String filename = dir + componentDefenition.Name + ".h";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, filename, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator_C.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_C.AddInclude(writer, "Rte_" + componentDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.MacrosLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.TypeDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.ExternalVariablesLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfExternalVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.GlobalFunctionsDeclarationLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfGlobalFunctionsDeclarationLine);
            writer.WriteLine("");

            RteFunctionsGenerator_C.CloseGuardDefine(writer);
            writer.Close();
        }

        void CreateRunnable(String dir, ApplicationSwComponentType compDefenition, RunnableDefenition runnable, String runnableDefenitionLine, String arguments, String returnType)
        {
            String filename = dir + compDefenition.Name + "_ru" + runnable.Name + ".c";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_C.AddInclude(writer, compDefenition.Name + ".h");
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.EndOfIncludesLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.MacrosLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfMacrosLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.TypeDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfTypeDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.VariablesLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfVariableLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.LocalFunctionsDeclarationLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfLocalFunctionsDeclarationLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.LocalFunctionsDefenitionsLine);
            writer.WriteLine(RteFunctionsGenerator_C.EndOfLocalFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_C.GlobalFunctionsDefenitionsLine);
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

            writer.WriteLine(RteFunctionsGenerator_C.EndOfGlobalFunctionsDefenitionsLine);
            writer.WriteLine("");

            RteFunctionsGenerator_C.WriteEndOfFile(writer);
            writer.Close();
        }

        public static void WriteAllFunctionWhichComponentCouldUse(ApplicationSwComponentType compDefenition, StreamWriter writer)
        {
            List<String> lines = new List<string>();

            /* Write function name and its body */
            
            foreach (PimDefenition pimDefenition in compDefenition.PerInstanceMemoryList)
            {
                lines.Add(" *  " + RteFunctionsGenerator_C.GenerateShortPimFunctionName(pimDefenition));
            }
            foreach (CDataDefenition cdataDefenition in compDefenition.CDataDefenitions)
            {
                lines.Add(" *  " + RteFunctionsGenerator_C.GenerateShortCDataFunctionName(cdataDefenition));
            }

            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(port.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        lines.Add(" *  " + RteFunctionsGenerator_C.GenerateReadWriteFunctionName(port, field));
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
                        lines.Add(" *  " + RteFunctionsGenerator_C.GenerateReadWriteFunctionName(port, field));
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
                        lines.Add(" *  " + RteFunctionsGenerator_C.Generate_InternalRteCall_FunctionName(port, operation));
                    }
                }
            }

            if (lines.Count > 0)
            {
                writer.WriteLine("/*");
                writer.WriteLine(" *  This RTE functions could be used:");
                foreach(String str in lines)
                {
                    writer.WriteLine(str);
                }
                writer.WriteLine(" */");
            }
        }
    }
}
