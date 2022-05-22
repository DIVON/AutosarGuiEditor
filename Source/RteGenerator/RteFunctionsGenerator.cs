using AutosarGuiEditor.Source.App.Settings;
using AutosarGuiEditor.Source.Autosar.OsTasks;
using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using AutosarGuiEditor.Source.Painters.Components.Runables;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public static class RteFunctionsGenerator
    {
        public const String IncludesLine                         = "/* INCLUDES */";
        public const String EndOfIncludesLine                    = "/* END OF INCLUDES */";

        public const String MacrosLine                           = "/* MACROS */";
        public const String EndOfMacrosLine                      = "/* END OF MACROS */";

        public const String TypeDefenitionsLine                  = "/* TYPE DEFINITIONS */";
        public const String EndOfTypeDefenitionsLine             = "/* END OF TYPE DEFINITIONS */";

        public const String VariablesLine                        = "/* VARIABLES */";
        public const String EndOfVariableLine                    = "/* END OF VARIABLES */";
 
        public const String ExternalVariablesLine                = "/* EXTERNAL VARIABLES */";
        public const String EndOfExternalVariableLine            = "/* END OF EXTERNAL VARIABLES */";

        public const String LocalFunctionsDeclarationLine        = "/* LOCAL FUNCTION DECLARATIONS */";
        public const String EndOfLocalFunctionsDeclarationLine   = "/* END OF LOCAL FUNCTION DECLARATIONS */";

        public const String GlobalFunctionsDeclarationLine       = "/* GLOBAL FUNCTION DECLARATIONS */";
        public const String EndOfGlobalFunctionsDeclarationLine  = "/* END OF GLOBAL FUNCTION DECLARATIONS */";

        public const String LocalFunctionsDefenitionsLine        = "/* LOCAL FUNCTION DEFINITIONS */";
        public const String EndOfLocalFunctionsDefenitionsLine   = "/* END OF LOCAL FUNCTION DEFINITIONS */";

        public const String GlobalFunctionsDefenitionsLine       = "/* GLOBAL FUNCTION DEFINITIONS */";
        public const String EndOfGlobalFunctionsDefenitionsLine  = "/* END OF GLOBAL FUNCTION DEFINITIONS */";

        public const String RteFunctionsDefenitionsLine          = "/* RTE FUNCTION DECLARATIONS */";
        public const String EndOfRteFunctionsDefenitionsLine     = "/* END OF RTE FUNCTION DECLARATIONS */";

        public const String ComponentInstancePointerDatatype     = "Rte_ComponentInstance";

        /**** LOCAL FUNCTION DECLARATIONS ****************************************************************/

        public static String GenerateComponentName(String compName)
        {
            return "cin" + compName;
        }

        public static String GenerateComponentHeaderFile(ApplicationSwComponentType compDef)
        {
            return "Rte_" + compDef.Name + ".h";
        }

        public static String ComponentDataStructureDefenitionName(ApplicationSwComponentType compDef)
        {
            return "Rte_CDS_" + compDef.Name;
        }

        public static String GeneratePortDataStructureDefenition(ApplicationSwComponentType compDef, SenderReceiverInterface srInterface, PortType PortType)
        {
            string baseStr = "Rte_PDS_"+ compDef.Name + "_" + srInterface.Name;
            if (PortType == PortDefenitions.PortType.Sender)
            {
                baseStr += "_P";
            }
            else
            {
                baseStr += "_R";
            }
            return baseStr;
        }

        public static String GeneratePortDataStructureDefenition(ApplicationSwComponentType compDef, ClientServerInterface csInterface)
        {
            string baseStr = "Rte_PDS_" + compDef.Name + "_" + csInterface.Name + "_C";

            return baseStr;
        }

        public static String GetRteFolder()
        {
            String resFolder = "";
            if (Path.IsPathRooted(AutosarApplication.GetInstance().GenerateRtePath))
            {
                resFolder = AutosarApplication.GetInstance().GenerateRtePath + "\\" + Properties.Resources.RTE_FOLDER;
            }
            else
            {
                resFolder = Path.GetDirectoryName(AutosarApplication.GetInstance().FileName) + "\\" + AutosarApplication.GetInstance().GenerateRtePath + "\\" + Properties.Resources.RTE_FOLDER;
            }

            return resFolder;
        }

        public static String GetComponentsFolder()
        {
            String resFolder = "";

                resFolder = AutosarApplication.GetInstance().GenerateComponentsPath + "\\" + Properties.Resources.COMPONENTS_FOLDER;
         

            return resFolder;
        }

        public static String Generate_RteCall_FunctionName(ApplicationSwComponentType compDef, PortDefenition portDef, ClientServerOperation operation)
        {
            return compDef.Name + "_" + portDef.Name + "_ru" + operation.Name;
        }

        public static String Generate_InternalRteCall_FunctionName(PortDefenition portDef, ClientServerOperation operation)
        {
            return "Rte_Call_" + portDef.Name + "_" + operation.Name;
        }

        public static String Generate_RteCall_ConnectionGroup_FunctionName(ApplicationSwComponentType compDef, PortDefenition port, ClientServerOperation operation)
        {
            return "Rte_Call_" + compDef.Name + "_" + port.Name + "_" + operation.Name;
        }


        public static String GenerateSenderReceiverInterfaceArguments(SenderReceiverInterfaceField field, PortType portType, Boolean MultipleInstantiation = false)
        {
            String result = "(";
            
            if (MultipleInstantiation)
            {
                result += RteFunctionsGenerator.ComponentInstancePointerDatatype + " instance, ";
            }

            String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(field.BaseDataTypeGUID);
            if (!field.IsPointer)
            {
                
                if (portType == PortType.Receiver)
                {
                    result += dataTypeName + " * const data";
                }
                else
                {
                    result += "const " + dataTypeName + " * const data";
                }
            }
            else
            {
                if (portType == PortType.Receiver)
                {
                    result += dataTypeName + " ** const data";
                }
                else
                {
                    result += "const " + dataTypeName + "** const data";
                }
            }
            return result + ")";
        }

        public static String GenerateMultipleInstanceFunctionArguments(Boolean MultipleInstantiation = false)
        {
            String result = "(";

            if (MultipleInstantiation)
            {
                result += RteFunctionsGenerator.ComponentInstancePointerDatatype + " instance";
            }
            else
            {
                result += "void";
            }
            result += ")";
            return result;
        }

        public static String GenerateClientServerInterfaceArguments(ClientServerOperation operation, Boolean MultipleInstantiation = false)
        {
            String result = "(";

            if (MultipleInstantiation)
            {
                result += RteFunctionsGenerator.ComponentInstancePointerDatatype + " instance";
                if (operation.Fields.Count > 0)
                {
                    result += ", ";
                }
            }
            else
            {
                if (operation.Fields.Count == 0)
                {
                    result += "void";
                }
            }

            for(int i = 0; i < operation.Fields.Count; i++)
            {
                ClientServerOperationField field = operation.Fields[i];

                result += GetClientServerOperationFieldIdentification(field);
                if (i != operation.Fields.Count - 1)
                {
                    result += ", ";   
                }
            }
            
            return result + ")";
        }

        public static String GenerateClientServerInterfaceArgumentsForDefine(ClientServerOperation operation, Boolean MultipleInstantiation = false)
        {
            String result = "(";

            if (MultipleInstantiation)
            {
                result += "instance";
                if (operation.Fields.Count > 0)
                {
                    result += ", ";
                }
            }

            for (int i = 0; i < operation.Fields.Count; i++)
            {
                ClientServerOperationField field = operation.Fields[i];

                result += field.Name;
                if (i != operation.Fields.Count - 1)
                {
                    result += ", ";
                }
            }

            return result + ")";
        }

        public static String GenerateClientServerInterfaceArgumentsForDefineWithoutInstance(ClientServerOperation operation, Boolean MultipleInstantiation = false)
        {
            String result = "(";

            for (int i = 0; i < operation.Fields.Count; i++)
            {
                ClientServerOperationField field = operation.Fields[i];

                result += "(" + field.Name + ")";
                if (i != operation.Fields.Count - 1)
                {
                    result += ", ";
                }
            }

            return result + ")";
        }

#region PIM

        public static String GenerateShortPimFunctionName(PimDefenition pim)
        {
            return "Rte_Pim_" + pim.Name;
        }

        public static String GenerateFullPimFunctionName(ApplicationSwComponentType compDefenition, PimDefenition pim)
        {
            return "Rte_Pim_" + compDefenition.Name + "_" + pim.Name;
        }

        public static String GenerateRtePimFieldInComponentDefenitionStruct(ApplicationSwComponentType compDefenition, PimDefenition pim)
        {
            return "Rte_PimField_" + compDefenition.Name + "_" + pim.Name;
        }

#endregion

#region CDATA

        public static String GenerateCDataDataType(ApplicationSwComponentType compDefenition, CDataDefenition cdata)
        {
            return "Rte_CDataType_" + compDefenition.Name + "_" + cdata.Name;
        }

        public static String GenerateShortCDataFunctionName(CDataDefenition cdata)
        {
            return "Rte_CData_" + cdata.Name;
        }

        public static String GenerateFullCDataFunctionName(ApplicationSwComponentType compDefenition, CDataDefenition cdata)
        {
            return "Rte_CData_" + compDefenition.Name + "_" + cdata.Name;
        }

        public static String GenerateFullCDataFunctionDefenitionNameAndReturnType(ApplicationSwComponentType compDefenition, CDataDefenition cdata)
        {
            String returnDataType = GenerateCDataDataType(compDefenition, cdata);
            String FunctionArguments = "(";
            if (compDefenition.MultipleInstantiation)
            {
                FunctionArguments = ComponentInstancePointerDatatype + " instance";
            }
            FunctionArguments += ")";
            String result = GenerateCDataDataType(compDefenition, cdata) + " " + GenerateFullCDataFunctionName(compDefenition, cdata) + FunctionArguments;
            return result;
        }

        public static String GenerateRteCDataFieldInComponentDefenitionStruct(ApplicationSwComponentType compDefenition, CDataDefenition cdata)
        {
            return "Rte_CDataField_" + compDefenition.Name + "_" + cdata.Name;
        }

#endregion

#region SENDER_RECEIVER
        public static String GenerateRteWriteFieldInComponentDefenitionStruct(PortDefenition portDefenition, SenderReceiverInterfaceField field)
        {
            SenderReceiverInterface srInterface = portDefenition.InterfaceDatatype as SenderReceiverInterface;
            return "Rte_WriteField_" + portDefenition.Name + "_" + field.Name;
        }
#endregion

        public static String GenerateReadWriteFunctionName(PortDefenition port, SenderReceiverInterfaceField field)
        {
            String res = "Rte_";
            if (port.PortType == PortType.Sender)
            {
                res += "Write_";
            }
            else
            {
                res += "Read_";
            }

            res += port.Name + "_" + field.Name;
            return res;
        }

        public static String GetClientServerOperationFieldIdentification(ClientServerOperationField field)
        {
            String result = "";
            String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(field.BaseDataTypeGUID);
            
            if (field.Direction == ClientServerOperationDirection.CONST_VAL_CONST_REF)
            {
                result = "const " + dataTypeName + " * const " + field.Name;
            }
            else if (field.Direction == ClientServerOperationDirection.CONST_VAL_REF)
            {
                result = "const " + dataTypeName + " * " + field.Name;
            }
            else if (field.Direction == ClientServerOperationDirection.CONST_VALUE)
            {
                result = "const " + dataTypeName + field.Name;
            }
            else if (field.Direction == ClientServerOperationDirection.VAL_CONST_REF)
            {
                result = dataTypeName + " * const " + field.Name;
            }
            else if (field.Direction == ClientServerOperationDirection.VAL_REF)
            {
                result = dataTypeName + " * " + field.Name;
            }
            else if (field.Direction == ClientServerOperationDirection.VALUE)
            {
                result = dataTypeName + " " + field.Name;
            }
            return result;
        }

        public static void AddInclude(StreamWriter writer, String filename)
        {
            if (filename[0] != '<')
            {
                writer.WriteLine("#include \"" + Path.GetFileName(filename) + "\"");
            }
            else
            {
                writer.WriteLine("#include " + filename);
            }
        }

        public static String GenerateDefaultValueForSimpleDataTypeField(String FieldName, int indent)
        {
            String str = FillStringForCount("", ' ', indent);

            str += "." + FieldName + " = 0";
            return str;
        }

        public static String GenerateRte_Component_SRInterface_Name(String ComponentName, String PortName, SenderReceiverInterfaceField field)
        {
            return ComponentName + "_" + PortName + "_" + field.Name + "_VALUE";
        }

        public static String Generate_RunnableFunctionName(ApplicationSwComponentType compDefenition, PeriodicRunnableDefenition runnable)
        {
            return compDefenition.Name + "_ru" + runnable.Name;
        }

        public static String Generate_RunnableFunction(ApplicationSwComponentType compDefenition, PeriodicRunnableDefenition runnable)
        {             
            if (compDefenition.MultipleInstantiation)
            {
                String CDSName = RteFunctionsGenerator.ComponentDataStructureDefenitionName(compDefenition);
                return "void " + Generate_RunnableFunctionName(compDefenition, runnable) + "(const " + CDSName + " * const instance)";
            }
            else /* Single instantiation */
            {
                return "void " + Generate_RunnableFunctionName(compDefenition, runnable) + "(void)";
            }
        }


        public static String Generate_CallOfRunnable(PeriodicRunnableInstance runnable)
        {
            ComponentInstance compInstance = AutosarApplication.GetInstance().FindComponentInstanceByRunnableGuid(runnable.GUID);
            if (!compInstance.ComponentDefenition.MultipleInstantiation)
            {
                return Generate_RunnableFunctionName(compInstance.ComponentDefenition, runnable.Defenition) + "();";
            }
            else
            {
                String funcName = Generate_RunnableFunctionName(compInstance.ComponentDefenition, runnable.Defenition);
                String compName = GenerateComponentName(compInstance.Name);

                return funcName + "(" + LinkToTheComponentInstance(compInstance) + ");";
            }
        }

        public static void GenerateFileTitle(StreamWriter writer, String FileName, String FileDescription)
        {
            writer.WriteLine("/*");
            writer.WriteLine(" * file: " + Path.GetFileName(FileName));
            writer.WriteLine(" *");
            writer.WriteLine(" * " + FileDescription);
            writer.WriteLine(" *");
            writer.WriteLine(" * $Author: $");
            writer.WriteLine(" * $Date: $");
            writer.WriteLine(" * $Revision: $");
            writer.WriteLine(" *");
            writer.WriteLine(" * " + AutosarApplication.GetInstance().Signature);
            writer.WriteLine(" */");
        }

        /* Returns generated define */
        public static String OpenGuardDefine(StreamWriter writer)
        {
            String fileName = ((FileStream)(writer.BaseStream)).Name;

            String define = GenerateFileNameDefine(fileName);
            writer.WriteLine("#ifndef " + define);
            writer.WriteLine("#define " + define);
            writer.WriteLine("");

            return define;
        }

        public static void CloseGuardDefine(StreamWriter writer)
        {
            String define = GenerateFileNameDefine(((FileStream)(writer.BaseStream)).Name);
            writer.WriteLine("#endif /* " + define + " */");

        }

        public static void WriteEndOfFile(StreamWriter writer)
        {
            writer.WriteLine("/* End of file */");
        }

        
        public static String GenerateDefineFromString(String data)
        {
            for (int i = data.Length - 1; i > 0; i--)
            {
                if (char.IsUpper(data[i]) && (char.IsLower(data[i - 1])))
                {
                    data = data.Insert(i, "_");
                }
            }
            data = data.ToUpper();
            return data;
        }

        public static String GenerateFileNameDefine(String data)
        {
            string define = Path.GetFileName(data);
            define = define.Replace(".", "_");
            return GenerateDefineFromString(define);
        }

        public static String CreateFrequencyDefineName(double frequency)
        {
            return "FREQUENCY_" + Math.Floor(frequency * 1000).ToString();
        }

        public static String CreateDefine(String source, String destination, bool useBracers = true, int macroStart = 80)
        {
            if (useBracers)
            {
                return FillStringForCount("#define " + source, ' ', macroStart) + "(" + destination + ")";
            }
            else
            {
                return FillStringForCount("#define " + source, ' ', macroStart) + destination;
            }
        }

        public static String CreateEnumValue(EnumField source)
        {
            return "    " + source.Name + " = " + source.Value;
        }

        public static String FillStringForCount(String baseString, char fillSymbol, int necessaryLength)
        {
            if (baseString.Length < necessaryLength)
            {
                while (baseString.Length < necessaryLength)
                {
                    baseString = baseString + fillSymbol;
                }
            }
            else
            {
                if (necessaryLength != 0)
                {
                    baseString = baseString + fillSymbol;
                }
            }
            return baseString;
        }

        public static String LinkToTheComponentInstance(ComponentInstance compInstance)
        {
            String compName = GenerateComponentName(compInstance.Name);
            String result = "(" + RteFunctionsGenerator.ComponentInstancePointerDatatype + ")&" + compName;
            return result;
        }

        public static String Generate_ClientServerPort_Arguments(ComponentInstance compInstance, ClientServerOperation operation, Boolean multipleInstantiation)
        {
            String serverPortCallFunction = "(";
            if (multipleInstantiation)
            {
                serverPortCallFunction += LinkToTheComponentInstance(compInstance);
                if (operation.Fields.Count > 0)
                {
                    serverPortCallFunction += ", ";
                }
            }
            for (int i = 0; i < operation.Fields.Count; i++)
            {
                serverPortCallFunction += operation.Fields[i].Name;
                if (i != operation.Fields.Count - 1)
                {
                    serverPortCallFunction += ", ";
                }
            }
            serverPortCallFunction += ")";
            return serverPortCallFunction;
        }

        

        public static String GenerateReadWriteConnectionFunctionName(PortDefenition port, SenderReceiverInterfaceField field)
        {
            String res = "Rte_";

            if (port.PortType == PortType.Sender)
            {
                res += "Write_";
            }
            else
            {
                res += "Read_";
            }

            ApplicationSwComponentType compDefenition = AutosarApplication.GetInstance().FindComponentDefenitionByPort(port);
            res += compDefenition.Name + "_" + port.Name + "_" + field.Name;

            return res;
        }


        public static String GenerateInternalReadWriteConnectionFunctionName(String componentName, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String res = "Rte_Internal";

            if (port.PortType == PortType.Sender)
            {
                res += "Write_";
            }
            else
            {
                res += "Read_";
            }

            res += componentName + "_" + port.Name + "_" + field.Name;

            return res;
        }

        public static String GenerateInternalCallConnectionFunctionName(String componentName, PortDefenition port, ClientServerOperation operation)
        {
            String res = "Rte_InternalCall_";

            res += componentName + "_" + port.Name + "_" + operation.Name;

            return res;
        }

        public static String GenerateInternalCDataFunctionName(String componentName, CDataDefenition cdata)
        {
            String res = "Rte_InternalCData_";

            res += componentName + "_" + cdata.Name;

            return res;
        }

        public static String GenerateInternalSendReceiveConnectionFunctionName(String componentName, PortDefenition port, SenderReceiverInterfaceField field)
        {
            String res = "Rte_Internal";

            if (port.PortType == PortType.Sender)
            {
                res += "Send_";
            }
            else
            {
                res += "Receive_";
            }

            res += componentName + "_" + port.Name + "_" + field.Name;

            return res;
        }

        public static String GenerateVariable(String Name, String DataType, bool Static = false, int PointerCount = 0, String defaultValue = "", int indent = 0)
        {
            String str = FillStringForCount("", ' ', indent);
            if (Static)
            {
                str += "static ";
            }
            str += DataType;
            if (PointerCount > 0)
            {
                for(int i = 0; i < PointerCount; i++)
                {
                    str += "*";
                }
            }
            str += " " + Name;
            if (defaultValue.Length > 0)
            {
                str += " = " + defaultValue;
            }
            str += ";";
            return str;
        }

        


        

        public static String GenerateRteOsTaskName(OsTask task)
        {
            return "Rte_Task_" + task.Name;
        }

        public static String GenerateRteOsTaskFunctionName(OsTask task)
        {
            return "Rte_Task_Runnable_" + task.Name;
        }
    }
}
