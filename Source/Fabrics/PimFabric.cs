using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Painters.Components.PerInstance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Fabrics
{
    public class PimFabric
    {
        static PimFabric instance = null;

        static public PimFabric GetInstance()
        {
            if (instance == null)
            {
                instance = new PimFabric();
            }
            return instance;
        }

        public PimInstance CreatePimInstance(PimDefenition pimDefenition)
        {
            PimInstance pimInstance = new PimInstance();
            pimInstance.DefenitionGuid = pimDefenition.GUID;
            pimInstance.Name = pimDefenition.Name;
            pimInstance.UpdateDefaultValues();
            return pimInstance;
        }

        public PimDefaultValue CreatePimDefaultValue()
        {
            PimDefaultValue pimDefValue = new PimDefaultValue();
            pimDefValue.Name = "";
            return pimDefValue;
        }
    }
}
