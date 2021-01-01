using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.Boundaries;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Composition
{
    public class CompositionInstance:IElementWithPorts
    {
        public ComponentInstancesList ComponentInstances = new ComponentInstancesList();
        public PortsConnectionsList Connections = new PortsConnectionsList();
        public PortDefenitionsList PortsDefenitions = new PortDefenitionsList(); 
        public PortPaintersList InternalPortsInstances = new PortPaintersList();

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);           
            ComponentInstances.LoadFromXML(xml);
            Connections.LoadFromXML(xml);
            PortsDefenitions.LoadFromXML(xml);
            InternalPortsInstances.LoadFromXML(xml, "Internal");
            foreach (PortPainter port in Ports)
            {
                port.IsDelegatePort = false;
            }
            foreach (PortPainter port in InternalPortsInstances)
            {
                port.IsDelegatePort = true;
            }
        }

        public Boundary GetInternalBoundary(RenderContext context)
        {
            Boundary bound = new Boundary();

            CompositionInstance mainComposition = AutosarApplication.GetInstance().Compositions.GetMainComposition();
            if (mainComposition.Equals(this))
            {
                foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                {
                    if (composition != mainComposition)
                    {
                        bound += composition.GetBoundary(context);
                    }
                }
            }

            
            foreach(ComponentInstance component in ComponentInstances)
            {
                bound += component.GetBoundary(context);
            }
            foreach (PortPainter port in InternalPortsInstances)
            {
                bound += port.GetBoundary(context);
            }
            foreach (PortConnection connection in Connections)
            {
                bound += connection.GetBoundary(context);
            }
            return bound;
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("CompositionInstance");
            base.WriteToXML(xmlElement);
            ComponentInstances.WriteToXML(xmlElement);
            Connections.WriteToXML(xmlElement);
            PortsDefenitions.WriteToXML(xmlElement);
            InternalPortsInstances.WriteToXML(xmlElement, "Internal");
            root.Add(xmlElement);
        }

        public void Clear()
        {
            Ports.Clear();
            Connections.Clear();
            ComponentInstances.Clear();
            PortsDefenitions.Clear();
            InternalPortsInstances.Clear();
        }

        public PortPainter GetExternalPortPainter(PortDefenition portDef)
        {
            PortPainter portPainter = base.GetPort(portDef);
            return portPainter;
        }

        public PortPainter GetInternalPortPainter(PortDefenition portDef)
        {
            foreach (PortPainter portPainter in InternalPortsInstances)
            {
                if (portPainter.PortDefenition.Equals(portDef))
                {
                    return portPainter;
                }
            }
            return null;           
        }

        public override void Render(RenderContext context)
        {
            base.Render(context);

            PortableFontDesc typefaceFont = AutosarApplication.GetInstance().ComponentNameFont;
            /* Draw component name*/
            GlyphFont glyphFont = LetterGlyphTool.GetFont(typefaceFont);
            int width = glyphFont.GetTextWidth(Name);
            int textHeight = glyphFont.GetTextHeight(Name);

            Point textCoord = Painter.Center;
            textCoord.Y = Painter.Top;
            textCoord = context.GetImageCoordinate(textCoord);
            textCoord.Y += textHeight;
            textCoord.X -= (double)width / 2;
            context.Bitmap.DrawString((int)textCoord.X, (int)textCoord.Y, Colors.Black, AutosarApplication.GetInstance().ComponentNameFont, Name);
        }

        public PortsConnectionsList GetConnections()
        {
            PortsConnectionsList list = new PortsConnectionsList();
            
            /* Check in component instances in main composition */
            CompositionInstance mainComposition = AutosarApplication.GetInstance().Compositions.GetMainComposition();
            if (mainComposition.Equals(this))
            {

            }
            else
            {
                foreach(PortConnection connection in mainComposition.Connections)
                {
                    foreach(PortPainter port in this.Ports )
                    {
                        if (connection.Port1.GUID.Equals(port.GUID) || connection.Port2.GUID.Equals(port.GUID))
                        {
                            list.Add(connection);                            
                        }
                    }
                }
            }
            return list;
        }

        public void UpdateConnections()
        {
            foreach (PortConnection connection in Connections)
            {
                connection.UpdateLines();
            }
        }


        public void RenderCompositionEntrails(RenderContext renderContext)
        {            
            foreach(ComponentInstance componentInstance in ComponentInstances)
            {
                componentInstance.Render(renderContext);
            }

            foreach(PortConnection connection in Connections)
            {
                connection.Render(renderContext);
            }

            InternalPortsInstances.RenderPorts(renderContext);
        }



        public virtual bool GetClickedObject(Point sceneCoordinates, out Object clickedObject)
        {
            if (AutosarApplication.GetInstance().ActiveComposition.Equals(this))
            {
                bool clicked = InternalPortsInstances.IsClicked(sceneCoordinates, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }

                clicked = ComponentInstances.IsClicked(sceneCoordinates, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }

                clicked = Connections.IsClicked(sceneCoordinates, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }
                                
            }
            else
            {
                bool clicked = base.IsClicked(sceneCoordinates, out clickedObject);
                if (clicked == true)
                {
                    return true;
                }
            }
            return false;
        }

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(ComponentInstances);
            list.Add(Connections);
            list.Add(PortsDefenitions);
            return list;
        }

        public void AddPort(PortDefenition newPortDefenition)
        {            
            PortsDefenitions.Add(newPortDefenition);

            PortPainter externalPort = new PortPainter(newPortDefenition.GUID, Painter.Left - PortPainter.DefaultWidth / 2.0, Painter.Top + 25, newPortDefenition.Name);
            externalPort.IsDelegatePort = false;
            Ports.Add(externalPort);

            PortPainter internalPort = new PortPainter(newPortDefenition.GUID, 0, 0, newPortDefenition.Name);
            internalPort.IsDelegatePort = true;
            InternalPortsInstances.Add(internalPort);

            Ports.DoSort();
            InternalPortsInstances.DoSort();
        }
    }
}
