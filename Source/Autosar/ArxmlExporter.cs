using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AutosarGuiEditor.Source.Painters.PortsPainters;
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
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters;

namespace AutosarGuiEditor.Source.Autosar
{
    /// <summary>
    /// Экспортер проекта в формат ARXML (AUTOSAR RTE Package format)
    /// </summary>
    public class ArxmlExporter
    {
        private AutosarApplication _app;
        private int _compositionIndex;
        private int _taskIndex;
        private int _eventIndex;
        private int _portDefIndex;
        private int _connectionIndex;

        // ARXML namespace
        private static readonly string ArNamespace = "http://autosar.org/schema/r4.0";
        private static readonly XmlNamespaceManager ArNs = new XmlNamespaceManager(new NameTable());

        static ArxmlExporter()
        {
            ArNs.AddNamespace("aar", ArNamespace);
        }

        /// <summary>
        /// Экспорт проекта в ARXML файл
        /// </summary>
        public bool ExportToArxml(AutosarApplication app, string filePath)
        {
            _app = app;
            ResetIndexes();

            XDocument document = CreateArxmlDocument();
            document.Save(filePath);

            return true;
        }

        private void ResetIndexes()
        {
            _compositionIndex = 0;
            _taskIndex = 0;
            _eventIndex = 0;
            _portDefIndex = 0;
            _connectionIndex = 0;
        }

        private XDocument CreateArxmlDocument()
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(ns + "AR-PACKAGES",
                    new XElement(ns + "AR-PACKAGE",
                        new XElement(ns + "SHORT-NAME", "AUTOSAR_Project"),
                        new XElement(ns + "ELEMENTS",
                            ExportDataDefinitions(),
                            ExportInterfaces(),
                            ExportSwCompositionDefinitions(),
                            ExportCompositionInstances(),
                            ExportOsElements()
                        ),
                        ExportPackageHierarchy()
                    )
                )
            );

