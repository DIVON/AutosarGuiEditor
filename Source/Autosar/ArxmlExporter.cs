using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.DataTypes;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;

namespace AutosarGuiEditor.Source.Autosar
{
    public class ArxmlExporter
    {
        private AutosarApplication _app;
        private int _eventIndex;

        private static readonly string ArNamespace = "http://autosar.org/schema/r4.0";

        public bool ExportToArxml(AutosarApplication app, string filePath)
        {
            _app = app;
            _eventIndex = 0;

            XDocument document = CreateArxmlDocument();
            
            // Save with pretty printing (indented XML)
            using (var writer = XmlWriter.Create(filePath, new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false), // false = no BOM
                Indent = true,
                OmitXmlDeclaration = false,
                NewLineHandling = NewLineHandling.Replace,
                NewLineOnAttributes = false
            }))
            {
                document.Save(writer);
            }
            
            return true;
        }

        private XDocument CreateArxmlDocument()
        {
            XNamespace ns = XNamespace.Get(ArNamespace);

            // Package hierarchy children
            XElement baseDataTypesHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "BaseDataTypes"));

            XElement userDefinedTypesHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "UserDefinedTypes"));

            XElement dataDefinitionsHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "DataDefinitions"),
                baseDataTypesHierarchy,
                userDefinedTypesHierarchy);

            XElement interfacesHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "Interfaces"));

            XElement swComponentDefinitionsHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "SwComponentDefinitions"));

            XElement compositionInstancesHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "CompositionInstances"));

            XElement osHierarchy = new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "Os"));

            XElement packageHierarchy = new XElement(ns + "PACKAGE-HIERARCHY",
                dataDefinitionsHierarchy,
                interfacesHierarchy,
                swComponentDefinitionsHierarchy,
                compositionInstancesHierarchy,
                osHierarchy);

            XElement root = new XElement(ns + "AR-PACKAGES",
                new XElement(ns + "AR-PACKAGE",
                    new XElement(ns + "SHORT-NAME", "AUTOSAR_Project"),
                    new XElement(ns + "ELEMENTS",
                        ExportDataDefinitions(ns),
                        ExportInterfaces(ns),
                        ExportSwComponentDefinitions(ns),
                        ExportCompositions(ns),
                        ExportOsElements(ns)
                    ),
                    packageHierarchy));

            // Create XDocument with explicit declaration for proper formatting
            XDocument document = new XDocument();
            document.Add(new XDeclaration("1.0", "utf-8", null));
            document.Add(root);
            return document;
        }

        // ==================== DATA DEFINITIONS ====================

        private XElement ExportDataDefinitions(XNamespace ns)
        {
            XElement dataDefs = new XElement(ns + "DATA-DEFINITIONS");

            foreach (BaseDataType baseDt in _app.BaseDataTypes)
            {
                dataDefs.Add(ExportPrimitiveType(ns, baseDt));
            }

            foreach (SimpleDataType simpleDt in _app.SimpleDataTypes)
            {
                dataDefs.Add(ExportSimpleDataType(ns, simpleDt));
            }

            foreach (ArrayDataType arrayDt in _app.ArrayDataTypes)
            {
                dataDefs.Add(ExportArrayDataType(ns, arrayDt));
            }

            foreach (ComplexDataType complexDt in _app.ComplexDataTypes)
            {
                dataDefs.Add(ExportComplexDataType(ns, complexDt));
            }

            foreach (EnumDataType enumDt in _app.Enums)
            {
                dataDefs.Add(ExportEnumDataType(ns, enumDt));
            }

            return dataDefs;
        }

        private XElement ExportPrimitiveType(XNamespace ns, BaseDataType baseDt)
        {
            (int size, string encoding) = GetBaseTypeInfo(baseDt);

            XElement primitiveType = new XElement(ns + "PRIMITIVE-TYPE",
                new XElement(ns + "SHORT-NAME", baseDt.Name),
                new XElement(ns + "BASE-TYPE-SIZE", size.ToString()),
                new XElement(ns + "BASE-TYPE-ENCODING", encoding)
            );

            return primitiveType;
        }

        private (int size, string encoding) GetBaseTypeInfo(BaseDataType baseDt)
        {
            string sysName = baseDt.SystemName ?? baseDt.Name;

            switch (sysName.ToLower())
            {
                case "unsigned char":
                case "boolean":
                    return (1, "UNSIGNED");
                case "signed char":
                    return (1, "SIGNED");
                case "unsigned short":
                    return (2, "UNSIGNED");
                case "signed short":
                    return (2, "SIGNED");
                case "unsigned int":
                case "uint8":
                case "uint16":
                case "uint32":
                    return (4, "UNSIGNED");
                case "signed int":
                case "int8":
                case "int16":
                case "int32":
                    return (4, "SIGNED");
                case "unsigned long long":
                case "uint64":
                    return (8, "UNSIGNED");
                case "signed long long":
                case "int64":
                    return (8, "SIGNED");
                case "float":
                    return (4, "FLOAT");
                case "double":
                    return (8, "FLOAT");
                case "const char*":
                case "str":
                    return (1, "UNSIGNED");
                default:
                    // Fallback: try to infer from name
                    if (sysName.StartsWith("uint") || sysName.StartsWith("unsigned"))
                        return (4, "UNSIGNED");
                    else if (sysName.StartsWith("int") || sysName.StartsWith("signed"))
                        return (4, "SIGNED");
                    else
                        return (1, "UNSIGNED");
            }
        }

        private XElement ExportSimpleDataType(XNamespace ns, SimpleDataType simpleDt)
        {
            BaseDataType baseType = _app.BaseDataTypes.FindObject(simpleDt.BaseDataTypeGUID);
            string baseTypeName = baseType != null ? baseType.Name : "int8";
            (int size, string encoding) = baseType != null ? GetBaseTypeInfo(baseType) : (4, "SIGNED");

            XElement dataType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", simpleDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-PRIMITIVE-TYPE",
                        new XElement(ns + "BASE-TYPE", baseTypeName),
                        new XElement(ns + "IS-SIGNED", encoding),
                        new XElement(ns + "SIZE", size.ToString())
                    )
                )
            );

            return dataType;
        }

        private XElement ExportArrayDataType(XNamespace ns, ArrayDataType arrayDt)
        {
            string elementRef = GetDataTypeRef(ns, arrayDt.DataTypeGUID);

            XElement arrayType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", arrayDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-ARRAY-DATA-TYPE",
                        new XElement(ns + "ELEMENT-TYPE-REF",
                            new XAttribute(ns + "DEST", "AR-PKG"),
                            elementRef),
                        new XElement(ns + "LOWER-LIMIT", "0"),
                        new XElement(ns + "UPPER-LIMIT", arrayDt.Size.ToString())
                    )
                )
            );

            return arrayType;
        }

        private XElement ExportComplexDataType(XNamespace ns, ComplexDataType complexDt)
        {
            XElement structElements = new XElement(ns + "STRUCTURE-ELEMENTS");

            foreach (ComplexDataTypeField field in complexDt.Fields)
            {
                XElement element = new XElement(ns + "STRUCTURE-ELEMENT",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "ACCESS-HINT", "OPTIONAL"),
                    new XElement(ns + "IS-QN-REQUIRED", "FALSE"),
                    new XElement(ns + "TYPE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        GetDataTypeRef(ns, field.DataTypeGUID))
                );
                structElements.Add(element);
            }

            XElement structType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", complexDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-STRUCT-DATA-TYPE"),
                    structElements
                )
            );

            return structType;
        }

        private XElement ExportEnumDataType(XNamespace ns, EnumDataType enumDt)
        {
            XElement enumValues = new XElement(ns + "ENUMERATION-VALUES");

            foreach (EnumField field in enumDt.Fields)
            {
                XElement value = new XElement(ns + "ENUMERATION-VALUE",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "VALUE", field.Value.ToString())
                );
                enumValues.Add(value);
            }

            XElement enumType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", enumDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-ENUM-DATA-TYPE",
                        new XElement(ns + "DISPLAY-VALUE", "DEC"),
                        enumValues)
                )
            );

            return enumType;
        }

        // ==================== INTERFACES ====================

        private XElement ExportInterfaces(XNamespace ns)
        {
            XElement interfaces = new XElement(ns + "INTERFACES");

            foreach (SenderReceiverInterface srInterface in _app.SenderReceiverInterfaces)
            {
                interfaces.Add(ExportSenderReceiverInterface(ns, srInterface));
            }

            foreach (ClientServerInterface csInterface in _app.ClientServerInterfaces)
            {
                interfaces.Add(ExportClientServerInterface(ns, csInterface));
            }

            return interfaces;
        }

        private XElement ExportSenderReceiverInterface(XNamespace ns, SenderReceiverInterface srInterface)
        {
            XElement subElements = new XElement(ns + "SENDER-RECEIVER-INTERFACE-SUB-ELEMENTS");

            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                string typeName = GetDataTypeName(field.BaseDataTypeGUID);
                XElement dataElement = new XElement(ns + "DATA-ELEMENT-PROTOTYPE",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "TYPE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//DataDefinitions/{typeName}")
                );
                subElements.Add(dataElement);
            }

            XElement senderReceiver = new XElement(ns + "SENDER-RECEIVER-INTERFACE",
                new XElement(ns + "SHORT-NAME", srInterface.Name),
                new XElement(ns + "SENDER-RECEIVER-INTERFACE-SUB-ELEMENTS", subElements)
            );

            return senderReceiver;
        }

        private XElement ExportClientServerInterface(XNamespace ns, ClientServerInterface csInterface)
        {
            XElement operations = new XElement(ns + "CLIENT-SERVER-INTERFACE-OPERATIONS");

            foreach (ClientServerOperation operation in csInterface.Operations)
            {
                XElement parameters = new XElement(ns + "PARAMETER-PROTOTYPES");

                foreach (ClientServerOperationField field in operation.Fields)
                {
                    string typeName = GetDataTypeName(field.BaseDataTypeGUID);
                    string direction = GetArxmlDirection(field.Direction);

                    XElement param = new XElement(ns + "PARAMETER-PROTOTYPE",
                        new XElement(ns + "SHORT-NAME", field.Name),
                        new XElement(ns + "DIRECTION", direction),
                        new XElement(ns + "TYPE-REF",
                            new XAttribute(ns + "DEST", "AR-PKG"),
                            $"//DataDefinitions/{typeName}")
                    );
                    parameters.Add(param);
                }

                XElement operationElem = new XElement(ns + "OPERATION-PROTOTYPE",
                    new XElement(ns + "SHORT-NAME", operation.Name),
                    parameters
                );
                operations.Add(operationElem);
            }

            XElement clientServer = new XElement(ns + "CLIENT-SERVER-INTERFACE",
                new XElement(ns + "SHORT-NAME", csInterface.Name),
                operations
            );

            return clientServer;
        }

        private string GetArxmlDirection(ClientServerOperationDirection direction)
        {
            switch (direction)
            {
                case ClientServerOperationDirection.VALUE:
                case ClientServerOperationDirection.CONST_VALUE:
                    return "IN";
                case ClientServerOperationDirection.VAL_REF:
                case ClientServerOperationDirection.CONST_VAL_REF:
                    return "IN-OUT";
                case ClientServerOperationDirection.VAL_CONST_REF:
                case ClientServerOperationDirection.CONST_VAL_CONST_REF:
                    return "IN";
                case ClientServerOperationDirection.CONST_REF:
                    return "IN";
                default:
                    return "IN";
            }
        }

        // ==================== SW COMPONENT DEFINITIONS ====================

        private XElement ExportSwComponentDefinitions(XNamespace ns)
        {
            XElement components = new XElement(ns + "SW-COMPONENT-DEFINITIONS");

            foreach (ApplicationSwComponentType compDef in _app.ComponentDefenitionsList)
            {
                if (compDef.IsComponentEmpty())
                    continue;

                components.Add(ExportSwComponentDefinition(ns, compDef));
            }

            return components;
        }

        private XElement ExportSwComponentDefinition(XNamespace ns, ApplicationSwComponentType compDef)
        {
            XElement portDefinitions = ExportPortDefinitions(ns, compDef);
            XElement runnableDefinitions = ExportRunnableDefinitions(ns, compDef);
            XElement eventsElement = ExportComponentEvents(ns, compDef);

            XElement swComp = new XElement(ns + "SW-COMPONENT-DEFINITION",
                new XElement(ns + "SHORT-NAME", compDef.Name),
                new XElement(ns + "INCLUDES-DATA-DEFINITIONS", "FALSE"),
                portDefinitions,
                runnableDefinitions,
                eventsElement
            );

            return swComp;
        }

        private XElement ExportPortDefinitions(XNamespace ns, ApplicationSwComponentType compDef)
        {
            XElement portDefs = new XElement(ns + "PORT-DEFINITIONS");

            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.PortType == PortType.Sender || portDef.PortType == PortType.Receiver)
                {
                    XElement srPort = new XElement(ns + "SENDER-RECEIVER-PORT-DEFINITION",
                        new XElement(ns + "SHORT-NAME", portDef.Name),
                        new XElement(ns + "MODE", (portDef.PortType == PortType.Sender) ? "SENDER" : "RECEIVER"),
                        new XElement(ns + "MODIFYABLE-ACCESS-REF", "TRUE")
                    );

                    if (portDef.InterfaceGUID != Guid.Empty)
                    {
                        SenderReceiverInterface srInterface = _app.SenderReceiverInterfaces.FindObject(portDef.InterfaceGUID);
                        if (srInterface != null)
                        {
                            srPort.Add(new XElement(ns + "INTERFACE-REF",
                                new XAttribute(ns + "DEST", "AR-PKG"),
                                $"//Interfaces/{srInterface.Name}"));
                        }
                    }

                    portDefs.Add(srPort);
                }
                else if ((portDef.PortType == PortType.Client) || (portDef.PortType == PortType.Server))
                {
                    XElement csPort = new XElement(ns + "CLIENT-SERVER-PORT-DEFINITION",
                        new XElement(ns + "SHORT-NAME", portDef.Name),
                        new XElement(ns + "MODE", (portDef.PortType == PortType.Client) ? "CLIENT" : "SERVER")
                    );

                    if (portDef.InterfaceGUID != Guid.Empty)
                    {
                        ClientServerInterface csInterface = _app.ClientServerInterfaces.FindObject(portDef.InterfaceGUID);
                        if (csInterface != null)
                        {
                            csPort.Add(new XElement(ns + "INTERFACE-REF",
                                new XAttribute(ns + "DEST", "AR-PKG"),
                                $"//Interfaces/{csInterface.Name}"));
                        }
                    }

                    portDefs.Add(csPort);
                }
            }

            return portDefs;
        }

        private XElement ExportRunnableDefinitions(XNamespace ns, ApplicationSwComponentType compDef)
        {
            XElement runnables = new XElement(ns + "RUNNABLE-DEFINITIONS");

            foreach (RunnableDefenition runnable in compDef.Runnables)
            {
                XElement runnableDef = new XElement(ns + "RUNNABLE-DEFINITION",
                    new XElement(ns + "SHORT-NAME", runnable.Name)
                );
                runnables.Add(runnableDef);
            }

            return runnables;
        }

        private XElement ExportComponentEvents(XNamespace ns, ApplicationSwComponentType compDef)
        {
            XElement result = new XElement(ns + "STARTUP-EVENTS",
                new XElement(ns + "SHORT-NAME", compDef.Name + "_StartupEvents")
            );

            XElement timingEvents = new XElement(ns + "TIMING-CONSTRAINED-EVENTS",
                new XElement(ns + "SHORT-NAME", compDef.Name + "_TimingEvents")
            );

            XElement clientServerEvents = new XElement(ns + "CLIENT-SERVER-EVENTS",
                new XElement(ns + "SHORT-NAME", compDef.Name + "_CSEvents")
            );

            // OneTimeEvents -> STARTUP-EVENT
            foreach (OneTimeEvent oneTimeEvent in compDef.OneTimeEvents)
            {
                string runnableRef = "./" + EscapeRunnableName(oneTimeEvent.Runnable?.Name ?? oneTimeEvent.Name);

                XElement startupEvent = new XElement(ns + "STARTUP-EVENT",
                    new XElement(ns + "SHORT-NAME", oneTimeEvent.Name),
                    new XElement(ns + "RUNNABLE-REF",
                        new XAttribute(ns + "DEST", "SW-COMPONENT"),
                        runnableRef)
                );
                result.Add(startupEvent);
            }

            // TimingEvents -> PERIODIC-TIMING-CONSTRAINED-EVENT
            foreach (TimingEvent timingEvent in compDef.TimingEvents)
            {
                double periodSeconds = timingEvent.PeriodMs / 1000.0;
                string runnableName = timingEvent.Runnable?.Name ?? timingEvent.Name;

                XElement timingEventElem = new XElement(ns + "PERIODIC-TIMING-CONSTRAINED-EVENT",
                    new XElement(ns + "SHORT-NAME", timingEvent.Name),
                    new XElement(ns + "PERIOD", periodSeconds.ToString("F6").TrimEnd('0').TrimEnd('.')),
                    new XElement(ns + "RUNNABLE-REF",
                        new XAttribute(ns + "DEST", "SW-COMPONENT"),
                        "./" + EscapeRunnableName(runnableName))
                );
                timingEvents.Add(timingEventElem);
            }

            // ServerCallEvent -> SYNC-CLIENT-SERVER-EVENT
            foreach (ClientServerEvent syncEvent in compDef.SyncClientServerEvents)
            {
                // Find the interface and operation from SourcePortGuid and SourceOperationGuid
                string operationRef = FindOperationRef(ns, syncEvent);

                XElement csEvent = new XElement(ns + "SYNC-CLIENT-SERVER-EVENT",
                    new XElement(ns + "SHORT-NAME", syncEvent.Name),
                    new XElement(ns + "OPERATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        operationRef)
                );
                clientServerEvents.Add(csEvent);
            }

            // AsyncClientServerEvent -> ASYNC-CLIENT-SERVER-EVENT
            foreach (ClientServerEvent asyncEvent in compDef.AsyncClientServerEvents)
            {
                string operationRef = FindOperationRef(ns, asyncEvent);

                XElement csEvent = new XElement(ns + "ASYNC-CLIENT-SERVER-EVENT",
                    new XElement(ns + "SHORT-NAME", asyncEvent.Name),
                    new XElement(ns + "OPERATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        operationRef)
                );
                clientServerEvents.Add(csEvent);
            }

            // Merge all into one element
            XElement allEvents = new XElement(ns + "EVENTS",
                new XElement(ns + "SHORT-NAME", compDef.Name + "_Events"),
                result,
                timingEvents,
                clientServerEvents
            );

            return allEvents;
        }

        private string FindOperationRef(XNamespace ns, ClientServerEvent eventInstance)
        {
            // Try to find the interface and operation from the event's source port
            if (eventInstance.SourcePort != null && eventInstance.SourceOperation != null)
            {
                ClientServerInterface csInterface = eventInstance.SourcePort.InterfaceDatatype as ClientServerInterface;
                if (csInterface != null)
                {
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        if (operation.GUID == eventInstance.SourceOperation.GUID)
                        {
                            return $"//Interfaces/{csInterface.Name}/OPERATION-PROTOTYPES/{operation.Name}";
                        }
                    }
                }

                // Also try by matching the event's SourcePort GUID
                foreach (ApplicationSwComponentType compDef in _app.ComponentDefenitionsList)
                {
                    foreach (PortDefenition portDef in compDef.Ports)
                    {
                        if (portDef.GUID == eventInstance.SourcePort.GUID)
                        {
                            if ((portDef.PortType == PortType.Client) || (portDef.PortType == PortType.Server))
                            {
                                if (portDef.InterfaceGUID != Guid.Empty)
                                {
                                    csInterface = _app.ClientServerInterfaces.FindObject(portDef.InterfaceGUID);
                                    if (csInterface != null)
                                    {
                                        foreach (ClientServerOperation operation in csInterface.Operations)
                                        {
                                            if (operation.GUID == eventInstance.SourceOperation.GUID)
                                            {
                                                return $"//Interfaces/{csInterface.Name}/OPERATION-PROTOTYPES/{operation.Name}";
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // Fallback: try to find by matching the event name pattern
            // e.g., syncEventrs1_DoSomething -> rs1 is port, DoSomething is operation
            string eventName = eventInstance.Name;
            foreach (ApplicationSwComponentType compDef in _app.ComponentDefenitionsList)
            {
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if ((portDef.PortType == PortType.Client) || (portDef.PortType == PortType.Server))
                    {
                        if (portDef.InterfaceGUID != Guid.Empty)
                        {
                            ClientServerInterface csInterface = _app.ClientServerInterfaces.FindObject(portDef.InterfaceGUID);
                            if (csInterface != null)
                            {
                                foreach (ClientServerOperation operation in csInterface.Operations)
                                {
                                    if (eventName.Contains(operation.Name))
                                    {
                                        return $"//Interfaces/{csInterface.Name}/OPERATION-PROTOTYPES/{operation.Name}";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Last fallback
            return "//Interfaces/None/OPERATION-PROTOTYPES/None";
        }

        private string EscapeRunnableName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unnamed";
            return name.Replace(" ", "_");
        }

        // ==================== COMPOSITIONS ====================

        private XElement ExportCompositions(XNamespace ns)
        {
            XElement compositions = new XElement(ns + "COMPOSITIONS");

            foreach (CompositionInstance composition in _app.Compositions)
            {
                compositions.Add(ExportComposition(ns, composition));
            }

            return compositions;
        }

        private XElement ExportComposition(XNamespace ns, CompositionInstance composition)
        {
            XElement componentInstances = new XElement(ns + "COMPONENT-INSTANCES");
            XElement portPairs = new XElement(ns + "PORT-PAIRS");

            // Export component instances
            foreach (ComponentInstance compInstance in composition.ComponentInstances)
            {
                ApplicationSwComponentType compDef = compInstance.ComponentDefenition;

                XElement swInstance = new XElement(ns + "SW-INSTANCE",
                    new XElement(ns + "SHORT-NAME", compInstance.Name),
                    new XElement(ns + "OCCURENCES", "1"),
                    new XElement(ns + "SW-INSTANCE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//SwComponentDefinitions/{compDef.Name}")
                );

                // Port instances for this component
                XElement portInstances = ExportPortInstances(ns, compInstance, compDef);
                swInstance.Add(portInstances);

                componentInstances.Add(swInstance);
            }

            // Export port pairs (connections)
            foreach (PortConnection connection in composition.Connections)
            {
                portPairs.Add(ExportPortPair(ns, connection, composition));
            }

            XElement compositionElem = new XElement(ns + "COMPOSITION",
                new XElement(ns + "SHORT-NAME", composition.Name),
                new XElement(ns + "CLARIFICATION", "SPECIFIED"),
                new XElement(ns + "VARIABLE-SPACING", "FALSE"),
                componentInstances,
                portPairs
            );

            return compositionElem;
        }

        private XElement ExportPortInstances(XNamespace ns, ComponentInstance compInstance, ApplicationSwComponentType compDef)
        {
            XElement portInstances = new XElement(ns + "PORT-INSTANCES");

            foreach (PortDefenition portDef in compDef.Ports)
            {
                XElement portInstance = new XElement(ns + "PORT-INSTANCE",
                    new XElement(ns + "SHORT-NAME", portDef.Name)
                );
                portInstances.Add(portInstance);
            }

            return portInstances;
        }

        private XElement ExportPortPair(XNamespace ns, PortConnection connection, CompositionInstance composition)
        {
            // Find source and destination from composition
            string sourceRef = null;
            string destRef = null;

            foreach (ComponentInstance compInstance in composition.ComponentInstances)
            {
                foreach (PortPainter portPainter in compInstance.Ports)
                {
                    if (portPainter.GUID == connection.Port1.GUID)
                    {
                        sourceRef = $"//CompositionInstances/{composition.Name}/ComponentInstances/{compInstance.Name}/PORT-INSTANCES/{portPainter.PortDefenition?.Name ?? portPainter.Name}";
                    }
                    if (portPainter.GUID == connection.Port2.GUID)
                    {
                        destRef = $"//CompositionInstances/{composition.Name}/ComponentInstances/{compInstance.Name}/PORT-INSTANCES/{portPainter.PortDefenition?.Name ?? portPainter.Name}";
                    }
                }
            }

            // Also check internal ports
            foreach (PortPainter internalPort in composition.InternalPortsInstances)
            {
                if (internalPort.GUID == connection.Port1.GUID)
                {
                    // Find which component this internal port belongs to
                    foreach (ComponentInstance compInstance in composition.ComponentInstances)
                    {
                        foreach (PortPainter portPainter in compInstance.Ports)
                        {
                            if (portPainter.PortDefenitionGuid == internalPort.PortDefenitionGuid)
                            {
                                sourceRef = $"//CompositionInstances/{composition.Name}/ComponentInstances/{compInstance.Name}/PORT-INSTANCES/{internalPort.PortDefenition?.Name ?? internalPort.Name}";
                                break;
                            }
                        }
                    }
                }
                if (internalPort.GUID == connection.Port2.GUID)
                {
                    foreach (ComponentInstance compInstance in composition.ComponentInstances)
                    {
                        foreach (PortPainter portPainter in compInstance.Ports)
                        {
                            if (portPainter.PortDefenitionGuid == internalPort.PortDefenitionGuid)
                            {
                                destRef = $"//CompositionInstances/{composition.Name}/ComponentInstances/{compInstance.Name}/PORT-INSTANCES/{internalPort.PortDefenition?.Name ?? internalPort.Name}";
                                break;
                            }
                        }
                    }
                }
            }

            if (sourceRef == null || destRef == null)
            {
                // Fallback: create a minimal port pair
                _connectionIndex++;
                string connNameFallback = $"Connection_{_connectionIndex}";
                return new XElement(ns + "PORT-PAIR",
                    new XElement(ns + "SHORT-NAME", connNameFallback),
                    new XElement(ns + "MAP-DELAY", "0us"),
                    new XElement(ns + "SOURCE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        sourceRef ?? "//CompositionInstances/None/PORT-INSTANCES/None"),
                    new XElement(ns + "DESTINATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        destRef ?? "//CompositionInstances/None/PORT-INSTANCES/None")
                );
            }

            _connectionIndex++;
            string connName = $"Connection_{_connectionIndex}";

            XElement portPair = new XElement(ns + "PORT-PAIR",
                new XElement(ns + "SHORT-NAME", connName),
                new XElement(ns + "MAP-DELAY", "0us"),
                new XElement(ns + "SOURCE-REF",
                    new XAttribute(ns + "DEST", "AR-PKG"),
                    sourceRef),
                new XElement(ns + "DESTINATION-REF",
                    new XAttribute(ns + "DEST", "AR-PKG"),
                    destRef)
            );

            return portPair;
        }

        private int _connectionIndex = 0;

        private int _portDefIndex = 0;

        // ==================== OS ELEMENTS ====================

        private XElement ExportOsElements(XNamespace ns)
        {
            XElement osElements = new XElement(ns + "OS-ELMENTS");

            osElements.Add(ExportOsTasks(ns));
            osElements.Add(ExportOsEvents(ns));

            return osElements;
        }

        private XElement ExportOsTasks(XNamespace ns)
        {
            XElement tasks = new XElement(ns + "TASKS");

            foreach (OsTask task in _app.OsTasks)
            {
                if (task.Name.Equals("Init", StringComparison.OrdinalIgnoreCase) ||
                    task.Name.Equals("Idle", StringComparison.OrdinalIgnoreCase))
                    continue;

                XElement taskElem = new XElement(ns + "TASK",
                    new XElement(ns + "SHORT-NAME", task.Name),
                    new XElement(ns + "SCHEDULE-CORNER", "FIRST_START"),
                    new XElement(ns + "SCHEDULE-CORNER-TIME", "0us"),
                    new XElement(ns + "TASK-ENTRY-POINT",
                        new XElement(ns + "SHORT-NAME", $"Rte_Task_{task.Name}_Entry")),
                    new XElement(ns + "MAX-AUDIT-LATENCY", "0us"),
                    new XElement(ns + "MAX-BANDWIDTH", "0.00%"),
                    new XElement(ns + "OPTIMIZE-STRING", "DEFAULT"),
                    new XElement(ns + "STARTUP-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//Os/EVENTS/Startup")
                );

                // Add event references
                foreach (AutosarEventInstance evt in task.Events)
                {
                    taskElem.Add(new XElement(ns + "EVENT-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//Os/EVENTS/{evt.Name}"));
                }

                tasks.Add(taskElem);
            }

            return tasks;
        }

        private XElement ExportOsEvents(XNamespace ns)
        {
            XElement events = new XElement(ns + "EVENTS");

            // Startup event
            events.Add(new XElement(ns + "STARTUP-EVENT",
                new XElement(ns + "SHORT-NAME", "Startup")
            ));

            // Idle event
            events.Add(new XElement(ns + "IDLE-EVENT",
                new XElement(ns + "SHORT-NAME", "Idle")
            ));

            // Periodic events from OsTasks
            foreach (OsTask task in _app.OsTasks)
            {
                if (task.Name.Equals("Init", StringComparison.OrdinalIgnoreCase) ||
                    task.Name.Equals("Idle", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (AutosarEventInstance evt in task.Events)
                {
                    _eventIndex++;

                    // PeriodMs returns milliseconds, convert to microseconds for AUTOSAR PERIOD
                    long periodMicroseconds = (long)(evt.PeriodMs * 1000);

                    XElement eventElem = new XElement(ns + "PERIODIC-EVENT",
                        new XElement(ns + "SHORT-NAME", evt.Name),
                        new XElement(ns + "PERIOD", $"{periodMicroseconds}us"),
                        new XElement(ns + "TASK-REF",
                            new XAttribute(ns + "DEST", "AR-PKG"),
                            $"//Os/TASKS/{task.Name}"),
                        new XElement(ns + "PENDING-BUFFER-SIZE", "1")
                    );
                    events.Add(eventElem);
                }
            }

            // Startup events from Init task
            OsTask initTask = _app.OsTasks.FirstOrDefault(t => t.Name.Equals("Init", StringComparison.OrdinalIgnoreCase));
            if (initTask != null)
            {
                foreach (AutosarEventInstance evt in initTask.Events)
                {
                    XElement startupEvent = new XElement(ns + "STARTUP-EVENT",
                        new XElement(ns + "SHORT-NAME", evt.Name),
                        new XElement(ns + "TASK-REF",
                            new XAttribute(ns + "DEST", "AR-PKG"),
                            $"//Os/TASKS/{initTask.Name}")
                    );
                    events.Add(startupEvent);
                }
            }

            return events;
        }

        // ==================== HELPERS ====================

        private string GetDataTypeName(Guid guid)
        {
            if (guid == Guid.Empty)
                return "int8";

            BaseDataType baseDt = _app.BaseDataTypes.FindObject(guid);
            if (baseDt != null)
                return baseDt.Name;

            SimpleDataType simpleDt = _app.SimpleDataTypes.FindObject(guid);
            if (simpleDt != null)
                return simpleDt.Name;

            ArrayDataType arrayDt = _app.ArrayDataTypes.FindObject(guid);
            if (arrayDt != null)
                return arrayDt.Name;

            ComplexDataType complexDt = _app.ComplexDataTypes.FindObject(guid);
            if (complexDt != null)
                return complexDt.Name;

            EnumDataType enumDt = _app.Enums.FindObject(guid);
            if (enumDt != null)
                return enumDt.Name;

            return "int8";
        }

        private string GetDataTypeRef(XNamespace ns, Guid guid)
        {
            string typeName = GetDataTypeName(guid);
            return $"//DataDefinitions/{typeName}";
        }
    }
}