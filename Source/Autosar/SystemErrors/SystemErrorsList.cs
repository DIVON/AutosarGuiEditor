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
            //SortErrorsByID();
        }

        public int ErrorCount(SystemErrorStrictness strictness)
        {
            int count = 0;
            foreach (SystemErrorObject error in this)
            {
                if (error.Strictness == strictness)
                {
                    count++;
                }
            }
            return count;
        }

        //public void SortErrorsByID()
        //{
        //    this.Sort(delegate(SystemErrorObject x, SystemErrorObject y)
        //    {
        //        return x.Value.CompareTo(y.Value);
        //    });
        //}
    }
}
