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
    public class CDataDefaultValue: IGUID
    {
        public Guid FieldGuid;

        public String DefaultValue
        {
            set;
            get;
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            String portDefString = XmlUtilits.GetFieldValue(xml, "FieldGuid", Guid.Empty.ToString());

            if (!Guid.TryParse(portDefString, out FieldGuid))
            {
                FieldGuid = Guid.Empty;
            }

            DefaultValue = XmlUtilits.GetFieldValue(xml, "DefaultValue", "");
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("CDataDefaultValue");
            base.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("FieldGuid", FieldGuid.ToString("B")));
            xmlElement.Add(new XElement("DefaultValue", DefaultValue));
            root.Add(xmlElement);
        }
    }
}
