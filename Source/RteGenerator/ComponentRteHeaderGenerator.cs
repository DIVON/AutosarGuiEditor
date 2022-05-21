﻿using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class ComponentRteHeaderGenerator
    {
        public static void GenerateHeader(String dir, ApplicationSwComponentType compDef)
        {
            String filename = dir +"\\"+ RteFunctionsGenerator.GenerateComponentHeaderFile(compDef);

            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for " + compDef.Name + " header file");
            string guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine(@"
#ifdef RTE_DEFINED
#ifndef TEST_RTE
#error ""Multiple RTE files included""
#endif
#endif

#include <Rte_DataTypes.h>
#include <" + RteFunctionsGenerator.GenerateComponentTypeHeaderFile(compDef) + @">


#define RTE_DEFINED

/*************************************************************
 * BEGIN Port Data Structure Definitions
 *************************************************************/
");
            SenderReceiverInterfacesList usedRPinterfaces = compDef.Ports.UsedReceiverProviderInterfaces();
            PortDefenitionsList rpPorts = compDef.Ports.PortsWithSenderReceiverInterface();
            List<String> createdInterfaces = new List<string>();

            foreach(PortDefenition portDef in rpPorts)
            {  
                SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;

                String portDataStructureName = RteFunctionsGenerator.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);
                
                if (!createdInterfaces.Contains(portDataStructureName))
                {
                    createdInterfaces.Add(portDataStructureName);

                    writer.WriteLine("typedef struct " + portDataStructureName + " {");
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        string data = "    Std_ReturnType (*";
                        data += (portDef.PortType == PortType.Sender) ? "Write_" : "Read_";
                        data += field.Name + ")(" + field.DataTypeName + "*);";
                        writer.WriteLine(data);
                    }
                    writer.WriteLine("}\n");
                }
            }



            ClientServerInterfacesList usedCinterfaces = compDef.Ports.UsedClientInterfaces();
            PortDefenitionsList clientPorts = compDef.Ports.PortsWithClientInterface();
            List<String> createdClientInterfaces = new List<string>();
            foreach (PortDefenition portDef in clientPorts)
            {
                ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;

                String portDataStructureName = RteFunctionsGenerator.GeneratePortDataStructureDefenition(compDef, csInterface);

                if (!createdClientInterfaces.Contains(portDataStructureName))
                {
                    createdClientInterfaces.Add(portDataStructureName);
                    
                    writer.WriteLine("typedef struct " + portDataStructureName + " {");
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        string data = "    Std_ReturnType (*";
                        data += "Call_";
                        data += operation.Name + ")" + RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, false) + ";";
                        writer.WriteLine(data);
                    }
                    writer.WriteLine("}\n");
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
            String CDSname = RteFunctionsGenerator.ComponentDataStructureDefenitionName(compDef);
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
                    String portDatatype = RteFunctionsGenerator.GeneratePortDataStructureDefenition(compDef, srInterface, portDef.PortType);
                    writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
                }
                else if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;
                    String portDatatype = RteFunctionsGenerator.GeneratePortDataStructureDefenition(compDef, csInterface);
                    writer.WriteLine("    " + portDatatype + " " + portDef.Name + ";");
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
             String singleComponentInstanceVariableName = "Rte_Instance_" +compDef.Name;
             if (compDef.MultipleInstantiation == false)
             {
                writer.WriteLine("extern const " + CDSname + " "+ singleComponentInstanceVariableName + ";");
             }

             writer.WriteLine(@"
/*************************************************************
 * END Component Instance Handle
 *************************************************************/

/*************************************************************
 * BEGIN Runnable Entity
 *************************************************************/
");
            RteComponentGenerator.WriteAllFunctionWhichComponentCouldUse(compDef, writer);
            
            foreach (PeriodicRunnableDefenition runnable in compDef.Runnables)
            {
                writer.WriteLine(RteFunctionsGenerator.Generate_RunnableFunction(compDef, runnable) + ";");
            }

            writer.WriteLine(
@"/*************************************************************
 * END Runnable Entity
 *************************************************************/

/*************************************************************
 * BEGIN RTE API DEFINITIONS 
 *************************************************************/
");

            /* Write all cdata */
            foreach (CDataDefenition cdata in compDef.CDataDefenitions)
            {
                String define;
                if (compDef.MultipleInstantiation == false)
                {
                    define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortCDataFunctionName(cdata) + "()", singleComponentInstanceVariableName + ".CData_" + cdata.Name + "()", true);
                }
                else
                {
                    define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortCDataFunctionName(cdata) + "(instance)", "(instance)->CData_" + cdata.Name + "()", true);
                }
                
                writer.WriteLine(define);
            }


            /* Write all pims */
            foreach (PimDefenition pim in compDef.PerInstanceMemoryList)
            {
                String define;
                if (compDef.MultipleInstantiation == false)
                {
                    define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortPimFunctionName(pim) + "()", singleComponentInstanceVariableName + ".Pim_" + pim.Name, true);
                }
                else
                {
                    define = RteFunctionsGenerator.CreateDefine(RteFunctionsGenerator.GenerateShortPimFunctionName(pim) + "(instance)", "(instance)->Pim_" + pim.Name , true);
                }

                
                writer.WriteLine(define);
            }

            

            String externFunctions = "";

            /* Add defines for all ports */
            foreach (PortDefenition portDefenition in compDef.Ports)
            {
                if ((portDefenition.PortType == PortType.Sender) || (portDefenition.PortType == PortType.Receiver))
                {
                    SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        String funcName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDefenition, field);
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
                        rteFuncName += (portDefenition.PortType == PortType.Sender ? "Write_" : "Read_") + field.Name + "((_data_))";

                        string define = RteFunctionsGenerator.CreateDefine(funcName, rteFuncName, true);

                        writer.WriteLine(define);
                    }
                }
                else if (portDefenition.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = AutosarApplication.GetInstance().ClientServerInterfaces.FindObject(portDefenition.InterfaceGUID);
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        String funcName = RteFunctionsGenerator.Generate_RteCall_FunctionName(portDefenition, operation);
                        String defineArguments = RteFunctionsGenerator.GenerateClientServerInterfaceArgumentsForDefine(operation, compDef.MultipleInstantiation);
                        String argumentsWithoutInstance = RteFunctionsGenerator.GenerateClientServerInterfaceArgumentsForDefineWithoutInstance(operation, compDef.MultipleInstantiation);

                        String rteFuncName;
                        if (compDef.MultipleInstantiation == false)
                        {
                            rteFuncName = singleComponentInstanceVariableName + "." + portDefenition.Name + ".";
                        }
                        else
                        {
                            rteFuncName = "(instance)->" + portDefenition.Name + ".";
                        }
                        rteFuncName += "Call_" + operation.Name + "(" + argumentsWithoutInstance + ")";

                        String funcArgument = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);


                        String define = RteFunctionsGenerator.CreateDefine(funcName + defineArguments, rteFuncName, true);
                        writer.WriteLine(define);
                        
                    }
                }
            }
            writer.WriteLine(
@"
/*************************************************************
 * END RTE API DEFINITIONS 
 *************************************************************/
");
            RteFunctionsGenerator.CloseGuardDefine(writer);
            writer.Close();
        }
    }

}