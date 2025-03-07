﻿using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.IO;
using AutosarGuiEditor.Source.RteGenerator.CLang;

namespace AutosarGuiEditor.Source.RteGenerator.TestGenerator
{
    class TestRteEnvironmentGenerator
    {
        public String TestRte_c_filename = "";

        public void GenerateRteEnvironment(String outputDir)
        {
            /* Generate Rte_<ComponentName>.h file */
            RteComponentGenerator_C compGenerator = new RteComponentGenerator_C();

            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateTestRteHFile(compDef, outputDir);
                ComponentRteHeaderGenerator_C.GenerateHeader(outputDir, compDef);
            }
            

            RteTestConnectionCGenerator rteConnectionsGenerator = new RteTestConnectionCGenerator();
            rteConnectionsGenerator.GenerateConnections(outputDir);

            TestRteCommonHGegenerator commonHgenerator = new TestRteCommonHGegenerator();
            commonHgenerator.GenerateRteTestCommonHFile(outputDir);

            RteSchedulerGenerator_C schedulerGenerator = new RteSchedulerGenerator_C();
            schedulerGenerator.GenerateShedulerFiles(outputDir);
        }

        public void GenerateCommonFiles(String outputDir)
        {
            /* Generate Rte_DataTypes.h file */
            RteDataTypesGenerator_C datatypeGenerator = new RteDataTypesGenerator_C();
            datatypeGenerator.GenerateDataTypesFile(outputDir);

            /* Generate SystemErrors.h */
            SystemErrorGenerator_C systemErrorGenerator = new SystemErrorGenerator_C();
            systemErrorGenerator.GenerateSystemErrorsFile(outputDir);

            /* Generate TestInitialization.h */
            //GenerateTestInitializationFile(AutosarApplication.GetInstance().ComponentDefenitionsList, outputDir);

            ReturnCodesGenerator_C returnCodesGenerator = new ReturnCodesGenerator_C();
            returnCodesGenerator.GenerateReturnCodesFile(outputDir);
        }

        void GenerateTestRteHFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" +  compDef.Name +"_" + Properties.Resources.TEST_RTE_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_H_FILENAME, "This file contains structure for provide test environment.");
            RteFunctionsGenerator_C.OpenCppGuardDefine(writer);
            RteFunctionsGenerator_C.OpenCGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator_C.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            writer.WriteLine("#define RTE_C");
            RteFunctionsGenerator_C.AddInclude(writer, "Rte_" + compDef.Name + ".h");
            writer.WriteLine("#undef  RTE_C");
            //RteFunctionsGenerator.AddInclude(writer, Properties.Resources.TEST_FRAMEWORK_H_FILENAME);
            RteFunctionsGenerator_C.AddInclude(writer, "<string.h>");

            writer.WriteLine();

            /* Create structure */
            if (compDef.IsComponentGenerable())
            {
                CreateTestArtefactStructure(writer, compDef);
            }
            else
            {
                writer.WriteLine("/* " + compDef.Name + " is empty. Nothing to generate. */");
            }

            writer.WriteLine();

