using AutosarGuiEditor.Source.Autosar.OsTasks;
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
using AutosarGuiEditor.Source.RteGenerator.CppLang;

namespace AutosarGuiEditor.Source.RteGenerator.TestGeneratorCpp
{
    class TestRteEnvironmentGeneratorCpp
    {
        public String TestRte_c_filename = "";

        public void GenerateRteEnvironment(String outputDir)
        {
            /* Generate Rte_<ComponentName>.h file */
            RteComponentGenerator_Cpp compGenerator = new RteComponentGenerator_Cpp();

            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                GenerateTestRteHppFile(compDef, outputDir);
                ComponentRteHeaderGenerator_Cpp.GenerateHeader(outputDir, compDef);
            }
            

            RteTestConnectionCGenerator rteConnectionsGenerator = new RteTestConnectionCGenerator();
            rteConnectionsGenerator.GenerateConnections(outputDir);

            TestRteCommonHppGegenerator commonHgenerator = new TestRteCommonHppGegenerator();
            commonHgenerator.GenerateRteTestCommonHFile(outputDir);

            RteSchedulerGenerator_Cpp schedulerGenerator = new RteSchedulerGenerator_Cpp();
            schedulerGenerator.GenerateShedulerFiles(outputDir);
        }

        public void GenerateCommonFiles(String outputDir)
        {
            /* Generate Rte_DataTypes.h file */
            RteDataTypesGenerator_Cpp datatypeGenerator = new RteDataTypesGenerator_Cpp();
            datatypeGenerator.GenerateDataTypesFile(outputDir);

            /* Generate SystemErrors.h */
            AutosarGuiEditor.Source.RteGenerator.CppLang.SystemErrorGenerator_Cpp systemErrorGenerator = new SystemErrorGenerator_Cpp();
            systemErrorGenerator.GenerateSystemErrorsFile(outputDir);

            /* Generate TestInitialization.h */
            //GenerateTestInitializationFile(AutosarApplication.GetInstance().ComponentDefenitionsList, outputDir);

            ReturnCodesGenerator_Cpp returnCodesGenerator = new ReturnCodesGenerator_Cpp();
            returnCodesGenerator.GenerateReturnCodesFile(outputDir);
        }

