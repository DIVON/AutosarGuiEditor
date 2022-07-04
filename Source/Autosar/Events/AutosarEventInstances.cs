﻿using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Autosar.Events
{
    public class AutosarEventInstance:IGUID
    {
        public Guid DefenitionGuid
        {
            set;
            get;
        }

        int startupOrder = 0;
        public int StartupOrder
        {
            set
            {
                startupOrder = value;
            }
            get
            {
                return startupOrder;
            }
        }

        public AutosarEvent Defenition
        {
            get
            {
                return AutosarApplication.GetInstance().FindEventDefenition(DefenitionGuid);
            }
        }

        public Double PeriodMs
        {
            get
            {
                if (this.Defenition != null)
                {
                    if (Defenition is TimingEvent)
                    {
                        return (Defenition as TimingEvent).PeriodMs;
                    }
                }
                return -1;
            }
        }

        public String PeriodMsString
        {
            get
            {
                if (this.Defenition != null)
                {
                    if (Defenition is TimingEvent)
                    {
                        return (Defenition as TimingEvent).PeriodMs.ToString();
                    }
                }

                return "-1";
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            int order = 0;
            if (int.TryParse(xml.Element("StartupOrder").Value, out order) == true)
            {
                startupOrder = order;
            }

            DefenitionGuid = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "DefenitionGuid", ""), false);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("RunnableInstance");
            base.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("StartupOrder", startupOrder.ToString()));
            xmlElement.Add(new XElement("DefenitionGuid", DefenitionGuid.ToString("B")));
            root.Add(xmlElement);
        }
    }




    public class AutosarEventInstancesList : IGuidList<AutosarEventInstance>
    {
        public AutosarEventInstance FindEvent(Guid DefenitionGuid)
        {
            foreach (AutosarEventInstance timingEventInstance in this)
            {
                if (timingEventInstance.DefenitionGuid.Equals(DefenitionGuid) && !DefenitionGuid.Equals(Guid.Empty))
                {
                    return timingEventInstance;
                }
            }
            return null;
        }


        public void LoadGuidsFromXML(XElement xml)
        {
            XElement xmlList = xml.Element("Guids");
            if (xmlList != null)
            {
                IEnumerable<XElement> elementsList = xmlList.Elements();
                foreach (XElement element in elementsList)
                {
                    XElement instanceGUID = element.Element("InstanceGuid");
                    if (instanceGUID != null)
                    {
                        Guid resultGuid;
                        if (Guid.TryParse(instanceGUID.Value, out resultGuid) == true)
                        {
                            if (!resultGuid.Equals(Guid.Empty))
                            {
                                AutosarEventInstance runnableInstance = AutosarApplication.GetInstance().GetEventInstance(resultGuid);
                                if (runnableInstance != null)
                                {
                                    this.Add(runnableInstance);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void WriteGuidsToXml(XElement root)
        {
            XElement Guids = new XElement("Guids");
            foreach (AutosarEventInstance eventInstance in this)
            {
                XElement xmlElement = new XElement("EventInstanceGuid");
                xmlElement.Add(new XElement("InstanceGuid", eventInstance.GUID.ToString("B")));
                Guids.Add(xmlElement);
            }
            root.Add(Guids);
        }

        public void SortByRunnablesIndex()
        {
            Sort(delegate(AutosarEventInstance x, AutosarEventInstance y)
            {
                object startupOrderX = GetProperty(x, "StartupOrder");
                object startupOrderY = GetProperty(y, "StartupOrder");
                int orderX = (int)startupOrderX;
                int orderY = (int)startupOrderY;

                return orderX.CompareTo(orderY);

                throw new Exception("Sort exception! Properties not exists!");
            });
        }

        public override void DoSort()
        {

        }

        public void SortByName()
        {
            base.SortByField("FullName");
        }
    }
}
