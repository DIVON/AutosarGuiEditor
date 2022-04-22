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

            root.Add(xmlElement);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Runnables);
            list.Add(Ports);
            list.Add(PerInstanceMemoryList);
            list.Add(CDataDefenitions);
            return list;
        }

       
    }
}
