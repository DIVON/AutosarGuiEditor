///////////////////////////////////////////////////////////
//  PortsConnectionsPaintersList.cs
//  Implementation of the Class PortsConnectionsPaintersList
//  Generated by Enterprise Architect
//  Created on:      24-���-2019 20:54:10
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Render;
using AutosarGuiEditor.Source.AutosarInterfaces.SenderReceiver;
using AutosarGuiEditor.Source.AutosarInterfaces;



namespace System 
{
	public class PortsConnectionsList : IGuidList<PortConnection> 
    {

		public PortsConnectionsList()
        {

		}

        public void Unselect()
        {
            foreach (PortConnection connectionPainter in this)
            {
                connectionPainter.Unselect();
            }
        }

        public bool IsClicked(Point point, out Object clickedObject, Boolean checkAnchors)
        {
            clickedObject = null;
            foreach (PortConnection connectionPainter in this)
            {
                bool clicked = connectionPainter.IsClicked(point, out clickedObject, checkAnchors);
                if (clicked == true)
                {
                    return true;
                }
            }
            
            return false;
        }

        public List<PortConnection> FindConnections(PortPainter portPainter)
        {
            List<PortConnection> portConnections = new List<PortConnection>();
            foreach (PortConnection connectionPainter in this)
            {
                if ((connectionPainter.Port1 == portPainter) || (connectionPainter.Port2 == portPainter))
                {
                    portConnections.Add(connectionPainter);
                }
            }
            return portConnections;
        }


        public void AddConnection(PortConnection newConnection)
        {
            /* Connection cann't be dublicated */
            bool foundSimilarConnection = false;
            foreach(PortConnection connection in this)
            {
                if (connection.Component1.GUID.Equals(newConnection.Component1.GUID) && connection.Port1.GUID.Equals(newConnection.Port1.GUID) &&
                    connection.Component2.GUID.Equals(newConnection.Component2.GUID) && connection.Port2.GUID.Equals(newConnection.Port2.GUID))
                {
                    foundSimilarConnection = true;
                    break;
                }

                if (connection.Component1.GUID.Equals(newConnection.Component2.GUID) && connection.Port1.GUID.Equals(newConnection.Port2.GUID) &&
                    connection.Component2.GUID.Equals(newConnection.Component1.GUID) && connection.Port2.GUID.Equals(newConnection.Port1.GUID))
                {
                    foundSimilarConnection = true;
                    break;
                }                
            }

            if (foundSimilarConnection)
            {
                return;
            }





            /* Client and Receiver connections can't have more than one connection */  
            PortDefenition portDef1 = AutosarApplication.GetInstance().GetPortDefenition(newConnection.Port1.PortDefenitionGuid);

            /* Check port at the start of connection line */
            if (!newConnection.Port1.IsDelegatePort) /* Port of component */
            {
                /* Client port shall have only one connection for sync operation */
                if (portDef1.PortType == PortType.Client)
                {
                    ClientServerInterface csInterface = portDef1.InterfaceDatatype as ClientServerInterface;

                    if (csInterface.IsAsync == false)
                    {
                        if (IsThisPointConnectionExists(newConnection.Component1, newConnection.Port1) == true)
                        {
                            return;
                        }
                    }
                }

                /* Receiver without queue shall have only one connection */
                if (portDef1.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = portDef1.InterfaceDatatype as SenderReceiverInterface;
                    if (srInterface.IsQueued == false)
                    {
                        if (IsThisPointConnectionExists(newConnection.Component1, newConnection.Port1) == true)
                        {
                            return;
                        }
                    }
                }

                /* Queued Sender shall have only one connection */
                if ((portDef1.PortType == PortType.Sender) && ((portDef1.InterfaceDatatype as SenderReceiverInterface).IsQueued == true))
                {
                    if (IsThisPointConnectionExists(newConnection.Component1, newConnection.Port1) == true)
                    {
                        return;
                    }
                }
            }
            else /* Port of composition */
            {
                if ((portDef1.PortType == PortType.Sender) || (portDef1.PortType == PortType.Server))
                {
                    if (IsThisPointConnectionExists(newConnection.Component1, newConnection.Port1) == true)
                    {
                        return;
                    }
                }
            }




            /* Check port at the end of connection line */
            PortDefenition portDef2 = AutosarApplication.GetInstance().GetPortDefenition(newConnection.Port2.PortDefenitionGuid);

            if (!newConnection.Port2.IsDelegatePort) /* Port of component */
            {
                ClientServerInterface csInterface = portDef2.InterfaceDatatype as ClientServerInterface;

                if (csInterface.IsAsync == false)
                {
                    /* Client port shall have only one connection for sync operation */
                    if (portDef2.PortType == PortType.Client)
                    {
                        if (IsThisPointConnectionExists(newConnection.Component2, newConnection.Port2) == true)
                        {
                            return;
                        }
                    }
                }

                /* Receiver without queue shall have only one connection */
                if (portDef2.PortType == PortType.Receiver)
                {
                    SenderReceiverInterface srInterface = portDef2.InterfaceDatatype as SenderReceiverInterface;
                    if (srInterface.IsQueued == false)
                    {
                        if (IsThisPointConnectionExists(newConnection.Component2, newConnection.Port2) == true)
                        {
                            return;
                        }
                    }
                }

                /* Sender shall have only one connection */
                if (portDef2.PortType == PortType.Sender)
                {
                    if (IsThisPointConnectionExists(newConnection.Component2, newConnection.Port2) == true)
                    {
                        return;
                    }
                }
            }
            else /* Port of composition */
            {
                if (portDef2.PortType == PortType.Sender)
                {
                    if ((portDef1.InterfaceDatatype as SenderReceiverInterface).IsQueued == false)
                    {
                        if (IsThisPointConnectionExists(newConnection.Component2, newConnection.Port2) == true)
                        {
                            return;
                        }
                    }
                }
                
                if (portDef2.PortType == PortType.Server)
                {
                    if (IsThisPointConnectionExists(newConnection.Component2, newConnection.Port2) == true)
                    {
                        return;
                    }
                }
            }

            
            this.Add(newConnection);
        }

        /* Search if this port has been already used in other connection */
        bool IsThisPointConnectionExists(IElementWithPorts component, PortPainter port)
        {
            foreach (PortConnection connection in this)
            {
                if ((connection.Component1 == component) && (connection.Port1 == port))
                {
                    return true;
                }
                if ((connection.Component2 == component) && (connection.Port2 == port))
                {
                    return true;
                }
            }
            return false;
        }

        public override String GetName()
        {
            return "Connections";
        }
	}
}