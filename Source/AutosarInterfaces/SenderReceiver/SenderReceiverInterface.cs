using AutosarGuiEditor.Source.DataTypes;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver
{
    public class SenderReceiverInterface : IGUID
    {
        public SenderReceiverInterfaceFieldsList Fields = new SenderReceiverInterfaceFieldsList();

        public SenderReceiverInterface()
        {
            
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("Interface");
            base.WriteToXML(xmldatatype);
            Fields.WriteToXML(xmldatatype);
            xml.Add(xmldatatype);
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
