///////////////////////////////////////////////////////////
//  ObjectSelectorPainter.cs
//  Implementation of the Class ObjectSelectorPainter
//  Generated by Enterprise Architect
//  Created on:      29-���-2019 15:28:44
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media;

namespace System 
{
	public class ObjectSelectorPainter : IRender 
    {
        public IRender selectedPainter;
        public double size = 5;
        public Color PickColor = Colors.Blue;

		public ObjectSelectorPainter()
        {
        
		}

		public void Render(RenderContext context){
            if (selectedPainter is RectanglePainter)
            {
                RectanglePainter painter = (RectanglePainter)selectedPainter;
                /* Top left angle */
                //context.DrawFillRectangle(painter.StartX - size / 2.0f, painter.StartY - size / 2.0f, painter.StartX - size / 2.0f, painter.StartY + size / 2.0f, PickColor);
                
                /* Top right angle */
                //context.DrawFillRectangle(painter.EndX - size / 2.0f, painter.StartY - size / 2.0f, painter.EndX - size / 2.0f, painter.StartY + size / 2.0f, PickColor);

                /* Bottom left angle */
                //context.DrawFillRectangle(painter.StartX - size / 2.0f, painter.EndY - size / 2.0f, painter.StartX - size / 2.0f, painter.EndY + size / 2.0f, PickColor);

                /* Bottom right angle */
                //context.DrawFillRectangle(painter.EndX - size / 2.0f, painter.EndY - size / 2.0f, painter.EndX - size / 2.0f, painter.EndY + size / 2.0f, PickColor);
            } 
            else if (selectedPainter is PortConnection)
            {
                PortConnection painter = (PortConnection)selectedPainter;

                
                /* Bottom left angle */
                //context.DrawFillRectangle(painter.StartX - size / 2.0f, painter.EndY - size / 2.0f, painter.StartX - size / 2.0f, painter.EndY + size / 2.0f, PickColor);

                /* Bottom right angle */
                //context.DrawFillRectangle(painter.EndX - size / 2.0f, painter.EndY - size / 2.0f, painter.EndX - size / 2.0f, painter.EndY + size / 2.0f, PickColor);
            }
		}
	}
}