﻿using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CppLang
{
    public class RteConnectionGenerator_Cpp
    {
        public void GenerateConnections(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_CONNECTIONS_CPP_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");

            /*Add #include */
            RteFunctionsGenerator_Cpp.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_EXTERNALS_HPP_FILENAME);
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_HPP_FILENAME);
            
            AddComponentIncludes(writer);

            /* Include */
            writer.WriteLine(
@"
/*************************************************************
 * BEGIN RTE buffers
 *************************************************************/
");
            GenerateAllPimBuffers(writer);
            GenerateAllCDataBuffers(writer);
            GenerateAllWriteDataBuffers(writer);
            GenerateAllAsyncServerNotificators(writer);
            GenerateQueuedDataBuffers(writer);

            writer.WriteLine(
@"/*************************************************************
 * END RTE buffers
 *************************************************************/
");

            GenerateWriteFunctions(writer);
            GenerateReadFunctions(writer);
            GenerateSendFunctions(writer);
            GenerateReceiveFunctions(writer);
            GenerateCallFunctions(writer);
            GenerateCDataFunctions(writer);

            GenerateAllRteComponentInstances(writer);
            GenerateAllComponentInstances(writer);
            writer.Close();


            RtePortInterfaceGenerator portInterfaceGenerator = new RtePortInterfaceGenerator();
            portInterfaceGenerator.GeneratePortInterfaces(folder);
        }

        public static void AddComponentIncludes(StreamWriter writer)
        {
            writer.WriteLine("#define RTE_CPP");
            foreach(ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator_Cpp.AddInclude(writer, "<" + compDef.Name + ".hpp>");
            }
            writer.WriteLine("#undef RTE_CPP");
        }

        public void AddComponentIncludesWithoutRte(StreamWriter writer)
        {
            writer.WriteLine("#define RTE_CPP");
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator_Cpp.AddInclude(writer, "<" + compDef.Name + ".hpp>");
            }
            writer.WriteLine("#undef RTE_CPP");
        }

        void GenerateCDataFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                    {
                        String returnDatatype = cdata.DataTypeName;
                        String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalCDataFunctionName(component.Name, cdata);
                        String cdataName = "Rte_CDataBuffer_" + component.Name + "_" + cdata.Name;

                        writer.WriteLine(returnDatatype + " " + RteFuncName + "(void)");
                        writer.WriteLine("{");
                        writer.WriteLine("    return " + cdataName + ";");
                        writer.WriteLine("}");
                        writer.WriteLine("");
                    }
                }
            }
        }

        void GenerateCallFunctions(StreamWriter writer)
        {
            writer.WriteLine(
@"/*************************************************************
 * BEGIN RTE Sync Call operation handlers
 *************************************************************/
");
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if (portDef.PortType == PortType.Client) 
                        {
                            ClientServerInterface csInterface = (portDef.InterfaceDatatype as ClientServerInterface);

                            /* Syncronous operation */
                            if (csInterface.IsAsync == false)
                            {
                                foreach (ClientServerOperation operation in csInterface.Operations)
                                {
                                    String returnValue = Properties.Resources.STD_RETURN_TYPE;

                                    String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                    String fieldVariable = RteFunctionsGenerator_Cpp.GenerateClientServerInterfaceArguments(operation, false);

                                    writer.WriteLine(returnValue + " " + RteFuncName + "(" + fieldVariable + ")");
                                    writer.WriteLine("{");
                                    PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                    ComponentInstance oppositCompInstance;
                                    PortPainter oppositePort;
                                    AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                                    if (oppositCompInstance != null)
                                    {
                                        /* Get assigned event */
                                        ClientServerEvent csEvent = oppositCompInstance.ComponentDefenition.GetEventsWithServerOperation(operation);
                                        
                                        String arguments = RteFunctionsGenerator_Cpp.Generate_ClientServerPort_Arguments(oppositCompInstance, operation, oppositCompInstance.ComponentDefenition.MultipleInstantiation);
                                        writer.WriteLine("    return Rte_CI_" + oppositCompInstance.Name +".ru" + csEvent.Runnable.Name + arguments + ";");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    }

                                    writer.WriteLine("}");
                                    writer.WriteLine("");
                                }
                            }
                            else
                            {
                                /* Asyncronous operation */
                                foreach (ClientServerOperation operation in csInterface.Operations)
                                {
                                    String returnValue = Properties.Resources.STD_RETURN_TYPE;

                                    String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);

                                    writer.WriteLine(returnValue + " " + RteFuncName + "(void)");
                                    writer.WriteLine("{");
                                    PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                    
                                    List<PortPainter> oppositePorts = new List<PortPainter>();

                                    AutosarApplication.GetInstance().GetOppositeComponentPorts(portPainter, oppositePorts);
                                    if (oppositePorts.Count > 0)
                                    {
                                        foreach(PortPainter oppositePort in oppositePorts)
                                        {
                                            ComponentInstance compInstance = AutosarApplication.GetInstance().FindComponentInstanceByPort(oppositePort) as ComponentInstance;

                                            String asyncField = "Rte_AsyncCall_" + compInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + operation.Name;
                                            writer.WriteLine("    " + asyncField + " = TRUE;");
                                        }
                                        
                                        writer.WriteLine("    return Std_ReturnType::RTE_E_OK;");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    }

                                    writer.WriteLine("}");
                                    writer.WriteLine("");
                                }
                            }
                        }
                    }
                }
            }
            writer.WriteLine(
@"/*************************************************************
 * END RTE Sync Call operation handlers
 *************************************************************/
");
        }

        void GenerateReceiveFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                int queueSize = srInterface.QueueSize;

                                String copyFromField = "Rte_ReceiveBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name;

                                writer.WriteLine("    Std_ReturnType _returnValue = Std_ReturnType::RTE_E_NO_DATA;");
                                writer.WriteLine("");
                                writer.WriteLine("    uint32 head = " + copyFromField + ".head;");
                                writer.WriteLine("    uint32 tail = " + copyFromField + ".tail;");
                                writer.WriteLine("");
                                writer.WriteLine("    if (head != tail)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        (*data) = " + copyFromField + ".elements[head % " + queueSize.ToString() + "U];");
                                writer.WriteLine("        " + copyFromField + ".head = (head + 1U) % " + (queueSize * 2).ToString() + "U;");
                                writer.WriteLine("        _returnValue = Std_ReturnType::RTE_E_OK | " + copyFromField + ".overlayError;");
                                writer.WriteLine("        " + copyFromField + ".overlayError = Std_ReturnType::RTE_E_OK;");
                                writer.WriteLine("    }");
                                writer.WriteLine("");
                                writer.WriteLine("    return _returnValue;");  

                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
        }

        void GenerateSendFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;
                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                                if (oppositCompInstance != null)
                                {                        
                                    int queueSize = srInterface.QueueSize;

                                    String copyFromField = "Rte_ReceiveBuffer_" + oppositCompInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + field.Name;

                                    writer.WriteLine("    Std_ReturnType _returnValue = Std_ReturnType::RTE_E_OK;");
                                    writer.WriteLine("");
                                    writer.WriteLine("    uint32 head = " + copyFromField + ".head;");  
                                    writer.WriteLine("    uint32 tail = " + copyFromField + ".tail;");
                                    writer.WriteLine("");
                                    writer.WriteLine("    if ((head == tail) || ((head % " +queueSize.ToString() + "U) != (tail % "+queueSize.ToString()+"U)))");
                                    writer.WriteLine("    {");  
                                    writer.WriteLine("        " + copyFromField + ".elements[tail % " + queueSize.ToString() + "U] = (*data);");  
                                    writer.WriteLine("        " + copyFromField + ".tail = (tail + 1U) % " + (queueSize * 2).ToString() + "U;");  
                                    writer.WriteLine("    }");  
                                    writer.WriteLine("    else"); 
                                    writer.WriteLine("    {");  
                                    writer.WriteLine("        " + copyFromField + ".overlayError = RTE_E_LOST_DATA;");  
                                    writer.WriteLine("        _returnValue = RTE_E_LIMIT;");
                                    writer.WriteLine("    }");  
                                    writer.WriteLine("");
                                    writer.WriteLine("    return _returnValue;");  
                                }
                                else
                                {
                                    writer.WriteLine("    memset(&data, 0, sizeof(" + field.DataTypeName + "));");
                                    writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                }
                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
        }

        void GenerateWriteFunctions(StreamWriter writer)
        {
            writer.WriteLine(
@"/*************************************************************
 * BEGIN RTE Write operation handlers
 *************************************************************/
");
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                String writeFieldName = "Rte_DataBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name;
                                if (field.IsPointer == false)
                                {
                                    writer.WriteLine("    " + writeFieldName + " = data;");
                                }
                                else
                                {
                                    writer.WriteLine("    " + writeFieldName + " = data;");
                                }

                                writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_OK + ";");
                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
            writer.WriteLine(
@"/*************************************************************
 * END RTE Write operation handlers
 *************************************************************/
");
        }

        void GenerateReadFunctions(StreamWriter writer)
        {
            writer.WriteLine(
@"/*************************************************************
 * BEGIN RTE Read operation handlers
 *************************************************************/
");
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");


                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;
                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                                if (oppositCompInstance != null)
                                {
                                    String copyFromField = "Rte_DataBuffer_" + oppositCompInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + field.Name;

                                    writer.WriteLine("    data = " + copyFromField + ";");
                                    writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_OK + ";");
                                }
                                else
                                {
                                    writer.WriteLine("    memset(&data, " + "0, sizeof(" + field.DataTypeName + "));");
                                    writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                }

                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
            writer.WriteLine(
@"/*************************************************************
 * END RTE Read operation handlers
 *************************************************************/
");
        }

        void GenerateAllWriteDataBuffers(StreamWriter writer)
        {
            Boolean lineAdded = false;

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {                        
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                if (field.IsPointer == false)
                                {
                                    String fieldData = field.DataTypeName + " Rte_DataBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name + ";";
                                    writer.WriteLine(fieldData);
                                }
                                else
                                {
                                    String fieldData = field.DataTypeName + " * Rte_DataBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name + ";";
                                    writer.WriteLine(fieldData);
                                }

                                lineAdded = true;
                            }
                        }
                    }
                }
            }
            if (lineAdded)
            {
                writer.WriteLine("");
            }
        }

        public static void GenerateAllAsyncServerNotificators(StreamWriter writer, Boolean isExtern = false)
        {
            Boolean lineAdded = false;

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Server) && ((portDef.InterfaceDatatype as ClientServerInterface).IsAsync == true))
                        {
                            ClientServerInterface csInterface = (portDef.InterfaceDatatype as ClientServerInterface);

                            foreach (ClientServerOperation operation in csInterface.Operations)
                            {
                                String asyncField = "bool Rte_AsyncCall_" + component.Name + "_" + portDef.Name + "_" + operation.Name + ";";
                                if (isExtern)
                                {
                                    asyncField = "extern " + asyncField;
                                }
                                writer.WriteLine(asyncField);

                                lineAdded = true;
                            }
                        }
                    }
                }
            }
            if (lineAdded)
            {
                writer.WriteLine("");
            }
        }

        public static void GenerateExternComponentInstances(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    if ((compDef.Ports.Count == 0) && (compDef.Runnables.Count == 0)) {
                        continue;
                    }
                    writer.WriteLine("extern " + compDef.Name + " Rte_CI_" + component.Name + ";");
                }  
            }
            writer.WriteLine("");
        }

        void GenerateQueuedDataBuffers(StreamWriter writer)
        {
            Boolean lineAdded = false;

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PortDefenition portDef in compDef.Ports)
                    {                        
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String datatype = field.QueuedInterfaceName(srInterface.Name);
                                String varName  = "Rte_ReceiveBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name + ";";
                                writer.WriteLine(datatype + " " + varName);
                                lineAdded = true;
                            }
                        }
                    }
                }
            }
            if (lineAdded)
            {
                writer.WriteLine("");
            }
        }

        void GenerateAllPimBuffers(StreamWriter writer)
        {
            Boolean lineAdded = false;

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (PimInstance pim in component.PerInstanceMemories)
                    {
                        String pimName = pim.Defenition.DataTypeName + " Rte_PimBuffer_" + component.Name + "_" + pim.Name;
                        String defaultValue = pim.GetDefaultValue();
                        if (defaultValue.Length > 0)
                        {
                            pimName += " = " + defaultValue + ";";
                        }
                        else
                        {
                            pimName += ";";
                        }
                        writer.WriteLine(pimName);

                        lineAdded = true;
                    }
                }
            }
            if (lineAdded)
            {
                writer.WriteLine("");
            }
        }

        void GenerateAllCDataBuffers(StreamWriter writer)
        {
            Boolean lineAdded = false;

            /* Without multiple instantiation */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    foreach (CDataInstance cdata in component.CDataInstances)
                    {
                        String cdataName = "Rte_CDataBuffer_" + component.Name + "_" + cdata.Name;
                        String defValue = cdata.GetDefaultValue();
                        String writeString = "const " + cdata.Defenition.DataTypeName + " " + cdataName + " = " + defValue + ";";
                        writer.WriteLine(writeString);

                        lineAdded = true;
                    }
                }
            }
            if (lineAdded)
            {
                writer.WriteLine("");
            }
        }

        void GenerateAllRteComponentInstances(StreamWriter writer)
        {
            writer.WriteLine(
@"/*************************************************************
 * BEGIN RTE Communication Objects
 *************************************************************/
");

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    
                    String CDSname = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDef);
                    writer.WriteLine("const " + CDSname + " Rte_CO_" + component.Name + " = ");
                    writer.WriteLine("{");

                    /* write pims first */
                    foreach (PimDefenition pim in compDef.PerInstanceMemoryList)
                    {
                        String pimName = "Rte_PimBuffer_" + component.Name + "_" + pim.Name;
                        writer.WriteLine("    " + pimName + ",");
                    }

                    /* write ports */
                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        

                        if (portDef.InterfaceDatatype is SenderReceiverInterface)
                        {
                            writer.WriteLine("    {");
                            SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                            if (srInterface.IsQueued == false)
                            {
                                foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                                {
                                    String rteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                    writer.WriteLine("        &" + rteFuncName + ",");
                                }
                            }
                            else
                            {
                                foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                                {
                                    String rteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                    writer.WriteLine("        &" + rteFuncName + ",");
                                }
                            }
                            writer.WriteLine("    },");
                        }
                        else if ((portDef.PortType == PortType.Client) && (portDef.InterfaceDatatype is ClientServerInterface) )
                        {
                            writer.WriteLine("    {");
                            ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;

                            foreach (ClientServerOperation operation in csInterface.Operations)
                            {
                                String rteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                writer.WriteLine("        &" + rteFuncName + ",");
                            }
                            writer.WriteLine("    },");
                        }
                    }

                    /* write cdata  */
                    foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                    {
                        String rteFuncName = RteFunctionsGenerator_Cpp.GenerateInternalCDataFunctionName(component.Name, cdata);
                        writer.WriteLine("    &" + rteFuncName + ",");
                    }

                    writer.WriteLine("};");
                    writer.WriteLine("");
                }
            }

            writer.WriteLine(
@"/*************************************************************
 * END RTE Communication Objects
 *************************************************************/
");
        }

        void GenerateAllComponentInstances(StreamWriter writer)
        {
            writer.WriteLine(
@"/*************************************************************
 * BEGIN Component Instances
 *************************************************************/
");

            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    if ((compDef.Runnables.Count == 0) && (compDef.Ports.Count == 0))
                    {
                        continue;
                        
                    }
                    String rteCommunicationObject = "Rte_CO_" + component.Name;
                    writer.WriteLine(compDef.Name + " Rte_CI_" + component.Name + "(" + rteCommunicationObject + ");");
                }                
            }

            writer.WriteLine(
@"
/*************************************************************
 * END Component Instances
 *************************************************************/
");
        }
    }
}
