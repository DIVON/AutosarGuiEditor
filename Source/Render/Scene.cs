///////////////////////////////////////////////////////////
//  Scene.cs
//  Implementation of the Class Scene
//  Generated by Enterprise Architect
//  Created on:      26-���-2019 7:30:39
//  Original author: Ivan
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using AutosarGuiEditor.Source.Painters;
using AutosarGuiEditor.Source.Composition;


namespace System 
{
	public class Scene 
    {
        private WriteableBitmap bmp;

        private RenderContext _context;
        public RenderContext Context
        {
            get { 
                return _context; 
            }
        }

        public Scene(WriteableBitmap bitmap)
        {
            bmp = bitmap;
            _context = new RenderContext(bmp);
            RenderAllElements = false;
		}

        public void UpdateBitmap(WriteableBitmap bitmap)
        {
            bmp = bitmap;
            RenderContext newcontext = new RenderContext(bmp);
            newcontext.Offset = _context.Offset;
            newcontext.scale = _context.scale;
            _context = newcontext;
        }

		public Point MouseToXY(Point mouseCoord)
        {
            return _context.GetWorldCoordinate(mouseCoord);
		}

        public Boolean RenderAllElements
        {
            set;
            get;
        }

		public void RenderScene()
        {
            _context.Clear();

            /* Draw components */
            if (AutosarApplication.GetInstance().ActiveComposition != null)
            {
                

                /* Draw another compositions in main composition */
                if (AutosarApplication.GetInstance().ActiveComposition == AutosarApplication.GetInstance().Compositions.GetMainComposition())                
                {                   
                    /* Draw another compositions */
                    foreach (CompositionInstance composition in AutosarApplication.GetInstance().Compositions)
                    {
                        if (composition != AutosarApplication.GetInstance().Compositions.GetMainComposition())
                        {
                            composition.Render(_context);
                        }
                    }
                }

                /* Render composition entrails */
                if (AutosarApplication.GetInstance().ActiveComposition != null)
                {
                    AutosarApplication.GetInstance().ActiveComposition.RenderCompositionEntrails(_context, RenderAllElements);
                }
            }
		}

		public Point XYtoImage(Point xyCoord)
        {
            return _context.GetImageCoordinate(xyCoord);
		}
	}
}