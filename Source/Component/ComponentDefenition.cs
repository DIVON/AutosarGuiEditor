using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Component
{
    public class ApplicationSwComponentType : IGUID
    {
        public String ShortName
        {
            set;
            get;
        }

        public PortDefenitionsList Ports = new PortDefenitionsList();

        public Boolean MultipleInstantiation = false;

        public RunnableDefenitionsList Runnables = new RunnableDefenitionsList();
        
        public PerInstanceMemoryDefenitionList PerInstanceMemoryList = new PerInstanceMemoryDefenitionList();
        public CDataDefenitionList CDataDefenitions = new CDataDefenitionList();
        
        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Ports.LoadFromXML(xml);
            Runnables.LoadFromXML(xml);
            PerInstanceMemoryList.LoadFromXML(xml);
            CDataDefenitions.LoadFromXML(xml);
            String portDefString = XmlUtilits.GetFieldValue(xml, "MultipleInstantiation", "false");
            
            if (!Boolean.TryParse(portDefString, out MultipleInstantiation))
            {
                MultipleInstantiation = false; 
            }
            syncClientServerEvents.LoadFromXML(xml);
            asyncClientServerEvents.LoadFromXML(xml);
            timingEvents.LoadFromXML(xml);
        }


        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("ComponentDefenition");
            base.WriteToXML(xmlElement);
            Ports.WriteToXML(xmlElement);
            Runnables.WriteToXML(xmlElement);
            PerInstanceMemoryList.WriteToXML(xmlElement);
            CDataDefenitions.WriteToXML(xmlElement);
            XElement multInstantiation = new XElement("MultipleInstantiation", MultipleInstantiation.ToString());
            xmlElement.Add(multInstantiation);
            syncClientServerEvents.WriteToXML(xmlElement);
            asyncClientServerEvents.WriteToXML(xmlElement);
            timingEvents.WriteToXML(xmlElement);

            root.Add(xmlElement);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Runnables);
            list.Add(Ports);
            list.Add(PerInstanceMemoryList);
            list.Add(CDataDefenitions);
            list.Add(timingEvents);
            list.Add(syncClientServerEvents);
            list.Add(asyncClientServerEvents);
            return list;
        }

        TimingEventList timingEvents = new TimingEventList();

        public TimingEventList TimingEvents
        {
            get
            {
                return timingEvents;
            }
        }


        ClientServerEventList syncClientServerEvents = new ClientServerEventList();
        public ClientServerEventList SyncClientServerEvents
        {
            get
            {
                return syncClientServerEvents;
            }
        }

        ClientServerEventList asyncClientServerEvents = new ClientServerEventList();
        public ClientServerEventList AsyncClientServerEvents
        {
            get
            {
                return asyncClientServerEvents;
            }
        }
    }
}
