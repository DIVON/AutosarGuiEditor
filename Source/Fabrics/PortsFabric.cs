///////////////////////////////////////////////////////////
//  PortsFabric.cs
//  Implementation of the Class PortsFabric
//  Generated by Enterprise Architect
//  Created on:      24-���-2019 20:54:09
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using AutosarGuiEditor.Source.PortDefenitions;



namespace System
{
    public class PortsFabric
    {
        public PortsFabric()
        {

        }

        ~PortsFabric()
        {

        }

        public PortDefenition CreatePortDefenition(string Name, PortType portType)
        {
            PortDefenition portDef = new PortDefenition();
            portDef.Name = Name;
            portDef.PortType = portType;
            portDef.GUID = Guid.NewGuid();
            return portDef;
        }
    }
}
