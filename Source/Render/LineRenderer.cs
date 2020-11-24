using AutosarGuiEditor.Source.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AutosarGuiEditor.Source.Render
{
    public class LineRenderer : IRender
    {
        public Point StartPoint = new Point();
        public Point EndPoint = new Point();

        Color lineColor = Colors.Black;
        public Color LineColor
        {
            get
            {
                return lineColor;
            }
            set
            {
                lineColor = value;
            }
        }

        public void Render(RenderContext context)
        {
            context.DrawLine(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, LineColor);
        }

        public bool IsClicked(Point point, out Object clickedObject)
        {
            double dist = MathUtility.DistanceFromPointToLine(point, StartPoint, EndPoint);
            if (dist < 5)
            {
                clickedObject = this;
                return true;
            }
            clickedObject = null;
            return false;
        }

        public void UpdateCoordinates(double startX, double startY, double endX, double endY)
        {
            StartPoint.X = startX;
            StartPoint.Y = startY;
            EndPoint.X = endX;
            EndPoint.Y = endY;
        }

        public void UpdateCoordinates(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }

    public class LineRendererList : List<LineRenderer>, IRender
    {
        Color lineColor = Colors.Black;
        public Color LinesColor
        {
            get
            {
                return lineColor;
            }
            set
            {
                if (lineColor != value)
                {
                    lineColor = value;
                    UpdateLinesColor(lineColor);
                }
            }
        }

        public void AddNewLine(Point start, Point end, Color color)
        {
            LineRenderer line = new LineRenderer();
            line.StartPoint = start;
            line.EndPoint = end;
            line.LineColor = color;
            this.Add(line);
        }

        public void AddNewLine(double startX, double startY, double endX, double endY, Color color)
        {
            LineRenderer line = new LineRenderer();
            line.StartPoint.X = startX;
            line.StartPoint.Y = startY;
            line.EndPoint.X = endX;
            line.EndPoint.Y = endY;
            line.LineColor = color;
            this.Add(line);
        }

        public void Render(RenderContext context)
        {
            foreach(LineRenderer line in this)
            {
                line.Render(context);
            }
        }

        public bool IsClicked(Point point, out Object clickedObject)
        {
            foreach(LineRenderer line in this)
            {
                if (line.IsClicked(point, out clickedObject))
                {
                    return true;
                }
            }
            clickedObject = null;
            return false;
        }

        public void SetThickness(double newthickness)
        {
            foreach(LineRenderer line in this)
            {
                
            }
        }

        void UpdateLinesColor(Color newColor)
        {
            foreach (LineRenderer line in this)
            {
                line.LineColor = newColor;
            }
        }
    }
}
