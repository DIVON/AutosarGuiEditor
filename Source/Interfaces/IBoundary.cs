using AutosarGuiEditor.Source.Painters.Boundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Interfaces
{
    interface IBoundary
    {
        Boundary GetBoundary(RenderContext context);
    }
}
