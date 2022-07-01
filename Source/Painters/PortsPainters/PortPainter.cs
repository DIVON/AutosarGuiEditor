///////////////////////////////////////////////////////////
//  PortPainter.cs
//  Implementation of the Class PortPainter
//  Generated by Enterprise Architect
//  Created on:      24-���-2019 20:54:09
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using AutosarGuiEditor.Source.Utility;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Painters.Boundaries;
using AutosarGuiEditor.Source.Render;

namespace AutosarGuiEditor.Source.Painters.PortsPainters
{
    public class PortPainter : IGUID, IClickable, IRender
    {
        public PortDefenition PortDefenition
        {
            get
            {
                return AutosarApplication.GetInstance().FindPortDefenition(PortDefenitionGuid);
            }
        }

        public Guid PortDefenitionGuid;

        private RectanglePainter rectanglePainter = new RectanglePainter();

        public RectanglePainter Painter
        {
            get
            {
                return rectanglePainter;
            }
        }

        TextPainter portNamePainter = new TextPainter();

        public PortPainter()
        {
            //Graphics.FromImage(null);
            //g = new Graphics();
        }

        bool isDelegate = false;
        public Boolean IsDelegatePort
        {
            set
            {
                isDelegate = value;
            }
            get
            {
                return isDelegate;
            }
        }

        public PortPainter(Guid portDefenitionGuid, double Left, double Top, String Name)
        {
            PortDefenitionGuid = portDefenitionGuid;

            rectanglePainter.BackgroundColor = Colors.White;
            rectanglePainter.Width = DefaultWidth;
            rectanglePainter.Height = DefaultHeight;
            rectanglePainter.Left = Left;
            rectanglePainter.Top = Top;

            this.Name = Name;
        }

        public const double DefaultWidth = 20.0;
        public const double DefaultHeight = 20.0;

        public bool Selected
        {
            set;
            get;
        }


        Point cummulatedShift = new Point(0, 0);

        public void Translate(double x, double y)
        {

            Point translate = new Point(0, 0);

            cummulatedShift.X += x;
            cummulatedShift.Y += y;

            bool doMove = false;

            if (Math.Abs(cummulatedShift.X) > AnchorsStep.Step)
            {
                double ceilPart = Math.Round(cummulatedShift.X / 5) * 5;
                rectanglePainter.MoveX(ceilPart);
                rectanglePainter.Left = Math.Round(cummulatedShift.X / 5) * 5;
                cummulatedShift.X -= ceilPart;
                translate.X = ceilPart;
                doMove = true;
            }

            if (Math.Abs(cummulatedShift.Y) > AnchorsStep.Step)
            {
                double ceilPart = Math.Round(cummulatedShift.Y / 5) * 5;
                rectanglePainter.MoveY(ceilPart);
                cummulatedShift.Y -= ceilPart;
                translate.Y = ceilPart;
                doMove = true;
            }
        }

        public void Translate(Point translation)
        {
            Translate(translation.X, translation.Y);
        }

        protected virtual void RenderEntrails(RenderContext context)
        {            
            PortDefenition portDef = AutosarApplication.GetInstance().GetPortDefenition(PortDefenitionGuid);
            if (portDef != null)
            {                
                if (portDef.PortType == PortDefenitions.PortType.Client)
                {
                    if (!isDelegate)
                    {
                        ClientPort.RenderEntrails(context, this.rectanglePainter, ConnectionPortLocation);
                    }
                    else
                    {
                        ClientPort.RenderEntrails(context, this.rectanglePainter, RectangleSide.Right);
                    }
                }                
                else if (portDef.PortType == PortDefenitions.PortType.Server)
                {
                    ServerPort.RenderEntrails(context, this.rectanglePainter);
                }                
                else if (portDef.PortType == PortDefenitions.PortType.Sender)
                {
                    if (!isDelegate)
                    {
                        SenderPort.RenderEntrails(context, this.rectanglePainter, ConnectionPortLocation);
                    }
                    else
                    {
                        SenderPort.RenderEntrails(context, this.rectanglePainter, RectangleSide.Right);
                    }
                }                
                else if (portDef.PortType == PortDefenitions.PortType.Receiver)
                {
                    ReceiverPortPainter.RenderEntrails(context, this.rectanglePainter, ConnectionPortLocation);
                }
            }
            else
            {
                EmptyPort.RenderEntrails(context, this.rectanglePainter);
            }
        }

