using AutosarGuiEditor.Source.SystemInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace AutosarGuiEditor.Source.Render
{
    public delegate void OnTranslate(object sender, Point translate);

    public class AnchorPoint : IRender
    {
        public int Size = 10;

        private RectanglePainter rectangle = new RectanglePainter();
         
        public Point Position;

        private Object owner;
        public Object GetOwner()
        {
            return owner;
        }

        public AnchorPoint(Object Owner)
        {
            owner = Owner;
            rectangle.BackgroundColor = Colors.Blue;
        }

        public void Render(RenderContext context)
        {
            rectangle.Top = Position.Y - Size / context.Scale / 2;
            rectangle.Left = Position.X - Size / context.Scale / 2;
            rectangle.Width = Size / context.Scale;
            rectangle.Height = Size / context.Scale;
            rectangle.Render(context);
        }

        public bool IsClicked(Point point, out Object clickedObject)
        {
            bool clicked = rectangle.IsClicked(point, out clickedObject);
            if (clicked)
            {
                clickedObject = this;
                return true;
            }
            return false;
        }

        public bool Selected
        {
            set;
            get;
        }

        public void Move(double X, double Y)
        {
            Point translate = new Point(X, Y);
            this.Position.X += X;
            this.Position.Y += Y;
            if (OnMove != null)
            {
                OnMove(this, translate);
            }
        }

        public void Translate(double X, double Y)
        {
            this.Position.X += X;
            this.Position.Y += Y;
        }

        public event OnTranslate OnMove;

        public void LoadFromXML(XElement xml)
        {
            double p;
            if (double.TryParse(xml.Element("X").Value, out p) == true)
            {
                Position.X = p;
            }
            if (double.TryParse(xml.Element("Y").Value, out p) == true)
            {
                Position.Y = p;
            }
        }

        public void WriteToXML(XElement root)
        {
            XElement xmlElement = new XElement("AnchorPoint");
            xmlElement.Add(new XElement("X", Position.X.ToString()));
            xmlElement.Add(new XElement("Y", Position.Y.ToString()));
            root.Add(xmlElement);
        }
    }
}
