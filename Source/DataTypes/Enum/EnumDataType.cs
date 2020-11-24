using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.DataTypes.Enum
{
    public enum LimitType
    {
        ltLowerLimit,
        ltUpperLimit        
    }

    public class EnumDataType : PlainDataType
    {
        public EnumFieldList Fields = new EnumFieldList();

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Fields.LoadFromXML(xml);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("EnumDataType");
            base.WriteToXML(xmlElement);
            Fields.WriteToXML(xmlElement);
            root.Add(xmlElement);
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Fields);
            return list;
        }

        public int GetLimit(LimitType limitType)
        {
            int limit = 0;
            if (Fields.Count > 0)
            {
                limit = Fields[0].Value;

                for (int i = 1; i < Fields.Count; i++)
                {
                    if (limitType == LimitType.ltLowerLimit)
                    {
                        if (Fields[i].Value < limit)
                        {
                            limit = Fields[i].Value;
                        }
                    }
                    else
                    {
                        if (Fields[i].Value > limit)
                        {
                            limit = Fields[i].Value;
                        }
                    }
                }
            }
            return limit;
        }


    }
}
