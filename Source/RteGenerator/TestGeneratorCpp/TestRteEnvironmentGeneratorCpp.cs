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
            RteComponentGenerator_Cpp compGenerator = new RteComponentGenerator_Cpp();
            
            foreach (ApplicationSwComponentType compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                String componentDir = outputDir + "\\Components\\" + compDef.Name;
                GenerateTestRteHppFile(compDef, componentDir);
                GenerateTestRteCppFile(compDef, componentDir);
            }
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

        void GenerateTestRteHppFile(ApplicationSwComponentType compDef, String componentDir)
        {
            String targetDir =  componentDir + "\\Tests\\TestArtifacts\\";
            System.IO.Directory.CreateDirectory(targetDir);

            String fileName = targetDir + compDef.Name + "_" + Properties.Resources.TEST_RTE_HPP_FILENAME;
            StreamWriter writer = new StreamWriter(fileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_HPP_FILENAME, "This file contains structure for provide test environment.");
            RteFunctionsGenerator_Cpp.OpenCppGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator_Cpp.AddInclude(writer, Properties.Resources.RTE_DATATYPES_HPP_FILENAME);
            writer.WriteLine("#define RTE_CPP");
            RteFunctionsGenerator_Cpp.AddInclude(writer, "Rte_" + compDef.Name + ".hpp");
            writer.WriteLine("#undef  RTE_CPP");

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
            //==================================================================================================================
            //       Функции используемые компонентом
            //==================================================================================================================
            writer.WriteLine("//Функции используемые компонентом");

            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        var funcName = RteFunctionsGenerator_Cpp.GenerateFullReadWriteTestFunctionDefenitionNameAndReturnType(compDef, portDef, field);
                        writer.WriteLine(funcName + ";");
                    }
                }
            }
            writer.WriteLine();

            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is ClientServerInterface)
                {
                    ClientServerInterface csInterface = (ClientServerInterface)portDef.InterfaceDatatype;
                    foreach (ClientServerOperation csOperation in csInterface.Operations)
                    {
                        var testFuncName = RteFunctionsGenerator_Cpp.GenerateFullCallTestFunctionDefenitionNameAndReturnType(compDef, portDef, csOperation);
                        writer.WriteLine(testFuncName + ";");
                    }
                }
            }
            writer.WriteLine();


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
            #region CDATA
            foreach (CDataDefenition cdataDef in compDef.CDataDefenitions)
            {
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
                writer.WriteLine("        " + cdataDef.DataType.Name + " CData_" + cdataDef.Name + " {" + defaultValue + "};");
            }
            writer.WriteLine();
            #endregion

            #region READ_WRITE
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
                            writer.WriteLine("        //--- FIELD: " + field.Name);
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
            #endregion

            #region PIM
            /* Write All pims */
            foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
            {
                string pimFieldName = "Pim_" + pimDef.Name;

                //---------------------
                var components = AutosarApplication.GetInstance().GetComponentInstanceByDefenition(compDef);
                String defaultValue = "0";

                if (components.Count > 0)
                {
                    try
                    {
                        defaultValue = components[0].PerInstanceMemories.GetPim(pimDef).DefaultValues[0].DefaultValue;
                    }
                    catch
                    {
                        defaultValue = "0";
                    }
                }
                writer.WriteLine("        " + pimDef.DataType.Name + " " + pimFieldName + " {" + defaultValue + "};");
            }
            #endregion

            #region CALL
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
            #endregion
            writer.WriteLine("    } stubs;");
            writer.WriteLine();

            //==================================================================================================================
            //        ГЕНЕРАЦИЯ ФУНКЦИЙ
            //==================================================================================================================

            writer.WriteLine("    // Функции для перенаправления работы с данными в тестовое RTE.");
            writer.WriteLine("    " + stubName + "() : ");
            writer.WriteLine("        Rte_" + compDef.Name + "{");

            // ГЕНЕРАЦИЯ СТРУКТУРЫ С ЗАПОЛНЕНИЕМ

            /* Write All PIMs */
            foreach (PimDefenition pimDef in compDef.PerInstanceMemoryList)
            {
                var pimField = "Pim_" + pimDef.Name;
                writer.WriteLine("        ." + pimField + " = stubs." + pimField + ",");
            }
            writer.WriteLine();

            /* Write All Rte Read structures */
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    
                    var portName = RteFunctionsGenerator_Cpp.GenerateReadWritePortName(portDef);
                    writer.WriteLine("        ." + portName + "{");
                    if (portDef.InterfaceDatatype is SenderReceiverInterface)
                    {
                        SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                        foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                        {

                            var funcName = RteFunctionsGenerator_Cpp.GenerateFullReadWriteTestFunctionDefenitionName(compDef, portDef, field);
                            writer.WriteLine("            ." + field.Name + " = " + funcName + ",");
                        }
                    }
                    else if (portDef.InterfaceDatatype is ClientServerInterface) {
                        ClientServerInterface csInterface = (ClientServerInterface)portDef.InterfaceDatatype;
                        foreach (ClientServerOperation csOperation in csInterface.Operations)
                        {
                            var funcName = RteFunctionsGenerator_Cpp.GenerateFullCallTestFunctionDefenitionName(compDef, portDef, csOperation);
                            writer.WriteLine("            ." + csOperation.Name + " = " + funcName + ",");
                        }
                    }
                    writer.WriteLine("        },");
                }
            }
            writer.WriteLine();

            /* Write All CData Read  */
            foreach (CDataDefenition cdataDef in compDef.CDataDefenitions)
            {
                var testCDataFuncName = RteFunctionsGenerator_Cpp.GenerateShortCDataFunctionName(cdataDef);
                writer.WriteLine("        .CData_" + cdataDef.Name + " = stubs.CData_" + cdataDef.Name + ",");
            }

            writer.WriteLine("    }");

            writer.WriteLine("    {");
            writer.WriteLine("        extern Rte_Test_" + compDef.Name + "* testRte;");
            writer.WriteLine("        testRte = this;");
            writer.WriteLine("    }");
            writer.WriteLine("};");
    
            
