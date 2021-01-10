using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Painters.Components.Runables
{
    public class PeriodicRunnableInstance:IGUID
    {
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

        public Guid DefenitionGuid
        {
            set;
            get;
        }

        public PeriodicRunnableDefenition Defenition
        {
            get 
            {
                return AutosarApplication.GetInstance().FindRunnableDefenition(DefenitionGuid);
            }
        }
        
        public String FullName
        {
            get
            {
                ComponentInstance componentInstance = AutosarApplication.GetInstance().FindComponentInstanceByRunnableGuid(this.GUID);
                String fullName = componentInstance.Name + "_ru" + Defenition.Name;
                return fullName;
            }
        }

        public Double PeriodMs
        {
            get
            {
                return Defenition.PeriodMs;
            }
        }

        public String PeriodMsString
        {
            get
            {
                return Defenition.PeriodMs.ToString();
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
}
