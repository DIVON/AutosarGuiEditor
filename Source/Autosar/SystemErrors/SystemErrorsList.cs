using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Autosar.SystemErrors
{
    public class SystemErrorsList: IGuidList<SystemErrorObject>
    {
        const string XmlListName = "SystemErrorsList";

        public override String GetName()
        {
            return "System errors";
        }

        public override void LoadFromXML(XElement xmlApp, string NameId = "")
        {
            base.LoadFromXML(xmlApp, NameId);
            SortErrorsByID();
        }

        public void SortErrorsByID()
        {
            this.Sort(delegate(SystemErrorObject x, SystemErrorObject y)
            {
                return x.Value.CompareTo(y.Value);
            });
        }
    }
}
