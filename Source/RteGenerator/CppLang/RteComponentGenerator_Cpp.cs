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

                String runnableDefenitionLine = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(component, runnable, true);
                
                CreateRunnableFile(srcDir, component, runnable, runnableDefenitionLine, arguments, returnType);
            }


            /* Generate funcitons for Sender-Receiver ports and call operations from client ports */
            ComponentRteHeaderGenerator_Cpp.GenerateHeader(rteDir, component);

            CreateComponentIncludes(incDir, component);
        }

        void CreateComponentIncludes(String dir, ApplicationSwComponentType componentDefenition)
        {
            String filename = dir + componentDefenition.Name + ".hpp";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for header of " + componentDefenition.Name);
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + componentDefenition.Name + ".hpp");
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
            
            WriteComponentClass(writer, componentDefenition);
            

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

        void WriteComponentClass(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            String rteStructureName = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDefenition);
            String baseClassName    = RteFunctionsGenerator_Cpp.ComponentBaseClassDefenitionName(compDefenition);

            writer.WriteLine("class " + compDefenition.Name + " final: private " + baseClassName);
            writer.WriteLine("{");
            writer.WriteLine("public:");

            writer.WriteLine("    /* Constructor */");
            writer.WriteLine("    " + compDefenition.Name + "(const " + rteStructureName + " &Rte);");
            writer.WriteLine("");

            writer.WriteLine("    /* Component runnables */");
            foreach (RunnableDefenition runnable in compDefenition.Runnables)
            {
                String runnableName = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(compDefenition, runnable, false);
                writer.WriteLine("    " + runnableName + ";");
            }
            writer.WriteLine("");
            writer.WriteLine("protected:");
            writer.WriteLine("    /* Make other functions here */ ");
            writer.WriteLine("private:");
            writer.WriteLine("");
            writer.WriteLine("};");
            writer.WriteLine("");
        }

        void CreateRunnableFile(String dir, ApplicationSwComponentType compDefenition, RunnableDefenition runnable, String runnableDefenitionLine, String arguments, String returnType)
        {
            String filename = dir + compDefenition.Name + "_ru" + runnable.Name + ".cpp";
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for " + compDefenition.Name + "_" + runnable.Name);

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.IncludesLine);
            writer.WriteLine("");
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDefenition.Name + ".hpp");
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
            writer.WriteLine(RteFunctionsGenerator_Cpp.PrivateFunctionsDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfPrivateFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.ProtectedFunctionsDefenitionsLine);
            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfProtectedFunctionsDefenitionsLine);
            writer.WriteLine("");

            writer.WriteLine("");
            writer.WriteLine(RteFunctionsGenerator_Cpp.PublicFunctionsDefenitionsLine);
            writer.WriteLine("");

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

            writer.WriteLine(RteFunctionsGenerator_Cpp.EndOfPublicFunctionsDefenitionsLine);
            writer.WriteLine("");

            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }
    }
}
