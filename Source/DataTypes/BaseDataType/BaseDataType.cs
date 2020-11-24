using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.BaseDataType
{
    public class BaseDataType : PlainDataType
    {
        public String SystemName
        {
            set;
            get;
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            SystemName = XmlUtilits.GetFieldValue(xml, "SystemName", "ERROR");
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("BaseDataType");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("SystemName", SystemName));
            xml.Add(xmldatatype);
        }
    }
}
