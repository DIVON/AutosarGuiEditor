using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Autosar.SystemErrors
{
    public enum SystemErrorStrictness
    {
        ImmediateSafeState,
        NoRestriction
    }

    public class SystemErrorStrictnessList : ObservableCollection<SystemErrorStrictness>
    {

    }

    public class SystemErrorObject:IGUID
    {
        public SystemErrorObject()
        {
            //Value = 0;
            Name = "ERROR";
            Strictness = SystemErrorStrictness.ImmediateSafeState;
        }

        //public UInt32 Value
        //{
        //    set;
        //    get;
        //}

        public SystemErrorStrictness Strictness
        {
            set;
            get;
        }

        public int StrictnessInt
        {
            get
            {
                return Convert.ToInt32(Strictness);
            }
            set
            {
                Strictness = (SystemErrorStrictness)value;
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            //XElement elem = xml.Element("Value");
            //if (elem != null)
            //{
            //    UInt32 val;
            //    if (UInt32.TryParse(elem.Value, out val) == true)
            //    {
            //        Value = val;
            //    }
            //    else
            //    {
            //        Value = 0;
            //    }
            //}

            XElement elem = xml.Element("Strictness");
            if (elem != null)
            {
                int val;
                if (int.TryParse(elem.Value, out val) == true)
                {
                    StrictnessInt = val;
                }
                else
                {
                    StrictnessInt = 0;
                }
            }
        }

        public override void WriteToXML(XElement xml)
        {
            XElement xmldatatype = new XElement("SystemErrorObject");
            base.WriteToXML(xmldatatype);            
            //xmldatatype.Add(new XElement("Value", Value.ToString()));
            xmldatatype.Add(new XElement("Strictness", StrictnessInt.ToString()));
            xml.Add(xmldatatype);
        }
    }
}
