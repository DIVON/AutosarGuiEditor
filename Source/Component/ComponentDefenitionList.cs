using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Component
{
    public class ComponentDefenitionList : IGuidList<ApplicationSwComponentType>
    {
        public override String GetName()
        {
            return "Component defenitions";
        }
        
        public override void LoadFromXML(System.Xml.Linq.XElement xmlApp, string NameId = "")
        {
            base.LoadFromXML(xmlApp, NameId);
            DoSort();
        }
    }
}
