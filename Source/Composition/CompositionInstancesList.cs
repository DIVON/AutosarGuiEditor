using AutosarGuiEditor.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Composition
{
    public class CompositionInstancesList : IGuidList<CompositionInstance>
    {
        private Dictionary<CompositionInstance, CompositionViewport> viewportStore = new Dictionary<CompositionInstance, CompositionViewport>();
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

        /// <summary>
        /// Saves the current viewport state for the given composition.
        /// </summary>
        public void SaveViewport(CompositionInstance composition, double scale, Point offset, bool isInitialized)
        {
            if (composition == null)
                return;

            CompositionViewport viewport;
            if (viewportStore.TryGetValue(composition, out viewport))
            {
                viewport.Scale = scale;
                viewport.Offset = offset;
                viewport.IsViewInitialized = isInitialized;
            }
            else
            {
                viewport = new CompositionViewport(scale, offset, isInitialized);
                viewportStore[composition] = viewport;
            }
        }

        /// <summary>
        /// Gets the saved viewport for the given composition.
        /// Returns null if no viewport has been saved.
        /// </summary>
        public CompositionViewport GetViewport(CompositionInstance composition)
        {
            CompositionViewport viewport;
            if (viewportStore.TryGetValue(composition, out viewport))
            {
                return viewport;
            }
            return null;
        }

        /// <summary>
        /// Marks the viewport as initialized for the given composition.
        /// </summary>
        public void MarkViewportInitialized(CompositionInstance composition)
        {
            CompositionViewport viewport;
            if (viewportStore.TryGetValue(composition, out viewport))
            {
                viewport.IsViewInitialized = true;
            }
            else
            {
                viewportStore[composition] = new CompositionViewport(1.0, new Point(0, 0), true);
            }
        }

        /// <summary>
        /// Removes the viewport storage for the given composition.
        /// </summary>
        public void RemoveViewport(CompositionInstance composition)
        {
            CompositionInstance mainComposition = GetMainComposition();
            if (composition != null && composition != mainComposition)
            {
                CompositionViewport viewport;
                viewportStore.Remove(composition);
            }
        }

        /// <summary>
        /// Clears all saved viewport data.
        /// </summary>
        public void ClearViewports()
        {
            viewportStore.Clear();
        }
    }
}
