using System;
using System.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.DataTypes;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Painters.Components.CData;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteGenerator
    {
        public bool Generate()
        {
            /* Create base folders */
            Directory.CreateDirectory(RteFunctionsGenerator.GetComponentsFolder());
            Directory.CreateDirectory(RteFunctionsGenerator.GetRteFolder());
            GenerateDataTypesFile();
            GenerateComponentsFiles();
            GenerateConnections();
            GenerateScheduler();

            ReturnCodesGenerator returnCodesGenerator = new ReturnCodesGenerator();
            returnCodesGenerator.GenerateReturnCodesFile(RteFunctionsGenerator.GetRteFolder());

            /* Create system errors file */
            SystemErrorGenerator systemErrorGenerator = new SystemErrorGenerator();
            systemErrorGenerator.GenerateSystemErrorsFile(RteFunctionsGenerator.GetRteFolder());

            return true;
        }

       
        void GenerateDataTypesFile()
        {
            RteDataTypesGenerator dataTypesGenerator = new RteDataTypesGenerator();
            dataTypesGenerator.GenerateDataTypesFile(RteFunctionsGenerator.GetRteFolder());
        }

        
        void GenerateComponentsFiles()
        {
            RteComponentGenerator componentGenerator = new RteComponentGenerator();
            componentGenerator.GenerateComponentsFiles();
        }
        

        void GenerateScheduler()
        {
            RteSchedulerGenerator schedulerGenerator = new RteSchedulerGenerator();
            schedulerGenerator.GenerateShedulerFiles();
        }
#region CONNECTIONS
        public void GenerateConnections()
        {
            String filename = RteFunctionsGenerator.GetRteFolder() + "\\"+ Properties.Resources.RTE_CONNECTIONS_C_FILENAME;
            StreamWriter writer = new StreamWriter(filename);
            RteFunctionsGenerator.GenerateFileTitle(writer, filename, "Implementation for RTE connections source file");
    
            /*Add #include */
            RteFunctionsGenerator.AddInclude(writer, "<string.h>");
            switch (AutosarApplication.GetInstance().MCUType.Type)
            {
                case MCUTypeDef.STM32F1xx:
                {
                    //RteFunctionsGenerator.AddInclude(writer, "<stm32f1xx.h>");
                    break;
                }
                case MCUTypeDef.STM32F4xx:
                {
                    //RteFunctionsGenerator.AddInclude(writer, "<stm32f4xx.h>");
                    break;
                }
            }
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.SYSTEM_ERRORS_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_EXTERNAL_RUNNABLES_H_FILENAME);

            /* Include */
            writer.WriteLine("");
         
            /* Create extern variables */
            GenerateAllComponentInstances(writer);

            /* Generate all rte write functions */
            foreach(ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                writer.WriteLine("/**************************************************");
                writer.WriteLine("        " + compDef.Name);
                writer.WriteLine("***************************************************/");

                /* Check if this components exists */
                if (AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef).Count > 0)
                {
                    if (compDef.Name == "DistanceProtector")
                    {

                    }
                    CreateRteWriteFunctions(writer, compDef);

                    CreateRteReadFunctions(writer, compDef);

                    CreateRteCallFunctions(writer, compDef);

                    CreateRtePimFunctions(writer, compDef);

                    CreateRteCDataFunctions(writer, compDef);
                }


            }

            writer.Close();
        }        
        

        void CreateRteWriteFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Sender)
                {
                    SenderReceiverInterface srInterface = port.InterfaceDatatype as SenderReceiverInterface;
                    if (srInterface != null) // prevent for unselected interface for port
                    {
                        foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                        {
                            GenerateRteWritePortFieldFunction(writer, compDefenition, port, field);
                        }
                    }
                }
            }
        }

        void CreateRteReadFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = port.InterfaceDatatype as SenderReceiverInterface;
                    if (srInterface != null) // prevent for unselected interface for port
                    {
                        foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                        {
                            GenerateRteReadPortFieldFunction(writer, compDefenition, port, field);
                        }
                    }
                }
            }
        }


        void CreateRteCallFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = port.InterfaceDatatype as ClientServerInterface;
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        GenerateRteCallPortFieldFunction(writer, compDefenition, port, operation);
                    }
                }
            }
        }

        void CreateRtePimFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PimDefenition pim in compDefenition.PerInstanceMemoryList)
            {                
                GenerateRtePimFunction(writer, compDefenition, pim);                
            }
        }


        void CreateRteCDataFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (CDataDefenition cdataDef in compDefenition.CDataDefenitions)
            {
                GenerateRteCDataFunction(writer, compDefenition, cdataDef);
            }
        }


        String GenerateComplexDatatypeDefaultValue(ComplexDataType datatype, int indent)
        {
            String strIndent = RteFunctionsGenerator.FillStringForCount("", ' ', indent);
            String str = "{" + Environment.NewLine;            
            for (int i = 0; i < datatype.Fields.Count; i++)
            {
                ComplexDataTypeField field = datatype.Fields[i];
                if (IsDatatypeSimple(field.DataTypeGUID))
                {
                    str += RteFunctionsGenerator.GenerateDefaultValueForSimpleDataTypeField(field.Name, indent + 4);
                }
                else
                {
                    ComplexDataType complexDt = AutosarApplication.GetInstance().ComplexDataTypes.FindObject(field.DataTypeGUID);
                    str += RteFunctionsGenerator.FillStringForCount("", ' ', indent + 4) + "." + field.Name + " = " + Environment.NewLine;
                    str += RteFunctionsGenerator.FillStringForCount("", ' ', indent + 4) + GenerateComplexDatatypeDefaultValue(complexDt, indent + 4);
                }
                if (i != datatype.Fields.Count - 1)
                {
                    str += "," + Environment.NewLine;
                }
            }
            str += Environment.NewLine + strIndent + "}";
            return str;
        }

        bool IsDatatypeSimple(Guid guid)
        {
            bool result = false;
            if (AutosarApplication.GetInstance().BaseDataTypes.FindObject(guid) != null)
            {
                result = true;
            }
            if (AutosarApplication.GetInstance().SimpleDataTypes.FindObject(guid) != null)
            {
                result = true;
            }
            if (AutosarApplication.GetInstance().Enums.FindObject(guid) != null)
            {
                result = true;
            }
            return result;
        }
        
       

        String GenerateRteComponentSenderReceiverInterfaceField_DefaultValue(String ComponentName, String PortName, SenderReceiverInterfaceField field)
        {
            String str = "";

            if (IsDatatypeSimple(field.BaseDataTypeGUID))
            {
                str = " 0";
            }
            else
            {
                str = " { 0 }";
            }

            return str;
        }

        void GenerateAllComponentInstances(StreamWriter writer)
        {
            /* Without multiple instantiation */
            foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
            {
                foreach (ComponentInstance component in composition.ComponentInstances)
                {
                    GenerateComponentInstance(writer, component);
                }
            }
        }

        void GenerateComponentInstance(StreamWriter writer, ComponentInstance component)
        {
            ApplicationSwComponentType compDefenition = component.ComponentDefenition;

            int elementsCount = compDefenition.Ports.PortsWithSenderInterface().Count + compDefenition.CDataDefenitions.Count + compDefenition.PerInstanceMemoryList.Count;

            if ((elementsCount == 0) && (compDefenition.MultipleInstantiation == false))
            {
                return; // because nothing to generate.
            }

            writer.WriteLine("/* Component instance :" + component.Name + "  */");

            String compDefString = RteFunctionsGenerator.FillStringForCount(compDefenition.Name, ' ', 50);
            String compName = RteFunctionsGenerator.GenerateComponentName(component.Name);
            writer.WriteLine(compDefString + compName + " = ");
            writer.WriteLine("{");
            /* Fill variables */
                        
            /* Component index */
            if (compDefenition.MultipleInstantiation)
            {
               ComponentInstancesList compInstances = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDefenition);
               int compIndex = compInstances.IndexOf(component);
               writer.WriteLine("    .index = " + compIndex.ToString() + ", ");
            }

            /* Pim*/
            int count = 0;
            for (int i = 0; i < compDefenition.PerInstanceMemoryList.Count; i++)
            {
                PimDefenition pimDef = compDefenition.PerInstanceMemoryList[i];
                count++;
                
                PimInstance pimInstance = component.PerInstanceMemories.GetPim(pimDef.GUID);

                writer.Write("    ." + RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(component.ComponentDefenition, pimDef) + " = ");
                
                IGUID datatype = pimDef.DataType;
                if ((datatype is BaseDataType) || (datatype is SimpleDataType) || (datatype is EnumDataType))
                {
                    RteFunctionsGenerator.WriteDefaultValueForPimDefenition(writer, component, pimInstance);
                }
                else if (datatype is ArrayDataType)
                {
                    writer.Write(" {{ ");
                    if (!((datatype as ArrayDataType).DataType is PlainDataType))
                    {
                        writer.Write("{ ");
                    }
                    writer.Write(" 0 ");

                    if (!((datatype as ArrayDataType).DataType is PlainDataType))
                    {
                        writer.Write("} ");
                    }

                    writer.Write(" }}");                 
                }
                else if (datatype is ComplexDataType)
                {
                    writer.Write("{ 0 }");
                }
                if ((i != compDefenition.PerInstanceMemoryList.Count - 1) || ((compDefenition.CDataDefenitions.Count > 0) || (compDefenition.Ports.PortsWithSenderInterface().Count > 0)))
                {
                    writer.WriteLine(",");
                }
            }

            /* CData */
            for (int i = 0; i < compDefenition.CDataDefenitions.Count; i++)
            {

                count++;
                CDataDefenition cdataDef = compDefenition.CDataDefenitions[i];

                CDataInstance cdataInstance = component.CDataInstances.GetCData(cdataDef.GUID);

                writer.Write("    ." + RteFunctionsGenerator.GenerateRteCDataFieldInComponentDefenitionStruct(component.ComponentDefenition, cdataDef) + " = ");
                
                IGUID datatype = cdataDef.DataType;
                if ((datatype is BaseDataType) || (datatype is SimpleDataType) || (datatype is EnumDataType))
                {
                    RteFunctionsGenerator.WriteDefaultValueForCDataDefenition(writer, component, cdataInstance);
                }
                else if (datatype is ArrayDataType)
                {
                    writer.Write(" {{ ");
                    if(!((datatype as ArrayDataType).DataType is PlainDataType))
                    {
                        writer.Write("{ ");
                    }
                    writer.Write(" 0 ");

                    if (!((datatype as ArrayDataType).DataType is PlainDataType))
                    {
                        writer.Write("} ");
                    }

                    writer.Write(" }}");                    
                }
                else if (datatype is ComplexDataType)
                {
                    writer.WriteLine(" { 0 }");
                }

                if ((i !=  compDefenition.CDataDefenitions.Count - 1) || (compDefenition.Ports.PortsWithSenderInterface().Count > 0))
                {
                    writer.WriteLine(",");
                }
            }

            /* Port */
            PortDefenitionsList senderPorts = component.ComponentDefenition.Ports.PortsWithSenderInterface();
            for (int i = 0; i < senderPorts.Count; i++)
            {
                SenderReceiverInterface srInterface = (SenderReceiverInterface)senderPorts[i].InterfaceDatatype;
                WriteZeroDefaultValueForSenderPort(writer, senderPorts[i], srInterface);

                if (i < senderPorts.Count - 1)
                {
                    writer.WriteLine(",");
                }
            }
            writer.WriteLine();
            writer.WriteLine("};");
            writer.WriteLine("");
        }


        void WriteZeroDefaultValueForSenderPort(StreamWriter writer, PortDefenition portDef, SenderReceiverInterface srInterface)
        {
            for (int j = 0; j < srInterface.Fields.Count; j++)
            {
                SenderReceiverInterfaceField field = srInterface.Fields[j];

                writer.Write("    ." + RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(portDef, field) + " = ");

                WriteZeroDefaultValue(writer, field.DataType);

                if (j < srInterface.Fields.Count - 1)
                {
                    writer.WriteLine(",");
                }
            }
        }

        void WriteZeroDefaultValueForPlainDataType(StreamWriter writer)
        {
            writer.Write("0");
        }

        void WriteZeroDefaultValuesForArrays(StreamWriter writer, ArrayDataType datatype)
        {
            writer.Write("{ { ");
            for (int i = 0; i < datatype.Size; i++)
            {
                WriteZeroDefaultValue(writer, datatype.DataType);
                if (i < datatype.Size - 1)
                {
                    writer.Write(", ");
                }
            }
            writer.Write(" } }");
        }

        void WriteZeroDefaultValuesForComplexDataType(StreamWriter writer, ComplexDataType datatype)
        {
            writer.Write("{ ");
            for (int i = 0; i < datatype.Fields.Count; i++)
            {
                WriteZeroDefaultValue(writer, datatype.Fields[i].DataType);
                if (i < datatype.Fields.Count - 1)
                {
                    writer.Write(", ");
                }
            }

            writer.Write(" }");
        }

        void WriteZeroDefaultValue(StreamWriter writer, object dataType)
        {
            if (dataType is PlainDataType)
            {
                WriteZeroDefaultValueForPlainDataType(writer);
            }
            else if (dataType is ArrayDataType)
            {
                WriteZeroDefaultValuesForArrays(writer, dataType as ArrayDataType);
            }
            else if (dataType is ComplexDataType)
            {
                WriteZeroDefaultValuesForComplexDataType(writer, dataType as ComplexDataType);
            }
            else
            {
                /* ? */
            }
        }

        void GenerateStaticVariablesForSenderPortsWithoutMultipleInstantiation(StreamWriter writer, PortPainter port, SenderReceiverInterfaceField field)
        {
            /* Declare variable and its default value */
            IElementWithPorts compInst = AutosarApplication.GetInstance().FindComponentInstanceByPortGuid(port.GUID);

            String staticVariableName = RteFunctionsGenerator.GenerateRte_Component_SRInterface_Name(compInst.Name, port.Name, field);
            if (!field.IsPointer)
            {
                writer.Write("static " + field.DataTypeName + " " + staticVariableName + " =");
                writer.Write(GenerateRteComponentSenderReceiverInterfaceField_DefaultValue(compInst.Name, port.Name, field));
            }
            else
            {
                writer.Write("static " + field.DataTypeName + "* " + staticVariableName + " = 0");
            }

            writer.WriteLine(";");
            writer.WriteLine("");       
        }

        

        void GenerateRteWritePortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);            
            writer.WriteLine("{");

            if (!compDef.MultipleInstantiation) //single instantiation component 
            { 
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    ComponentInstance compInstance = components[0];
                    String compName = RteFunctionsGenerator.GenerateComponentName(compInstance.Name);
                    String fieldName = RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(port, field);
                    writer.WriteLine("    " + compName + "." + fieldName + " = *" + field.Name + ";");
                }
            }
            else //multiple instantiation component 
            {
                String rteStructFieldName = RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(port, field);
                String instanceLine = "((" + compDef.Name + "*)instance)->" + rteStructFieldName + " = *" + field.Name + ";";
                writer.WriteLine("    " + instanceLine);
            }

            writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
            writer.WriteLine("}");
            writer.WriteLine("");
            //}
        }


        void GenerateRteReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

            if (!compDef.MultipleInstantiation) //single instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    /* At least one component exists at this step */
                    ComponentInstance compInstance = components[0];

                    PortPainter portPainter = compInstance.Ports.FindPortByItsDefenition(port);
                    ComponentInstance oppositCompInstance;
                    PortPainter oppositePort;
                    AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter,out oppositCompInstance, out oppositePort);
                    if (oppositCompInstance != null)
                    {
                        String copyFromField = RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                        String oppositeCompName = RteFunctionsGenerator.GenerateComponentName(oppositCompInstance.Name);
                        writer.WriteLine("    memcpy(" + field.Name + ", " + "&" + oppositeCompName + "." + copyFromField + ", sizeof(" + field.DataTypeName + "));");
                        writer.WriteLine("    return " + Properties.Resources.RTE_E_OK + ";");
                    }
                    else 
                    {
                        writer.WriteLine("    memset(" + field.Name + ", " + "0, sizeof(" + field.DataTypeName + "));");
                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                    }
                }
            }
            else //multiple instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    writer.WriteLine("    switch(((" + compDef.Name + "*)" + "instance)->index)");
                    writer.WriteLine("    {");
                    for (int i = 0; i < components.Count; i++)
                    {
                        ComponentInstance compInstance = components[i];
                        PortPainter portPainter = compInstance.Ports.FindPortByItsDefenition(port);
                        ComponentInstance oppositCompInstance;
                        PortPainter oppositePort;
                        AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                        writer.WriteLine("        case " + i.ToString() + ": ");
                        writer.WriteLine("        {");

                        if (oppositCompInstance != null)
                        {
                            String copyFromField = RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                            String oppositeCompName = RteFunctionsGenerator.GenerateComponentName(oppositCompInstance.Name);
                            writer.WriteLine("            memcpy(" + field.Name + ", " + "&" + oppositeCompName + "." + copyFromField + ", sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return " + Properties.Resources.RTE_E_OK + ";");
                        }
                        else
                        {
                            writer.WriteLine("            memset(" + field.Name + ", " + "0, sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                        }

                        writer.WriteLine("        }");
                    }
                     writer.WriteLine("    }");
                }
                writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCallPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, ClientServerOperation operation)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(compDef, port, operation);
            String fieldVariable = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);

            writer.WriteLine("#ifndef TEST_RTE");

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

            
            if (!compDef.MultipleInstantiation) //single instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    
                    ComponentInstance compInstance = components[0];

                    PortPainter portPainter = compInstance.Ports.FindPortByItsDefenition(port);
                    ComponentInstance oppositCompInstance;
                    PortPainter oppositePort;
                    AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);
                    if (oppositCompInstance != null)
                    {
                        String functionName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(oppositCompInstance.ComponentDefenition, oppositePort.PortDefenition, operation);
                        String arguments = RteFunctionsGenerator.Generate_ClientServerPort_Arguments(oppositCompInstance, operation, oppositCompInstance.ComponentDefenition.MultipleInstantiation);
                        writer.WriteLine("    return " + functionName + arguments + ";");
                    }
                    else
                    {                        
                        writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                    }
                }
            }
            else //multiple instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    writer.WriteLine("    switch(((" + compDef.Name + "*)" + "instance)->index)");
                    writer.WriteLine("    {");
                    for (int i = 0; i < components.Count; i++)
                    {
                        ComponentInstance compInstance = components[i];
                        PortPainter portPainter = compInstance.Ports.FindPortByItsDefenition(port);
                        ComponentInstance oppositCompInstance;
                        PortPainter oppositePort;
                        AutosarApplication.GetInstance().GetOppositePortAndComponent(portPainter, out oppositCompInstance, out oppositePort);

                        writer.WriteLine("        case " + i.ToString() + ": ");
                        writer.WriteLine("        {");

                        if (oppositCompInstance != null)
                        {
                            String functionName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(oppositCompInstance.ComponentDefenition, oppositePort.PortDefenition, operation);
                            String arguments = RteFunctionsGenerator.Generate_ClientServerPort_Arguments(oppositCompInstance, operation, oppositCompInstance.ComponentDefenition.MultipleInstantiation);
                            writer.WriteLine("            return " + functionName + arguments + ";");
                        }
                        else
                        {                            
                            writer.WriteLine("            return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
                        }

                        writer.WriteLine("        }");
                    }
                    writer.WriteLine("    }");
                }
                writer.WriteLine("    return " + Properties.Resources.RTE_E_UNCONNECTED + ";");
            }
            writer.WriteLine("}");
            writer.WriteLine("#endif /* TEST_RTE */");
            writer.WriteLine("");
        }


        void GenerateRtePimFunction(StreamWriter writer, ApplicationSwComponentType compDef, PimDefenition pimDef)
        {
            String returnValue = pimDef.DataTypeName + " * const ";
            String RteFuncName = RteFunctionsGenerator.GenerateFullPimFunctionName(compDef, pimDef);

            String fieldVariable = RteFunctionsGenerator.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

            if (!compDef.MultipleInstantiation) //single instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    /* At least one component exists at this step */
                    ComponentInstance compInstance = components[0];
                    String compName = RteFunctionsGenerator.GenerateComponentName(compInstance.Name);
                    String field = RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(compDef, pimDef);
                    writer.WriteLine("    return &" + compName + "." + field + ";");
                }
            }
            else //multiple instantiation component 
            {
                String field = RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(compDef, pimDef);
                writer.WriteLine("    return &((" + compDef.Name + "*)instance)->" + field + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCDataFunction(StreamWriter writer, ApplicationSwComponentType compDef, CDataDefenition cdataDef)
        {
            String returnValue = cdataDef.DataTypeName + " ";
            String RteFuncName = RteFunctionsGenerator.GenerateFullCDataFunctionName(compDef, cdataDef);

            String fieldVariable = RteFunctionsGenerator.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

            if (!compDef.MultipleInstantiation) //single instantiation component 
            {
                ComponentInstancesList components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                if (components.Count > 0)
                {
                    /* At least one component exists at this step */
                    ComponentInstance compInstance = components[0];
                    String compName = RteFunctionsGenerator.GenerateComponentName(compInstance.Name);
                    String field = RteFunctionsGenerator.GenerateRteCDataFieldInComponentDefenitionStruct(compDef, cdataDef);
                    writer.WriteLine("    return " + compName + "." + field + ";");
                }
            }
            else //multiple instantiation component 
            {
                String field = RteFunctionsGenerator.GenerateRteCDataFieldInComponentDefenitionStruct(compDef, cdataDef);
                writer.WriteLine("    return ((" + compDef.Name + "*)instance)->" + field + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }
#endregion
    }
}
