using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.Enum
{
    public class EnumFieldList : IGuidList<EnumField>
    {
        public override String GetName()
        {
            return "Enums";
        }

        public override Boolean CreateRoot()
        {
            return false;
        }

        public void SortById()
        {
            Sort(delegate(EnumField field1, EnumField field2)
            {
                object strValue1 = GetProperty(field1, "Value");
                object strValue2 = GetProperty(field2, "Value");

                int value1, value2;

                if ((Int32.TryParse(strValue1.ToString(), out value1) == true) && (Int32.TryParse(strValue2.ToString(), out value2) == true))
                {
                    return value1.CompareTo(value2);
                }

                return 1;
            });
        }

        public override void LoadFromXML(XElement xmlApp, string NameId = "")
        {
            base.LoadFromXML(xmlApp, NameId);
            SortById();
        }
    }
}
