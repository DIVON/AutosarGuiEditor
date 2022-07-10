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
using AutosarGuiEditor.Source.RteGenerator.TestGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteTestConnectionCGenerator
    {
        public void GenerateConnections(String folder)
        {
            String filename = folder + "\\" + Properties.Resources.RTE_CONNECTIONS_C_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");

            /*Add #include */
            RteFunctionsGenerator.AddInclude(writer, "<string.h>");
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, "<Rte_TestCommon.h>");

            /* Include */
            writer.WriteLine("");

            GenerateTestStubRecord(writer);
            GenerateAllPimBuffers(writer);
            GenerateAllCDataBuffers(writer);
            GenerateWriteFunctions(writer);
            GenerateReadFunctions(writer);
            GenerateSendFunctions(writer);
            GenerateReceiveFunctions(writer);
            GenerateCallFunctions(writer);
            GenerateCDataFunctions(writer);

            GenerateAllComponentInstances(writer);

            writer.Close();
        }

        void GenerateTestStubRecord(StreamWriter writer)
        {
            writer.WriteLine("TEST_STUB_RECORD_TYPE TEST_STUB_RECORD;");
            writer.WriteLine("");
        }

        void AddComponentIncludes(StreamWriter writer)
        {
            foreach(ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator.AddInclude(writer, "<Rte_" + compDef.Name + ".h>");
            }
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                RteFunctionsGenerator.AddInclude(writer, "<" + compDef.Name + "_TestRte.h>");
            }
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
                        String RteFuncName = RteFunctionsGenerator.GenerateInternalCDataFunctionName(component.Name, cdata);
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
                            foreach (ClientServerOperation operation in csInterface.Operations)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;

                                String RteFuncName = RteFunctionsGenerator.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                String fieldVariable = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, false);

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

                                    String functionName = RteFunctionsGenerator.Generate_RteCall_FunctionName(oppositCompInstance.ComponentDefenition, csEvent.Runnable);

                                    String arguments = RteFunctionsGenerator.Generate_ClientServerPort_Arguments(oppositCompInstance, operation, oppositCompInstance.ComponentDefenition.MultipleInstantiation);
                                    writer.WriteLine("    return " + functionName + arguments + ";");
                                }
                                else
                                {
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
                                String RteFuncName = RteFunctionsGenerator.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                int queueSize = srInterface.QueueSize;

                                String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, field);
                                String copyFromField = "TEST_STUB_RECORD." + component.Name + "." + fieldName;

                                writer.WriteLine("    Std_ReturnType _returnValue = RTE_E_NO_DATA;");
                                writer.WriteLine("");
                                writer.WriteLine("    uint32 head = " + copyFromField + ".Arguments.data.head;");
                                writer.WriteLine("    uint32 tail = " + copyFromField + ".Arguments.data.tail;");
                                writer.WriteLine("");
                                writer.WriteLine("    if (head != tail)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        (*data) = " + copyFromField + ".Arguments.data.elements[head % " + queueSize.ToString() + "U];");
                                writer.WriteLine("        " + copyFromField + ".Arguments.data.head = (head + 1U) % " + (queueSize * 2).ToString() + "U;");
                                writer.WriteLine("        _returnValue = RTE_E_OK | " + copyFromField + ".Arguments.data.overlayError;");
                                writer.WriteLine("        " + copyFromField + ".Arguments.data.overlayError = RTE_E_OK;");
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
                                String RteFuncName = RteFunctionsGenerator.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;
                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                                String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, field);
                                String testRecordField = "TEST_STUB_RECORD." + component.Name + "." + fieldName;

                                writer.WriteLine("    " + testRecordField + ".CallCount++;");
                                writer.WriteLine("    if (" + testRecordField + ".hook != NULL)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        " + testRecordField + ".hook(data);");
                                writer.WriteLine("    }");
                                writer.WriteLine("");

                                writer.WriteLine("    if (" + testRecordField + ".redirection == NULL)");
                                writer.WriteLine("    {");

                                    if (oppositCompInstance != null)
                                    {
                                        int queueSize = srInterface.QueueSize;

                                        String copyFromField = "TEST_STUB_RECORD." + oppositCompInstance.Name + "." + RteFunctionsGenerator.GenerateReadWriteFunctionName(oppositePort.PortDefenition, field);

                                        writer.WriteLine("        uint32 head = " + copyFromField + ".Arguments.data.head;");
                                        writer.WriteLine("        uint32 tail = " + copyFromField + ".Arguments.data.tail;");
                                        writer.WriteLine("");
                                        writer.WriteLine("        if ((head == tail) || ((head % " + queueSize.ToString() + "U) != (tail % " + queueSize.ToString() + "U)))");
                                        writer.WriteLine("        {");
                                        writer.WriteLine("            " + copyFromField + ".Arguments.data.elements[tail % " + queueSize.ToString() + "U] = (*data);");
                                        writer.WriteLine("            " + copyFromField + ".Arguments.data.tail = (tail + 1U) % " + (queueSize * 2).ToString() + "U;");
                                        writer.WriteLine("            return " + testRecordField + ".ReturnValue;");
                                        writer.WriteLine("        }");
                                        writer.WriteLine("        else");
                                        writer.WriteLine("        {");
                                        writer.WriteLine("            " + copyFromField + ".Arguments.data.overlayError = RTE_E_LOST_DATA;");
                                        writer.WriteLine("            return RTE_E_LIMIT;");
                                        writer.WriteLine("        }");
                                    }
                                    else
                                    {
                                        writer.WriteLine("    memset(data, 0, sizeof(" + field.DataTypeName + "));");
                                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                    }

                                writer.WriteLine("    }");
                                writer.WriteLine("    else");
                                writer.WriteLine("    {");
                                writer.WriteLine("        return " + testRecordField + ".redirection(data);");
                                writer.WriteLine("    }");

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

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Sender) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");


                                String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, field);
                                String testRecordField = "TEST_STUB_RECORD." + component.Name + "." + fieldName;

                                writer.WriteLine("    " + testRecordField + ".CallCount++;");
                                writer.WriteLine("    if (" + testRecordField + ".hook != NULL)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        " + testRecordField + ".hook(data);");
                                writer.WriteLine("    }");
                                writer.WriteLine("");

                                writer.WriteLine("    if (" + testRecordField + ".redirection == NULL)");
                                writer.WriteLine("    {");

                                /* Write data to the output port field of the current component */
                                if (field.IsPointer == false)
                                {
                                    writer.WriteLine("        " + testRecordField + ".Arguments.data = *data;");
                                }
                                else
                                {
                                    writer.WriteLine("        " + testRecordField + ".Arguments.data = data;");
                                }
                                
                                /* Write data to all opposite receiver ports */
                                PortPainter portPainter = component.GetPort(portDef);
                                List<PortPainter> oppositePorts = new List<PortPainter>();
                                AutosarApplication.GetInstance().GetOppositeComponentPorts(portPainter, oppositePorts);

                                if (component.Name == "McuInfoProvider")
                                {

                                }
                                foreach(PortPainter oppositePort in oppositePorts)
                                {
                                    ComponentInstance oppositeComponent = AutosarApplication.GetInstance().FindComponentInstanceByPort(oppositePort) as ComponentInstance;
                                    String oppositefieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(oppositePort.PortDefenition, field);
                                    String oppositeTestRecordField = "TEST_STUB_RECORD." + oppositeComponent.Name + "." + oppositefieldName;

                                    if (field.IsPointer == false)
                                    {
                                        writer.WriteLine("        " + oppositeTestRecordField + ".Arguments.data = *data;");
                                    }
                                    else
                                    {
                                        writer.WriteLine("        " + oppositeTestRecordField + ".Arguments.data = data;");
                                    }
                                }


                                writer.WriteLine("        return " + testRecordField + ".ReturnValue;");
                                writer.WriteLine("    }");
                                writer.WriteLine("    else");
                                writer.WriteLine("    {");
                                writer.WriteLine("        return " + testRecordField + ".redirection(data);");
                                writer.WriteLine("    }");

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

                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if ((portDef.PortType == PortType.Receiver) && ((portDef.InterfaceDatatype as SenderReceiverInterface).IsQueued == false))
                        {
                            SenderReceiverInterface srInterface = (portDef.InterfaceDatatype as SenderReceiverInterface);
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                String returnValue = Properties.Resources.STD_RETURN_TYPE;
                                String RteFuncName = RteFunctionsGenerator.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false);

                                writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
                                writer.WriteLine("{");

                                String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, field);
                                String testRecordField = "TEST_STUB_RECORD." + component.Name + "." + fieldName;

                                PortPainter portPainter = component.Ports.FindPortByItsDefenition(portDef);
                                ComponentInstance oppositCompInstance;
                                PortPainter oppositePort;

                                AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                               
                                writer.WriteLine("    " + testRecordField + ".CallCount++;");
                                writer.WriteLine("    if (" + testRecordField + ".hook != NULL)");
                                writer.WriteLine("    {");
                                writer.WriteLine("        " + testRecordField + ".hook(data);");
                                writer.WriteLine("    }");
                                writer.WriteLine("");

                                writer.WriteLine("    if (" + testRecordField + ".redirection == NULL)");
                                writer.WriteLine("    {");

                                if (oppositCompInstance != null)
                                {
                                    String myField = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, field);
                                    String copyFromField = "TEST_STUB_RECORD." + component.Name + "." + myField;

                                    writer.WriteLine("        *data = " + copyFromField + ".Arguments.data;");
                                    writer.WriteLine("        return " + testRecordField + ".ReturnValue;");
                                }
                                else
                                {
                                    writer.WriteLine("        memset(data, " + "0u, sizeof(" + field.DataTypeName + "));");
                                    writer.WriteLine("        return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                                }

                                writer.WriteLine("    }");
                                writer.WriteLine("    else");
                                writer.WriteLine("    {");
                                writer.WriteLine("        return " + testRecordField + ".redirection(data);");
                                writer.WriteLine("    }");

                                writer.WriteLine("}");
                                writer.WriteLine("");
                            }
                        }
                    }
                }
            }
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
                    String CDSname = RteFunctionsGenerator.ComponentDataStructureDefenitionName(compDef);
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
                                    String rteFuncName = RteFunctionsGenerator.GenerateInternalReadWriteConnectionFunctionName(component.Name, portDef, field);
                                    writer.WriteLine("        &" + rteFuncName + ",");
                                }
                            }
                            else
                            {
                                foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                                {
                                    String rteFuncName = RteFunctionsGenerator.GenerateInternalSendReceiveConnectionFunctionName(component.Name, portDef, field);
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
                                String rteFuncName = RteFunctionsGenerator.GenerateInternalCallConnectionFunctionName(component.Name, portDef, operation);
                                writer.WriteLine("        &" + rteFuncName + ",");
                            }
                            writer.WriteLine("    },");
                        }
                    }

                    /* write cdata  */
                    foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                    {
                        String rteFuncName = RteFunctionsGenerator.GenerateInternalCDataFunctionName(component.Name, cdata);
                        writer.WriteLine("    &" + rteFuncName + ",");
                    }

                    writer.WriteLine("};");
                    writer.WriteLine("");
                }
            }
        }
    }
}
