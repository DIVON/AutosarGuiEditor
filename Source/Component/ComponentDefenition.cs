using AutosarGuiEditor.Source.Autosar.Events;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
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
            onetimeEvents.LoadFromXML(xml);
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
            onetimeEvents.WriteToXML(xmlElement);
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
            list.Add(onetimeEvents);
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


        SyncClientServerEventList syncClientServerEvents = new SyncClientServerEventList();
        public SyncClientServerEventList SyncClientServerEvents
        {
            get
            {
                return syncClientServerEvents;
            }
        }

        AsyncClientServerEventList asyncClientServerEvents = new AsyncClientServerEventList();
        public AsyncClientServerEventList AsyncClientServerEvents
        {
            get
            {
                return asyncClientServerEvents;
            }
        }


        OneTimeEventList onetimeEvents = new OneTimeEventList();
        public OneTimeEventList OneTimeEvents
        {
            get
            {
                return onetimeEvents;
            }
        }

        public AutosarEventsList GetEventsWithTheRunnable(RunnableDefenition runnableDef)
        {
            AutosarEventsList list = new AutosarEventsList();

            foreach (AutosarEvent aEvent in this.asyncClientServerEvents)
            {
                if (aEvent.Runnable == runnableDef)
                {
                    list.Add(aEvent);
                }
            }
            foreach (AutosarEvent aEvent in this.syncClientServerEvents)
            {
                if (aEvent.Runnable == runnableDef)
                {
                    list.Add(aEvent);
                }
            }
            foreach (AutosarEvent aEvent in this.timingEvents)
            {
                if (aEvent.Runnable == runnableDef)
                {
                    list.Add(aEvent);
                }
            }
            foreach (AutosarEvent aEvent in this.onetimeEvents)
            {
                if (aEvent.Runnable == runnableDef)
                {
                    list.Add(aEvent);
                }
            }
            return list;
        }

        public ClientServerEvent GetEventsWithServerOperation(ClientServerOperation csOperation)
        {
            foreach (AutosarEvent aEvent in this.asyncClientServerEvents)
            {
                ClientServerEvent csEvent = aEvent as ClientServerEvent;
                if (csEvent.SourceOperation == csOperation)
                {
                    return csEvent;
                }
            }
            foreach (AutosarEvent aEvent in this.syncClientServerEvents)
            {
                ClientServerEvent csEvent = aEvent as ClientServerEvent;
                if (csEvent.SourceOperation == csOperation)
                {
                    return csEvent;
                }
            }
            return null;
        }

        public bool IsClientServerEventSync(ClientServerEvent csEvent)
        {
            foreach (ClientServerEvent eventInList in this.asyncClientServerEvents)
            {
                if (csEvent == eventInList)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsComponentGenerable()
        {
            bool generable = Ports.Count > 0 | CDataDefenitions.Count > 0 | PerInstanceMemoryList.Count > 0;
            return generable;
        }
    }
}
