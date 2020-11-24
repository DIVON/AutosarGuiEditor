using AutosarGuiEditor.Source.Painters.Components.Runables;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Autosar.OsTasks
{
    public class OsTask: IGUID
    {
        public OsTask()
        {
            StackSizeInBytes = defaultStackSize;
        }

        public RunnableInstancesList Runnables = new RunnableInstancesList();

        public Double PeriodMs
        {
            set;
            get;
        }

        public String PeriodMsString
        {
            get
            {
                return PeriodMs.ToString();
            }
            set
            {
                Double newPeriod;
                if (Double.TryParse(value, out newPeriod))
                {
                    PeriodMs = newPeriod;
                }
            }
        }

        int priority = 0;
        public int Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Runnables.LoadGuidsFromXML(xml);
            XElement msElement = xml.Element("PeriodMs");
            if (msElement == null)
            {
                PeriodMs = 0.250;
            }
            else
            {
                Double newPeriod;
                String replasedString = msElement.Value.Replace('.', ',');
                if (Double.TryParse(replasedString, out newPeriod))
                {
                    PeriodMs = newPeriod;
                }
            }

            XElement priorityElement = xml.Element("Priority");
            if (priorityElement == null)
            {
                priority = 0;
            }
            else
            {
                int val;
                if (int.TryParse(priorityElement.Value, out val) == true)
                {
                    priority = val;
                }
            }

            XElement stackSizeElement = xml.Element("StackSizeInBytes");
            if (stackSizeElement == null)
            {
                StackSizeInBytes = defaultStackSize;
            }
            else
            {
                int val;
                if (int.TryParse(stackSizeElement.Value, out val) == true)
                {
                    StackSizeInBytes = val;
                }
            }

            Runnables.SortByRunnablesIndex();
        }

        private int defaultStackSize = 128;

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("OsTask");
            base.WriteToXML(xmlElement);

            xmlElement.Add(new XElement("PeriodMs", PeriodMs));
            xmlElement.Add(new XElement("Priority", Priority));
            xmlElement.Add(new XElement("StackSizeInBytes", StackSizeInBytes));

            Runnables.WriteGuidsToXml(xmlElement);
            root.Add(xmlElement);
        }

        public int Number
        {
            set;
            get;
        }

        public override String ToString()
        {
            return this.Name;
        }

        public int StackSizeInBytes
        {
            set;
            get;
        }
    }
}
