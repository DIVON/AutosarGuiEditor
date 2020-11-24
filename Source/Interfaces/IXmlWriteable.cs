using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Interfaces
{
    public interface IXmlWriteable
    {
        void LoadFromXML(XElement xmlApp);
        void WriteToXML(XElement root);
    }
}
