using AutosarGuiEditor.Source.Painters.PortsPainters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutosarGuiEditor.Source.Utility
{
    public static class MathUtility
    {


        public static double DistanceFromPointToLine(Point point, Point lineStart, Point lineEnd)
        {
            /* This code found at https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment */

            double A = point.X - lineStart.X;
            double B = point.Y - lineStart.Y;
            double C = lineEnd.X - lineStart.X;
            double D = lineEnd.Y - lineStart.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = -1.0d;
            if (len_sq != 0)
            {
                param = dot / len_sq;
            }
            double xx, yy;
            if (param < 0)
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * C;
                yy = lineStart.Y + param * D;
            }

            double dx = point.X - xx;
            double dy = point.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
            //return Math.Abs((lineEnd.X - lineStart.X) * (lineStart.Y - point.Y) - (lineStart.X - point.X) * (lineEnd.Y - lineStart.Y)) /
            //        Math.Sqrt(Math.Pow(lineEnd.X - lineStart.X, 2) + Math.Pow(lineEnd.Y - lineStart.Y, 2));
        }

        public static double Distance(Point p1, Point p2)
        {
            return (Math.Sqrt(Math.Pow(p1.X - p2.X, 2.0d) + Math.Pow(p1.Y - p2.Y, 2.0d)));
        }

        public static bool PointInRectangle(Point point, Point rectPoint1, Point rectPoint2)
        {
            double maxX = Math.Max(rectPoint1.X, rectPoint2.X);
            double maxY = Math.Max(rectPoint1.Y, rectPoint2.Y);
            double minX = Math.Min(rectPoint1.X, rectPoint2.X);
            double minY = Math.Min(rectPoint1.Y, rectPoint2.Y);
            if ((minX <= point.X && point.X <= maxX) && (minY <= point.Y && point.Y <= maxY))
            {
                return true;
            }
            return false;
        }

        public static bool InRange(double min, double max, double value)
        {
            return ((min <= value) && (value <= max));
        }

        private static double clamp(double n,double lower,double upper) {
            return Math.Max(lower, Math.Min(upper, n));
        }

        public static Point getNearestPointInPerimeter(double left, double top, double width, double height, double x, double y, out RectangleSide side)
        {
            double right = left + width;
            double bottom = top + height;

            double X = clamp(x, left, right);
            double Y = clamp(y, top, bottom);

            double dl = Math.Abs(X - left);
            double dr = Math.Abs(X - right);
            double dt = Math.Abs(Y - top);
            double db = Math.Abs(Y - bottom);

            double m = Math.Min(dl, dr);
            m = Math.Min(m, dt);
            m = Math.Min(m, db);

            if (m == dt)
            {
                side = RectangleSide.Top;
                return new Point(X,top);
            }
            else if (m == db)
            {
                side = RectangleSide.Bottom;
                return new Point(X,bottom);
            }
            else if (m == dl)
            {
                side = RectangleSide.Left;
                return new Point(left,Y);
            }
            else 
            {
                side = RectangleSide.Right;
                return new Point(right,Y);
            }
        }

        public static bool IsDoubleEqual(double d1, double d2, double accuracy = 0.01)
        {
            return Math.Abs(d1 - d2) < 0.01;
        }

        public static int NaimensheeObsheeKratnoe(int a, int b)
        {
            int krat = 0;
            int n;

            if (a > b) 
                n = b;
            else 
                n = a;


            for (int i = n; i <= (a + b) * 2; i++)
            {
                if ((i % a == 0) && (i % b) == 0)
                {
                    krat = i;
                    break;
                }
            }
            return krat;
        }
    }
}
