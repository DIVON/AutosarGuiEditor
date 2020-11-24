using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters.Components.CData;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Fabrics
{
    public class CDataFabric
    {
        static CDataFabric instance = null;

        static public CDataFabric GetInstance()
        {
            if (instance == null)
            {
                instance = new CDataFabric();
            }
            return instance;
        }

        public CDataInstance CreateCDataInstance(CDataDefenition defenition)
        {
            CDataInstance instance = new CDataInstance();
            instance.DefenitionGuid = defenition.GUID;
            instance.Name = defenition.Name;
            instance.UpdateDefaultValues();
            return instance;
        }

        public CDataDefaultValue CreateCDataDefaultValue()
        {
            CDataDefaultValue defValue = new CDataDefaultValue();
            defValue.Name = "";
            return defValue;
        }
    }
}
