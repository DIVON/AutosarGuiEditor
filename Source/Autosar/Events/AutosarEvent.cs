using AutosarGuiEditor.Source.AutosarInterfaces;
using AutosarGuiEditor.Source.AutosarInterfaces.ClientServer;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
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
            set
            {

            }
        }
    }

    public class OneTimeEvent : AutosarEvent
    {
        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            RunnableGuid = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "RunnableGuid", ""), false);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("OneTimeEvent");
            base.WriteToXML(xmlElement);
            if (RunnableGuid != null)
            {
                xmlElement.Add(new XElement("RunnableGuid", RunnableGuid.ToString("B")));
            }
           
            root.Add(xmlElement);
        }
    }

    public class OneTimeEventList : IGuidList<OneTimeEvent>
    {

    }

    public class ClientServerEvent : AutosarEvent
    {
        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            RunnableGuid        = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "RunnableGuid", ""),          false);
            SourcePortGuid      = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "SourcePortGuid", ""),        false);
            SourceOperationGuid = GuidUtils.GetGuid(XmlUtilits.GetFieldValue(xml, "SourceOperationGuid", ""),   false);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("ServerCallEvent");
            base.WriteToXML(xmlElement);
            if (RunnableGuid != null)
            {
                xmlElement.Add(new XElement("RunnableGuid", RunnableGuid.ToString("B")));
            }
            if (SourcePortGuid != null)
            {
                xmlElement.Add(new XElement("SourcePortGuid", SourcePortGuid.ToString("B")));
            }
            if (SourceOperationGuid != null)
            {
                xmlElement.Add(new XElement("SourceOperationGuid", SourceOperationGuid.ToString("B")));
            }
            root.Add(xmlElement);
        }

        private Guid SourcePortGuid
        {
            set;
            get;
        }

        PortDefenition sourcePort = null;
        public PortDefenition SourcePort
        {
            set
            {
                sourcePort = value;
                SourcePortGuid = value.GUID;
            }
            get
            {
                if (sourcePort == null)
                {
                    sourcePort = AutosarApplication.GetInstance().FindPortDefenition(SourcePortGuid);                    
                }

                return sourcePort;
            }
        }

        public String EventSource
        {
            set
            {

            }
            get
            {
                if ((SourcePort != null) && (SourceOperation != null))
                {
                    return SourcePort.Name + ": " + sourceOperation.Name;
                }
                else
                {
                    return "ERROR";
                }
            }
        }

        private Guid SourceOperationGuid
        {
            set;
            get;
        }

        ClientServerOperation sourceOperation = null;
        public ClientServerOperation SourceOperation
        {
            set
            {
                SourceOperationGuid = value.GUID;
                sourceOperation = value;
            }
            get
            {
                /* Find source once */
                if (sourceOperation == null)
                {
                    ClientServerInterface csInterface = SourcePort.InterfaceDatatype as ClientServerInterface;
                    if (csInterface != null)
                    {
                        sourceOperation = csInterface.Operations.Find(op => op.GUID == SourceOperationGuid);
                    }
                }
                return sourceOperation;
            }
        }
    }

    public class ClientServerEventList : IGuidList<ClientServerEvent>
    {

    }

    public class SyncClientServerEventList : ClientServerEventList
    {

    }

    public class AsyncClientServerEventList : ClientServerEventList
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
