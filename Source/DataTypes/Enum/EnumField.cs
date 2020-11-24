using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.Enum
{
    public class EnumField : IGUID
    {
        public EnumField()
        {
            this.Name = "ENUM_FIELD";
        }

        public Int32 evalue = 0;

        public Int32 Value
        {
            set
            {
                evalue = value;
            }
            get
            {
                return evalue;
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            XElement elem = xml.Element("Value");
            if (elem != null)
            {
                int val;
                if (int.TryParse(elem.Value, out val) == true)
                {
                    Value = val;
                }
                else
                {
                    MessageBox.Show("error for: " + elem.Name);
                }
            }            
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("EnumField");
            base.WriteToXML(xmldatatype);            
            xmldatatype.Add(new XElement("Value", Value));            
            xml.Add(xmldatatype);            
        }
    }
}
