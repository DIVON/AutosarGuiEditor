using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.ArrayDataType
{
    public class ArrayDataType : IGUID
    {
        public Guid DataTypeGUID { set; get; }
        
        public int Size { set; get; }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            String dataTypeGuidString = XmlUtilits.GetFieldValue(xml, "DataTypeGUID", "");
            DataTypeGUID = GuidUtils.GetGuid(dataTypeGuidString, false);

            int size;
            string arrSizeStr = xml.Element("Size").Value;

            if (int.TryParse(arrSizeStr, out size))
            {
                Size = size;
            }
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("ArrayDataType");

            base.WriteToXML(xmldatatype);

            xmldatatype.Add(new XElement("DataTypeGUID", DataTypeGUID.ToString("B")));
            xmldatatype.Add(new XElement("Size", Size.ToString()));

            xml.Add(xmldatatype);
        }

        public String DataTypeName
        {
            get
            {
                String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(DataTypeGUID);
                return dataTypeName;
            }
        }

        public IGUID DataType
        {
            get
            {
                return AutosarApplication.GetInstance().GetDataType(DataTypeGUID);
            }
        }
    }
}