            RteFunctionsGenerator_C.CloseCGuardDefine(writer);
            RteFunctionsGenerator_C.CloseCppGuardDefine(writer);
            writer.Close();
        }

        void GenerateStructureOfComponent(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            writer.WriteLine("/***********************************************/");
            writer.WriteLine("/* Structure of " + compDefenition.Name +  "*/");
            writer.WriteLine("/***********************************************/");
            writer.WriteLine("typedef struct ");
        }

        public static string TestArtefactsStructureDataType(ApplicationSwComponentType compDef)
        {
            return "TestArtefacts_" + compDef.Name;
        }

        string TestArtefactsVariable(ApplicationSwComponentType compDef)
        {
            return "testArtefacts_" + compDef.Name;
        }

        const string TestInitializationFunctionName = "Rte_TestInitialization";
        const string ParametersCount = "ParametersCount";

        void CreateTestArtefactStructure(StreamWriter writer, ApplicationSwComponentType compDef)
        {
            writer.WriteLine("typedef struct");
            writer.WriteLine("{");
            
            /* Write All CData structures */
            foreach (CDataDefenition cdataDef in compDef.CDataDefenitions)
            {
                writer.WriteLine("    struct");
                writer.WriteLine("    {");
                writer.WriteLine("        uint32 CallCount;");
                writer.WriteLine("        " + cdataDef.DataType.Name + " Value;");
                writer.WriteLine("    } " + RteFunctionsGenerator_C.GenerateShortCDataFunctionName(cdataDef) + ";");
                writer.WriteLine();
            }

            /* Write All Rte Read structures */
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        writer.WriteLine("    struct");
                        writer.WriteLine("    {");
                        writer.WriteLine("        uint32 CallCount;");
                        writer.WriteLine("        Std_ReturnType ReturnValue;");

                        writer.WriteLine("        struct");
                        writer.WriteLine("        {");
                        if (srInterface.IsQueued == false)
                        {
                            if (field.IsPointer == false)
                            {
                                writer.WriteLine("            " + field.DataTypeName + " data;");
                            }
                            else
                            {
                                writer.WriteLine("            " + field.DataTypeName + " * data;");
                            }
                        }
                        else
                        {
                            writer.WriteLine("            " + field.QueuedInterfaceName(srInterface.Name) + " data;");
                        }
                        writer.WriteLine("        } Arguments;");
                        writer.WriteLine("        Std_ReturnType (*redirection)" + RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false) + ";");
                        writer.WriteLine("        Std_ReturnType (*hook)" + RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType, false) + ";");
                        String fieldName = RteFunctionsGenerator_C.GenerateReadWriteFunctionName(portDef, field);
                        writer.WriteLine("    } " + fieldName + ";");
                        writer.WriteLine();
                    }
                }
            }

            /* Write All pims */
            foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
            {
                writer.WriteLine("    struct");
                writer.WriteLine("    {");
                writer.WriteLine("        uint32 CallCount;");
                writer.WriteLine("        " + pimDef.DataTypeName + " data;");
                string pimFieldName = RteFunctionsGenerator_C.GenerateShortPimFunctionName(pimDef);
                writer.WriteLine("    } " + pimFieldName + ";");
                writer.WriteLine();
            }

            /* Write all client calls */
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface srInterface = (ClientServerInterface)portDef.InterfaceDatatype;
                    foreach (ClientServerOperation csOperation in srInterface.Operations)
                    {
                        writer.WriteLine("    struct");
                        writer.WriteLine("    {");
                        writer.WriteLine("        uint32 CallCount;");
                        writer.WriteLine("        Std_ReturnType ReturnValue;");

                        if (csOperation.Fields.Count > 0)
                        {
                            writer.WriteLine("        struct");
                            writer.WriteLine("        {");
                            foreach (var field in csOperation.Fields)
                            {
                                string fieldDataType = "";

                                switch (field.Direction)
                                {
                                    case ClientServerOperationDirection.VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.CONST_VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.VAL_REF: fieldDataType = field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.CONST_VAL_REF: fieldDataType = "const " + field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.VAL_CONST_REF: fieldDataType = field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.CONST_VAL_CONST_REF: fieldDataType = "const " + field.DataTypeName + " *"; break;
                                }

                                writer.WriteLine("            " + fieldDataType + " " + field.Name + ";");
                            }
                        
                            writer.WriteLine("        } Arguments;");
                        }
                        writer.WriteLine("        Std_ReturnType (*redirection)(" + RteFunctionsGenerator_C.GenerateClientServerInterfaceArguments(csOperation, compDef.MultipleInstantiation) + ");");
                        writer.WriteLine("        Std_ReturnType (*hook)(" + RteFunctionsGenerator_C.GenerateClientServerInterfaceArguments(csOperation, compDef.MultipleInstantiation) + ");");
                        String operationName = RteFunctionsGenerator_C.Generate_InternalRteCall_FunctionName(portDef, csOperation);
                        writer.WriteLine("    } " + operationName + ";");
                        writer.WriteLine();
                    }
                }
            }

            writer.WriteLine("} " + TestArtefactsStructureDataType(compDef) + ";");
            writer.WriteLine();
        }

        void GenerateTestRteCFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" + compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME, "This file contains function for test environment.");

            /* Add includes */
            RteFunctionsGenerator_C.AddInclude(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_H_FILENAME);
            
            writer.WriteLine();

            /* Declare test structure */
            writer.WriteLine(TestArtefactsStructureDataType(compDef) + " " + TestArtefactsVariable(compDef) + ";");
            
            writer.WriteLine();

            /* Create functions */
            CreateRteWriteFunctions(writer, compDef);

            CreateRteReadFunctions(writer, compDef);

            CreateRteCDataFunctions(writer, compDef);

            CreateRtePimFunctions(writer, compDef);

            CreateRteCallFunctions(writer, compDef);
          

            RteFunctionsGenerator_C.WriteEndOfFile(writer);
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
                            if (srInterface.IsQueued == false)
                            {
                                GenerateRteWritePortFieldFunction(writer, compDefenition, port, field);
                            }
                            else
                            {
                                GenerateQueuedRteWritePortFieldFunction(writer, compDefenition, port, field);
                            }
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
                            if (srInterface.IsQueued == false)
                            {
                                GenerateRteReadPortFieldFunction(writer, compDefenition, port, field);
                            }
                            else
                            {
                                GenerateRteQueuedReadPortFieldFunction(writer, compDefenition, port, field);
                            }
                        }
                    }
                }
            }
        }

        void CreateRteCDataFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (CDataDefenition cdataDef in compDefenition.CDataDefenitions)
            {
                GenerateRteCDataFunction(writer, compDefenition, cdataDef);
            }
        }

        void CreateRtePimFunctions(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PimDefenition pim in compDefenition.PerInstanceMemoryList)
            {
                GenerateRtePimFunction(writer, compDefenition, pim);
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

        void GenerateRteWritePortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_C.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            String fieldName = RteFunctionsGenerator_C.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + ".data" + fieldName + ".CallCount++;");
            writer.WriteLine("    memcpy(&" + TestArtefactsVariable(compDef) + "." + fieldName + ".Arguments.data, data, sizeof(" + field.DataTypeName + "));");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateQueuedRteWritePortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_C.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

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
                    {
                        String copyFromField = RteFunctionsGenerator_C.GenerateComponentName(compInstance.Name) + "." + RteFunctionsGenerator_C.GenerateRteWriteFieldInComponentDefenitionStruct(port, field);
                        SenderReceiverInterface srInterface = port.InterfaceDatatype as SenderReceiverInterface;
                        int queueSize = srInterface.QueueSize;

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
                        writer.WriteLine("    return _returnValue;");
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
                            String copyFromField = RteFunctionsGenerator_C.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                            String oppositeCompName = RteFunctionsGenerator_C.GenerateComponentName(oppositCompInstance.Name);
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

        void GenerateRteReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_C.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            String fieldName = RteFunctionsGenerator_C.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + fieldName + ".CallCount++;");
            writer.WriteLine("    memcpy(data, &" + TestArtefactsVariable(compDef) + "." + fieldName + ".Arguments.data, sizeof(" + field.DataTypeName + "));");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteQueuedReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_C.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_C.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

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
                    {
                        String copyFromField = TestArtefactsVariable(compInstance.ComponentDefenition) + "." + RteFunctionsGenerator_C.GenerateReadWriteFunctionName(port, field) + ".Arguments.data";
                        SenderReceiverInterface srInterface = port.InterfaceDatatype as SenderReceiverInterface;
                        int queueSize = srInterface.QueueSize;

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
                        writer.WriteLine("    return _returnValue;");
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
                            String copyFromField = RteFunctionsGenerator_C.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                            String oppositeCompName = RteFunctionsGenerator_C.GenerateComponentName(oppositCompInstance.Name);
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

        void GenerateRteCDataFunction(StreamWriter writer, ApplicationSwComponentType compDef, CDataDefenition cdataDef)
        {
            String returnValue = cdataDef.DataTypeName + " ";
            String RteFuncName = RteFunctionsGenerator_C.GenerateShortCDataFunctionName(cdataDef);

            String fieldVariable = RteFunctionsGenerator_C.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".CallCount++;");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".Value;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRtePimFunction(StreamWriter writer, ApplicationSwComponentType compDef, PimDefenition pimDef)
        {
            String returnValue = pimDef.DataTypeName + " * ";
            String RteFuncName = RteFunctionsGenerator_C.GenerateShortPimFunctionName(pimDef);

            String fieldVariable = RteFunctionsGenerator_C.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".CallCount++;");
            writer.WriteLine("    return &" + TestArtefactsVariable(compDef) + "." + RteFuncName + ".data;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCallPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, ClientServerOperation operation)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_C.Generate_RteCall_ConnectionGroup_FunctionName(compDef, port, operation);
            String fieldVariable = RteFunctionsGenerator_C.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    if (" + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + "redirection == NULL)");
            writer.WriteLine("    {");
            writer.WriteLine("        " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".CallCount++;");
            foreach (var csField in operation.Fields)
            {
                writer.WriteLine("        " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".Arguments." + csField.Name + " = " + csField.Name + ";");
            }
            writer.WriteLine("        return " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".ReturnValue;");
            writer.WriteLine("    }");
            writer.WriteLine("    else");
            writer.WriteLine("    {");
            string fparams = "";
            for(int i = 0; i < operation.Fields.Count; i++)
            {
                if (i != 0)
                {
                    fparams += ", ";
                }
                fparams += operation.Fields[i].Name;
            }
            writer.WriteLine("        return " + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + "redirection(" + fparams + ");");
            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateTestInitializationFile(ComponentDefenitionList compDefs, String outputDir)
        {
            String FileName = outputDir + "\\" + Properties.Resources.TEST_INITIALIZATION_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_C.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_C_FILENAME, "This file contains functions for component initialization before each test");
            RteFunctionsGenerator_C.OpenCppGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator_C.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator_C.AddInclude(writer, "<Rte_TestCommon.h>");
            foreach (ApplicationSwComponentType swCompType in compDefs)
            {
                RteFunctionsGenerator_C.AddInclude(writer, swCompType.Name + "_TestRte.h");
            }
            
            writer.WriteLine();

            /* Write parameters count as equal value to component instances */
            //ComponentInstancesList componentInstances = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);

            //writer.WriteLine("const int " + ParametersCount + " = " + componentInstances.Count.ToString() + ";");
            //writer.WriteLine();

            /* Add function for component variable initialization */
            //writer.WriteLine("/* User function for variable initialization */");
            //writer.WriteLine("extern void  VariableInitialization();");
            writer.WriteLine();
            

            /* Declare test initialization function */
            writer.WriteLine("void " + TestInitializationFunctionName + "(int paramIndex)");

            writer.WriteLine("{");

            writer.WriteLine("    memset(&TEST_STUB_RECORD, 0u, sizeof(TEST_STUB_RECORD));");
            foreach (OsTask task in AutosarApplication.GetInstance().OsTasks)
            {
                String taskCounter = RteFunctionsGenerator_C.CreateTaskCounter(task.Name);
                writer.WriteLine("    extern uint32 " + taskCounter + ";");
                writer.WriteLine("    " + taskCounter + " = 0U;");
            }
            /* Clear test structure */
            //foreach (ApplicationSwComponentType swCompType in compDefs)
            //{
            //    writer.WriteLine("    memset(&" + TestArtefactsVariable(swCompType) + ", 0, sizeof(" + TestArtefactsVariable(swCompType) + " ));");
            //}
            //if (componentInstances.Count > 0)
            //{
            //    /* Fill CData constants */
            //    writer.WriteLine("    switch(paramIndex)");
            //    writer.WriteLine("    {");
            //    for (int i = 0; i < componentInstances.Count; i++)
            //    {
            //        writer.WriteLine("        case " + i.ToString() + ":");
            //        writer.WriteLine("        {");

            //        /* CDATA */
            //        for (int j = 0; j < compDef.CDataDefenitions.Count; j++)
            //        {
            //            CDataDefenition cdataDef = compDef.CDataDefenitions[j];
            //            CDataInstance cdataInstance = componentInstances[i].CDataInstances.GetCData(cdataDef.GUID);

            //            writer.Write("            " + TestArtefactsVariable + "." + RteFunctionsGenerator.GenerateShortCDataFunctionName(cdataDef) + ".Value = ");
                
            //            IGUID datatype = cdataDef.DataType;
            //            if ((datatype is BaseDataType) || (datatype is SimpleDataType) || (datatype is EnumDataType))
            //            {
            //                RteFunctionsGenerator.WriteDefaultValueForCDataDefenition(writer, componentInstances[i], cdataInstance);
            //            }
            //            writer.WriteLine(";");
            //        }

            //        for (int j = 0; j < compDef.PerInstanceMemoryList.Count; j++)
            //        {
            //            PimDefenition pimDef = compDef.PerInstanceMemoryList[j];
            //            PimInstance pimInstance = componentInstances[i].PerInstanceMemories.GetPim(pimDef.GUID);

            //            WritePIMDefaultValues(writer, pimInstance);
            //        }

            //        writer.WriteLine("            break;");
            //        writer.WriteLine("        }");
            //    }
            //    writer.WriteLine("    }");
            //}

            /* Write RTE_OK to all return functions */
            //foreach (ApplicationSwComponentType compDef in compDefs)
            //{
            //    foreach (PortDefenition portDef in compDef.Ports)
            //    {
            //        if (portDef.InterfaceDatatype is SenderReceiverInterface)
            //        {
            //            SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
            //            foreach (SenderReceiverInterfaceField srField in srInterface.Fields)
            //            {
            //                String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, srField);
            //                writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue = RTE_E_OK;");
            //            }
            //        }
            //    }
            //}
            //writer.WriteLine(); 

            /* Initialize component variables */
            //writer.WriteLine("    VariableInitialization();");

            writer.WriteLine("}");           

            RteFunctionsGenerator_C.CloseCppGuardDefine(writer);
            RteFunctionsGenerator_C.WriteEndOfFile(writer);
            writer.Close();
        }

        void WritePIMDefaultValues(StreamWriter writer, ApplicationSwComponentType compDef, PimInstance pim)
        {
            String FieldValue = "            " + TestArtefactsVariable(compDef) + "." + RteFunctionsGenerator_C.GenerateShortPimFunctionName(pim.Defenition) + ".data";

            IGUID datatype = pim.Defenition.DataType;

            if ((datatype is BaseDataType) || (datatype is SimpleDataType) || (datatype is EnumDataType))
            {
                String defValue = GetDefaultValueForUnComplexDataType(pim);
                writer.WriteLine(FieldValue + " = " + defValue + ";");
            }
            else if (datatype is ComplexDataType)
            {
                for (int defValueIndex = 0; defValueIndex < pim.DefaultValues.Count; defValueIndex++)
                {           
                    ComplexDataType complexDataType = (ComplexDataType)datatype;
                    PimDefaultValue defaultValue = pim.DefaultValues[defValueIndex];

                    ComplexDataTypeField dataTypefield = complexDataType.Fields.FindObject(defaultValue.FieldGuid);
                    if (dataTypefield != null)
                    {
                        String dataToWrite = FieldValue + "." + dataTypefield.Name + " = " + defaultValue.DefaultValue + ";";
                        writer.WriteLine(dataToWrite);
                    }
                }
            }
        }

        String GetDefaultValueForUnComplexDataType(PimInstance pim)
        {
            if (pim.DefaultValues.Count == 1)
            {
                if (pim.DefaultValues[0].DefaultValue.Length > 0)
                {
                    return pim.DefaultValues[0].DefaultValue;
                }
                else
                {
                    return "0";
                }
            }
            else
            {
                return "0";
            }
        }
    }
}
