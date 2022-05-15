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
            QueueSize = 0;
            IsQueued = false;
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("Interface");
            base.WriteToXML(xmldatatype);
            Fields.WriteToXML(xmldatatype);

            if (IsQueued)
            {
                xmldatatype.Add(new XElement("IsQueued", IsQueued.ToString()));
                xmldatatype.Add(new XElement("QueueSize", QueueSize.ToString()));
            }
            xml.Add(xmldatatype);
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Fields.LoadFromXML(xml);
            IsQueued = XmlUtilits.GetBooleanValue(xml, "IsQueued", false);
            QueueSize = XmlUtilits.GetIntegerValue(xml, "QueueSize", 0);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Fields);
            return list;
        }

        public Boolean IsQueued
        {
            set;
            get;
        }

        public int QueueSize
        {
            set;
            get;
        }
       
    }  
}
