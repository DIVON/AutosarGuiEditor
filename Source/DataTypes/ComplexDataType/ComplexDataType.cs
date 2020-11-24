///////////////////////////////////////////////////////////
//  ComplexDataType.cs
//  Implementation of the Class ComplexDataType
//  Generated by Enterprise Architect
//  Created on:      24-���-2019 20:54:11
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Interfaces;

namespace AutosarGuiEditor.Source.DataTypes.ComplexDataType 
{
	public class ComplexDataType : IGUID
    {
        public ComplexDataTypeFieldsList Fields = new ComplexDataTypeFieldsList();		

		public ComplexDataType(){

		}		

		public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            Fields.LoadFromXML(xml);
		}
        
        public override void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("ComplexDataType");
            base.WriteToXML(xmlElement);            
            Fields.WriteToXML(xmlElement);
            root.Add(xmlElement);
		}

        public override List<IAutosarTreeList> GetLists()
        {
            List<IAutosarTreeList> list = new List<IAutosarTreeList>();
            list.Add(Fields);
            return list;
        }
	}
}