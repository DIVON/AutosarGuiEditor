using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Autosar
{
    public class ArxmlPackage
    {
        public String ShortName
        {
            set;
            get;
        }

        List<ArxmlPackage> packages = new List<ArxmlPackage>();
        public List<ArxmlPackage> Packages
        {
            get 
            {
                return packages;
            }
        }
    }

    public class RootArxmlPackage
    {
        public String FileName {
            set;get;
        }
    }

    
}
