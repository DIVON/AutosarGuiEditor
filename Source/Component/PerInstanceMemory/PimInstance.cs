using AutosarGuiEditor.Source.Component.PerInstanceMemory;
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

namespace AutosarGuiEditor.Source.Painters.Components.PerInstance
{
    public class PimInstance:IGUID
    {
        public Guid DefenitionGuid;
        public PimDefaultValuesList DefaultValues = new PimDefaultValuesList();

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

        public void SyncronizeName()
        {
            foreach(PimDefaultValue defValue in DefaultValues)
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

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("PimInstance");
            base.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("DefenitionGuid", DefenitionGuid.ToString("B")));
            DefaultValues.WriteToXML(xmlElement);
            root.Add(xmlElement);
        }

        public PimDefenition Defenition
        {
            get
            {
                return AutosarApplication.GetInstance().FindPimDefenition(DefenitionGuid);
            }
        }

        public void UpdateDefaultValues()
        {
            this.Name = Defenition.Name;

            PimDefaultValuesList newDefValuesList = new PimDefaultValuesList();

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
                    foreach(ComplexDataTypeField field in compDataType.Fields)
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
                foreach (PimDefaultValue pimDefValue in newDefValuesList)
                {
                    if (FindName(DefaultValues, pimDefValue.Name) == false)
                    {
                        DefaultValues.Add(pimDefValue);
                    }
                }
            }
        }

        bool FindName(PimDefaultValuesList newDefValuesList, String nameToFind)
        {
            foreach(PimDefaultValue pimDefValue in newDefValuesList)
            {
                if (pimDefValue.Name.Equals(nameToFind))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddComplexDataTypeFieldDefaultValue(PimDefaultValuesList newDefValuesList, String baseName, ComplexDataTypeField cmplfield)
        {
            String additionalInfo = "";
            if (cmplfield.IsPointer)
            {
                 additionalInfo = "*";
            }

            PimDefaultValue defValue = PimFabric.GetInstance().CreatePimDefaultValue();
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

        private void AddSimpleDataTypeDefaultValue(PimDefaultValuesList newDefValuesList, String baseName, Boolean IsArray, String FieldName, int arraySize)
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

            PimDefaultValue defValue = PimFabric.GetInstance().CreatePimDefaultValue();
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

        private void AddComplexDataTypeFields(PimDefaultValuesList newDefValuesList, String baseName, ComplexDataTypeField field)
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

        public String GetDefaultValue()
        {
            String defValue = "";
            if (DefaultValues.Count >= 1)
            {
                if (DefaultValues[0].DefaultValue.Length > 0)
                {                    
                    defValue = DefaultValues[0].DefaultValue;
                }
            }
            return defValue;
        }
    }
}
