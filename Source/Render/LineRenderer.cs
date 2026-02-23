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
            Point endPoint1 = line1.EndPoint;
            Point startPoint2 = line2.StartPoint;

            // Находим точки на линиях, от которых начинается дуга
            double dist1 = line1.Length();
            double dist2 = line2.Length();

            if (dist1 < gapSize || dist2 < gapSize) return;

            // Точка на первой линии перед концом
            double dx1 = (line1.EndPoint.X - line1.StartPoint.X) / dist1;
            double dy1 = (line1.EndPoint.Y - line1.StartPoint.Y) / dist1;

            Point arcStartPoint = new Point(
                line1.EndPoint.X - dx1 * gapSize,
                line1.EndPoint.Y - dy1 * gapSize
            );

            // Точка на второй линии после начала
            double dx2 = (line2.EndPoint.X - line2.StartPoint.X) / dist2;
            double dy2 = (line2.EndPoint.Y - line2.StartPoint.Y) / dist2;

            Point arcEndPoint = new Point(
                line2.StartPoint.X + dx2 * gapSize,
                line2.StartPoint.Y + dy2 * gapSize
            );

            // Вычисляем контрольную точку для дуги (используем кривую Безье)
            Point midPoint = new Point(
                (arcStartPoint.X + arcEndPoint.X) / 2,
                (arcStartPoint.Y + arcEndPoint.Y) / 2
            );

            // Вычисляем перпендикулярное направление
            double perpX = -(dy1 + dy2) / 2;
            double perpY = (dx1 + dx2) / 2;
            double perpLength = (float)Math.Sqrt(perpX * perpX + perpY * perpY);

            if (perpLength > 0)
            {
                perpX /= perpLength;
                perpY /= perpLength;
            }

            // Контрольная точка для создания дуги
            Point controlPoint = new Point(
                midPoint.X + perpX * arcRadius,
                midPoint.Y + perpY * arcRadius
            );

            // Рисуем дугу как кривую Безье
            context.DrawArc(arcStartPoint.X, arcStartPoint.Y,
                            controlPoint.X,  controlPoint.Y, 
                            controlPoint.X,  controlPoint.Y, 
                            arcEndPoint.X,   arcEndPoint.Y,
                            color);
        }

        public void Render(RenderContext context)
        {
            int leftCount = Count;
            bool isSingleLine = Count == 1;

            double gapSize = 20;
            double arcRadius = gapSize / 2.0 * Math.Sqrt(2.0); // Радиус закругления

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
                        // Первая линия - рисуем от начала до точки перед концом
                        currentLine.Render(context, false, false, gapSize);
                    }
                    else if (i == this.Count - 1)
                    {
                        // Последняя линия - рисуем от точки после начала до конца
                        currentLine.Render(context, false, false, gapSize);
                    }
                    else
                    {
                        // Промежуточные линии - рисуем с отступами с обоих концов
                        currentLine.Render(context, false, false, gapSize);
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
