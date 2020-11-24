using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces
{
    public class ClientServerInterface : IGUID
    {
        public ClientServerOperationList Operations = new ClientServerOperationList();

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("Interface");
            base.WriteToXML(xmldatatype);
            Operations.WriteToXML(xmldatatype);
            xml.Add(xmldatatype);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Operations.LoadFromXML(xml);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Operations);
            return list;
        }
    }
}
