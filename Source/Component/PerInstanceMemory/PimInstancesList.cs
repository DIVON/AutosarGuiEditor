using AutosarGuiEditor.Source.Component.PerInstanceMemory;
using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Painters.Components.PerInstance
{
    public class PimInstancesList : IGuidList<PimInstance>
    {

        public PimInstance GetPim(PimDefenition pimDefenition)
        {
            return GetPim(pimDefenition.GUID);
        }

        public PimInstance GetPim(Guid DefenitionGuid)
        {
            foreach(PimInstance pimInst in this)
            {
                if (pimInst.Defenition.GUID.Equals(DefenitionGuid))
                {
                    return pimInst;
                }
            }
            return null;
        }

        public PimDefaultValuesList CollectAllPimValuesForGrid()
        {
            PimDefaultValuesList defsList = new PimDefaultValuesList();
            foreach(PimInstance pim in this)
            {
                defsList.AddRange(pim.DefaultValues);
            }
            return defsList;
        }
    }
}