        public void Render(RenderContext context)
        {
            if (Selected)
            {
                context.DrawFillRectangle(rectanglePainter.TopLeft.X - 5, rectanglePainter.TopLeft.Y - 5, rectanglePainter.BottomRight.X + 5, rectanglePainter.BottomRight.Y, Colors.Green);
                context.DrawFillRectangle(rectanglePainter.TopLeft.X - 5, rectanglePainter.BottomRight.Y, rectanglePainter.BottomRight.X + 5, rectanglePainter.BottomRight.Y + 5, Colors.Green);
                context.DrawFillRectangle(rectanglePainter.TopLeft.X - 5, rectanglePainter.TopLeft.Y - 5, rectanglePainter.TopLeft.X, rectanglePainter.BottomRight.Y + 5, Colors.Green);
                context.DrawFillRectangle(rectanglePainter.BottomRight.X, rectanglePainter.TopLeft.Y - 5, rectanglePainter.BottomRight.X + 5, rectanglePainter.BottomRight.Y + 5, Colors.Green);
            }

            rectanglePainter.Render(context);
            context.DrawFillRectangle(rectanglePainter.TopLeft.X, rectanglePainter.TopLeft.Y, rectanglePainter.BottomRight.X, rectanglePainter.BottomRight.Y, rectanglePainter.BackgroundColor);           

            RenderEntrails(context);

            context.DrawRectangle(rectanglePainter.TopLeft.X, rectanglePainter.TopLeft.Y, rectanglePainter.BottomRight.X, rectanglePainter.BottomRight.Y, Colors.Black);

            /* Draw port name*/
            Point rectangleCenter = rectanglePainter.Center;

            portNamePainter.Text = PortDefenition.Name;
            portNamePainter.Font = AutosarApplication.GetInstance().PortsNamesFont;
            portNamePainter.TextColor = Colors.Black;

            if (isDelegate == false)
            {
                switch (ConnectionPortLocation)
                {
                    case RectangleSide.Left:
                    {
                        portNamePainter.Direction = TextDirection.LeftToRight;
                        portNamePainter.Coordinates.X = rectangleCenter.X + 15.0;
                        portNamePainter.Coordinates.Y = rectangleCenter.Y;
                        break;
                    }
                    case RectangleSide.Right:
                    {
                        portNamePainter.Direction = TextDirection.RightToLeft;
                        portNamePainter.Coordinates.X = rectangleCenter.X - 15.0;
                        portNamePainter.Coordinates.Y = rectangleCenter.Y;
                        break;
                    }
                    case RectangleSide.Top:
                    {
                        portNamePainter.Direction = TextDirection.TopToBottom;
                        portNamePainter.Coordinates.X = rectangleCenter.X;
                        portNamePainter.Coordinates.Y = rectangleCenter.Y + 15.0;
                        break;
                    }
                    case RectangleSide.Bottom:
                    {
                        portNamePainter.Direction = TextDirection.BottomToTop;
                        portNamePainter.Coordinates.X = rectangleCenter.X;
                        portNamePainter.Coordinates.Y = rectangleCenter.Y - 15.0;
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            else /* isDelegate is true */
            {
                PortDefenition portDef = AutosarApplication.GetInstance().GetPortDefenition(PortDefenitionGuid);
                if ((portDef.PortType == PortDefenitions.PortType.Client) || (portDef.PortType == PortDefenitions.PortType.Sender))
                {
                    portNamePainter.Direction = TextDirection.LeftToRight;
                    portNamePainter.Coordinates.X = rectangleCenter.X + 15.0;
                    portNamePainter.Coordinates.Y = rectangleCenter.Y;
                }
                else 
                {
                    portNamePainter.Direction = TextDirection.RightToLeft;
                    portNamePainter.Coordinates.X = rectangleCenter.X - 15.0;
                    portNamePainter.Coordinates.Y = rectangleCenter.Y;
                }
            }
            portNamePainter.Render(context);
        }

        public Boundary GetBoundary(RenderContext context)
        {
            Boundary boundary = new Boundary();
            boundary.Left = rectanglePainter.Left;
            boundary.Right = rectanglePainter.Right;
            boundary.Top = rectanglePainter.Top;
            boundary.Bottom = rectanglePainter.Bottom;

            boundary += portNamePainter.GetBoundary(context);
            return boundary;
        }


        public RectangleSide ConnectionPortLocation = RectangleSide.Left;

        public Point GetConnectionPoint()
        {            
            Point point = new Point();
            switch (ConnectionPortLocation)
            {
                case RectangleSide.Unknown:
                case RectangleSide.Left:
                {
                    point.X = rectanglePainter.Left;
                    point.Y = rectanglePainter.Center.Y;
                    break;
                }
                case RectangleSide.Right:
                {
                    point.X = rectanglePainter.Right;
                    point.Y = rectanglePainter.Center.Y;
                    break;
                }
                case RectangleSide.Top:
                {
                    point.X = rectanglePainter.Center.X;
                    point.Y = rectanglePainter.Top;
                    break;
                }
                case RectangleSide.Bottom:
                {
                    point.X = rectanglePainter.Center.X;
                    point.Y = rectanglePainter.Bottom;
                    break;
                }
            }
            return point;
        }

        public bool IsClicked(Point point, out Object clickedObject)
        {
            bool Selected = rectanglePainter.IsClicked(point, out clickedObject);
            if (Selected)
            {
                clickedObject = this;
            }
            return Selected;
        }

        public virtual String GetPortTypeString()
        {
            return "";
        }


        public PortType PortType
        {
            get 
            {
                PortDefenition portDef = AutosarApplication.GetInstance().GetPortDefenition(PortDefenitionGuid);
                if (portDef != null)
                {
                    return portDef.PortType;
                }
                else
                {
                    return PortType.Unknown;
                }
            }
        }


        public String PortTypeString
        {
            get
            {
                return GetPortTypeString();
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);

            String portDefString = XmlUtilits.GetFieldValue(xml, "PortDefenitionGuid", Guid.Empty.ToString());

            if (!Guid.TryParse(portDefString, out PortDefenitionGuid))
            {
                PortDefenitionGuid = Guid.Empty;
            }
            rectanglePainter.LoadFromXML(xml);
            String side = XmlUtilits.GetFieldValue(xml, "ConnectionPortLocation", RectangleSide.Left.ToString());
            ConnectionPortLocation = (RectangleSide)Enum.Parse(typeof(RectangleSide), side, true);
        }

        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("PortPainter");
            base.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("PortDefenitionGuid", PortDefenitionGuid.ToString("B")));
            xmlElement.Add(new XElement("ConnectionPortLocation", ConnectionPortLocation.ToString()));
            rectanglePainter.WriteToXML(xmlElement);
            root.Add(xmlElement);
        }
    }
}