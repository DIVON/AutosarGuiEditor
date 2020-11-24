using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.DataTypes.BaseDataType
{
    public class BaseDataTypesList : IGuidList<BaseDataType>
    {
        public void CheckBaseDataTypes()
        {
            if (FindObject("uint8") == null)
            {
                this.Add(CreateBaseDataType("uint8", "unsigned char"));
            }

            if (FindObject("int8") == null)
            {
                this.Add(CreateBaseDataType("int8", "signed char"));
            }

            if (FindObject("uint16") == null)
            {
                this.Add(CreateBaseDataType("uint16", "unsigned short"));
            }

            if (FindObject("int16") == null)
            {
                this.Add(CreateBaseDataType("int16", "signed short"));
            }


            if (FindObject("uint32") == null)
            {
                this.Add(CreateBaseDataType("uint32", "unsigned int"));
            }

            if (FindObject("int32") == null)
            {
                this.Add(CreateBaseDataType("int32", "signed int"));
            }

            if (FindObject("float32") == null)
            {
                this.Add(CreateBaseDataType("float32", "float"));
            }

            if (FindObject("boolean") == null)
            {
                this.Add(CreateBaseDataType("boolean", "unsigned char"));
            }
        }

        public BaseDataType uint8 { get { return this.FindObject("uint8"); } }
        public BaseDataType int8 { get { return this.FindObject("int8"); } }
        public BaseDataType uint16 { get { return this.FindObject("uint16"); } }
        public BaseDataType int16 { get { return this.FindObject("int16"); } }
        public BaseDataType int32 { get { return this.FindObject("int32"); } }
        public BaseDataType uint32 { get { return this.FindObject("uint32"); } }
        public BaseDataType float32 { get { return this.FindObject("float32"); } }
        public BaseDataType boolean { get { return this.FindObject("boolean"); } }

        public BaseDataType CreateBaseDataType(String Name, String SystemName)
        {
            BaseDataType newDataType = new BaseDataType();
            newDataType.Name = Name;
            newDataType.SystemName = SystemName;
            newDataType.GUID = Guid.NewGuid();
            return newDataType;
        }

        public override String GetName()
        {
            return "Base data types";
        }
    }
}
