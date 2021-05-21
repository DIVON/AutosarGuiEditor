using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source
{
    public enum MCUTypeDef
    {
        STM32F1xx,
        STM32F4xx
    }

    public class CMCUType : IXmlWriteable
    {
        public CMCUType(MCUTypeDef type = MCUTypeDef.STM32F4xx)
        {
            Type = type;
        }

        public int ToInt()
        {
            return (int)Type;
        }

        public MCUTypeDef Type { set; get; }

        public virtual void LoadFromXML(XElement root)
        {
            var mcuTypeNodes = root.Descendants("MCUType");
            XElement mcuTypeNode = mcuTypeNodes.FirstOrDefault();
            if (mcuTypeNode != null)
            {
                if (mcuTypeNode.Value != null)
                {
                    String value = mcuTypeNode.Value;
                    MCUTypeDef enValue;

                    if (MCUTypeDef.TryParse(value, out enValue) == true)
                    {
                        Type = enValue;
                    }
                }
            }
        }

        public virtual void WriteToXML(XElement root)
        {
            XElement BaseElement = new XElement("MCUType", Type.ToString("F"));
            root.Add(BaseElement);
        }
    }
}