            return doc;
        }

        private string ns => "{" + ArNamespace + "}";

        private XElement ExportDataDefinitions()
        {
            XElement dataDefs = new XElement(ns + "DATA-DEFINITIONS");

            // Base data types
            foreach (BaseDataType baseDt in _app.BaseDataTypes)
            {
                dataDefs.Add(ExportBaseDataType(baseDt));
            }

            // Simple data types
            foreach (SimpleDataType simpleDt in _app.SimpleDataTypes)
            {
                dataDefs.Add(ExportSimpleDataType(simpleDt));
            }

            // Array data types
            foreach (ArrayDataType arrayDt in _app.ArrayDataTypes)
            {
                dataDefs.Add(ExportArrayDataType(arrayDt));
            }

            // Complex data types
            foreach (ComplexDataType complexDt in _app.ComplexDataTypes)
            {
                dataDefs.Add(ExportComplexDataType(complexDt));
            }

            // Enum data types
            foreach (EnumDataType enumDt in _app.Enums)
            {
                dataDefs.Add(ExportEnumDataType(enumDt));
            }

            return dataDefs;
        }

        private XElement ExportBaseDataType(BaseDataType baseDt)
        {
            // BaseDataType inherits from PlainDataType which doesn't have SizeInBits/IsSigned
            // Use default values for ARXML export
            XElement primitiveType = new XElement(ns + "PRIMITIVE-TYPE",
                new XElement(ns + "SHORT-NAME", baseDt.Name),
                new XElement(ns + "BASE-TYPE-SIZE", "4"),
                new XElement(ns + "BASE-TYPE-ENCODING", "SIGNED")
            );

            return primitiveType;
        }

        private XElement ExportSimpleDataType(SimpleDataType simpleDt)
        {
            // Find base type (SimpleDataType.BaseDataTypeGUID)
            BaseDataType baseType = _app.BaseDataTypes.FindObject(simpleDt.BaseDataTypeGUID);
            string baseTypeName = baseType != null ? baseType.Name : "UNKNOWN";
            
            // Get size and signed info from base type if available
            string isSigned = "FALSE";
            int size = 32;
            if (baseType != null)
            {
                // BaseDataType doesn't have SizeInBits/IsSigned, use defaults
                isSigned = "SIGNED";
                size = 32;
            }

            XElement dataType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", simpleDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-PRIMITIVE-TYPE",
                        new XElement(ns + "BASE-TYPE", baseTypeName),
                        new XElement(ns + "IS-SIGNED", isSigned),
                        new XElement(ns + "SIZE", size)
                    )
                )
            );

            return dataType;
        }

        private XElement ExportArrayDataType(ArrayDataType arrayDt)
        {
            XElement dataType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", arrayDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-ARRAY-DATA-TYPE",
                        ExportArrayElementType(arrayDt),
                        new XElement(ns + "LOWER-LIMIT", "0"),
                        new XElement(ns + "UPPER-LIMIT", arrayDt.Size.ToString())
                    )
                )
            );

            return dataType;
        }

        private XElement ExportArrayElementType(ArrayDataType arrayDt)
        {
            string elementRef = GetDataTypeRef(arrayDt.DataTypeGUID);
            if (elementRef != null)
            {
                return new XElement(ns + "ELEMENT-TYPE-REF",
                    new XAttribute(ns + "DEST", "AR-PKG"),
                    elementRef);
            }
            return new XElement(ns + "ELEMENT-TYPE",
                new XElement(ns + "SHORT-NAME", "UnknownElementType"));
        }

        private XElement ExportComplexDataType(ComplexDataType complexDt)
        {
            XElement structType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", complexDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-STRUCT-DATA-TYPE")
                ),
                ExportComplexDataTypeFields(complexDt)
            );

            return structType;
        }

        private XElement ExportComplexDataTypeFields(ComplexDataType complexDt)
        {
            XElement fields = new XElement(ns + "STRUCTURE-ELEMENTS");

            foreach (ComplexDataTypeField field in complexDt.Fields)
            {
                XElement element = new XElement(ns + "STRUCTURE-ELEMENT",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "ACCESS-HINT", "OPTIONAL"),
                    new XElement(ns + "IS-QN-REQUIRED", "FALSE"),
                new XElement(ns + "TYPE-REF",
                    new XAttribute(ns + "DEST", "AR-PKG"),
                    GetDataTypeRef(field.DataTypeGUID) ?? "")
                );

                fields.Add(element);
            }

            return fields;
        }

        private XElement ExportEnumDataType(EnumDataType enumDt)
        {
            XElement enumType = new XElement(ns + "DATA-TYPE",
                new XElement(ns + "SHORT-NAME", enumDt.Name),
                new XElement(ns + "TYPE-IDENTIFIER",
                    new XElement(ns + "T-ENUM-DATA-TYPE",
                        new XElement(ns + "DISPLAY-VALUE", "DEC"),
                        ExportEnumValues(enumDt)
                    )
                )
            );

            return enumType;
        }

        private XElement ExportEnumValues(EnumDataType enumDt)
        {
            XElement values = new XElement(ns + "ENUMERATION-VALUES");

            foreach (EnumField field in enumDt.Fields)
            {
                XElement value = new XElement(ns + "ENUMERATION-VALUE",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "VALUE", field.Value.ToString())
                );
                values.Add(value);
            }

            return values;
        }

        private XElement ExportInterfaces()
        {
            XElement interfaces = new XElement(ns + "INTERFACES");

            foreach (SenderReceiverInterface srInterface in _app.SenderReceiverInterfaces)
            {
                interfaces.Add(ExportSenderReceiverInterface(srInterface));
            }

            foreach (ClientServerInterface csInterface in _app.ClientServerInterfaces)
            {
                interfaces.Add(ExportClientServerInterface(csInterface));
            }

            return interfaces;
        }

        private XElement ExportSenderReceiverInterface(SenderReceiverInterface srInterface)
        {
            XElement senderReceiver = new XElement(ns + "SENDER-RECEIVER-INTERFACE",
                new XElement(ns + "SHORT-NAME", srInterface.Name),
                ExportSenderReceiverTopologies(srInterface),
                ExportSenderReceiverFields(srInterface)
            );

            return senderReceiver;
        }

        private XElement ExportSenderReceiverFields(SenderReceiverInterface srInterface)
        {
            XElement fields = new XElement(ns + "SENDER-RECEIVER-INTERFACE-SUB-ELEMENTS");

            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
            {
                XElement dataEndpoint = new XElement(ns + "DATA-ENDPOINT-INST-PROXY-SUB-ELEMENT",
                    new XElement(ns + "SHORT-NAME", field.Name),
                    new XElement(ns + "TOPIC-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//DataDefinitions/{GetDataTypeName(field.BaseDataTypeGUID)}")
                );
                fields.Add(dataEndpoint);
            }

            return fields;
        }

        private XElement ExportSenderReceiverTopologies(SenderReceiverInterface srInterface)
        {
            // Skip for now - would need topology data from composition
            return new XElement(ns + "TOPOLOGIES");
        }

        private XElement ExportClientServerInterface(ClientServerInterface csInterface)
        {
            XElement clientServer = new XElement(ns + "CLIENT-SERVER-INTERFACE",
                new XElement(ns + "SHORT-NAME", csInterface.Name),
                ExportClientServerOperations(csInterface)
            );

            return clientServer;
        }

        private XElement ExportClientServerOperations(ClientServerInterface csInterface)
        {
            XElement operations = new XElement(ns + "CLIENT-SERVER-INTERFACE-OPERATIONS");

            foreach (ClientServerOperation operation in csInterface.Operations)
            {
                // Use the interface's IsAsync property to determine mode
                string mode = csInterface.IsAsync ? "ASYNC" : "SYNC";
                XElement op = new XElement(ns + "CLIENT-SERVER-INTERFACE-OPERATION",
                    new XElement(ns + "SHORT-NAME", operation.Name),
                    new XElement(ns + "MODE", mode),
                    ExportClientServerOperationParameters(operation, csInterface)
                );
                operations.Add(op);
            }

            return operations;
        }

        private XElement ExportClientServerOperationParameters(ClientServerOperation operation, ClientServerInterface csInterface)
        {
            XElement parameters = new XElement(ns + "I-O-CONSTRAINTS");

            foreach (ClientServerOperationField field in operation.Fields)
            {
                string paramName = field.Name;
                if (string.IsNullOrEmpty(paramName))
                    paramName = field.Direction.ToString().ToLower();

                string direction = GetArxmlParameterDirection(field.Direction);
                string dataTypeRef = GetDataTypeRef(field.BaseDataTypeGUID) ?? "//DataDefinitions/None";

                XElement param = new XElement(ns + "I-O-PARAMETER-CONSTRAINT",
                    new XElement(ns + "SHORT-NAME", paramName),
                    new XElement(ns + "SEQUENCE-POSITION", "0"),
                    new XElement(ns + "DIRECTION", direction),
                    new XElement(ns + "TYPE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        dataTypeRef)
                );

                parameters.Add(param);
            }

            return parameters;
        }

        private string GetArxmlParameterDirection(ClientServerOperationDirection direction)
        {
            switch (direction)
            {
                case ClientServerOperationDirection.VALUE:
                case ClientServerOperationDirection.CONST_VALUE:
                    return "IN-OUT";
                case ClientServerOperationDirection.VAL_REF:
                case ClientServerOperationDirection.CONST_VAL_REF:
                case ClientServerOperationDirection.VAL_CONST_REF:
                case ClientServerOperationDirection.CONST_VAL_CONST_REF:
                case ClientServerOperationDirection.CONST_REF:
                    return "IN-OUT";
                default:
                    return "IN-OUT";
            }
        }

        private XElement ExportSwCompositionDefinitions()
        {
            XElement components = new XElement(ns + "SW-COMPONENT-DEFINITIONS");

            foreach (ApplicationSwComponentType compDef in _app.ComponentDefenitionsList)
            {
                if (compDef.IsComponentEmpty())
                    continue;

                components.Add(ExportSwComponentDefinition(compDef));
            }

            return components;
        }

        private XElement ExportSwComponentDefinition(ApplicationSwComponentType compDef)
        {
            XElement swComp = new XElement(ns + "SW-COMPONENT-DEFINITION",
                new XElement(ns + "SHORT-NAME", compDef.Name),
                new XElement(ns + "INCLUDES-DATA-DEFINITIONS", "FALSE"),
                ExportSwComponentPortDefinitions(compDef),
                ExportSwComponentRunnables(compDef),
                ExportSwComponentTimingEvents(compDef),
                ExportSwComponentClientServerEvents(compDef)
            );

            return swComp;
        }

        private XElement ExportSwComponentPortDefinitions(ApplicationSwComponentType compDef)
        {
            XElement portDefs = new XElement(ns + "PORT-DEFINITIONS");

            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.PortType == PortType.Sender || portDef.PortType == PortType.Receiver)
                {
                    portDefs.Add(ExportSenderReceiverPortDefinition(compDef.Name, portDef));
                }
                else if (portDef.PortType == PortType.Client || portDef.PortType == PortType.Server)
                {
                    ExportClientServerPortDefinition(compDef.Name, portDef, portDefs);
                }
            }

            return portDefs;
        }

        private XElement ExportSenderReceiverPortDefinition(string compName, PortDefenition portDef)
        {
            _portDefIndex++;

            string direction = portDef.PortType == PortType.Sender ? "SENDER" : "RECEIVER";

            XElement portDefElement = new XElement(ns + "SENDER-RECEIVER-PORT-DEFINITION",
                new XElement(ns + "SHORT-NAME", portDef.Name),
                new XElement(ns + "MODE", direction),
                new XElement(ns + "MODIFYABLE-ACCESS-REF", "TRUE")
            );

            // Add interface reference for Sender-Receiver ports
            if (portDef.InterfaceGUID != Guid.Empty)
            {
                SenderReceiverInterface srInterface = _app.SenderReceiverInterfaces.FindObject(portDef.InterfaceGUID);
                if (srInterface != null)
                {
                    portDefElement.Add(new XElement(ns + "INTERFACE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//Interfaces/{srInterface.Name}"));
                }
            }

            return portDefElement;
        }


        private void ExportClientServerPortDefinition(string compName, PortDefenition portDef, XElement portDefs)
        {
            _portDefIndex++;

            string direction = portDef.PortType == PortType.Client ? "CLIENT" : "SERVER";

            // Get the interface name for this port
            string interfaceName = "";
            if (portDef.InterfaceGUID != Guid.Empty)
            {
                ClientServerInterface csInterface = _app.ClientServerInterfaces.FindObject(portDef.InterfaceGUID);
                if (csInterface != null)
                    interfaceName = csInterface.Name;
            }

            XElement portDefElement = new XElement(ns + "CLIENT-SERVER-PORT-DEFINITION",
                new XElement(ns + "SHORT-NAME", portDef.Name),
                new XElement(ns + "MODE", direction)
            );

            if (!string.IsNullOrEmpty(interfaceName))
            {
                portDefElement.Add(new XElement(ns + "INTERFACE-REF",
                    new XAttribute(ns + "DEST", "AR-PKG"),
                    $"//Interfaces/{interfaceName}"));
            }

            portDefs.Add(portDefElement);
        }

        private XElement ExportSwComponentRunnables(ApplicationSwComponentType compDef)
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

        private string FindRunnableEventName(string compDefName, string runnableName)
        {
            // Try to find matching event from exported events
            // Events are named as Event_{index} in ExportEvents
            // We need to match by looking at the task that this runnable belongs to
            
            // First check if there's a timing event with this name
            foreach (var task in _app.OsTasks)
            {
                foreach (var evt in task.Events)
                {
                    if (evt.Defenition is TimingEvent timingEvent)
                    {
                        string expectedName = $"{compDefName}_{timingEvent.Name}_Event";
                        if (expectedName == evt.Name || evt.Name == $"{compDefName}_{runnableName}_Event")
                        {
                            return evt.Name;
                        }
                    }
                }
            }

            // Fallback: use the original naming convention
            return $"{compDefName}_{runnableName}_Event";
        }

        private XElement ExportSwComponentTimingEvents(ApplicationSwComponentType compDef)
        {
            XElement events = new XElement(ns + "TIMING-CONSTRAINED-EVENTS");

            foreach (TimingEvent timingEvent in compDef.TimingEvents)
            {
                string runnableRefName = timingEvent.Runnable?.Name ?? timingEvent.Name;
                XElement eventDef = new XElement(ns + "PERIODIC-TIMING-CONSTRAINED-EVENT",
                    new XElement(ns + "SHORT-NAME", timingEvent.Name),
                    new XElement(ns + "FREQUENCY", $"{(1000.0 / timingEvent.PeriodMs) * 1000000}us"),
                    new XElement(ns + "DELAY", "0us"),
                    new XElement(ns + "RUNNABLE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        runnableRefName)
                );
                events.Add(eventDef);
            }

            return events;
        }

        private XElement ExportSwComponentClientServerEvents(ApplicationSwComponentType compDef)
        {
            XElement events = new XElement(ns + "CLIENT-SERVER-EVENTS");

            foreach (ClientServerEvent asyncEvent in compDef.AsyncClientServerEvents)
            {
                string runnableRefName = asyncEvent.Runnable?.Name ?? asyncEvent.Name;
                XElement eventDef = new XElement(ns + "ASYNC-CLIENT-SERVER-EVENT",
                    new XElement(ns + "SHORT-NAME", asyncEvent.Name),
                    new XElement(ns + "OPERATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        runnableRefName)
                );
                events.Add(eventDef);
            }

            foreach (ClientServerEvent syncEvent in compDef.SyncClientServerEvents)
            {
                string runnableRefName = syncEvent.Runnable?.Name ?? syncEvent.Name;
                XElement eventDef = new XElement(ns + "SYNC-CLIENT-SERVER-EVENT",
                    new XElement(ns + "SHORT-NAME", syncEvent.Name),
                    new XElement(ns + "OPERATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        runnableRefName)
                );
                events.Add(eventDef);
            }

            return events;
        }

        private XElement ExportCompositionInstances()
        {
            XElement compositions = new XElement(ns + "COMPOSITIONS");

            foreach (CompositionInstance composition in _app.Compositions)
            {
                // Skip main composition for PCD export
                if (composition.Name == CompositionInstancesList.MainCompositionName)
                    continue;

                compositions.Add(ExportComposition(composition));
            }

            // Also export main composition
            foreach (CompositionInstance composition in _app.Compositions)
            {
                if (composition.Name == CompositionInstancesList.MainCompositionName)
                {
                    compositions.Add(ExportComposition(composition));
                    break;
                }
            }

            return compositions;
        }

        private XElement ExportComposition(CompositionInstance composition)
        {
            _compositionIndex++;
            string compositionId = $"Composition_{_compositionIndex}";

            XElement comp = new XElement(ns + "COMPOSITION",
                new XElement(ns + "SHORT-NAME", compositionId),
                new XElement(ns + "CLARIFICATION", "SPECIFIED"),
                new XElement(ns + "VARIABLE-SPACING", "FALSE"),
                ExportCompositionInstancesList(composition),
                ExportCompositionConnections(composition)
            );

            return comp;
        }

        private XElement ExportCompositionInstancesList(CompositionInstance composition)
        {
            XElement instances = new XElement(ns + "COMPONENT-INSTANCES");

            foreach (ComponentInstance compInstance in composition.ComponentInstances)
            {
                ApplicationSwComponentType compDef = compInstance.ComponentDefenition;

                XElement instance = new XElement(ns + "SW-INSTANCE",
                    new XElement(ns + "SHORT-NAME", compInstance.Name),
                    new XElement(ns + "OCCURENCES", "1"),
                    new XElement(ns + "SW-INSTANCE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//SwComponentDefinitions/{compDef.Name}")
                );

                instances.Add(instance);
            }

            return instances;
        }

        private XElement ExportCompositionConnections(CompositionInstance composition)
        {
            XElement connections = new XElement(ns + "PORT-PAIRS");

            foreach (PortConnection connection in composition.Connections)
            {
                _connectionIndex++;
                string connectionId = $"Connection_{_connectionIndex}";

                // Find component instances for each port directly from the composition
                string comp1Name = null;
                string comp2Name = null;
                string port1Name = null;
                string port2Name = null;

                foreach (ComponentInstance compInstance in composition.ComponentInstances)
                {
                    foreach (PortPainter portInComp in compInstance.Ports)
                    {
                        if (portInComp.GUID.Equals(connection.Port1.GUID))
                        {
                            comp1Name = compInstance.Name;
                            port1Name = portInComp.Name;
                        }
                        if (portInComp.GUID.Equals(connection.Port2.GUID))
                        {
                            comp2Name = compInstance.Name;
                            port2Name = portInComp.Name;
                        }
                    }
                }

                if (comp1Name == null || comp2Name == null)
                    continue;

                XElement portPair = new XElement(ns + "PORT-PAIR",
                    new XElement(ns + "SHORT-NAME", connectionId),
                    new XElement(ns + "MAP-DELAY", "0us"),
                    new XElement(ns + "SOURCE-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//CompositionInstances/{comp1Name}/PORT-INSTANCES/{port1Name}"),
                    new XElement(ns + "DESTINATION-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//CompositionInstances/{comp2Name}/PORT-INSTANCES/{port2Name}")
                );

                connections.Add(portPair);
            }

            return connections;
        }

        private string GetPortInstanceName(PortPainter port)
        {
            return port.Name;
        }

        private XElement ExportOsElements()
        {
            XElement osElements = new XElement(ns + "OS-ELMENTS");

            osElements.Add(ExportTasks());
            osElements.Add(ExportEvents());

            return osElements;
        }

        private XElement ExportTasks()
        {
            XElement tasks = new XElement(ns + "TASKS");

            foreach (OsTask task in _app.OsTasks)
            {
                if (task.Name.Equals("Init") || task.Name.Equals("Idle"))
                    continue;

                _taskIndex++;
                string taskId = $"Task_{_taskIndex}";

                XElement taskElement = new XElement(ns + "TASK",
                    new XElement(ns + "SHORT-NAME", taskId),
                    new XElement(ns + "SCHEDULE-CORNER", "FIRST_START"),
                    new XElement(ns + "SCHEDULE-CORNER-TIME", "0us"),
                    new XElement(ns + "TASK-ENTRY-POINT",
                        new XElement(ns + "SHORT-NAME", $"Rte_{taskId}_Entry")),
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
                    taskElement.Add(new XElement(ns + "EVENT-REF",
                        new XAttribute(ns + "DEST", "AR-PKG"),
                        $"//Os/EVENTS/{evt.Name}"));
                }

                tasks.Add(taskElement);
            }

            return tasks;
        }

        private XElement ExportEvents()
        {
            XElement events = new XElement(ns + "EVENTS");

            // Startup event
            events.Add(new XElement(ns + "STARTUP-EVENT",
                new XElement(ns + "SHORT-NAME", "Startup")));

            // Idle event
            events.Add(new XElement(ns + "IDLE-EVENT",
                new XElement(ns + "SHORT-NAME", "Idle")));

            // Task events
            foreach (OsTask task in _app.OsTasks)
            {
                foreach (AutosarEventInstance evt in task.Events)
                {
                    _eventIndex++;

                    // Generate proper event name matching Runnable references
                    string eventName = GenerateEventName(evt);

                    XElement eventElement = new XElement(ns + "EVENT",
                        new XElement(ns + "SHORT-NAME", eventName),
                        new XElement(ns + "TASK-REF",
                            new XAttribute(ns + "DEST", "AR-PKG"),
                            $"//Os/TASKS/Task_{task.Priority}")
                    );

                    // Timing events get pending buffer size
                    if (evt.Defenition is TimingEvent)
                    {
                        eventElement.Add(new XElement(ns + "PENDING-BUFFER-SIZE", "1"));
                    }

                    events.Add(eventElement);
                }
            }

            return events;
        }

        private string GenerateEventName(AutosarEventInstance evt)
        {
            // Get the component instance that owns this event
            ComponentInstance compInstance = null;
            try
            {
                compInstance = AutosarApplication.GetInstance().FindComponentInstanceByEventId(evt.GUID) as ComponentInstance;
            }
            catch
            {
                // Component instance not found
            }

            if (compInstance?.ComponentDefenition != null && evt.Defenition != null)
            {
                string compDefName = compInstance.ComponentDefenition.Name;
                
                // For TimingEvent, use the Runnable name from the definition
                if (evt.Defenition is TimingEvent timingEvent && timingEvent.Runnable != null)
                {
                    return $"{compDefName}_{timingEvent.Runnable.Name}_Event";
                }
                
                // For other event types, use a fallback name
                return $"{compDefName}_{evt.Defenition.Name}_Event";
            }

            // Fallback: use numeric index
            return $"Event_{_eventIndex}";
        }

        private XElement ExportPackageHierarchy()
        {
            XElement hierarchy = new XElement(ns + "PACKAGE-HIERARCHY");

            hierarchy.Add(new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "DataDefinitions"),
                new XElement(ns + "CHILD-PACKAGES",
                    new XElement(ns + "SHORT-NAME", "BaseDataTypes")
                ),
                new XElement(ns + "CHILD-PACKAGES",
                    new XElement(ns + "SHORT-NAME", "UserDefinedTypes")
                )
            ));

            hierarchy.Add(new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "SwComponentDefinitions")
            ));

            hierarchy.Add(new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "Interfaces")
            ));

            hierarchy.Add(new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "CompositionInstances")
            ));

            hierarchy.Add(new XElement(ns + "CHILD-PACKAGES",
                new XElement(ns + "SHORT-NAME", "Os")
            ));

            return hierarchy;
        }

        private string GetDataTypeName(Guid guid)
        {
            if (guid == Guid.Empty)
                return "None";

            // Check base data types
            BaseDataType baseDt = _app.BaseDataTypes.FindObject(guid);
            if (baseDt != null)
                return baseDt.Name;

            // Check simple data types
            SimpleDataType simpleDt = _app.SimpleDataTypes.FindObject(guid);
            if (simpleDt != null)
                return simpleDt.Name;

            // Check array data types
            ArrayDataType arrayDt = _app.ArrayDataTypes.FindObject(guid);
            if (arrayDt != null)
                return arrayDt.Name;

            // Check complex data types
            ComplexDataType complexDt = _app.ComplexDataTypes.FindObject(guid);
            if (complexDt != null)
                return complexDt.Name;

            // Check enum data types
            EnumDataType enumDt = _app.Enums.FindObject(guid);
            if (enumDt != null)
                return enumDt.Name;

            return "UnknownType";
        }

        private string GetDataTypeRef(Guid guid)
        {
            string typeName = GetDataTypeName(guid);
            if (typeName == "UnknownType" || typeName == "None")
                return null;
            return $"//DataDefinitions/{typeName}";
        }

        private string GetCsInterfaceName(PortDefenition portDef)
        {
            if (portDef.InterfaceGUID == Guid.Empty)
                return "UnknownInterface";

            ClientServerInterface csInterface = _app.ClientServerInterfaces.FindObject(portDef.InterfaceGUID);
            if (csInterface != null)
                return csInterface.Name;

            return "UnknownInterface";
        }

        private string GetCsInterfaceNameByPort(PortPainter portPainter)
        {
            if (portPainter.PortDefenition == null)
                return "UnknownInterface";
            
            return GetCsInterfaceName(portPainter.PortDefenition);
        }
    }
}