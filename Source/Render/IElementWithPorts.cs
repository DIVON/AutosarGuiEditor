using AutosarGuiEditor.Source.Painters.Boundaries;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Render
{
    public class IElementWithPorts : ResizableRectangleElement
    {
        PortPaintersList ports = new PortPaintersList();
        public PortPaintersList Ports
        {
            get
            {
                return ports;
            }
        }

        Boolean portsSticked = true;
        Boolean PortsSticked
        {
            set
            {
                portsSticked = value;
            }
            get
            {
                return portsSticked;
            }
        }

        public IElementWithPorts()
        {
            this.OnMoveTopLeftAnchor += ComponentInstance_OnMoveTopLeftAnchor;
            this.OnMoveTopRightAnchor += ComponentInstance_OnMoveTopRightAnchor;
            this.OnMoveBottomLeftAnchor += ComponentInstance_OnMoveBottomLeftAnchor;
            this.OnMoveBottomRightAnchor += ComponentInstance_OnMoveBottomRightAnchor;
        }

        void ComponentInstance_OnMoveBottomRightAnchor(AnchorPoint anchorPoint, Point translate)
        {
            if (portsSticked)
            {
                foreach (PortPainter portPainter in this.Ports)
                {
                    if (portPainter.ConnectionPortLocation == RectangleSide.Bottom)
                    {
                        portPainter.Painter.SetYCenter(anchorPoint.Position.Y);
                    }
                    if (portPainter.ConnectionPortLocation == RectangleSide.Right)
                    {                        
                        portPainter.Painter.SetXCenter(anchorPoint.Position.X);
                    }
                }
            }
        }

        void ComponentInstance_OnMoveBottomLeftAnchor(AnchorPoint anchorPoint, Point translate)
        {
            if (portsSticked)
            {
                foreach (PortPainter portPainter in this.Ports)
                {
                    if (portPainter.ConnectionPortLocation == RectangleSide.Bottom)
                    {
                        portPainter.Painter.SetYCenter(anchorPoint.Position.Y);
                    }
                    if (portPainter.ConnectionPortLocation == RectangleSide.Left)
                    {
                        portPainter.Painter.SetXCenter(anchorPoint.Position.X);
                    }
                }
            }
        }

        void ComponentInstance_OnMoveTopRightAnchor(AnchorPoint anchorPoint, Point translate)
        {
            if (portsSticked)
            {
                foreach (PortPainter portPainter in this.Ports)
                {
                    if (portPainter.ConnectionPortLocation == RectangleSide.Top)
                    {
                        portPainter.Painter.SetYCenter(anchorPoint.Position.Y);
                    }
                    if (portPainter.ConnectionPortLocation == RectangleSide.Right)
                    {
                        portPainter.Painter.SetXCenter(anchorPoint.Position.X);
                    }
                }
            }
        }

        void ComponentInstance_OnMoveTopLeftAnchor(AnchorPoint anchorPoint, Point translate)
        {
            if (portsSticked)
            {
                foreach (PortPainter portPainter in this.Ports)
                {
                    if (portPainter.ConnectionPortLocation == RectangleSide.Top)
                    {
                        portPainter.Painter.SetYCenter(anchorPoint.Position.Y);
                    }
                    if (portPainter.ConnectionPortLocation == RectangleSide.Left)
                    {
                        portPainter.Painter.SetXCenter(anchorPoint.Position.X);
                    }
                }
            }
        }

        public override void Render(RenderContext context)
        {
            base.Render(context);

            /* Render ports */
            Ports.RenderPorts(context);
        }

        public override void Translate(double X, double Y)
        {
            base.Translate(X, Y);
            Ports.Translate(X, Y);
        }


        public override void Unselect()
        {
            base.Unselect();
            foreach (PortPainter portPainter in this.Ports)
            {
                portPainter.Selected = false;
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Ports.LoadFromXML(xml);         
        }

        public override void WriteToXML(XElement root)
        {
            base.WriteToXML(root);
            Ports.WriteToXML(root);
        }

        public PortPainter GetPort(PortDefenition portDef)
        {
            foreach (PortPainter portPainter in Ports)
            {
                if (portPainter.PortDefenition.Equals(portDef))
                {
                    return portPainter;
                }
            }
            return null;
        }

        public override bool IsClicked(System.Windows.Point point, out Object clickedObject)
        {
            /* Check click on ports */
            clickedObject = null;

            Unselect();

            bool clicked = Ports.IsClicked(point, out clickedObject);
            if (clicked == true)
            {
                Unselect();
                return true;
            }

            return base.IsClicked(point, out clickedObject);
        }

        public override Boundary GetBoundary(RenderContext context)
        {
            Boundary bound = base.GetBoundary(context);
            foreach (PortPainter portPainter in this.Ports)
            {
                bound += portPainter.GetBoundary(context);
            }
            return bound;
        }
    }
}
