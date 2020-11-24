using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.DataTypes.ArrayDataType;
using AutosarGuiEditor.Source.DataTypes.BaseDataType;
using AutosarGuiEditor.Source.DataTypes.ComplexDataType;
using AutosarGuiEditor.Source.DataTypes.Enum;
using AutosarGuiEditor.Source.Fabrics;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Painters.Components.CData
{
    public class CDataInstance:IGUID
    {
        public Guid DefenitionGuid;
        public CDataDefaultValuesList DefaultValues = new CDataDefaultValuesList();

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            DefaultValues.LoadFromXML(xml);

            String portDefString = XmlUtilits.GetFieldValue(xml, "DefenitionGuid", Guid.Empty.ToString());

            if (!Guid.TryParse(portDefString, out DefenitionGuid))
            {
                DefenitionGuid = Guid.Empty;
            }
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("CDataInstance");
            base.WriteToXML(xmlElement);
            DefaultValues.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("DefenitionGuid", DefenitionGuid.ToString("B")));

            root.Add(xmlElement);
        }

        public void SyncronizeName()
        {
            foreach (CDataDefaultValue defValue in DefaultValues)
            {
                int dotIndex = defValue.Name.IndexOf(".");
                if (dotIndex > 0)
                {
                    String propertyName = defValue.Name.Substring(dotIndex);
                    defValue.Name = Defenition.Name + propertyName;
                }
                else
                {
                    defValue.Name = Defenition.Name;
                }
            }
        }

        public CDataDefenition Defenition
        {
            get
            {
                return AutosarApplication.GetInstance().FindCDataDefenition(DefenitionGuid);
            }
        }

        public void UpdateDefaultValues()
        {
            this.Name = Defenition.Name;

            CDataDefaultValuesList newDefValuesList = new CDataDefaultValuesList();

            /* Create new list */
            object datatype = AutosarApplication.GetInstance().GetDataType(Defenition.DatatypeGuid);
            if (datatype != null)
            {
                if ((datatype is BaseDataType) || (datatype is SimpleDataType) || (datatype is EnumDataType) || (datatype is ArrayDataType))
                {
                    AddSimpleDataTypeDefaultValue(newDefValuesList, "", false, Name, 0);
                }
                else if (datatype is ComplexDataType)
                {
                    ComplexDataType compDataType = (ComplexDataType)datatype;
                    foreach (ComplexDataTypeField field in compDataType.Fields)
                    {
                        if (AutosarApplication.GetInstance().IsDataTypeComlex(field.DataTypeGUID))
                        {
                            AddComplexDataTypeFields(newDefValuesList, Name + "." + field.Name, field);
                        }
                        else
                        {
                            AddComplexDataTypeFieldDefaultValue(newDefValuesList, Name, field);
                        }
                    }
                }
            }

            /* Remove unexists */
            for (int i = DefaultValues.Count - 1; i >= 0; i--)
            {
                if (FindName(newDefValuesList, DefaultValues[i].Name) == false)
                {
                    DefaultValues.RemoveAt(i);
                }
            }

            /* Add new */
            {
                foreach (CDataDefaultValue defValue in newDefValuesList)
                {
                    if (FindName(DefaultValues, defValue.Name) == false)
                    {
                        DefaultValues.Add(defValue);
                    }
                }
            }
        }

        bool FindName(CDataDefaultValuesList newDefValuesList, String nameToFind)
        {
            foreach(CDataDefaultValue defValue in newDefValuesList)
            {
                if (defValue.Name.Equals(nameToFind))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddComplexDataTypeFieldDefaultValue(CDataDefaultValuesList newDefValuesList, String baseName, ComplexDataTypeField cmplfield)
        {
            String additionalInfo = "";
            if ((cmplfield.IsPointer))
            {
                additionalInfo = "*";
            }

            CDataDefaultValue defValue = CDataFabric.GetInstance().CreateCDataDefaultValue();
            defValue.DefaultValue = "";
            defValue.FieldGuid = cmplfield.GUID;
            if (baseName.Length > 0)
            {
                defValue.Name = baseName + "." + cmplfield.Name + additionalInfo;
            }
            else
            {
                defValue.Name = cmplfield.Name + additionalInfo;
            }
            newDefValuesList.Add(defValue);
        }

        private void AddSimpleDataTypeDefaultValue(CDataDefaultValuesList newDefValuesList, String baseName, Boolean IsArray, String FieldName, int arraySize)
        {
            String additionalInfo = "";
            if ((IsArray))
            {
                if (arraySize > 0)
                {
                    additionalInfo = "[" + arraySize.ToString() + "]";
                }
                else
                {
                    additionalInfo = "*";
                }
            }

            CDataDefaultValue defValue = CDataFabric.GetInstance().CreateCDataDefaultValue();
            defValue.DefaultValue = "";
            defValue.FieldGuid = Guid.Empty;
            if (baseName.Length > 0)
            {
                defValue.Name = baseName + "." + FieldName + additionalInfo;
            }
            else
            {
                defValue.Name = FieldName + additionalInfo;
            }
            newDefValuesList.Add(defValue);
        }


        private void AddComplexDataTypeFields(CDataDefaultValuesList newDefValuesList, String baseName, ComplexDataTypeField field)
        {
            object datatype = AutosarApplication.GetInstance().GetDataType(field.DataTypeGUID);

            ComplexDataType compDataType = (ComplexDataType)datatype;
            foreach (ComplexDataTypeField cmplfield in compDataType.Fields)
            {
                if (AutosarApplication.GetInstance().IsDataTypeComlex(cmplfield.DataTypeGUID))
                {
                    AddComplexDataTypeFields(newDefValuesList, baseName + "." + cmplfield.Name, cmplfield);
                }
                else
                {
                    AddComplexDataTypeFieldDefaultValue(newDefValuesList, baseName, cmplfield);
                }
            }
        }
    }
}
