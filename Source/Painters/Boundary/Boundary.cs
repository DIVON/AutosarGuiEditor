using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutosarGuiEditor.Source.Painters.Boundaries
{
    public class Boundary
    {
        public double Left
        {
            set;
            get;
        }

        public double Right
        {
            set;
            get;
        }

        public double Top
        {
            set;
            get;
        }

        public double Bottom
        {
            set;
            get;
        }
        
        public static Boundary operator+(Boundary bound1, Boundary bound2)
        {
            Boundary bound = new Boundary();
            
            bound.Left = Math.Min(bound1.Left, bound2.Left);
            bound.Right = Math.Max(bound1.Right, bound2.Right);
            bound.Top = Math.Min(bound1.Top, bound2.Top);
            bound.Bottom = Math.Max(bound1.Bottom, bound2.Bottom);
            return bound;
        }

        public double Width
        {
            get
            {
                return Math.Abs(Right - Left);
            }
        }

        public double Height
        {
            get
            {
                return Math.Abs(Bottom - Top);
            }
        }

        public double MiddleX
        {
            get
            {
                return (Left + Right) / 2;
            }
        }

        public double MiddleY
        {
            get
            {
                return (Top + Bottom) / 2;
            }
        }
    }
}
