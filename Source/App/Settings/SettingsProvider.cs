using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.App.Settings
{
    public class SettingsProvider
    {
        SettingsProvider()
        {

        }

        IniFile iniFile = new IniFile("config.ini");

        const string LastProjectFileNameFN = "LastProjectFileName";
        const string projectSignature = "Signature";
        const string Section = "Default";

        public String LastProjectFileName
        {
            set
            {
                iniFile.Write(Section, LastProjectFileNameFN, value);                
            }

            get
            {
                String fileName = iniFile.ReadINI(Section, LastProjectFileNameFN);
                return fileName;
            }
        }

        const String fixXmlFormating = "FixXmlFormating";

        public Boolean FixXmlFormating
        {
            set
            {
                iniFile.Write(Section, fixXmlFormating, value.ToString());  
            }
            get
            {
                String fixStr = iniFile.ReadINI(Section, fixXmlFormating);
                Boolean res = true;
                if (Boolean.TryParse(fixStr, out res) == false)
                {
                    res = true;
                    FixXmlFormating = true;
                }
                return res;
            }
        }

        static SettingsProvider instance = null;
        public static SettingsProvider GetInstance()
        {
            if (instance == null)
            {
                instance = new SettingsProvider();
            }
            return instance;
        }
    }
}