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

        public static Boolean GetBooleanValue(XElement element, String fieldName, Boolean defValue)
        {
            String elem = GetFieldValue(element, fieldName, defValue.ToString());
            Boolean retValue;
            try
            {
                retValue = Convert.ToBoolean(elem);
            } 
            catch
            {
                retValue = defValue;
            }
            return retValue;
        }

        public static int GetIntegerValue(XElement element, String fieldName, int defValue)
        {
            String elem = GetFieldValue(element, fieldName, defValue.ToString());
            int retValue;
            try
            {
                retValue = Convert.ToInt32(elem);
            }
            catch
            {
                retValue = defValue;
            }
            return retValue;
        }
    }
}
