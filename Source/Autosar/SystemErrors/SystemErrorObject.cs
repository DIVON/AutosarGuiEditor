using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Autosar.SystemErrors
{
    public class SystemErrorObject:IGUID
    {
        public SystemErrorObject()
        {
            Value = 0;
            Name = "ERROR";
        }

        public UInt32 Value
        {
            set;
            get;
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            XElement elem = xml.Element("Value");
            if (elem != null)
            {
                UInt32 val;
                if (UInt32.TryParse(elem.Value, out val) == true)
                {
                    Value = val;
                }
                else
                {
                    Value = 0;
                }
            }
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("SystemErrorObject");
            base.WriteToXML(xmldatatype);            
            xmldatatype.Add(new XElement("Value", Value.ToString()));

            xml.Add(xmldatatype);
        }
    }
}
