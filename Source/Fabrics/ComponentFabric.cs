///////////////////////////////////////////////////////////
//  ComponentFabric.cs
//  Implementation of the Class ComponentFabric
//  Generated by Enterprise Architect
//  Created on:      24-���-2019 20:54:09
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Component;
using AutosarGuiEditor.Source.PortDefenitions;
using AutosarGuiEditor.Source.Painters.PortsPainters;
using AutosarGuiEditor.Source.Utility;
using AutosarGuiEditor.Source.Painters.Components.Runables;



namespace System 
{
	public class ComponentFabric 
    {

        static ComponentFabric instance = null;

        static public ComponentFabric GetInstance()
        {
            if (instance == null)
            {
                instance = new ComponentFabric();
            }
            return instance;
        }

		ComponentFabric()
        {

		}

		~ComponentFabric()
        {

		}

        public ApplicationSwComponentType CreateComponentDefenition(string name)
        {
            ApplicationSwComponentType compDef = new ApplicationSwComponentType();
            compDef.Name = name;
            compDef.Runnables.Add(CreateRunnableDefenition("Refresh"));
            return compDef;
        }

        public ComponentInstance CreateComponent(ApplicationSwComponentType componentDefenition, double X, double Y)
        {
            int leftPortCount = 0;
            int rightPortCount = 0;
            foreach (PortDefenition portDef in componentDefenition.Ports)
            {
                if ((portDef.PortType == PortType.Receiver) || (portDef.PortType == PortType.Server))
                {
                    leftPortCount++;
                }
                else 
                {
                    rightPortCount++;
                }
            }

            double componentHeight = Math.Max(leftPortCount, rightPortCount) * 50 + 60;

            ComponentInstance componentInstance = new ComponentInstance();
            componentInstance.Name = GetComponentName(componentDefenition);
            componentInstance.ComponentDefenitionGuid = componentDefenition.GUID;
            componentInstance.Painter.Left = X - 60;
            componentInstance.Painter.Right = X + 60;
            componentInstance.Painter.Top = Y - componentHeight / 2.0;
            componentInstance.Painter.Height = componentHeight;
            componentInstance.UpdatePims();

            double LeftTop = componentInstance.Painter.Top + 30;
            double RightTop = LeftTop;
            double LeftPos = componentInstance.Painter.Left - 10;
            double RightPos = componentInstance.Painter.Right - 10;

            /* Create all port painters from defenition */
            foreach (PortDefenition portDef in componentDefenition.Ports)
            {             
                PortPainter newPort;
                if ((portDef.PortType == PortType.Receiver) || (portDef.PortType == PortType.Server))
                {
                    newPort = CreatePortPainter(portDef, LeftPos, LeftTop);
                    newPort.ConnectionPortLocation = RectangleSide.Left;
                    LeftTop += 50;
                }
                else 
                {
                    newPort = CreatePortPainter(portDef, RightPos, RightTop);
                    newPort.ConnectionPortLocation = RectangleSide.Right;
                    RightTop += 50;
                }
                componentInstance.Ports.Add(newPort);                
            }
            componentInstance.UpdateCData();
            componentInstance.UpdatePims();
            componentInstance.SyncronizeRunnablesWithDefenition();
            return componentInstance;
		}

        public String GetComponentName(ApplicationSwComponentType componentDefenition)
        {
            return componentDefenition.Name;
            
            /*int count = 0;
            foreach (ComponentInstance compInstance in AutosarApplication.GetInstance().componentInstancesList)
            {
                if (compInstance.ComponentDefenitionGuid.Equals(componentDefenition.GUID))
                {
                    count++;
                }
            }

            return componentDefenition.Name + count.ToString();*/
        }

        public RunnableInstance CreateRunnableInstance(RunnableDefenition runnableDefenition)
        {
            RunnableInstance runable = new RunnableInstance();
            runable.Name = runnableDefenition.Name;
            runable.DefenitionGuid = runnableDefenition.GUID;
            return runable;
        }

        public RunnableDefenition CreateRunnableDefenition(string Name)
        {
            RunnableDefenition runable = new RunnableDefenition(Name);
            return runable;
        }

        public PortPainter CreatePortPainter(PortDefenition portDef, double X, double Y)
        {
            
            String portName = NameUtils.GetInterfaceName(portDef);
            PortPainter portPainter = new PortPainter(portDef.GUID, X, Y, portName);
            return portPainter;
        }
	}
}