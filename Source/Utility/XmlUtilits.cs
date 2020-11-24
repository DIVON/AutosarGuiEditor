using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Utility
{
    public static class XmlUtilits
    {
        public static String GetFieldValue(XElement element, String fieldName, String defValue)
        {
            XElement elem = element.Element(fieldName);
            if (elem != null)
            {
                return elem.Value;
            }
            else
            {
                return defValue;
            }
        }
    }
}
