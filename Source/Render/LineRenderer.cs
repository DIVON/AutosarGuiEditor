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

        public float Length()
        {
            double dx = EndPoint.X - StartPoint.X;
            double dy = EndPoint.Y - StartPoint.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public Point GetShortedStart(double shortDistance = 0)
        {
            float totalLength = Length();

            // Вычисляем направление линии
            double dx = (EndPoint.X - StartPoint.X) / totalLength;
            double dy = (EndPoint.Y - StartPoint.Y) / totalLength;

            var newStart = new Point();
            // Вычисляем новое начало (с отступом)
            newStart.X = StartPoint.X + dx * shortDistance;
            newStart.Y = StartPoint.Y + dy * shortDistance;

            return newStart;
        }

        public Point GetShortedEnd(double shortDistance = 0)
        {
            float totalLength = Length();

            // Вычисляем направление линии
            double dx = (EndPoint.X - StartPoint.X) / totalLength;
            double dy = (EndPoint.Y - StartPoint.Y) / totalLength;

            // Вычисляем новый конец (с отступом)
            var newEnd = new Point();
            newEnd.X = EndPoint.X - dx * shortDistance;
            newEnd.Y = EndPoint.Y - dy * shortDistance;

            return newEnd;
        }

        public void Render(RenderContext context, bool startShorted = false, bool endShorted = false, double shortDistance = 0)
        {
            float totalLength = Length();

            if (startShorted || endShorted) {
                // Вычисляем направление линии
                double dx = (EndPoint.X - StartPoint.X) / totalLength;
                double dy = (EndPoint.Y - StartPoint.Y) / totalLength;

                Point newStart = StartPoint;
                // Вычисляем новое начало (с отступом)
                if (startShorted)
                {
                    newStart.X =StartPoint.X + dx * shortDistance;
                    newStart.Y =StartPoint.Y + dy * shortDistance;
                }

                Point newEnd = EndPoint;

                // Вычисляем новый конец (с отступом)
                if (endShorted)
                {
                    newEnd.X = EndPoint.X - dx * shortDistance;
                    newEnd.Y = EndPoint.Y - dy * shortDistance;
                };

                context.DrawLine(newStart.X, newStart.Y, newEnd.X, newEnd.Y, LineColor);
            }
            else
            {
                context.DrawLine(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, LineColor);
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

        public Point GetVector()
        {
            return new Point(
                EndPoint.X - StartPoint.X,
                EndPoint.Y - StartPoint.Y
            );
        }

        public double GetAngleInRadians()
        {
            Point vec = GetVector();

            // Math.Atan2(y, x) is robust against division by zero and handles quadrants correctly.
            return Math.Atan2(vec.Y, vec.X);
        }

        public double GetAngleInDegrees()
        {
            double radians = GetAngleInRadians();

            // Convert radians to degrees
            return radians * (180.0 / Math.PI);
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

        private static void DrawConnectingArc(RenderContext context, LineRenderer line1, LineRenderer line2,
                                         double gapSize, double arcRadius, Color color)
        {
            // Точки, между которыми нужно нарисовать дугу
            Point startPoint = line1.GetShortedEnd(gapSize);
            Point endPoint = line2.GetShortedStart(gapSize);

            double angle1 = (int)-line1.GetAngleInDegrees();
            double angle2 = (int)-line2.GetAngleInDegrees();

            double angleDeg = 0;
            angleDeg = (int)(angle2 - angle1);



            if ((angle1 == 0) && (angle2 == 90))
            {
                var center = new Point(startPoint.X, endPoint.Y);
                context.DrawArc(center, arcRadius, 270, 360, color);
            }
            else if ((angle1 == 0) && (angle2 == -90))
            {
                var center = new Point(startPoint.X, endPoint.Y);
                context.DrawArc(center, arcRadius, 0, 90, color);
            }
            else if ((angle1 == -90) && (angle2 == 0))
            {
                var center = new Point(endPoint.X, startPoint.Y);
                context.DrawArc(center, arcRadius, 180, 270, color);
            }
            else if ((angle1 == -90) && (angle2 == -180))
            {
                var center = new Point(endPoint.X, startPoint.Y);
                context.DrawArc(center, arcRadius, 270, 360, color);
            }
            else if ((angle1 == 90) && (angle2 == -180))
            {
                var center = new Point(endPoint.X, startPoint.Y);
                context.DrawArc(center, arcRadius, 0, 90, color);
            }
            else if ((angle1 == 90) && (angle2 == 0))
            {
                var center = new Point(endPoint.X, startPoint.Y);
                context.DrawArc(center, arcRadius, 90, 180, color);
            }
            else if ((angle1 == -180) && (angle2 == 90))
            {
                var center = new Point(startPoint.X, endPoint.Y);
                context.DrawArc(center, arcRadius, 180, 270, color);
            }
            else if ((angle1 == -180) && (angle2 == -90))
            {
                var center = new Point(startPoint.X, endPoint.Y);
                context.DrawArc(center, arcRadius, 90, 180, color);
            }
        }

        public void Render(RenderContext context)
        {
            int leftCount = Count;
            bool isSingleLine = Count == 1;

            double gapSize = 20;
            double arcRadius = gapSize; // Радиус закругления

            for (int i = 0; i < this.Count; i++)
            {
                LineRenderer currentLine = this[i];

                if (isSingleLine)
                {
                    currentLine.Render(context);
                }
                else {
                    if (i == 0) 
                    {
                        if (this.Count == 1)
                        {
                            // единственная линия
                            currentLine.Render(context, false, false, gapSize);
                        }
                        else
                        {
                            // Первая линия - рисуем от начала до точки перед концом
                            currentLine.Render(context, false, true, gapSize);
                        }
                        
                    }
                    else if (i == this.Count - 1)
                    {
                        // Последняя линия - рисуем от точки после начала до конца
                        currentLine.Render(context, true, false, gapSize);
                    }
                    else
                    {
                        // Промежуточные линии - рисуем с отступами с обоих концов
                        currentLine.Render(context, true, true, gapSize);
                    }

                    // Рисуем соединительную дугу между текущей и следующей линией
                    if (i < this.Count - 1)
                    {
                        DrawConnectingArc(context, currentLine, this[i + 1], gapSize, arcRadius, currentLine.LineColor);
                    }
                }

                leftCount--;
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
