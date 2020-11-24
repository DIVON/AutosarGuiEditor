using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Painters.Components.Runables
{
    public class RunnableInstancesList : IGuidList<PeriodicRunnableInstance>
    {
        public PeriodicRunnableInstance FindRunnable(Guid DefenitionGuid)
        {
            foreach(PeriodicRunnableInstance runnableInstance in this)
            {
                if (runnableInstance.DefenitionGuid.Equals(DefenitionGuid) && !DefenitionGuid.Equals(Guid.Empty))
                {
                    return runnableInstance;
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
                                PeriodicRunnableInstance runnableInstance = AutosarApplication.GetInstance().GetRunnableInstance(resultGuid);
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
            foreach (PeriodicRunnableInstance runnableInstance in this)
            {
                XElement xmlElement = new XElement("RunnableInstanceGuid");
                xmlElement.Add(new XElement("InstanceGuid", runnableInstance.GUID.ToString("B")));
                Guids.Add(xmlElement);
            }
            root.Add(Guids);
        }

        public void SortByRunnablesIndex()
        {
            Sort(delegate(PeriodicRunnableInstance x, PeriodicRunnableInstance y)
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
