using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
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
            xmldatatype.Add(new XElement("IsAsync", IsAsync));


            xml.Add(xmldatatype);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Operations.LoadFromXML(xml);

            String isAsync = XmlUtilits.GetFieldValue(xml, "IsAsync", "false");
            IsAsync = Convert.ToBoolean(isAsync);
        }

        public Boolean IsAsync
        {
            set;
            get;
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Operations);
            return list;
        }
    }
}
