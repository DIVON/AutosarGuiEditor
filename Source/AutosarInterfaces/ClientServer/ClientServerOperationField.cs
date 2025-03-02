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
        VALUE,
        CONST_VALUE,
        VAL_REF,
        CONST_VAL_REF,
        VAL_CONST_REF,
        CONST_VAL_CONST_REF,
        CONST_REF
    }


    public class ClientServerOperationField : IGUID
    {
        public const string STR_VALUE = "value";
        public const string STR_CONST_VALUE = "const value";
        public const string STR_VAL_REF = "reference";
        public const string STR_CONST_VAL_REF = "const dt * vn / const &";
        public const string STR_VAL_CONST_REF = "dt * const vn";
        public const string STR_CONST_VAL_CONST_REF = "const dt * const vn";

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
            this.Direction = ClientServerOperationDirection.VALUE;
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmldatatype = new XElement("Field");
            base.WriteToXML(xmldatatype);
            xmldatatype.Add(new XElement("BaseDataTypeGUID", BaseDataTypeGUID.ToString("B")));
            String dir = "";
            switch (this.Direction)
            {
                case ClientServerOperationDirection.VALUE:                  dir = STR_VALUE; break;
                case ClientServerOperationDirection.CONST_VALUE:            dir = STR_CONST_VALUE; break;
                case ClientServerOperationDirection.VAL_REF:                dir = STR_VAL_REF; break;
                case ClientServerOperationDirection.CONST_VAL_REF:          dir = STR_CONST_VAL_REF; break;
                case ClientServerOperationDirection.VAL_CONST_REF:          dir = STR_VAL_CONST_REF; break;
                case ClientServerOperationDirection.CONST_VAL_CONST_REF:    dir = STR_CONST_VAL_CONST_REF; break;
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
                case STR_VALUE: Direction = ClientServerOperationDirection.VALUE; break;
                case STR_CONST_VALUE: Direction = ClientServerOperationDirection.CONST_VALUE; break;
                case STR_VAL_REF: Direction = ClientServerOperationDirection.VAL_REF; break;
                case STR_CONST_VAL_REF: Direction = ClientServerOperationDirection.CONST_VAL_REF; break;
                case STR_VAL_CONST_REF: Direction = ClientServerOperationDirection.VAL_CONST_REF; break;
                case STR_CONST_VAL_CONST_REF: Direction = ClientServerOperationDirection.CONST_VAL_CONST_REF; break;
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
