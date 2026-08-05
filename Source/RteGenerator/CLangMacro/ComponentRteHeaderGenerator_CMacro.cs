using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CMacro
{
    public class ComponentRteHeaderGenerator_CMacro
    {
        public static void GenerateHeader(String dir, ApplicationSwComponentType compDef)
        {
            String filename = dir + "\\" + RteFunctionsGenerator_CMacro.GenerateComponentHeaderFile(compDef);

            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_CMacro.GenerateFileTitle(writer, filename, "Implementation for " + compDef.Name + " header file");
            RteFunctionsGenerator_CMacro.OpenCppGuardDefine(writer);
            RteFunctionsGenerator_CMacro.OpenCGuardDefine(writer);

            writer.WriteLine(@"
#ifndef RTE_C
    #ifdef RTE_APP_HEADER_FILE
        #error Multiple application header files included.
    #else
        #define RTE_APP_HEADER_FILE
    #endif
#endif

#include <Rte_DataTypes.h>

#define RTE_DEFINED
");

            /* Port Data Structure and Component Data Structure are only needed for multipleInstance components */
            if (compDef.MultipleInstantiation == true)
            {
                writer.WriteLine(@"
/*************************************************************
 * BEGIN Port Data Structure Definitions
 *************************************************************/
");
                SenderReceiverInterfacesList usedRPinterfaces = compDef.Ports.UsedReceiverProviderInterfaces();
                PortDefenitionsList rpPorts = compDef.Ports.PortsWithSenderReceiverInterface();
                List<String> createdInterfaces = new List<string>();

                foreach (PortDefenition portDef in rpPorts)
                {
                    SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;

                    String portDataStructureName = RteFunctionsGenerator_CMacro.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);

                    if (!createdInterfaces.Contains(portDataStructureName))
                    {
                        createdInterfaces.Add(portDataStructureName);

                        writer.WriteLine("typedef struct " + portDataStructureName + " {");
                        foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                        {
                            string data = "    Std_ReturnType (*";
                            data += (portDef.PortType == PortType.Sender) ? "Write_" : "Read_";
                            data += field.Name + ")";
                            String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);
                            data += fieldVariable + ";";
                            writer.WriteLine(data);
                        }
                        writer.WriteLine("} " + portDataStructureName + ";");
                        writer.WriteLine();
                    }
                }

                ClientServerInterfacesList usedCinterfaces = compDef.Ports.UsedClientInterfaces();
                PortDefenitionsList clientPorts = compDef.Ports.PortsWithClientInterface();
                List<String> createdClientInterfaces = new List<string>();
                foreach (PortDefenition portDef in clientPorts)
                {
                    ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;

                    if (portDef.PortType == PortType.Client)
                    {
                        String portDataStructureName = RteFunctionsGenerator_CMacro.GeneratePortDataStructureDefenition(compDef, csInterface);

                        if (!createdClientInterfaces.Contains(portDataStructureName))
                        {
                            createdClientInterfaces.Add(portDataStructureName);

                            writer.WriteLine("typedef struct " + portDataStructureName + " {");
                            foreach (ClientServerOperation operation in csInterface.Operations)
                            {
                                string data = "    Std_ReturnType (*";
                                data += "Call_";
                                data += operation.Name + ")(" + RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArguments(operation, false) + ");";
                                writer.WriteLine(data);
                            }
                            writer.WriteLine("} " + portDataStructureName + ";");
                            writer.WriteLine();
                        }
                    }
                }

                writer.WriteLine(@"
/*************************************************************
 * END Port Data Structure Definitions
 *************************************************************/

/*************************************************************
 * BEGIN Component Data Structure Definitions
 *************************************************************/
");
                String CDSname = RteFunctionsGenerator_CMacro.ComponentDataStructureDefenitionName(compDef);
                writer.WriteLine("typedef struct " + CDSname + " {");
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
                        String portDatatype = RteFunctionsGenerator_CMacro.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);
                        writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                    }
                    else if (portDef.InterfaceDatatype is ClientServerInterface)
                    {
                        ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                        if (portDef.PortType == PortType.Client)
                        {
                            String portDatatype = RteFunctionsGenerator_CMacro.GeneratePortDataStructureDefenition(compDef, csInterface);
                            writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                        }
                    }
                }

                writer.WriteLine("    /* Calibration Parameter Handles Section */");
                foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                {
                    writer.WriteLine("    " + cdata.DataTypeName + " (*CData_" + cdata.Name + ")(void);");
                }


                writer.WriteLine("} " + CDSname + ";");

                writer.WriteLine(@"
/*************************************************************
 * END Component Data Structure Definitions
 *************************************************************/


/*************************************************************
 * BEGIN Component Instance Handle
 *************************************************************/
");
                foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                {
                    foreach (ComponentInstance component in composition.ComponentInstances)
                    {
                        if (component.ComponentDefenition == compDef)
                        {
                            writer.WriteLine("extern const " + CDSname + " Rte_Instance_" + component.Name + ";");
                        }
                    }
                }
            }

            writer.WriteLine(@"
/*************************************************************
 * END Component Instance Handle
 *************************************************************/

/*************************************************************
 * BEGIN External Function Declarations for non-multipleInstance
 *************************************************************/
");
            
            /* For non-multipleInstance components, declare external functions that will be used by macros */
            if (compDef.MultipleInstantiation == false)
            {
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if ((portDef.PortType == PortType.Sender) && (portDef.InterfaceDatatype is SenderReceiverInterface))
                    {
                        SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                        if (srInterface.IsQueued == false)
                        {
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String internalFuncName = "Rte_InternalWrite_" + compDef.Name + "_" + portDef.Name + "_" + field.Name;
                                String fieldVar = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);
                                writer.WriteLine("extern " + Properties.Resources.STD_RETURN_TYPE + " " + internalFuncName + fieldVar + ";");
                            }
                        }
                    }
                    else if ((portDef.PortType == PortType.Receiver) && (portDef.InterfaceDatatype is SenderReceiverInterface))
                    {
                        SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                        if (srInterface.IsQueued == false)
                        {
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String internalFuncName = "Rte_InternalRead_" + compDef.Name + "_" + portDef.Name + "_" + field.Name;
                                String fieldVar = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);
                                writer.WriteLine("extern " + Properties.Resources.STD_RETURN_TYPE + " " + internalFuncName + fieldVar + ";");
                            }
                        }
                    }
                }
                
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if (portDef.PortType == PortType.Client && (portDef.InterfaceDatatype is ClientServerInterface))
                    {
                        ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                        foreach (ClientServerOperation operation in csInterface.Operations)
                        {
                            String internalFuncName = "Rte_InternalCall_" + compDef.Name + "_" + portDef.Name + "_" + operation.Name;
                            /* Generate correct argument list with data types for extern declaration */
                            String externArgs = RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArgumentsForExternDeclare(operation, false);
                            writer.WriteLine("extern " + Properties.Resources.STD_RETURN_TYPE + " " + internalFuncName + externArgs + ";");
                        }
                    }
                }
            }
            else
            {
                /* For multipleInstance, just include the runnable declarations */
            }

            writer.WriteLine(@"
/*************************************************************
 * END External Function Declarations
 *************************************************************/

/*************************************************************
 * BEGIN Runnable Entity
 *************************************************************/
");
            RteComponentGenerator_CMacro.WriteAllFunctionWhichComponentCouldUse(compDef, writer);

            foreach (RunnableDefenition runnable in compDef.Runnables)
            {
                String returnType;
                writer.WriteLine(RteFunctionsGenerator_CMacro.Generate_RunnableDeclaration(compDef, runnable, out returnType) + ";");
            }

            writer.WriteLine(
@"
/*************************************************************
 * END Runnable Entity
 *************************************************************/

/*************************************************************
 * BEGIN RTE API DEFINITIONS 
 *************************************************************/
#ifndef RTE_C
");

            /* Add defines for all ports */
            foreach (PortDefenition portDefenition in compDef.Ports)
            {
                if ((portDefenition.PortType == PortType.Sender) || (portDefenition.PortType == PortType.Receiver))
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        String funcName = RteFunctionsGenerator_CMacro.GenerateReadWriteFunctionName(portDefenition, field);
                        
                        if (compDef.MultipleInstantiation == false)
                        {
                            /* For non-multipleInstance: use internal function directly */
                            /* Left side: Rte_Write_SenderPort1_f1(_data_) or Rte_Read_ReceiverPort_f1(_data_) */
                            /* Right side: internal function call */
                            String internalFuncPrefix = (portDefenition.PortType == PortType.Sender) ? "Rte_InternalWrite_" : "Rte_InternalRead_";
                            String internalFuncName = internalFuncPrefix + compDef.Name + "_" + portDefenition.Name + "_" + field.Name;
                            String macroName = (portDefenition.PortType == PortType.Sender) ? "Rte_Write_" : "Rte_Read_";
                            macroName += portDefenition.Name + "_" + field.Name;
                            writer.WriteLine(RteFunctionsGenerator_CMacro.CreateDefine(macroName + "(_data_)", internalFuncName + "(_data_)", false));
                        }
                        else
                        {
                            /* For multipleInstance: use instance-based access */
                            funcName += "(instance, _data_)";
                            String instance = "(Rte_CDS_" + compDef.Name + "*)instance";
                            String rteFuncName = "(" + instance + ")->" + portDefenition.Name + ".";
                            rteFuncName += (portDefenition.PortType == PortType.Sender ? "Write_" : "Read_") + field.Name + "(_data_)";
                            writer.WriteLine(RteFunctionsGenerator_CMacro.CreateDefine(funcName, rteFuncName, true));
                        }
                    }
                }
                else if (portDefenition.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        if (compDef.MultipleInstantiation == false)
                        {
                            /* For non-multipleInstance: use internal function with ComponentDef name */
                            /* Left side: Rte_Call_Client1_DoSomething() - uses port name */
                            /* Right side: Rte_InternalCall_ComponentDefName_PortName_OperationName() - uses component def name */
                            String macroName = "Rte_Call_" + portDefenition.Name + "_" + operation.Name;
                            String internalFuncName = "Rte_InternalCall_" + compDef.Name + "_" + portDefenition.Name + "_" + operation.Name;
                            
                            /* Build the argument list for the define */
                            String defineArgs = "(";
                            for (int i = 0; i < operation.Fields.Count; i++)
                            {
                                defineArgs += operation.Fields[i].Name;
                                if (i != operation.Fields.Count - 1)
                                {
                                    defineArgs += ", ";
                                }
                            }
                            defineArgs += ")";
                            
                            /* Call the internal function */
                            String callFunc = internalFuncName + defineArgs;
                            
                            writer.WriteLine(RteFunctionsGenerator_CMacro.CreateDefine(macroName + defineArgs, callFunc, false));
                        }
                        else
                        {
                            /* For multipleInstance: use instance-based call */
                            String funcName = RteFunctionsGenerator_CMacro.Generate_InternalRteCall_FunctionName(portDefenition, operation);
                            String defineArgs = RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArgumentsForDefine(operation, true);
                            String argumentsWithoutInstance = RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArgumentsForDefineWithoutInstance(operation, true);

                            String instance = "(Rte_CDS_" + compDef.Name + "*)instance";
                            String rteFuncName = "(" + instance + ")->" + portDefenition.Name + ".";
                            rteFuncName += "Call_" + operation.Name + argumentsWithoutInstance;

                            String define = RteFunctionsGenerator_CMacro.CreateDefine(funcName + defineArgs, rteFuncName, true);
                            writer.WriteLine(define);
                        }
                    }
                }
            }

            writer.WriteLine(
@"
#endif /* RTE_C */

/*************************************************************
 * END RTE API DEFINITIONS 
 *************************************************************/
");

            RteFunctionsGenerator_CMacro.CloseCGuardDefine(writer);
            RteFunctionsGenerator_CMacro.CloseCppGuardDefine(writer);
            writer.Close();
        }
    }

}