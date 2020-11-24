using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutosarGuiEditor.Source.Controllers
{
    public class MoveObjectsController
    {
        private Point previousPosition;
        public object SelectedObject;
        private bool leftMouseDown = false;

        public void Viewport_MouseLeftButtonDown(Point sceneCoordinates)
        {
            AutosarApplication.GetInstance().UnselectComponents();

            if (AutosarApplication.GetInstance().ActiveComposition != null)
            {                
                bool clicked = AutosarApplication.GetInstance().ActiveComposition.GetClickedObject(sceneCoordinates, out SelectedObject);
                if (clicked == true)
                {
                    previousPosition = sceneCoordinates;
                    leftMouseDown = true;
                    return;
                }

                if (AutosarApplication.GetInstance().ActiveComposition == AutosarApplication.GetInstance().Compositions.GetMainComposition())
                {
                    foreach(CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                    {
                        if (composition != AutosarApplication.GetInstance().Compositions.GetMainComposition())
                        {
                            clicked = composition.IsClicked(sceneCoordinates, out SelectedObject);
                            if (clicked == true)
                            {
                                previousPosition = sceneCoordinates;
                                leftMouseDown = true;
                                return;
                            }
                        }
                    }
                }
            }
        }

        public Boolean Viewport_MouseMove(Point sceneCoordinates, MouseEventArgs e)
        {
            Boolean needRedraw = false;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (SelectedObject != null)
                {
                    if (leftMouseDown)
                    {
                        double translateX = (sceneCoordinates.X - previousPosition.X);
                        double translateY = (sceneCoordinates.Y - previousPosition.Y);
                        if (SelectedObject is ResizableRectangleElement)
                        {
                            ResizableRectangleElement componentPainter = (ResizableRectangleElement)SelectedObject;
                            componentPainter.Translate(translateX, translateY);
                            AutosarApplication.GetInstance().ActiveComposition.UpdateConnections();
                            needRedraw = true;
                        } 
                        else if (SelectedObject is AnchorPoint)
                        {
                            AnchorPoint anchor = (AnchorPoint)SelectedObject;
                            anchor.Move(translateX, translateY);
                            AutosarApplication.GetInstance().ActiveComposition.UpdateConnections();
                            needRedraw = true;
                        }
                        else if (SelectedObject is PortPainter)
                        {
                            PortPainter portPainter = (PortPainter)SelectedObject;
                            TranslatePortPainter(portPainter, sceneCoordinates, translateX, translateY);
                            AutosarApplication.GetInstance().ActiveComposition.UpdateConnections();
                            needRedraw = true;
                        }
                    }
                }
                previousPosition = sceneCoordinates;
            }
            return needRedraw;
        }

        public void Viewport_MouseLeftButtonUp()
        {
            leftMouseDown = false;
            SelectedObject = null;
        }

        public void TranslatePortPainter(PortPainter portPainter, Point worldCordinates, double deltaX, double deltaY)
        {
            IElementWithPorts Owner = AutosarApplication.GetInstance().FindComponentInstanceByPortGuid(portPainter.GUID);
            
            /* Move port of the component */
            if (!AutosarApplication.GetInstance().ActiveComposition.Equals(Owner))
            {
                RectangleSide side;
                Point closestPoint = MathUtility.getNearestPointInPerimeter(
                   Owner.Painter.Left,
                   Owner.Painter.Top,
                   Owner.Painter.Width,
                   Owner.Painter.Height,
                   worldCordinates.X,
                   worldCordinates.Y,
                   out side
                );
                if (side == portPainter.ConnectionPortLocation)
                {
                    if ((side == RectangleSide.Left) || (side == RectangleSide.Right))
                    {
                        if ((worldCordinates.Y >= Owner.Painter.Top) && (worldCordinates.Y <= Owner.Painter.Bottom))
                        {
                            double newPos = portPainter.Painter.Top + deltaY;
                            if ((newPos >= Owner.Painter.Top) && (newPos <= Owner.Painter.Bottom))
                            {
                                portPainter.Painter.Top = newPos;
                            }
                            else
                            {
                                portPainter.Painter.Left = closestPoint.X - portPainter.Painter.Width / 2.0f;
                                portPainter.Painter.Top = closestPoint.Y - portPainter.Painter.Height / 2.0f;
                            }
                        }
                        else
                        {
                            portPainter.Painter.Left = closestPoint.X - portPainter.Painter.Width / 2.0f;
                            portPainter.Painter.Top = closestPoint.Y - portPainter.Painter.Height / 2.0f;
                        }
                    }
                    else
                    {
                        if ((worldCordinates.X >= Owner.Painter.Left) && (worldCordinates.X <= Owner.Painter.Right))
                        {
                            double newPos = portPainter.Painter.Left + deltaX;
                            if ((newPos >= Owner.Painter.Left - portPainter.Painter.Width / 2) && (newPos <= Owner.Painter.Right - portPainter.Painter.Width / 2))
                            {
                                portPainter.Painter.Left = newPos;
                            }
                            else
                            {
                                portPainter.Painter.Left = closestPoint.X - portPainter.Painter.Width / 2.0f;
                                portPainter.Painter.Top = closestPoint.Y - portPainter.Painter.Height / 2.0f;
                            }
                        }
                        else
                        {
                            portPainter.Painter.Left = closestPoint.X - portPainter.Painter.Width / 2.0f;
                            portPainter.Painter.Top = closestPoint.Y - portPainter.Painter.Height / 2.0f;
                        }
                    }
                }
                else
                {
                    portPainter.Painter.Left = closestPoint.X - portPainter.Painter.Width / 2.0f;
                    portPainter.Painter.Top = closestPoint.Y - portPainter.Painter.Height / 2.0f;
                    portPainter.ConnectionPortLocation = side;
                }
            }
            else /* Move composition's port inside the composition */
            {
                portPainter.Painter.Left += deltaX;
                portPainter.Painter.Top += deltaY; 
            }
        }
    }
}
