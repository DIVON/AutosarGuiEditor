using AutosarGuiEditor.Source.Component;
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
    public abstract class AutosarEvent:IGUID
    {
        protected Guid RunnableGuid;

        RunnableDefenition runnable;

        public RunnableDefenition Runnable
        {
            set
            {
                runnable = value;
                RunnableGuid = value.GUID;
            }
            get
            {
                return runnable;
            }
        }

        public String RunnableName
        {
            get
            {
                if ((RunnableGuid != null) && (runnable == null))
                {
                    runnable = AutosarApplication.GetInstance().FindRunnableDefenition(RunnableGuid);
                }

                String runnableName = "ERROR";
                if (runnable != null)
                {
                    runnableName = runnable.Name;
                }
                return runnableName;
            }
        }
    }




    public class ServerCallEvent : AutosarEvent
    {
        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            RunnableGuid = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "RunnableGuid", ""), false);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("ServerCallEvent");
            base.WriteToXML(xmlElement);
            if (Runnable != null)
            {
                xmlElement.Add(new XElement("RunnableGuid", RunnableGuid.ToString("B")));
            }
            root.Add(xmlElement);
        }
    }

    public class ServerCallEventList : IGuidList<ServerCallEvent>
    {

    }




    public class TimingEvent : AutosarEvent
    {
        public TimingEvent()
        {
            Name = "etiXXXEvent";
        }
        public double PeriodMs
        {
            set;
            get;
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            RunnableGuid = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "RunnableGuid", ""), false);
            String periodStr = XmlUtilits.GetFieldValue(xml, "PeriodMs", "-1");

            double period;
            if (double.TryParse(periodStr, out period))
            {
                PeriodMs = period;
            }
            else
            {
                PeriodMs = -1;
            }
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("TimingEvent");
            base.WriteToXML(xmlElement);
            if (Runnable != null)
            {
                xmlElement.Add(new XElement("RunnableGuid", Runnable.GUID.ToString("B")));
            }
            xmlElement.Add(new XElement("PeriodMs", PeriodMs.ToString()));

            root.Add(xmlElement);
        }
    }

    public class TimingEventList : IGuidList<TimingEvent>
    {

    }
}