#if false
            writer.WriteLine();

            foreach (var portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                    foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                    {
                        var testCDataFuncName = RteFunctionsGenerator_Cpp.GenerateFullReadWriteTestFunctionDefenitionNameAndReturnType(compDef, portDef, field);
                       // writer.WriteLine("        testRte->" + cdataDef.Name + " = " + testCDataFuncName + ";");
                    }
                }
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
#endif
        }

        void GenerateTestRteCppFile(ApplicationSwComponentType compDef, String componentDir)
        {
            String targetDir = componentDir + "\\Tests\\TestArtifacts\\";
            System.IO.Directory.CreateDirectory(targetDir);

            String fileName = targetDir + compDef.Name + "_" + Properties.Resources.TEST_RTE_CPP_FILENAME;
            StreamWriter writer = new StreamWriter(fileName);
            RteFunctionsGenerator_Cpp.GenerateFileTitle(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_CPP_FILENAME, "This file contains function for test environment.");

            /* Add includes */
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_HPP_FILENAME);
            RteFunctionsGenerator_Cpp.AddInclude(writer, compDef.Name + ".hpp");
            
            writer.WriteLine();

            writer.WriteLine("Rte_Test_" + compDef.Name + "* testRte;");
            writer.WriteLine();

            WriteComponentConstructor(writer, compDef);

            /* Create functions */
            CreateRteWriteFunctions(writer, compDef);

            CreateRteReadFunctions(writer, compDef);

            //CreateRteCDataFunctions(writer, compDef);

            //CreateRtePimFunctions(writer, compDef);

            CreateRteCallFunctions(writer, compDef);
          

            RteFunctionsGenerator_Cpp.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteComponentConstructor(StreamWriter writer, ApplicationSwComponentType compDefenition){
            writer.WriteLine(compDefenition.Name + "::" + compDefenition.Name + "( const Rte_" + compDefenition.Name + "& Rte ) : Rte_Base_" + compDefenition.Name + "( Rte ) {}");
            writer.WriteLine();
            //Inverter::Inverter( const Rte_Inverter& Rte ) : Rte_Base_Inverter( Rte ) {}
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

#if false
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
#endif

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
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteTestConnectionFunctionName(compDef, port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

            String fieldName = RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    testRte->stubs.Write_" + port.Name + "." + field.Name + ".callCount++;");
            writer.WriteLine("    testRte->stubs.Write_" + port.Name + "." +field.Name + ".arguments = data;");
            writer.WriteLine("    return testRte->stubs.Write_" + port.Name + "." + field.Name + ".returnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateQueuedRteWritePortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            writer.WriteLine("#pragma error " + port.Name + "_" + field.Name + "Not implemented yet");
#if false
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");

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

            writer.WriteLine("}");
            writer.WriteLine("");
#endif
        }

        void GenerateRteReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator_Cpp.GenerateReadWriteTestConnectionFunctionName(compDef, port, field);
            String fieldVariable = RteFunctionsGenerator_Cpp.GenerateSenderReceiverInterfaceArguments(field, port.PortType);

            String fieldName = RteFunctionsGenerator_Cpp.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + " " + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    testRte->stubs.Read_" + port.Name + "." + field.Name + ".callCount++;");
            writer.WriteLine("    data = testRte->stubs.Read_" + port.Name + "." + field.Name + ".arguments;");
            writer.WriteLine("    return testRte->stubs.Read_" + port.Name + "." + field.Name + ".returnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteQueuedReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            writer.WriteLine("#pragma error "+ port.Name +"_" +field.Name + "Not implemented yet");
#if false
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
#endif
        }
#if false
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
#endif
        void GenerateRteCallPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, ClientServerOperation operation)
        {
            writer.WriteLine("#pragma error " + port.Name + "_" + operation.Name + "Not implemented yet");
#if false
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
#endif
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
