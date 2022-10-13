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
    public class ComponentRteHeaderGenerator_C
    {
        public static void GenerateHeader(String dir, ApplicationSwComponentType compDef)
        {
            String filename = dir + "\\" + RteFunctionsGenerator_C.GenerateComponentHeaderFile(compDef);

            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, filename, "Implementation for " + compDef.Name + " header file");
            RteFunctionsGenerator_C.OpenGuardDefine(writer);
            RteFunctionsGenerator_C.OpenCGuardDefine(writer);

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

                String portDataStructureName = RteFunctionsGenerator_C.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);

                if (!createdInterfaces.Contains(portDataStructureName))
                {
                    createdInterfaces.Add(portDataStructureName);

                    writer.WriteLine("typedef struct " + portDataStructureName + " {");
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        string data = "    Std_ReturnType (*";
                        data += (portDef.PortType == PortType.Sender) ? "Write_" : "Read_";
                        data += field.Name + ")";
                        String fieldVariable = RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);
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
                    String portDataStructureName = RteFunctionsGenerator_C.GeneratePortDataStructureDefenition(compDef, csInterface);

                    if (!createdClientInterfaces.Contains(portDataStructureName))
                    {
                        createdClientInterfaces.Add(portDataStructureName);

                        writer.WriteLine("typedef struct " + portDataStructureName + " {");
                        foreach (ClientServerOperation operation in csInterface.Operations)
                        {
                            string data = "    Std_ReturnType (*";
                            data += "Call_";
                            data += operation.Name + ")(" + RteFunctionsGenerator_C.GenerateClientServerInterfaceArguments(operation, false) + ");";
                            writer.WriteLine(data);
                        }
                        writer.WriteLine("} " + portDataStructureName + ";");
                        writer.WriteLine();
                    }
                }
            }

            writer.WriteLine(
@"/*************************************************************
 * END Port Data Structure Definitions
 *************************************************************/

/*************************************************************
 * BEGIN Component Data Structure Definitions
 *************************************************************/

");
            String CDSname = RteFunctionsGenerator_C.ComponentDataStructureDefenitionName(compDef);
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
                    String portDatatype = RteFunctionsGenerator_C.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);
                    writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                }
                else if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                    if (portDef.PortType == PortType.Client)
                    {
                        String portDatatype = RteFunctionsGenerator_C.GeneratePortDataStructureDefenition(compDef, csInterface);
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

            writer.WriteLine(
@"
/*************************************************************
 * END Component Data Structure Definitions
 *************************************************************/


/*************************************************************
 * BEGIN Component Instance Handle
 *************************************************************/
");
            String singleComponentInstanceVariableName = "Rte_Instance_" + compDef.Name;
            if (compDef.MultipleInstantiation == false)
            {
                writer.WriteLine("extern const " + CDSname + " " + singleComponentInstanceVariableName + ";");
            }

            writer.WriteLine(@"
/*************************************************************
 * END Component Instance Handle
 *************************************************************/

/*************************************************************
 * BEGIN Runnable Entity
 *************************************************************/
");
            RteComponentGenerator_C.WriteAllFunctionWhichComponentCouldUse(compDef, writer);

            foreach (RunnableDefenition runnable in compDef.Runnables)
            {
                String returnType;
                writer.WriteLine(RteFunctionsGenerator_C.Generate_RunnableDeclaration(compDef, runnable, out returnType) + ";");
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

            /* Write all cdata */
            foreach (CDataDefenition cdata in compDef.CDataDefenitions)
            {
                String define;
                if (compDef.MultipleInstantiation == false)
                {
                    define = RteFunctionsGenerator_C.CreateDefine(RteFunctionsGenerator_C.GenerateShortCDataFunctionName(cdata) + "()", singleComponentInstanceVariableName + ".CData_" + cdata.Name + "()", true);
                }
                else
                {
                    define = RteFunctionsGenerator_C.CreateDefine(RteFunctionsGenerator_C.GenerateShortCDataFunctionName(cdata) + "(instance)", "(instance)->CData_" + cdata.Name + "()", true);
                }

                writer.WriteLine(define);
            }


            /* Write all pims */
            foreach (PimDefenition pim in compDef.PerInstanceMemoryList)
            {
                String define;
                if (compDef.MultipleInstantiation == false)
                {
                    define = RteFunctionsGenerator_C.CreateDefine(RteFunctionsGenerator_C.GenerateShortPimFunctionName(pim) + "()", singleComponentInstanceVariableName + ".Pim_" + pim.Name, true);
                }
                else
                {
                    define = RteFunctionsGenerator_C.CreateDefine(RteFunctionsGenerator_C.GenerateShortPimFunctionName(pim) + "(instance)", "(instance)->Pim_" + pim.Name, true);
                }


                writer.WriteLine(define);
            }

            /* Add defines for all ports */
            foreach (PortDefenition portDefenition in compDef.Ports)
            {
                if ((portDefenition.PortType == PortType.Sender) || (portDefenition.PortType == PortType.Receiver))
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        String funcName = RteFunctionsGenerator_C.GenerateReadWriteFunctionName(portDefenition, field);
                        if (compDef.MultipleInstantiation == false)
                        {
                            funcName += "(_data_)";
                        }
                        else
                        {
                            funcName += "(instance, _data_)";
                        }

                        String rteFuncName;
                        if (compDef.MultipleInstantiation == false)
                        {
                            rteFuncName = singleComponentInstanceVariableName + "." + portDefenition.Name + ".";
                        }
                        else
                        {
                            rteFuncName = "(instance)->" + portDefenition.Name + ".";
                        }
                        rteFuncName += (portDefenition.PortType == PortType.Sender ? "Write_" : "Read_") + field.Name + "(_data_)";

                        string define = RteFunctionsGenerator_C.CreateDefine(funcName, rteFuncName, true);

                        writer.WriteLine(define);
                    }
                }
                else if (portDefenition.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        String funcName = RteFunctionsGenerator_C.Generate_InternalRteCall_FunctionName(portDefenition, operation);
                        String defineArguments = RteFunctionsGenerator_C.GenerateClientServerInterfaceArgumentsForDefine(operation, compDef.MultipleInstantiation);
                        String argumentsWithoutInstance = RteFunctionsGenerator_C.GenerateClientServerInterfaceArgumentsForDefineWithoutInstance(operation, compDef.MultipleInstantiation);

                        String rteFuncName;
                        if (compDef.MultipleInstantiation == false)
                        {
                            rteFuncName = singleComponentInstanceVariableName + "." + portDefenition.Name + ".";
                        }
                        else
                        {
                            rteFuncName = "(instance)->" + portDefenition.Name + ".";
                        }
                        rteFuncName += "Call_" + operation.Name + argumentsWithoutInstance;

                        String funcArgument = RteFunctionsGenerator_C.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);


                        String define = RteFunctionsGenerator_C.CreateDefine(funcName + defineArguments, rteFuncName, true);
                        writer.WriteLine(define);

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

            RteFunctionsGenerator_C.CloseCGuardDefine(writer);
            RteFunctionsGenerator_C.CloseGuardDefine(writer);
            writer.Close();
        }
    }

}
