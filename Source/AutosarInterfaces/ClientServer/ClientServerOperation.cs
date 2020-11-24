using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces.ClientServer
{
    public class ClientServerOperation : IGUID
    {
        public ClientServerOperation()
        {
            this.Name = "Operation";
        }

        public ClientServerOperation(String Name = "Operation")
        {
            this.Name = Name;
        }

        public ClientServerOperationFieldsList Fields = new ClientServerOperationFieldsList();

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("Operation");
            base.WriteToXML(xmlElement);
            Fields.WriteToXML(xmlElement);
            root.Add(xmlElement);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Fields.LoadFromXML(xml);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Fields);
            return list;
        }
    }
}
