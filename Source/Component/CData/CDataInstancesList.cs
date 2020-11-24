using AutosarGuiEditor.Source.Component.CData;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters.Components.CData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Painters.Components.PerInstance
{
    public class CDataInstancesList : IGuidList<CDataInstance>
    {

        public CDataInstance GetCData(CDataDefenition pimDefenition)
        {
            return GetCData(pimDefenition.GUID);
        }

        public CDataInstance GetCData(Guid DefenitionGuid)
        {
            foreach (CDataInstance instance in this)
            {
                if (instance.Defenition.GUID.Equals(DefenitionGuid))
                {
                    return instance;
                }
            }
            return null;
        }

        public CDataDefaultValuesList CollectAllCDataValuesForGrid()
        {
            CDataDefaultValuesList defsList = new CDataDefaultValuesList();
            foreach(CDataInstance instance in this)
            {
                defsList.AddRange(instance.DefaultValues);
            }
            return defsList;
        }
    }
}
