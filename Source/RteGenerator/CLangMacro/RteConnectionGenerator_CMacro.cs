using AutosarGuiEditor.Source.Autosar.Events;
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
using AutosarGuiEditor.Source.RteGenerator.CppLang;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using System;
using System.Collections.Generic;
using System.IO;

namespace AutosarGuiEditor.Source.RteGenerator.CMacro
{
    public class RteConnectionGenerator_CMacro
    {
        public void GenerateConnections(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_CONNECTIONS_C_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator_CMacro.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");

            /*Add #include */
            RteFunctionsGenerator_CMacro.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator_CMacro.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator_CMacro.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME);
            RteFunctionsGenerator_CMacro.AddInclude(writer, Properties.Resources.RTE_THREAD_PROTECTION_H_FILENAME);

            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_EXTERNALS_FILENAME);

            /* Include component headers for multipleInstance components (needed for CDS type definitions) */
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                if (compDef.MultipleInstantiation == true)
                {
                    RteFunctionsGenerator_CMacro.AddInclude(writer, "<Rte_" + compDef.Name + ".h>");
                }
            }

            GenerateAllPimBuffers(writer);
            GenerateAllCDataBuffers(writer);
            GenerateAllWriteDataBuffers(writer);
            GenerateAllAsyncServerNotificators(writer);

            GenerateQueuedDataBuffers(writer);

            /* Generate functions for multipleInstance components */
            GenerateWriteFunctions(writer);
            GenerateReadFunctions(writer);
            GenerateSendFunctions(writer);
            GenerateReceiveFunctions(writer);
            GenerateCallFunctions(writer);
            GeneratePimFunctions(writer);
            GenerateCDataFunctions(writer);

            GenerateAllComponentInstances(writer);

            /* Generate functions for non-multipleInstance components */
            GenerateNonMultipleInstanceFunctions(writer);

            writer.Close();
        }

        public static void AddComponentIncludes(StreamWriter writer)
        {
            writer.WriteLine("#define RTE_C");
            foreach(ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator_CMacro.AddInclude(writer, "<Rte_" + compDef.Name + ".h>");
            }
            writer.WriteLine("#undef RTE_C");
        }

        void GeneratePimFunctions(StreamWriter writer)
        {
            /* Generate PIM functions for multipleInstance components with instance parameter.
             * Functions are generated once per component definition (not per instance) to avoid duplicates.
             * Non-multipleInstance PIM functions are generated in GenerateNonMultipleInstanceFunctions. */
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                /* Only process multipleInstance components */
                if (compDef.MultipleInstantiation != true)
                {
                    continue;
                }

                String cdsName = RteFunctionsGenerator_CMacro.ComponentDataStructureDefenitionName(compDef);

                /* Find first component instance for buffer naming (PIM buffer names are instance-specific) */
                ComponentInstance firstComponentInstance = null;
                foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                {
                    foreach (ComponentInstance component in composition.ComponentInstances)
                    {
                        if (component.ComponentDefenition == compDef)
                        {
                            firstComponentInstance = component;
                            break;
                        }
                    }
                    if (firstComponentInstance != null)
                        break;
                }

                if (firstComponentInstance == null)
                    continue;

                /* Generate one function per PIM definition (not per instance) */
                foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
                {
                    String returnDatatype = pimDef.DataTypeName;
                    String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalPimFunctionName(compDef, pimDef);

                    writer.WriteLine(returnDatatype + " * " + RteFuncName + "(Rte_ComponentInstance instance)");
                    writer.WriteLine("{");
                    writer.WriteLine("    return ((" + cdsName + "*)instance)->Pim_" + pimDef.Name + ";");
                    writer.WriteLine("}");
                    writer.WriteLine("");
                }
            }
        }

        void GenerateCDataFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    /* Skip non-multipleInstance components - handled in GenerateNonMultipleInstanceFunctions */
                    if (compDef.MultipleInstantiation == false)
                    {
                        continue;
                    }

                    foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                    {
                        String returnDatatype = cdata.DataTypeName;
                        String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCDataFunctionName(component.Name, cdata);
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
            /* Only generate call functions for multipleInstance components.
             * Non-multipleInstance call functions are generated in GenerateNonMultipleInstanceFunctions. */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    /* Skip non-multipleInstance components - handled in GenerateNonMultipleInstanceFunctions */
                    if (compDef.MultipleInstantiation != true)
                    {
                        continue;
                    }

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if (portDef.PortType == PortType.Client)
                        {
                            ClientServerInterface csInterface = (portDef.InterfaceDatatype as ClientServerInterface);

                            /* Synchronous operation */
                            if (csInterface.IsAsync == false)
                            {
                                foreach (ClientServerOperation operation in csInterface.Operations)
                                {
                                    String returnValue = Properties.Resources.STD_RETURN_TYPE;

                                    String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                    String fieldVariable = RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArguments(operation, false);

                                    writer.WriteLine(returnValue + " " + RteFuncName + "(" + fieldVariable + ")");
                                    writer.WriteLine("{");
                                    PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                    ComponentInstance oppositCompInstance;//корректно найден
                                    PortPainter oppositePort; //корректно найден
                                    AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                                    if (oppositCompInstance != null)
                                    {
                                        /* Get assigned event from the CLIENT component (not server) */
                                        /* ClientServerEvent is stored in the client component */
                                        ApplicationSwComponentType oppositeCompDef = oppositCompInstance.ComponentDefenition;
                                        PortDefenition oppositePortDefinition = oppositePort.PortDefenition;

                                        ClientServerEvent csEvent = oppositeCompDef.GetEventsWithServerOperation(oppositePortDefinition, operation);

                                        String functionName = RteFunctionsGenerator_CMacro.Generate_RteCall_FunctionName(oppositCompInstance.ComponentDefenition, csEvent.Runnable);
                                        String arguments = RteFunctionsGenerator_CMacro.Generate_ClientServerPort_Arguments(oppositCompInstance, csEvent.SourceOperation, oppositCompInstance.ComponentDefenition.MultipleInstantiation);
                                        writer.WriteLine("    return " + functionName + arguments + ";");
                                    }
                                    else
                                    {
                                        /* Mark all parameters unused */
                                        foreach (ClientServerOperationField field in operation.Fields )
                                        {
                                            writer.WriteLine("    (void)" + field.Name + ";");
                                        }
                                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
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

                                    String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);

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

                                        writer.WriteLine("    return RTE_E_OK;");
                                    }
                                    else
                                    {
                                        /* Mark all parameters unused */
                                        foreach (ClientServerOperationField field in operation.Fields)
                                        {
                                            writer.WriteLine("    (void)" + field.Name + ";");
                                        }
                                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    }

                                    writer.WriteLine("}");
                                    writer.WriteLine("");
                                }
                            }
                        }
                    }
                }
            }
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
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                }

                                int queueSize = srInterface.QueueSize;

                                String copyFromField = "Rte_ReceiveBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name;

                                writer.WriteLine("    Std_ReturnType _returnValue = RTE_E_NO_DATA;");
                                writer.WriteLine("");
                                writer.WriteLine("    uint32 head = " + copyFromField + ".head;");
                                writer.WriteLine("    uint32 tail = " + copyFromField + ".tail;");
                                writer.WriteLine("");
                                writer.WriteLine("    if (head != tail)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        (*data) = " + copyFromField + ".elements[head % " + queueSize.ToString() + "U];");
                                writer.WriteLine("        " + copyFromField + ".head = (head + 1U) % " + (queueSize * 2).ToString() + "U;");
                                writer.WriteLine("        _returnValue = RTE_E_OK | " + copyFromField + ".overlayError;");
                                writer.WriteLine("        " + copyFromField + ".overlayError = RTE_E_OK;");
                                writer.WriteLine("    }");
                                writer.WriteLine("");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                }

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
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);

                                List<PortPainter> oppositePorts = new List<PortPainter>();

                                AutosarApplication.GetInstance().GetOppositeComponentPorts(portPainter, oppositePorts);
                                bool connected = false;
                                if (oppositePorts.Count != 0)
                                {
                                    foreach (PortPainter oppositePort in oppositePorts)
                                    {
                                        ComponentInstance oppositCompInstance = AutosarApplication.GetInstance().FindComponentInstanceByPort(oppositePort) as ComponentInstance;

                                        if (oppositCompInstance != null)
                                        {
                                            if (connected == false)
                                            {
                                                if (srInterface.IsThreadIrqProtected == true)
                                                {
                                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                                }
                                                writer.WriteLine();
                                                writer.WriteLine("    uint32 head;");
                                                writer.WriteLine("    uint32 tail;");
                                                writer.WriteLine("    Std_ReturnType _returnValue = RTE_E_OK;");

                                                connected = true;
                                            }

                                            int queueSize = srInterface.QueueSize;

                                            String copyFromField = "Rte_ReceiveBuffer_" + oppositCompInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + field.Name;

                                            writer.WriteLine("");
                                            writer.WriteLine("    head = " + copyFromField + ".head;");
                                            writer.WriteLine("    tail = " + copyFromField + ".tail;");
                                            writer.WriteLine("");
                                            writer.WriteLine("    if ((head == tail) || ((head % " + queueSize.ToString() + "U) != (tail % " + queueSize.ToString() + "U)))");
                                            writer.WriteLine("    {");
                                            writer.WriteLine("        " + copyFromField + ".elements[tail % " + queueSize.ToString() + "U] = (*data);");
                                            writer.WriteLine("        " + copyFromField + ".tail = (tail + 1U) % " + (queueSize * 2).ToString() + "U;");
                                            writer.WriteLine("    }");
                                            writer.WriteLine("    else");
                                            writer.WriteLine("    {");
                                            writer.WriteLine("        " + copyFromField + ".overlayError = RTE_E_LOST_DATA;");
                                            writer.WriteLine("        _returnValue = RTE_E_LIMIT;");
                                            writer.WriteLine("    }");
                                        }
                                    }
                                    if (connected)
                                    {
                                        if (srInterface.IsThreadIrqProtected == true)
                                        {
                                            writer.WriteLine();
                                            writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                        }

                                        writer.WriteLine();
                                        writer.WriteLine("    return _returnValue;");
                                    }
                                    else
                                    {
                                        /* Mark all parameters unused */
                                        writer.WriteLine("    (void)data;");
                                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    }
                                }
                                else
                                {
                                    if (srInterface.IsThreadIrqProtected == true)
                                    {
                                        writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                    }

                                    writer.WriteLine("    (void)data;");
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
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
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    /* Skip non-multipleInstance components - handled in GenerateNonMultipleInstanceFunctions */
                    if (compDef.MultipleInstantiation == false)
                    {
                        continue;
                    }

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                }

                                String writeFieldName = "Rte_DataBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name;
                                if (field.IsPointer == false)
                                {


                                    if (!(field.DataType is ArrayDataType))
                                    {
                                        writer.WriteLine("    " + writeFieldName + " = (*data);");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    memcpy("+ writeFieldName + ", (*data), sizeof(" + field.DataTypeName + "));");
                                    }
                                }
                                else
                                {
                                    writer.WriteLine("    " + writeFieldName + " = data;");
                                }

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                }

                                writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
        }

        void GenerateReadFunctions(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    /* Skip non-multipleInstance components - handled in GenerateNonMultipleInstanceFunctions */
                    if (compDef.MultipleInstantiation == false)
                    {
                        continue;
                    }

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                }
                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;
                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                                if (oppositCompInstance != null)
                                {
                                    String copyFromField = "Rte_DataBuffer_" + oppositCompInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + field.Name;

                                    if (!(field.DataType is ArrayDataType))
                                    {
                                        writer.WriteLine("    *data = " + copyFromField + ";");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    memcpy(*data, " + copyFromField + ", sizeof(" + field .DataTypeName + "));");
                                    }

                                    if (srInterface.IsThreadIrqProtected == true)
                                    {
                                        writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                    }
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                                }
                                else
                                {
                                    writer.WriteLine("    memset(data, " + "0, sizeof(" + field.DataTypeName + "));");
                                    if (srInterface.IsThreadIrqProtected == true)
                                    {
                                        writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                    }
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                }

                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
        }

        void GenerateAllWriteDataBuffers(StreamWriter writer)
        {
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
                            }
                        }
                    }
                }
            }
            writer.WriteLine("");
        }

        public static void GenerateAllAsyncServerNotificators(StreamWriter writer, Boolean isExtern = false)
        {
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
                                String asyncField = "boolean Rte_AsyncCall_" + component.Name + "_" + portDef.Name + "_" + operation.Name + ";";
                                if (isExtern)
                                {
                                    asyncField = "extern " + asyncField;
                                }
                                writer.WriteLine(asyncField);
                            }
                        }
                    }
                }
            }
            writer.WriteLine("");
        }

        public static void GenerateExternComponentInstances(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    if (!component.ComponentDefenition.MultipleInstantiation)
                    {
                        // Структуры генерируются только для MultipleInstance компонентов
                        continue;
                    }
                    ApplicationSwComponentType compDef = component.ComponentDefenition;
                    String CDSname = RteFunctionsGenerator_CMacro.ComponentDataStructureDefenitionName(compDef);
                    writer.WriteLine("extern const " + CDSname + " Rte_Instance_" + component.Name + ";");
                }
            }
            writer.WriteLine("");
        }

        void GenerateQueuedDataBuffers(StreamWriter writer)
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
                                String datatype = field.QueuedInterfaceName(srInterface.Name);
                                String varName  = "Rte_ReceiveBuffer_" + component.Name + "_" + portDef.Name + "_" + field.Name + ";";
                                writer.WriteLine(datatype + " " + varName);
                            }
                        }
                    }
                }
            }
            writer.WriteLine("");
        }

        void GenerateAllPimBuffers(StreamWriter writer)
        {
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
                    }
                }
            }
            writer.WriteLine("");
        }

        void GenerateAllCDataBuffers(StreamWriter writer)
        {
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
                    }
                }
            }
            writer.WriteLine("");
        }

        void GenerateAllComponentInstances(StreamWriter writer)
        {
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    ApplicationSwComponentType compDef = component.ComponentDefenition;

                    /* For non-multipleInstance components, do not generate structures */
                    if (compDef.MultipleInstantiation == false)
                    {
                        continue;
                    }

                    String CDSname = RteFunctionsGenerator_CMacro.ComponentDataStructureDefenitionName(compDef);
                    writer.WriteLine("const " + CDSname + " Rte_Instance_" + component.Name + " = ");
                    writer.WriteLine("{");

                    /* write pims first */
                    foreach (PimDefenition pim in compDef.PerInstanceMemoryList)
                    {
                        String pimName = "Rte_PimBuffer_" + component.Name + "_" + pim.Name;
                        writer.WriteLine("    &" + pimName + ",");
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
                                    String rteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                    writer.WriteLine("        &" + rteFuncName + ",");
                                }
                            }
                            else
                            {
                                foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                                {
                                    String rteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
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
                                String rteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                writer.WriteLine("        &" + rteFuncName + ",");
                            }
                            writer.WriteLine("    },");
                        }
                    }

                    /* write cdata  */
                    foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                    {
                        String rteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCDataFunctionName(component.Name, cdata);
                        writer.WriteLine("    &" + rteFuncName + ",");
                    }

                    writer.WriteLine("};");
                    writer.WriteLine("");
                }
            }
        }

        void GenerateNonMultipleInstanceFunctions(StreamWriter writer)
        {
            /* Generate functions for non-multipleInstance components */
            /* We iterate over component definitions, not instances, and use definition names */
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                /* Only process non-multipleInstance components */
                if (compDef.MultipleInstantiation == true)
                {
                    continue;
                }

                /* Find the component instance for buffer naming (there should be only one for non-multipleInstance) */
                ComponentInstance componentInstance = null;
                foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                {
                    foreach (ComponentInstance component in composition.ComponentInstances)
                    {
                        if (component.ComponentDefenition == compDef)
                        {
                            componentInstance = component;
                            break;
                        }
                    }
                    if (componentInstance != null)
                        break;
                }

                if (componentInstance == null)
                    continue;

                /* Generate write functions - inline without static for non-multipleInstance */
                /* Use compDef.Name (definition name) for function naming */
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if ((portDef.PortType == PortType.Sender) && (portDef.InterfaceDatatype is SenderReceiverInterface))
                    {
                        SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                        if (srInterface.IsQueued == false)
                        {
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalReadWriteConnectionFunctionName(compDef.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine("ALWAYS_INLINE " + Properties.Resources.STD_RETURN_TYPE + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                }

                                String writeFieldName = "Rte_DataBuffer_" + componentInstance.Name + "_" + portDef.Name + "_" + field.Name;
                                if (field.IsPointer == false)
                                {
                                    if (!(field.DataType is AutosarGuiEditor.Source.DataTypes.ArrayDataType.ArrayDataType))
                                    {
                                        writer.WriteLine("    " + writeFieldName + " = (*data);");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    memcpy(" + writeFieldName + ", (*data), sizeof(" + field.DataTypeName + "));");
                                    }
                                }
                                else
                                {
                                    writer.WriteLine("    " + writeFieldName + " = data;");
                                }

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                }

                                writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }

                /* Generate read functions - inline without static for non-multipleInstance */
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if ((portDef.PortType == PortType.Receiver) && (portDef.InterfaceDatatype is SenderReceiverInterface))
                    {
                        SenderReceiverInterface srInterface = portDef.InterfaceDatatype as SenderReceiverInterface;
                        if (srInterface.IsQueued == false)
                        {
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalReadWriteConnectionFunctionName(compDef.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator_CMacro.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine("ALWAYS_INLINE " + Properties.Resources.STD_RETURN_TYPE + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                if (srInterface.IsThreadIrqProtected == true)
                                {
                                    writer.WriteLine("    OnBefore_" + RteFuncName + "();");
                                }

                                PortPainter portPainter = null;
                                foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                                {
                                    foreach (ComponentInstance component in composition.ComponentInstances)
                                    {
                                        if (component.ComponentDefenition == compDef)
                                        {
                                            portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                            break;
                                        }
                                    }
                                    if (portPainter != null)
                                        break;
                                }

                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;
                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                                if (oppositCompInstance != null)
                                {
                                    String copyFromField = "Rte_DataBuffer_" + oppositCompInstance.Name + "_" + oppositePort.PortDefenition.Name + "_" + field.Name;

                                    if (!(field.DataType is AutosarGuiEditor.Source.DataTypes.ArrayDataType.ArrayDataType))
                                    {
                                        writer.WriteLine("    *data = " + copyFromField + ";");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    memcpy(*data, " + copyFromField + ", sizeof(" + field.DataTypeName + "));");
                                    }

                                    if (srInterface.IsThreadIrqProtected == true)
                                    {
                                        writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                    }
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                                }
                                else
                                {
                                    writer.WriteLine("    memset(data, 0, sizeof(" + field.DataTypeName + "));");
                                    if (srInterface.IsThreadIrqProtected == true)
                                    {
                                        writer.WriteLine("    OnAfter_" + RteFuncName + "();");
                                    }
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                }

                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }

                /* Generate call functions - inline without static for non-multipleInstance */
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if (portDef.PortType == PortType.Client && (portDef.InterfaceDatatype is ClientServerInterface))
                    {
                        ClientServerInterface csInterface = portDef.InterfaceDatatype as ClientServerInterface;

                        foreach (ClientServerOperation operation in csInterface.Operations)
                        {
                            String returnValue = Properties.Resources.STD_RETURN_TYPE;
                            String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCallConnectionFunctionName(compDef.Name, portDef, operation);
                            String fieldVariable = RteFunctionsGenerator_CMacro.GenerateClientServerInterfaceArguments(operation, false);

                            writer.WriteLine("ALWAYS_INLINE " + returnValue + " " + RteFuncName + "(" + fieldVariable + ")");
                            writer.WriteLine("{");

                            PortPainter portPainter = null;
                            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                            {
                                foreach (ComponentInstance component in composition.ComponentInstances)
                                {
                                    if (component.ComponentDefenition == compDef)
                                    {
                                        portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                        break;
                                    }
                                }
                                if (portPainter != null)
                                    break;
                            }

                            ComponentInstance oppositCompInstance;
                            PortPainter oppositePort;
                            AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                            if (oppositCompInstance != null)
                            {
                                /* Get assigned event from the CLIENT component definition (not server) */
                                /* ClientServerEvent is stored in the server component */
                                ClientServerEvent csEvent = oppositCompInstance.ComponentDefenition.GetEventsWithServerOperation(oppositePort.PortDefenition, operation);

                                if (csEvent == null)
                                {
                                    /* Mark all parameters unused */
                                    foreach (ClientServerOperationField field in operation.Fields)
                                    {
                                        writer.WriteLine("    (void)" + field.Name + ";");
                                    }
                                    writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    writer.WriteLine("}");
                                    writer.WriteLine("");
                                    continue;
                                }

                                /* Get the server runnable from the event */
                                RunnableDefenition serverRunnable = csEvent.Runnable;
                                ApplicationSwComponentType serverCompDef = oppositCompInstance.ComponentDefenition;

                                String functionName = RteFunctionsGenerator_CMacro.Generate_RteCall_FunctionName(serverCompDef, serverRunnable);

                                /* Determine if we need to pass instance parameter */
                                if (oppositCompInstance.ComponentDefenition.MultipleInstantiation == false)
                                {
                                    /* Target is non-multipleInstance: call without instance */
                                    String arguments = RteFunctionsGenerator_CMacro.Generate_ClientServerPort_ArgumentsWithoutInstance(oppositCompInstance, operation);
                                    writer.WriteLine("    return " + functionName + arguments + ";");
                                }
                                else
                                {
                                    /* Target is multipleInstance: pass the server instance pointer */
                                    String arguments = RteFunctionsGenerator_CMacro.Generate_ClientServerPort_ArgumentsForNonMultipleClientToMultipleServer(componentInstance, oppositCompInstance, operation);
                                    writer.WriteLine("    return " + functionName + arguments + ";");
                                }
                            }
                            else
                            {
                                /* Mark all parameters unused */
                                foreach (ClientServerOperationField field in operation.Fields)
                                {
                                    writer.WriteLine("    (void)" + field.Name + ";");
                                }
                                writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                            }

                            writer.WriteLine("}");
                            writer.WriteLine("");
                        }
                    }
                }

                /* Generate CData functions */
                foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                {
                        String returnDatatype = cdata.DataTypeName;
                        String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalCDataFunctionName(compDef.Name, cdata);

                        writer.WriteLine("ALWAYS_INLINE " + returnDatatype + " " + RteFuncName + "(void)");
                    writer.WriteLine("{");
                    writer.WriteLine("    return Rte_CDataBuffer_" + componentInstance.Name + "_" + cdata.Name + ";");
                    writer.WriteLine("}");
                    writer.WriteLine("");
                }

                /* Generate PIM functions - these return pointers */
                foreach (PimInstance pim in componentInstance.PerInstanceMemories)
                {
                    String returnDatatype = pim.Defenition.DataTypeName;
                    String RteFuncName = RteFunctionsGenerator_CMacro.GenerateInternalPimFunctionName(compDef, pim.Defenition);
                    String pimName = "Rte_PimBuffer_" + componentInstance.Name + "_" + pim.Name;

                    writer.WriteLine("ALWAYS_INLINE " + returnDatatype + " * " + RteFuncName + "(void)");
                    writer.WriteLine("{");
                    writer.WriteLine("    return &" + pimName + ";");
                    writer.WriteLine("}");
                    writer.WriteLine("");
                }
            }
        }
    }
}
