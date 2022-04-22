using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator.TestGenerator
{
    class TestRteEnvironmentGenerator
    {
        public void GenerateRteEnvironment(ApplicationSwComponentType compDef, String outputDir)
        {
            /* Generate Rte_<ComponentName>.h file */
            RteComponentGenerator compGenerator = new RteComponentGenerator();
            compGenerator.CreateRteIncludes(outputDir, compDef);

            GenerateTestRteHFile(compDef, outputDir);
            GenerateTestRteCFile(compDef, outputDir);
        }

        public void GenerateCommonFiles(String outputDir)
        {
            /* Generate Rte_DataTypes.h file */
            RteDataTypesGenerator datatypeGenerator = new RteDataTypesGenerator();
            datatypeGenerator.GenerateDataTypesFile(outputDir);

            /* Generate SystemErrors.h */
            SystemErrorGenerator systemErrorGenerator = new SystemErrorGenerator();
            systemErrorGenerator.GenerateSystemErrorsFile(outputDir);

            /* Generate TestInitialization.h */
            GenerateTestInitializationFile(AutosarApplication.GetInstance().ComponentDefenitionsList, outputDir);

            ReturnCodesGenerator returnCodesGenerator = new ReturnCodesGenerator();
            returnCodesGenerator.GenerateReturnCodesFile(outputDir);
        }

        void GenerateTestRteHFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" +  compDef.Name +"_" + Properties.Resources.TEST_RTE_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_H_FILENAME, "This file contains structure for provide test environment.");
            String guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, "Rte_" + compDef.Name + ".h");
            //RteFunctionsGenerator.AddInclude(writer, Properties.Resources.TEST_FRAMEWORK_H_FILENAME);
            RteFunctionsGenerator.AddInclude(writer, "<string.h>");

            /* Create structure */
            CreateTestArtefactStructure(writer, compDef);

            writer.WriteLine("/* All periodical runnables */");
            foreach (PeriodicRunnableDefenition runnable in compDef.Runnables)
            {
                writer.WriteLine(RteFunctionsGenerator.Generate_RunnableFunction(compDef, runnable) + ";");
            }
            writer.WriteLine();

            writer.WriteLine("/* All server calls */");
            CreateRteCallFunctionDeclarations(writer, compDef);

            writer.WriteLine();

            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);

            writer.Close();
        }

        string TestArtefactsStructureDataType(ApplicationSwComponentType compDef)
        {
            return "TestArtefacts" + "_" + compDef.Name;
        }

        string TestArtefactsVariable(ApplicationSwComponentType compDef)
        {
            return "testArtefacts" + "_" + compDef.Name;
        }

        const string CallCount = "CallCount";
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
                writer.WriteLine("        uint32 " + CallCount +";");
                writer.WriteLine("        " + cdataDef.DataType.Name + " Value;");
                writer.WriteLine("    } " + RteFunctionsGenerator.GenerateShortCDataFunctionName(cdataDef) + ";");
                writer.WriteLine();
            }

            /* Write All Rte Read structures */            
            foreach (PortDefenition portDef in compDef.Ports)
            {
                if (portDef.InterfaceDatatype is SenderReceiverInterface)
                {
                    SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                    foreach (SenderReceiverInterfaceField srField in srInterface.Fields)
                    {
                        writer.WriteLine("    struct");
                        writer.WriteLine("    {");
                        writer.WriteLine("        uint32 " + CallCount + ";");
                        writer.WriteLine("        Std_ReturnType ReturnValue;");
                
                        writer.WriteLine("        struct");
                        writer.WriteLine("        {");
                        writer.WriteLine("            " + srField.DataTypeName + " data;");                         
                        writer.WriteLine("        } Arguments;");
                        String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, srField);
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
                writer.WriteLine("        uint32 " + CallCount + ";");
                writer.WriteLine("        " + pimDef.DataTypeName + " data;");
                string pimFieldName = RteFunctionsGenerator.GenerateShortPimFunctionName(pimDef);
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
                        writer.WriteLine("        uint32 " + CallCount + ";");
                        writer.WriteLine("        Std_ReturnType ReturnValue;");
                        writer.WriteLine("        struct");
                        writer.WriteLine("        {");
                        foreach (var field in csOperation.Fields)
                        {
                            writer.WriteLine("            " + field.DataTypeName + " " + field.Name + ";");
                        }
                        writer.WriteLine("        } Arguments;");
                        writer.WriteLine("        Std_ReturnType (*redirection)" + RteFunctionsGenerator.GenerateClientServerInterfaceArguments(csOperation, compDef.MultipleInstantiation) + ";");
                        String operationName = RteFunctionsGenerator.Generate_RteCall_FunctionName(portDef, csOperation);
                        writer.WriteLine("    } " + operationName + ";");
                        writer.WriteLine();
                    }
                }
            }

            writer.WriteLine("} " + TestArtefactsStructureDataType(compDef) + ";");
            writer.WriteLine();
            writer.WriteLine("extern " + TestArtefactsStructureDataType(compDef) + " " + TestArtefactsVariable(compDef) + ";");
            writer.WriteLine();
        }

        void GenerateTestRteCFile(ApplicationSwComponentType compDef, String outputDir)
        {
            String FileName = outputDir + "\\" + compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator.GenerateFileTitle(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_C_FILENAME, "This file contains function for test environment.");

            /* Add includes */
            RteFunctionsGenerator.AddInclude(writer, compDef.Name + "_" + Properties.Resources.TEST_RTE_H_FILENAME);
            
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
          

            RteFunctionsGenerator.WriteEndOfFile(writer);
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


        void CreateRteCallFunctionDeclarations(StreamWriter writer, ApplicationSwComponentType compDefenition)
        {
            foreach (PortDefenition port in compDefenition.Ports)
            {
                if (port.PortType == PortType.Server)
                {
                    ClientServerInterface csInterface = port.InterfaceDatatype as ClientServerInterface;
                    foreach (ClientServerOperation operation in csInterface.Operations)
                    {
                        String returnValue = Properties.Resources.STD_RETURN_TYPE;
                        String RteFuncName = RteFunctionsGenerator.Generate_RteCall_FunctionName(port, operation);
                        String fieldVariable = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, compDefenition.MultipleInstantiation);
                        String writeValue = returnValue + RteFuncName + fieldVariable + ";";
                        writer.WriteLine(writeValue);
                    }
                }
            }
        }

        void GenerateRteWritePortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + fieldName + "." + CallCount + "++;");
            writer.WriteLine("    memcpy(&" + TestArtefactsVariable(compDef) + "." + fieldName + ".Arguments.data, " + field.Name + ", sizeof(" + field.DataTypeName + "));");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteReadPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.GenerateReadWriteConnectionFunctionName(port, field);
            String fieldVariable = RteFunctionsGenerator.GenerateSenderReceiverInterfaceArguments(field, port.PortType, compDef.MultipleInstantiation);

            String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(port, field);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + fieldName + "." + CallCount + "++;");
            writer.WriteLine("    memcpy(" + field.Name + ", &" + TestArtefactsVariable(compDef) + "." + fieldName + ".Arguments.data, sizeof(" + field.DataTypeName + "));");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCDataFunction(StreamWriter writer, ApplicationSwComponentType compDef, CDataDefenition cdataDef)
        {
            String returnValue = cdataDef.DataTypeName + " ";
            String RteFuncName = RteFunctionsGenerator.GenerateShortCDataFunctionName(cdataDef);

            String fieldVariable = RteFunctionsGenerator.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + CallCount + "++;");
            writer.WriteLine("    return " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".Value;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRtePimFunction(StreamWriter writer, ApplicationSwComponentType compDef, PimDefenition pimDef)
        {
            String returnValue = pimDef.DataTypeName + " * ";
            String RteFuncName = RteFunctionsGenerator.GenerateShortPimFunctionName(pimDef);

            String fieldVariable = RteFunctionsGenerator.GenerateMultipleInstanceFunctionArguments(compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + CallCount + "++;");
            writer.WriteLine("    return &" + TestArtefactsVariable(compDef) + "." + RteFuncName + ".data;");
            writer.WriteLine("}");
            writer.WriteLine("");
        }

        void GenerateRteCallPortFieldFunction(StreamWriter writer, ApplicationSwComponentType compDef, PortDefenition port, ClientServerOperation operation)
        {
            String returnValue = Properties.Resources.STD_RETURN_TYPE;
            String RteFuncName = RteFunctionsGenerator.Generate_RteCall_ConnectionGroup_FunctionName(compDef, port, operation);
            String fieldVariable = RteFunctionsGenerator.GenerateClientServerInterfaceArguments(operation, compDef.MultipleInstantiation);

            writer.WriteLine(returnValue + RteFuncName + fieldVariable);
            writer.WriteLine("{");
            writer.WriteLine("    if (" + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + "redirection != NULL)");
            writer.WriteLine("    {");
            writer.WriteLine("        " + TestArtefactsVariable(compDef) + "." + RteFuncName + "." + CallCount + "++;");
            foreach (var csField in operation.Fields)
            {
                writer.WriteLine("        " + TestArtefactsVariable(compDef) + "." + RteFuncName + ".Arguments." + csField.Name + " = *" + csField.Name + ";");
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
            RteFunctionsGenerator.GenerateFileTitle(writer, Properties.Resources.TEST_RTE_C_FILENAME, "This file contains functions for component initialization before each test");
            String guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            /* Add includes */
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME);
            foreach (ApplicationSwComponentType swCompType in compDefs)
            {
                RteFunctionsGenerator.AddInclude(writer, swCompType.Name + "_TestRte.h");
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
            
            /* Clear test structure */
            foreach (ApplicationSwComponentType swCompType in compDefs)
            {
                writer.WriteLine("    memset(&" + TestArtefactsVariable(swCompType) + ", 0, sizeof(" + TestArtefactsVariable(swCompType) + " ));");
            }
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
            foreach (ApplicationSwComponentType compDef in compDefs)
            {
                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if (portDef.InterfaceDatatype is SenderReceiverInterface)
                    {
                        SenderReceiverInterface srInterface = (SenderReceiverInterface)portDef.InterfaceDatatype;
                        foreach (SenderReceiverInterfaceField srField in srInterface.Fields)
                        {
                            String fieldName = RteFunctionsGenerator.GenerateReadWriteFunctionName(portDef, srField);
                            writer.WriteLine("    " + TestArtefactsVariable(compDef) + "." + fieldName + ".ReturnValue = RTE_E_OK;");                        
                        }
                    }
                }
            }
            //writer.WriteLine(); 

            /* Initialize component variables */
            //writer.WriteLine("    VariableInitialization();");

            writer.WriteLine("}");           

            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void WritePIMDefaultValues(StreamWriter writer, ApplicationSwComponentType compDef, PimInstance pim)
        {
            String FieldValue = "            " + TestArtefactsVariable(compDef) + "." + RteFunctionsGenerator.GenerateShortPimFunctionName(pim.Defenition) + ".data";

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
