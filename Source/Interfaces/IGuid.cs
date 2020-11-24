using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.SystemInterfaces
{
    public class IGUID : IXmlWriteable
    {
        private String name;
        private Guid guid = Guid.NewGuid();
        
        public String Name
        {
            set 
            {
                name = value;
            }
            get 
            {
                return name;
            }
        }

        public Guid GUID
        {
            set
            {
                guid = value;
            }
            get
            {
                return guid;
            }
        }

        public virtual void LoadFromXML(XElement xml)
        {
            Name = XmlUtilits.GetFieldValue(xml, "Name", "ERROR");
            GUID = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "GUID", ""), false);
        }

        public virtual void WriteToXML(XElement root)
        {
            root.Add(new XElement("Name", Name));
            root.Add(new XElement("GUID", GUID.ToString("B")));
        }

        public virtual List<IAutosarTreeList> GetLists()
        {
            return null;
        }

        public virtual Boolean CreateRoot()
        {
            return true;
        }
    }
}
