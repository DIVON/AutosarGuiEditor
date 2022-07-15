using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.RteGenerator
{
    public enum ProgrammingLanguageTypeDef
    {
        C,
        Cpp
    }

    public class ProgrammingLanguage
    {
        public ProgrammingLanguage(ProgrammingLanguageTypeDef type = ProgrammingLanguageTypeDef.C)
        {
            Type = type;
        }

        public int ToInt()
        {
            return (int)Type;
        }

        public ProgrammingLanguageTypeDef Type { set; get; }

        public virtual void LoadFromXML(XElement root)
        {
            var langTypeNodes = root.Descendants("ProgrammingLanguage");
            XElement langTypeNode = langTypeNodes.FirstOrDefault();
            if (langTypeNode != null)
            {
                if (langTypeNode.Value != null)
                {
                    String value = langTypeNode.Value;
                    ProgrammingLanguageTypeDef enValue;

                    if (ProgrammingLanguageTypeDef.TryParse(value, out enValue) == true)
                    {
                        Type = enValue;
                    }
                }
            }
        }

        public virtual void WriteToXML(XElement root)
        {
            XElement BaseElement = new XElement("ProgrammingLanguage", Type.ToString("F"));
            root.Add(BaseElement);
        }
    }
}
