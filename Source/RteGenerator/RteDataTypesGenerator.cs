using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public class RteDataTypesGenerator
    {
        public void GenerateDataTypesFile(String folder)
        {
            String FileName = folder + "\\" + Properties.Resources.RTE_DATATYPES_H_FILENAME;
            StreamWriter writer = new StreamWriter(FileName);
            RteFunctionsGenerator.GenerateFileTitle(writer, Properties.Resources.RTE_DATATYPES_H_FILENAME, Properties.Resources.DATATYPES_H_FILE_DESCRIPTION);
            String guardDefine = RteFunctionsGenerator.OpenGuardDefine(writer);

            writer.WriteLine();
            RteFunctionsGenerator.AddInclude(writer, Properties.Resources.RTE_RETURN_CODES_FILENAME);
            writer.WriteLine();

            WriteStaticGlobal(writer);
            WriteBaseDataTypes(writer);
            WriteEnumDataTypes(writer);
            WriteSimpleDataTypes(writer);

            ComplexDataTypesList sortedComplexDataTypes = SortComplexDataTypeWithDependencies();
            WriteComplexDataTypes(sortedComplexDataTypes, writer);

            GenerateComponentsDataTypes(writer);

            RteFunctionsGenerator.CloseGuardDefine(writer);
            RteFunctionsGenerator.WriteEndOfFile(writer);
            writer.Close();
        }

        void WriteStaticGlobal(StreamWriter writer)
        {
            writer.WriteLine("#ifdef COMPONENT_TEST");
            writer.WriteLine("#define STATIC_GLOBAL");
            writer.WriteLine("#else");
            writer.WriteLine("#define STATIC_GLOBAL static");
            writer.WriteLine("#endif");
            writer.WriteLine();
        }

        void GenerateComponentsDataTypes(StreamWriter writer)
        {
            foreach (ComponentDefenition compDef in AutosarApplication.GetInstance().ComponentDefenitionsList)
            {
                writer.WriteLine("/* Datatype for " + compDef.Name  + " */");
                writer.WriteLine("typedef struct");
                writer.WriteLine("{");
                
                /* Create index for components with multipleinstantiation */
                if (compDef.MultipleInstantiation == true)
                {
                    writer.WriteLine("    uint32 index;");
                }

                foreach(PimDefenition pimDefenition in compDef.PerInstanceMemoryList)
                {
                    GenerateDataTypeForPim(writer, compDef, pimDefenition);
                }

                foreach (CDataDefenition cdata in compDef.CDataDefenitions)
                {
                    GenerateDataTypeForCData(writer, compDef, cdata);
                }


                foreach (PortDefenition portDef in compDef.Ports)
                {
                    if (portDef.PortType == PortType.Sender)
                    {
                        SenderReceiverInterface srInterface = AutosarApplication.GetInstance().SenderReceiverInterfaces.FindObject(portDef.InterfaceGUID);
                        if (srInterface != null)
                        {
                            foreach (SenderReceiverInterfaceField field in srInterface.Fields)
                            {
                                GenerateFieldsForSenderPorts(writer, compDef, portDef, field);
                            }
                        }
                    }
                }

                writer.WriteLine("} " + compDef.Name + ";"); 
                writer.WriteLine();
            }
        }

        public void GenerateDataTypeForPim(StreamWriter writer, ComponentDefenition componentDefenition, PimDefenition pimDefenition)       
        {
            writer.WriteLine("    " + pimDefenition.DataTypeName + " " + RteFunctionsGenerator.GenerateRtePimFieldInComponentDefenitionStruct(componentDefenition, pimDefenition) + ";");            
        }

        public void GenerateDataTypeForCData(StreamWriter writer, ComponentDefenition componentDefenition, CDataDefenition cdata)
        {
            writer.WriteLine("    " + cdata.DataTypeName + " " + RteFunctionsGenerator.GenerateRteCDataFieldInComponentDefenitionStruct(componentDefenition, cdata) + ";");     
        }

        public void GenerateFieldsForSenderPorts(StreamWriter writer, ComponentDefenition componentDefenition, PortDefenition portDefenition, SenderReceiverInterfaceField srInterfaceField)
        {
            writer.WriteLine("    " + srInterfaceField.DataTypeName + " " + RteFunctionsGenerator.GenerateRteWriteFieldInComponentDefenitionStruct(portDefenition, srInterfaceField) + ";");
        }

        static bool isComplexDataTypeWithoutDependenciesToOtherCDT(ComplexDataType cdt)
        {
            ComplexDataTypesList allCDT = AutosarApplication.GetInstance().ComplexDataTypes;
            foreach(ComplexDataTypeField field in cdt.Fields)
            {
                ComplexDataType findedCDT = allCDT.FindObject(field.DataTypeGUID);
                if (findedCDT != null)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool SortDependenciedCDT(ComplexDataTypesList datatypes)
        {
            /* Select dependencies */
            int IterationCount = 0;

            bool hasDependencyLower = true;
            while (hasDependencyLower && (IterationCount < 100))
            {
                IterationCount++;
                hasDependencyLower = false;
                for (int i = 0; i < datatypes.Count - 1; i++)
                {
                    /* Find max dependency index */
                    int maxDependency = findMaxDependency(datatypes, datatypes[i], i);
                    
                    /* if there is dependency then move the datatype after dependency and start cycle again */
                    if (maxDependency != -1)
                    {
                        ComplexDataType cdt = datatypes[i];
                        datatypes.RemoveAt(i);
                        datatypes.Insert(maxDependency, cdt);
                        hasDependencyLower = true;
                        break;                        
                    }
                }
            }
       
            return IterationCount < 100;
        }

        static int findMaxDependency(ComplexDataTypesList datatypes, ComplexDataType currentDataType, int startIndex)
        {
             int maxDependency = -1;
             foreach (ComplexDataTypeField field in currentDataType.Fields)
             {
                 for (int i = startIndex; i < datatypes.Count; i++)
                 {
                     if (field.DataTypeGUID.Equals(datatypes[i].GUID))
                     {
                         if (maxDependency < i)
                         {
                             maxDependency = i;
                         }
                     }
                 }                 
             }
             return maxDependency;
        }

        ComplexDataTypesList SortComplexDataTypeWithDependencies()
        {
            /* Base list */
            ComplexDataTypesList allComplexDataTypes = new ComplexDataTypesList();
            allComplexDataTypes.Capacity = AutosarApplication.GetInstance().ComplexDataTypes.Count;

            ComplexDataTypesList sortedComplexDataTypes = new ComplexDataTypesList();
            sortedComplexDataTypes.Capacity = AutosarApplication.GetInstance().ComplexDataTypes.Count;

            allComplexDataTypes.AddRange(AutosarApplication.GetInstance().ComplexDataTypes);

            /* First shall be structures without dependencies to complex datatypes */
            for (int i = allComplexDataTypes.Count - 1; i >= 0; i--)
            {
                /* If component doesn't have dependencies to other complex data types remove it from list */
                if (isComplexDataTypeWithoutDependenciesToOtherCDT(allComplexDataTypes[i]) == true)
                {
                    sortedComplexDataTypes.Add(allComplexDataTypes[i]);
                    allComplexDataTypes.RemoveAt(i);
                }
            }

            sortedComplexDataTypes.DoSort();

            bool sortingCDTResult = SortDependenciedCDT(allComplexDataTypes);
            if (sortingCDTResult == false)
            {
                String strRes = "Imposible to generate RTE because there is recursion dependencies in components: " + Environment.NewLine;
                foreach(ComplexDataType cdt in allComplexDataTypes)
                {
                    strRes += cdt.Name + Environment.NewLine;
                }
                MessageBox.Show(strRes);
            }

            sortedComplexDataTypes.AddRange(allComplexDataTypes);

            return sortedComplexDataTypes;
        }

        public static void RemoveDataTypesWithoutDependencies(ComplexDataTypesList allComplexDataTypes)
        {
            for (int i = allComplexDataTypes.Count - 1; i >= 0; i--)
            {
                /* If component doesn't have dependencies to other complex data types remove it from list */
                if (isComplexDataTypeWithoutDependenciesToOtherCDT(allComplexDataTypes[i]) == true)
                {
                    allComplexDataTypes.RemoveAt(i);
                }
            }
        }

        public void WriteComplexDataTypes(ComplexDataTypesList datatypes, StreamWriter writer)
        {
            writer.WriteLine("/* Complex data types */");
            writer.WriteLine("");
            foreach (ComplexDataType datatype in datatypes)
            {
                WriteComplexDataType(writer, datatype);
            }
        }

        public void WriteComplexDataType(StreamWriter writer, ComplexDataType datatype)
        {
            writer.WriteLine("/* Complex datatype : " + datatype.Name + " */");
            writer.WriteLine("typedef struct");
            writer.WriteLine("{");
            foreach (ComplexDataTypeField field in datatype.Fields)
            {
                if (!field.IsPointer)
                {
                    writer.WriteLine("    " + field.DataTypeName + " " + field.Name + ";");
                }
                else
                {
                    writer.WriteLine("    " + field.DataTypeName + " *" + field.Name + ";");
                }
            }
            writer.WriteLine("} " + datatype.Name + ";");
            writer.WriteLine("");

            /* Generate an array if it existis*/
            ArrayDataTypeGenerator.GenerateArrayForDataType(writer, datatype);
        }

        public void WriteBaseDataTypes(StreamWriter writer)
        {
            writer.WriteLine("/* Base datatypes */");
            writer.WriteLine("#define TRUE	1");
            writer.WriteLine("#define FALSE	0");
            writer.WriteLine("");

            BaseDataTypesCodeGenerator.GenerateCode(writer, AutosarApplication.GetInstance().BaseDataTypes);

            writer.WriteLine("");
            writer.WriteLine("/* Rte data types */");
            writer.WriteLine("typedef uint16               Std_ReturnType;");
            writer.WriteLine("typedef void    *" + RteFunctionsGenerator.ComponentInstancePointerDatatype + ";");
            writer.WriteLine("");
        }

        public void WriteSimpleDataTypes(StreamWriter writer)
        {
            writer.WriteLine("/* Simple data types */");
            writer.WriteLine("");
            foreach (SimpleDataType datatype in AutosarApplication.GetInstance().SimpleDataTypes)
            {
                WriteSimpleDataType(writer, datatype);
            }
        }

        public void WriteEnumDataTypes(StreamWriter writer)
        {
            writer.WriteLine("/* Enums data types */");
            writer.WriteLine("");
            foreach (EnumDataType datatype in AutosarApplication.GetInstance().Enums)
            {
                WriteEnumDataType(writer, datatype);
            }
        }

        String acceptFloatValue(String floatValue)
        {
            /* Convert comma to dot */
            floatValue = floatValue.Replace(",", ".");
            floatValue = floatValue.Replace("E", "e");

            if ((floatValue.IndexOf("f") == -1) && (floatValue.IndexOf("e") == -1))
            {
                if (floatValue.IndexOf(".") == -1)
                {
                    /* There is neither "f" nor "." */
                    floatValue += ".f";
                }
                else
                {
                    /* "." exists */
                    floatValue += "f";
                }
            }
            return floatValue;
        }

        public void WriteSimpleDataType(StreamWriter writer, SimpleDataType datatype)
        {
            writer.WriteLine("/* Datatype : " + datatype.Name + " */");

            /* Write limits */
            String upperLimit = datatype.Name + "_UPPER_LIMIT";
            string maxValue = datatype.MaxValue;
            string minValue = datatype.MinValue;
            if (datatype.GetBaseDataType() == AutosarApplication.GetInstance().BaseDataTypes.float32)
            {
                minValue = acceptFloatValue(datatype.MinValue);
                maxValue = acceptFloatValue(datatype.MaxValue);
            }
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(upperLimit, maxValue, false));

            String lowerLimit = datatype.Name + "_LOWER_LIMIT";
            writer.WriteLine(RteFunctionsGenerator.CreateDefine(lowerLimit, minValue, false));

            /* Write datatype */
            String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(datatype.BaseDataTypeGUID);
            string typedef = RteFunctionsGenerator.FillStringForCount("typedef  " + dataTypeName, ' ', 24);
            writer.WriteLine(typedef + datatype.Name + ";");
            writer.WriteLine("");

            /* Generate an array if it existis*/
            ArrayDataTypeGenerator.GenerateArrayForDataType(writer, datatype);
        }

        public void WriteEnumDataType(StreamWriter writer, EnumDataType datatype)
        {
            writer.WriteLine("/* Enum Datatype : " + datatype.Name + " */");

            /* Write datatype */            
            writer.WriteLine("typedef enum ");
            writer.WriteLine("{");

            /* Write values */
            for (int i = 0; i < datatype.Fields.Count; i++)                
            {                
                writer.Write(RteFunctionsGenerator.CreateEnumValue(datatype.Fields[i]));
                writer.WriteLine(",");
            }

            /* Write elements count */
            //writer.WriteLine("    " + datatype.Name + "_ELEMENTS_COUNT");

            writer.WriteLine("} " + datatype.Name + ";");
            writer.WriteLine("");

            int minLimit = datatype.GetLimit(LimitType.ltLowerLimit);
            String defineMin = RteFunctionsGenerator.CreateDefine(datatype.Name + "_LOWER_LIMIT", minLimit.ToString(), false);
            writer.WriteLine(defineMin);

            int upperLimit = datatype.GetLimit(LimitType.ltUpperLimit);
            String defineMax = RteFunctionsGenerator.CreateDefine(datatype.Name + "_UPPER_LIMIT", upperLimit.ToString(), false);
            writer.WriteLine(defineMax);

            int elementsCount = datatype.Fields.Count;
            String elementsCountStr = RteFunctionsGenerator.CreateDefine(datatype.Name + "_ELEMENTS_COUNT", elementsCount.ToString(), false);
            writer.WriteLine(elementsCountStr);

            writer.WriteLine("");

            /* Generate an array if it existis*/
            ArrayDataTypeGenerator.GenerateArrayForDataType(writer, datatype);
        }
    }
}
