using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace System 
{
	public interface IRender
    {
        void Render(RenderContext context);
	}
}