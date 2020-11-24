using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Interfaces
{
    public interface IAutosarTreeList
    {
        String GetName();
        List<IGUID> GetItems();
        Boolean CreateRoot();


    }
}
