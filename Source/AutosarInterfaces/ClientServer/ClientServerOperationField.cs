using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces.ClientServer
{
    public enum ClientServerOperationDirection
    {
        IN,
        OUT,
        INOUT
    }


    public class ClientServerOperationField : IGUID
    {
        public ClientServerOperationDirection Direction
        {
            set;
            get;
        }

        public Guid BaseDataTypeGUID
        {
            set;
            get;
        }


        public ClientServerOperationField()
        {
            BaseDataTypeGUID = Guid.Empty;
            Name = "Field";
            this.Direction = ClientServerOperationDirection.IN;
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmldatatype = new XElement("Field");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("BaseDataTypeGUID", BaseDataTypeGUID.ToString("B")));
            String dir = "";
            switch (this.Direction)
            {
                case ClientServerOperationDirection.IN: dir = "in"; break;
                case ClientServerOperationDirection.OUT: dir = "out"; break;
                case ClientServerOperationDirection.INOUT: dir = "in-out"; break;
            }
            xmldatatype.Add(new XElement("Direction", dir));
            root.Add(xmldatatype);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Name = xml.Element("Name").Value;
            
            String baseGuidString = XmlUtilits.GetFieldValue(xml, "BaseDataTypeGUID", "");
            BaseDataTypeGUID = GuidUtils.GetGuid(baseGuidString, false);

            String dir = xml.Element("Direction").Value;
            switch (dir)
            {
                case "in": Direction = ClientServerOperationDirection.IN; break;
                case "out": Direction = ClientServerOperationDirection.OUT; break;
                case "in-out": Direction = ClientServerOperationDirection.INOUT; break;
            }
        }

        public String DataTypeName
        {
            get
            {
                String dataTypeName = AutosarApplication.GetInstance().GetDataTypeName(BaseDataTypeGUID);
                return dataTypeName;
            }
        }

        public IGUID Datatype
        {
            get
            {
                return AutosarApplication.GetInstance().GetDataType(BaseDataTypeGUID);
            }
        }
    }
}
