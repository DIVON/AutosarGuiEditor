using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Component.CData
{
    public class CDataDefenition:IGUID
    {
        public Guid DatatypeGuid = new Guid();

        public CDataDefenition()
        {
            this.Name = "CData";           
		}

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            String dataTypeGuidString = XmlUtilits.GetFieldValue(xml, "DatatypeGuid", "");
            DatatypeGuid = GuidUtils.GetGuid(dataTypeGuidString, false);
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("PerInstanceMemoryDefenition");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("DatatypeGuid", DatatypeGuid.ToString("B"))); 
            xml.Add(xmldatatype);
        }

        public String DataTypeName
        {
            get
            {
                String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(DatatypeGuid);
                return dataTypeName;
            }
        }

        public IGUID DataType
        {
            get
            {
                return AutosarApplication.GetInstance().GetDataType(DatatypeGuid);
            }
        }
    }
}
