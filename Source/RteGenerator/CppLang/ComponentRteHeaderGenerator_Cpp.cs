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
    public class ComponentRteHeaderGenerator_Cpp
    {
        public static void GenerateHeader(String dir, ApplicationSwComponentType compDef)
        {
            String filename = dir + "\\" + RteFunctionsGenerator_Cpp.GenerateComponentHeaderFile(compDef);

            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for " + compDef.Name + " header file");
            RteFunctionsGenerator_Cpp.OpenGuardDefine(writer);


            
writer.WriteLine(@"#ifndef RTE_C
    #ifdef RTE_APP_HEADER_FILE
        #error Multiple application header files included.
    #else
        #define RTE_APP_HEADER_FILE
    #endif
#endif

#include <Rte_DataTypes.hpp>
#include <" + Properties.Resources.RTE_PORT_INTERFACES_HPP_FILENAME + @">

#define RTE_DEFINED");
            writer.WriteLine(
@"
/*************************************************************
 * BEGIN RTE Component Data Structure Definition
 *************************************************************/
");
            String rteStructureName = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDef);

            writer.WriteLine("struct " + rteStructureName + " {");
            writer.WriteLine("    /* Per Instance Memory Section */");
            foreach (PimDefenition pim in compDef.PerInstanceMemoryList)
            {
                writer.WriteLine("    " + pim.DataTypeName + " * Pim_" + pim.Name + ";");
            }


            writer.WriteLine("    /* Port API Section */");
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                    String portDatatype = RteFunctionsGenerator_Cpp.GeneratePortDataStructureDefenition(srInterface, portDef.PortType);
                    writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                }
                else if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                    if (portDef.PortType == PortType.Client)
                    {
                        String portDatatype = RteFunctionsGenerator_Cpp.GeneratePortDataStructureDefenition(csInterface);
                        writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                    }
                }
            }

            writer.WriteLine("    /* Calibration Parameter Handles Section */");
            foreach (CDataDefenition cdata in compDef.CDataDefenitions)
            {
                writer.WriteLine("    " + cdata.DataTypeName + " (*CData_" + cdata.Name + ")(void);");
            }


            writer.WriteLine("};");

            writer.WriteLine(
@"
/*************************************************************
 * END RTE Component Data Structure Definition
 *************************************************************/
");

            writer.WriteLine(
@"
/*************************************************************
 * BEGIN Component Base Class Definition
 *************************************************************/
");
            String baseClassName = RteFunctionsGenerator_Cpp.ComponentBaseClassDefenitionName(compDef);


            writer.WriteLine("class " + baseClassName);
            writer.WriteLine("{");
            writer.WriteLine("public:");

            writer.WriteLine("    /* Constructor */");
            writer.WriteLine("    " + baseClassName + "(const " + rteStructureName + " &Rte);");
            writer.WriteLine("");

            writer.WriteLine("    /* Abstract component's runnables */");
            foreach (RunnableDefenition runnable in compDef.Runnables)
            {
                String runnableName = RteFunctionsGenerator_Cpp.Generate_RunnableDeclaration(compDef, runnable, false);
                writer.WriteLine("    virtual " + runnableName + " = 0;");
            }
            writer.WriteLine("protected:");
            writer.WriteLine("    const Rte_" + compDef.Name + " &Rte;");
            writer.WriteLine("private:");
            writer.WriteLine("};");            

            writer.WriteLine(
@"
/*************************************************************
 * END Component Base Class Definition
 *************************************************************/
");
            RteFunctionsGenerator_Cpp.CloseGuardDefine(writer);
            writer.Close();
        }
    }

}