        void GenerateTestRteHppFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" +  compDef.Name +"_" + Properties.Resources.TEST_RTE_HPP_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_HPP_FILENAME, "This file contains structure for provide test environment.");
            RteFunctionsGenerator_Cpp.OpenCppGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);
            writer.WriteLine("#define RTE_CPP");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + compDef.Name + ".hpp");
            writer.WriteLine("#undef  RTE_CPP");
            //RteFunctionsGenerator.AddInclude(writer, Properties.Resources.TEST_FRAMEWORK_H_FILENAME);
            //RteFunctionsGenerator_Cpp.AddInclude(writer, "<string.h>");

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

            RteFunctionsGenerator_Cpp.CloseCppGuardDefine(writer);
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
            String stubName = RteFunctionsGenerator_Cpp.GenerateTestRteClassName(compDef);
            String rteName = RteFunctionsGenerator_Cpp.ComponentRteDataStructureDefenitionName(compDef);
            writer.WriteLine("struct " + stubName + " : " + rteName);
            writer.WriteLine("{");

            //==================================================================================================================
            //       ГЕНЕРАЦИЯ ЗАГЛУШЕК
            //==================================================================================================================
            writer.WriteLine("    struct");
            writer.WriteLine("    {");
            /* Write All CData structures */
            foreach (CDataDefenition cdataDef in compDef.CDataDefenitions)
            {
                writer.WriteLine("        struct");
                writer.WriteLine("        {");
                writer.WriteLine("            uint32 callCount {0};");
                var components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                String defaultValue = "0";

                if (components.Count > 0) {
                    try
                    {
                        defaultValue = components[0].CDataInstances.GetCData(cdataDef).DefaultValues[0].DefaultValue;
                    }
                    catch
                    {
                        defaultValue = "0";
                    }
                }
                writer.WriteLine("            " + cdataDef.DataType.Name + " returnValue {" + defaultValue + "};");

                writer.WriteLine("        } CData_" + cdataDef.Name + ";");
                writer.WriteLine();
            }

            /* Write All Rte Read structures */
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    writer.WriteLine("//----- PORT: " + portDef.Name);
                    writer.WriteLine("        struct");
                    SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                    writer.WriteLine("        {");
                    {
                        int count = 0;
                        foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                        {
                            writer.WriteLine("//--------- FIELD: " + field.Name);
                            writer.WriteLine("            struct");
                            writer.WriteLine("            {");
                            writer.WriteLine("                uint32         callCount { 0u };");
                            writer.WriteLine("                Std_ReturnType returnValue { Std_ReturnType::RTE_E_OK };");

                            if (srInterface.IsQueued == false)
                            {
                                if (field.IsPointer == false)
                                {
                                    writer.WriteLine("                " + field.DataTypeName + " arguments;");
                                }
                                else
                                {
                                    writer.WriteLine("                " + field.DataTypeName + " * arguments;");
                                }
                            }
                            else
                            {
                                writer.WriteLine("                " + field.QueuedInterfaceName(srInterface.Name) + " arguments;");
                            }
                            writer.WriteLine("            } " + field.Name + ";");
                            if ((srInterface.Fields.Count > 1) && (count != srInterface.Fields.Count - 1))
                            {
                                writer.WriteLine();
                            }
                            count++;
                        }
                    }
                    String portName = RteFunctionsGenerator_Cpp.GenerateReadWritePortName(portDef);
                    writer.WriteLine("        } " + portName + ";");
                    writer.WriteLine();
                } 
            }

            /* Write All pims */
            foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
            {
                writer.WriteLine("        struct");
                writer.WriteLine("        {");
                writer.WriteLine("            uint32 callCount { 0u };");
                writer.WriteLine("            " + pimDef.DataTypeName + " data;");
                string pimFieldName = RteFunctionsGenerator_Cpp.GenerateShortPimFunctionName(pimDef);
                writer.WriteLine("        } " + pimFieldName + ";");
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
                        writer.WriteLine("        struct");
                        writer.WriteLine("        {");
                        writer.WriteLine("            uint32 callCount { 0u };");
                        writer.WriteLine("            Std_ReturnType ReturnValue { Std_ReturnType::RTE_E_OK } ;");

                        if (csOperation.Fields.Count > 0)
                        {
                            writer.WriteLine("            struct");
                            writer.WriteLine("            {");
                            foreach (var field in csOperation.Fields)
                            {
                                string fieldDataType = "";

                                switch (field.Direction)
                                {
                                    case ClientServerOperationDirection.VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.CONST_VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.VAL_REF: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.CONST_VAL_REF: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.VAL_CONST_REF: fieldDataType = field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.CONST_VAL_CONST_REF: fieldDataType = field.DataTypeName; break;
                                }

                                writer.WriteLine("                " + fieldDataType + " " + field.Name + ";");
                            }

                            writer.WriteLine("            } arguments;");
                        }
                        String operationName = RteFunctionsGenerator_Cpp.Generate_InternalRteCall_FunctionName(portDef, csOperation);
                        writer.WriteLine("        } " + operationName + ";");
                        writer.WriteLine();
                    }
                }
            }

            writer.WriteLine("    } stubs;");
            writer.WriteLine();

            //==================================================================================================================
            //        ГЕНЕРАЦИЯ ФУНКЦИЙ
            //==================================================================================================================

            writer.WriteLine("    // Функции для перенаправления работы с данными в тестовое RTE.");
            writer.WriteLine("    " + stubName + "()");
            writer.WriteLine("    {");
            /* Write All CData structures */
            foreach (CDataDefenition cdataDef in compDef.CDataDefenitions)
            {
                writer.WriteLine("        this->CData_" + cdataDef.Name + " = [this]() {");
                writer.WriteLine("            this->stubs.CData_" + cdataDef.Name + ".callCount++;");
                writer.WriteLine("            return this->stubs.CData_" + cdataDef.Name + ".returnValue;");
                writer.WriteLine("        };");
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

                        String fieldName = RteFunctionsGenerator_Cpp.GenerateReadWritePortName(portDef) + "." + field.Name;
                        String arguments = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, portDef.PortType);

                        writer.WriteLine("        //========================================");
                        writer.WriteLine("        //      " + fieldName);
                        writer.WriteLine("        //========================================");

                        string fieldName2 = (portDef.PortType == PortType.Sender) ? "Write_" : "Read_";
                        //fieldName2 + 

                        writer.WriteLine("        this->" + RteFunctionsGenerator_Cpp.GenerateReadWritePortName(portDef) + "." + field.Name + " = [this]" + arguments + " {");
                        writer.WriteLine("            this->stubs." + fieldName + ".callCount++;");
                        if (portDef.PortType == PortType.Receiver)
                        {
                            writer.WriteLine("            data = this->stubs." + fieldName + ".arguments;");
                        }
                        else
                        {
                            writer.WriteLine("            this->stubs." + fieldName + ".arguments = data;");
                        }

                        writer.WriteLine("            return this->stubs." + fieldName + ".returnValue;");
                        writer.WriteLine("        };");
                        writer.WriteLine();
                    }
                }
            }

            /* Write All pims */
            foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
            {
                writer.WriteLine("        //========================================");
                writer.WriteLine("        //      Pim_" + pimDef.Name);
                writer.WriteLine("        //========================================");

                writer.WriteLine("        this->Pim_" + pimDef.Name + " = [this]()->&" + pimDef.DataType.Name + " {");
                writer.WriteLine("            this->stubs.Pim_" + pimDef.Name + ".callCount++;");
                writer.WriteLine("            return this->stubs.Pim_" + pimDef.Name + ".data;");
                writer.WriteLine("        };");
                writer.WriteLine();
            }

            /* Write all client calls */
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface csInterface = (ClientServerInterface)portDef.InterfaceDatatype;
                    foreach (ClientServerOperation csOperation in csInterface.Operations)
                    {
                        writer.WriteLine("        //========================================");
                        writer.WriteLine("        //      Call_" + csOperation.Name);
                        writer.WriteLine("        //========================================");

                        String arguments = "(";

                        if (csOperation.Fields.Count > 0)
                        {
                            int i = 0;
                            foreach (var field in csOperation.Fields)
                            {
                                string fieldDataType = "";

                                switch (field.Direction)
                                {
                                    case ClientServerOperationDirection.VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.CONST_VALUE: fieldDataType = field.DataTypeName; break;
                                    case ClientServerOperationDirection.VAL_REF: fieldDataType = field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.CONST_VAL_REF: fieldDataType = "const " + field.DataTypeName + "&"; break;
                                    case ClientServerOperationDirection.VAL_CONST_REF: fieldDataType = field.DataTypeName + " *"; break;
                                    case ClientServerOperationDirection.CONST_VAL_CONST_REF: fieldDataType = "const " + field.DataTypeName + " *"; break;
                                }
                                if (i != 0)
                                {
                                    arguments += ", ";
                                }
                                i++;
                                arguments += fieldDataType + " " + field.Name;
                            }
                        }

                        arguments += ")";
                        String portStubName = "Call_" + portDef.Name;
                        writer.WriteLine("        this->" + portStubName + "." + csOperation.Name + " = [this]" + arguments + " {");
                        writer.WriteLine("            this->stubs." + portStubName + ".callCount++;");
                        foreach (var field in csOperation.Fields)
                        {
                            writer.WriteLine("            this->stubs." + portStubName + ".arguments." + field.Name + " = " + field.Name + ";");
                        }
                        writer.WriteLine("        };");
                        writer.WriteLine();
                    }
                }
            }
            writer.WriteLine("    }");

            writer.WriteLine("};");
            writer.WriteLine();
        }

        void GenerateTestRteCFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" + compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME, "This file contains function for test environment.");

            /* Add includes */
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_H_FILENAME);
            
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
          

            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
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
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

            String fieldName = RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field);

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
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

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
                        String copyFromField = RteFunctionsGenerator_Cpp.GenerateComponentName(compInstance.Name) + "." + RteFunctionsGenerator_Cpp.GenerateRteWriteFieldInComponentDefenitionStruct(port, field);
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
                            String copyFromField = RteFunctionsGenerator_Cpp.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                            String oppositeCompName = RteFunctionsGenerator_Cpp.GenerateComponentName(oppositCompInstance.Name);
                            writer.WriteLine("            memcpy(" + field.Name + ", " + "&" + oppositeCompName + "." + copyFromField + ", sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return " + Properties.Resources.RTE_E_OK + ";");
                        }
                        else
                        {
                            writer.WriteLine("            memset(" + field.Name + ", " + "0, sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                        }

                        writer.WriteLine("        }");
                    }
                    writer.WriteLine("    }");
                }
                writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

            String fieldName = RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field);

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
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

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
                        String copyFromField = TestArtefactsVariable(compInstance.ComponentDefenition) + "." + RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field) + ".Arguments.data";
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
                            String copyFromField = RteFunctionsGenerator_Cpp.GenerateRteWriteFieldInComponentDefenitionStruct(oppositePort.PortDefenition, field);
                            String oppositeCompName = RteFunctionsGenerator_Cpp.GenerateComponentName(oppositCompInstance.Name);
                            writer.WriteLine("            memcpy(" + field.Name + ", " + "&" + oppositeCompName + "." + copyFromField + ", sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return " + Properties.Resources.RTE_E_OK + ";");
                        }
                        else
                        {
                            writer.WriteLine("            memset(" + field.Name + ", " + "0, sizeof(" + field.DataTypeName + "));");
                            writer.WriteLine("            return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
                        }

                        writer.WriteLine("        }");
                    }
                    writer.WriteLine("    }");
                }
                writer.WriteLine("    return Std_ReturnType::" + Properties.Resources.RTE_E_UNCONNECTED + ";");
            }

            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCDataFunction(StreamWriter writer, ApplicationSwComponentType compDef, CDataDefenition cdataDef)
        {
            String returnValue = cdataDef.DataTypeName + " ";
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateShortCDataFunctionName(cdataDef);

            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateMultipleInstanceFunctionArguments();

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
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateShortPimFunctionName(pimDef);

            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateMultipleInstanceFunctionArguments();

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
            String RteFuncName = RteFunctionsGenerator_Cpp.Generate_RteCall_ConnectionGroup_FunctionName(compDef, port, operation);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);

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
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_C_FILENAME, "This file contains functions for component initialization before each test");
            RteFunctionsGenerator_Cpp.OpenCppGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator_Cpp.AddInclude(writer, "<Rte_TestCommon.h>");
            foreach (ApplicationSwComponentType swCompType in compDefs)
            {
                RteFunctionsGenerator_Cpp.AddInclude(writer, swCompType.Name + "_TestRte.h");
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
                String taskCounter = RteFunctionsGenerator_Cpp.CreateTaskCounter(task.Name);
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

            RteFunctionsGenerator_Cpp.CloseCppGuardDefine(writer);
            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        void WritePIMDefaultValues(StreamWriter writer, ApplicationSwComponentType compDef, PimInstance pim)
        {
            String FieldValue = "            " + TestArtefactsVariable(compDef) + "." + RteFunctionsGenerator_Cpp.GenerateShortPimFunctionName(pim.Defenition) + ".data";

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
