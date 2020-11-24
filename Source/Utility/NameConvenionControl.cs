using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Utility
{
    public static class NameUtils
    {
        public static bool CheckComponentName(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }

            bool startCheck =
                ((name[0] >= '0') && (name[0] <= '9')) ||
                ((name[0] == '_'));
            
            if (startCheck)
            {
                return false;
            }

            foreach(char symbol in name.ToArray<char>())
            {
                bool checkResult =
                    ((symbol >= 'A') && (symbol <= 'Z')) ||
                    ((symbol >= 'a') && (symbol <= 'z')) ||
                    ((symbol >= '0') && (symbol <= '9')) ||
                    (symbol == '_');
                
                if (!checkResult)
                {
                    return false;
                }                
            }
            return true; 
        }

        public static  bool IsNumerric(string value)
        {
            return value.All(char.IsNumber);
        }

        public static bool CheckMacroName(String name)
        {
            if (name.Length == 0)
            {
                return false;
            }

            bool startCheck =
                ((name[0] >= '0') && (name[0] <= '9')) ||
                ((name[0] == '_'));

            if (startCheck)
            {
                return false;
            }

            foreach (char symbol in name.ToArray<char>())
            {
                bool checkResult =
                    ((symbol >= 'A') && (symbol <= 'Z')) ||
                    ((symbol >= '0') && (symbol <= '9')) ||
                    (symbol == '_');

                if (!checkResult)
                {
                    return false;
                }
            }
            return true;
        }

        public static String GetInterfaceName(PortDefenition portDef)
        {
            String interfaceName = portDef.InterfaceName;
            if ((interfaceName.IndexOf("sr") == 0) || (interfaceName.IndexOf("cs") == 0))
            {
                return interfaceName.Substring(2);
            }
            return interfaceName;
        }
    }
}
