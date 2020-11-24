using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Composition
{
    public class CompositionInstancesList : IGuidList<CompositionInstance>
    {
        public const String MainCompositionName = "Main";
        public void ClearCompositions()
        {
            for(int i = this.Count - 1; i >= 0; i--)
            {
                this.RemoveAt(i);
            }
        }

        public override String GetName()
        {
            return "Composition instances";
        }

        public CompositionInstance GetMainComposition()
        {
            return this.FindObject(MainCompositionName);
        }

        public override void LoadFromXML(XElement xmlApp, String NameId = "")
        {
            base.LoadFromXML(xmlApp, NameId);
            /*
            CompositionInstance mainComposition = GetMainComposition();
            if (mainComposition == null)
            {
                mainComposition = new CompositionInstance();
                mainComposition.Name = MainCompositionName;
                this.Add(mainComposition);
                
            }
            */

            CompositionInstance mainComposition = GetMainComposition();
            AutosarApplication.GetInstance().ActiveComposition = mainComposition;
        }
    }
}
