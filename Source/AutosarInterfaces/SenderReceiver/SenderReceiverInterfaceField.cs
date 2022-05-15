using AutosarGuiEditor.Source.DataTypes;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver
{
    public class SenderReceiverInterfaceField : IGUID
    {
        public Boolean IsPointer
        {
            set;
            get;
        }

        public Guid BaseDataTypeGUID = Guid.Empty;

        public DefaultValuesList DefaultValues
        {
            set;
            get;
        }


        public SenderReceiverInterfaceField()
        {
            this.Name = "SenderReceiverInterfaceField";
            this.IsPointer = false;
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("Field");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("IsPointer", IsPointer.ToString()));
            xmldatatype.Add(new XElement("BaseDataTypeGUID", BaseDataTypeGUID.ToString("B")));
            xml.Add(xmldatatype);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            IsPointer = xml.Element("IsPointer").Value.Equals(true.ToString());
            String baseGuidString = XmlUtilits.GetFieldValue(xml, "BaseDataTypeGUID", "");
            BaseDataTypeGUID = GuidUtils.GetGuid(baseGuidString, false);
        }

        public String DataTypeName
        {
            get
            {
                return AutosarApplication.GetInstance().GetDataTypeName(this.BaseDataTypeGUID);
            }
        }

        public object DataType
        {
            get
            {
                return AutosarApplication.GetInstance().GetDataType(this.BaseDataTypeGUID);
            }
        }

        public String QueuedInterfaceName(string parentInterfaceName)
        {
            return "dtRte_" + parentInterfaceName + "_" + Name + "_Handler";
        }
    }
}
