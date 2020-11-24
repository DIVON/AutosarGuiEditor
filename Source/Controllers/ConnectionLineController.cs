using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Fabrics;
using AutosarGuiEditor.Source.Forms.Controls;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutosarGuiEditor.Source.Controllers
{
    public class ConnectionLineController
    {
        AutosarTreeViewControl treeView;
        PortPainter firstPort = null;
        Boolean leftMouseDown = false;

        public ConnectionLineController(AutosarTreeViewControl treeView)
        {
            this.treeView = treeView;
        }

        public void StartConnection()
        {
            AddConnectionLineActive = true;
            connectionPainter = null;
        }

        public void EndConnection()
        {
            AddConnectionLineActive = false;
            firstPort = null;
            leftMouseDown = false;
        }

        public Boolean AddConnectionLineActive = false;

        public PortConnection connectionPainter;

        public void Viewport_MouseDown(MouseButtonEventArgs e, Point sceneCursorPosition)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                /* Add Connection Line command started */
                if (this.AddConnectionLineActive == true)
                {
                    Object selectedObject = null;

                    bool clicked;

                    
                    clicked = AutosarApplication.GetInstance().ActiveComposition.GetClickedObject(sceneCursorPosition, out selectedObject);

                    /* if we check in main composition */
                    if (!clicked)
                    {
                        CompositionInstance mainComposition = AutosarApplication.GetInstance().Compositions.GetMainComposition();
                        foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                        {
                            if (!composition.Equals(mainComposition))
                            {
                                clicked = composition.IsClicked(sceneCursorPosition, out selectedObject);
                                if (clicked == true)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    
                    

                    if (clicked == true)
                    {

                        /* Check that PortPainter has been selected  first */
                        if (selectedObject is PortPainter)
                        {
                            firstPort = selectedObject as PortPainter;
                            connectionPainter = PortConnectionFabric.GetInstance().CreatePortConnection();
                            connectionPainter.Port1 = firstPort;
                            connectionPainter.SecondPoint = sceneCursorPosition;
                            leftMouseDown = true;
                            return;
                        }
                        else
                        {
                            AddConnectionLineActive = false;
                        }
                    }
                    else
                    {
                        AddConnectionLineActive = false;
                    }
                }
            }
        }

        public Boolean Viewport_MouseMove(MouseEventArgs e, Point SceneCursorPosition)
        {
            Boolean needRedraw = false;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (firstPort != null)
                {
                    if (leftMouseDown)
                    {
                        /* Add Connection Line command started */
                        if (this.AddConnectionLineActive == true)
                        {
                            connectionPainter.SecondPoint = SceneCursorPosition;
                            needRedraw = true;
                        }
                    }
                }
            }
            return needRedraw;
        }

        public void Viewport_MouseLeftButtonUp(Point sceneCoordinates)
        {
            /* Add Connection Line command started */
            if (this.AddConnectionLineActive == true)
            {
                Object newSelectedObject;
                bool clicked = AutosarApplication.GetInstance().ActiveComposition.GetClickedObject(sceneCoordinates, out newSelectedObject);

                /* if we check in main composition */
                if (!clicked)
                {
                    CompositionInstance mainComposition = AutosarApplication.GetInstance().Compositions.GetMainComposition();
                    foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                    {
                        if (!composition.Equals(mainComposition))
                        {
                            clicked = composition.IsClicked(sceneCoordinates, out newSelectedObject);
                            if (clicked)
                            {
                                break;
                            }
                        }
                    }
                }
                    
                if (newSelectedObject is PortPainter)
                {                    
                    PortPainter startPort = connectionPainter.Port1;
                    PortPainter endPort = newSelectedObject as PortPainter;

                    /* Check that we can assign another port to this */
                    if (CouldPortsBeAssigned(startPort, endPort))
                    {                        
                        connectionPainter.Port2 = endPort;
                        connectionPainter.UpdateLines();
                        connectionPainter.UpdateName();
                        CompositionInstance currentComposition = AutosarApplication.GetInstance().ActiveComposition;
                        currentComposition.Connections.AddConnection(connectionPainter);
                        connectionPainter = null;
                        treeView.UpdateAutosarTreeView(null);
                    }
                }
                AddConnectionLineActive = false;
            }
            leftMouseDown = false;
        }


        public bool CouldPortsBeAssigned(PortPainter port1, PortPainter port2)
        {
            /* Ports shall be different */
            if (port1 == port2)
            {
                return false;
            }

            /* ports shall have different parent */
            IElementWithPorts component1 = AutosarApplication.GetInstance().FindComponentInstanceByPort(port1);
            IElementWithPorts component2 = AutosarApplication.GetInstance().FindComponentInstanceByPort(port2);
            if ((component1 == component2) || (component1 == null) || (component2 == null))
            {
                return false;
            }


            /* Ports shall have the same interfaces */
            PortDefenition portDef1 = AutosarApplication.GetInstance().GetPortDefenition(port1.PortDefenitionGuid);
            PortDefenition portDef2 = AutosarApplication.GetInstance().GetPortDefenition(port2.PortDefenitionGuid);
            if (!portDef1.InterfaceGUID.Equals(portDef2.InterfaceGUID))
            {
                return false;
            }

            if (!port1.IsDelegatePort && !port2.IsDelegatePort)
            {           
                /* Another port shall be the correspondent type */
                bool check = (portDef1.PortType == PortType.Client) && (portDef2.PortType == PortType.Server);
                check |= (portDef1.PortType == PortType.Server) && (portDef2.PortType == PortType.Client);
                check |= (portDef1.PortType == PortType.Receiver) && (portDef2.PortType == PortType.Sender);
                check |= (portDef1.PortType == PortType.Sender) && (portDef2.PortType == PortType.Receiver);

                if (!check)
                {
                    return false;
                }
            }
            else /* One of the ports is delegate */
            {
                /* Port shall have the same type */
                bool check = (portDef1.PortType == portDef2.PortType);

                if (!check)
                {
                    return false;
                }
            }

            return true;
        }

        public void Render(RenderContext context)
        {
            if ((connectionPainter != null) && (AddConnectionLineActive))
            {
                connectionPainter.Render(context);
            }
        }
    }
}
