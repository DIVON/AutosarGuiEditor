using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutosarGuiEditor.Source.Painters.PortsPainters
{
    public class PortPaintersList: IGuidList<PortPainter>
    {
        public bool IsClicked(Point point, out Object clickedObject)
        {
            foreach (PortPainter portPainter in this)
            {
                bool clicked = portPainter.IsClicked(point, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }
            }
            clickedObject = false;
            return false;
        }

        public void RenderPorts(RenderContext context)
        {
            if (renderPorts)
            {
                foreach (PortPainter portPainter in this)
                {
                    portPainter.Render(context);
                }
            }
        }

        public void Translate(double X, double Y)
        {
            foreach (PortPainter port in this)
            {
                port.Painter.TopLeft.X += X;
                port.Painter.TopLeft.Y += Y;

                port.Painter.BottomRight.X += X;
                port.Painter.BottomRight.Y += Y;
            }
        }

        bool renderPorts = true;
        public bool DoRenderPorts
        {
            set
            {
                renderPorts = value;
            }
            get
            {
                return renderPorts;
            }
        }

        public PortPainter FindPortByItsDefenition(PortDefenition portDef)
        {
            foreach (PortPainter portPainter in this)
            {                
                if (portPainter.PortDefenitionGuid.Equals(portDef.GUID))
                {
                    return portPainter;
                }
            }
            return null;
        }

        public override void DoSort()
        {
            /* Do nothing */
        }
    }
}
