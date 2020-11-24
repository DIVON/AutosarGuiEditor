using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters.Boundaries;
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
    public delegate void OnUpdateSizeEvent();
    public delegate void OnMoveAnchor(AnchorPoint anchorPoint, Point translate);

    public class ResizableRectangleElement : IGUID, IClickable, IRender, ISelectable, IBoundary
    {
        public AnchorList Anchors = new AnchorList();

        public AnchorPoint TopLeftAnchor;
        public AnchorPoint TopRightAnchor;
        public AnchorPoint BottomLeftAnchor;
        public AnchorPoint BottomRightAnchor;

        private RectanglePainter painter = new RectanglePainter();
        public RectanglePainter Painter
        {
            get
            {
                return painter;
            }
        }

        public double GetMinHeight()
        {
            double MinSize = 50;
            return MinSize;
        }

        public double GetMinWidth()
        {
            double MinSize = 50;
            return MinSize;
        }

        public ResizableRectangleElement()
        {
            painter.BackgroundColor = Colors.LightGray;

            TopLeftAnchor = new AnchorPoint(this);
            TopRightAnchor = new AnchorPoint(this);
            BottomLeftAnchor = new AnchorPoint(this);
            BottomRightAnchor = new AnchorPoint(this);

            Anchors.Add(TopLeftAnchor);
            Anchors.Add(TopRightAnchor);
            Anchors.Add(BottomLeftAnchor);
            Anchors.Add(BottomRightAnchor);

            TopLeftAnchor.OnMove += TopLeftAnchor_OnMove;
            TopRightAnchor.OnMove += TopRightAnchor_OnMove;
            BottomLeftAnchor.OnMove += BottomLeftAnchor_OnMove;
            BottomRightAnchor.OnMove += BottomRightAnchor_OnMove;
        }

        public event OnMoveAnchor OnMoveTopRightAnchor;
        public event OnMoveAnchor OnMoveTopLeftAnchor;
        public event OnMoveAnchor OnMoveBottomRightAnchor;
        public event OnMoveAnchor OnMoveBottomLeftAnchor;

        public virtual void Translate(double X, double Y)
        {
            painter.TopLeft.X += X;
            painter.TopLeft.Y += Y;

            painter.BottomRight.X += X;
            painter.BottomRight.Y += Y;

            foreach(AnchorPoint anchor in Anchors)
            {
                anchor.Translate(X, Y);
            }
        }

        public virtual Boundary GetBoundary(RenderContext context)
        {
            Boundary boundary = new Boundary();
            boundary.Left = painter.Left;
            boundary.Right = painter.Right;
            boundary.Top = painter.Top;
            boundary.Bottom = painter.Bottom;
            return boundary;
        }

        public void UpdateAnchorsPositions()
        {
            TopLeftAnchor.Position.X = painter.Left;
            TopLeftAnchor.Position.Y = painter.Top;

            TopRightAnchor.Position.X = painter.Left + painter.Width;
            TopRightAnchor.Position.Y = painter.Top;

            BottomLeftAnchor.Position.X = painter.Left;
            BottomLeftAnchor.Position.Y = painter.Top + painter.Height;

            BottomRightAnchor.Position.X = painter.Left + painter.Width;
            BottomRightAnchor.Position.Y = painter.Top + painter.Height;            
        }

        void BottomRightAnchor_OnMove(object sender, System.Windows.Point translate)
        {
            if (painter.Width + translate.X > GetMinWidth())
            {
                painter.Width += translate.X;
                TopRightAnchor.Translate(translate.X, 0);
            }
            else
            {
                BottomRightAnchor.Position.X = painter.Left + painter.Width;
            }

            if (painter.Height + translate.Y > GetMinHeight())
            {
                painter.Height += translate.Y;
                BottomLeftAnchor.Translate(0, translate.Y);
            }
            else
            {
                BottomRightAnchor.Position.Y = painter.Top + painter.Height;
            }
            OnMoveBottomRightAnchor(BottomRightAnchor, translate);
        }

        void BottomLeftAnchor_OnMove(object sender, System.Windows.Point translate)
        {
            if (painter.Width - translate.X > GetMinWidth())
            {
                painter.Left += translate.X;
                painter.Width -= translate.X;
                TopLeftAnchor.Translate(translate.X, 0);
            }
            else
            {
                BottomLeftAnchor.Position.X = painter.Right - painter.Width;
            }

            if (painter.Height + translate.Y > GetMinHeight())
            {
                painter.Height += translate.Y;
                BottomRightAnchor.Translate(0, translate.Y);
            }
            else
            {
                BottomLeftAnchor.Position.Y = painter.Top + painter.Height;
            }
            OnMoveBottomLeftAnchor(BottomLeftAnchor, translate);
        }

        void TopRightAnchor_OnMove(object sender, Point translate)
        {
            if (painter.Width + translate.X > GetMinWidth())
            {
                painter.Width += translate.X;
                BottomRightAnchor.Translate(translate.X, 0);
            }
            else
            {
                TopRightAnchor.Position.X = painter.Left + painter.Width;
            }

            if (painter.Height - translate.Y > GetMinHeight())
            {
                painter.Top += translate.Y;
                painter.Height -= translate.Y;
                TopLeftAnchor.Translate(0, translate.Y);
            }
            else
            {
                TopRightAnchor.Position.Y = painter.Bottom - painter.Height;
            }
            OnMoveTopRightAnchor(TopRightAnchor, translate);
        }

        void TopLeftAnchor_OnMove(object sender, Point translate)
        {
            if (painter.Width - translate.X > GetMinWidth())
            {
                painter.Left += translate.X;
                painter.Width -= translate.X;
                BottomLeftAnchor.Translate(translate.X, 0);
            }
            else
            {
                TopLeftAnchor.Position.X = painter.Right - painter.Width;
            }

            if (painter.Height - translate.Y > GetMinHeight())
            {
                painter.Top += translate.Y;
                painter.Height -= translate.Y;
                TopRightAnchor.Translate(0, translate.Y);
            }
            else
            {
                TopLeftAnchor.Position.Y = painter.Bottom - painter.Height;
            }
            OnMoveTopLeftAnchor(TopLeftAnchor, translate);
        }

        public virtual bool IsClicked(Point point, out Object clickedObject)
        {
            /* Check click on ports */
            clickedObject = null;

            Unselect();

            bool clicked = Anchors.IsClicked(point, out clickedObject);
            if (clicked == true)
            {
                Select();
                return true;
            }

            clicked = painter.IsClicked(point, out clickedObject);
            if (clicked)
            {
                Select();
                clickedObject = this;
            }
            return clicked;
        }


        public virtual void Render(RenderContext context)
        {
            painter.Render(context);

            /* Draw anchors */
            if (IsSelected())
            {
                Anchors.RenderAnchors(context);
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            base.LoadFromXML(xml);
            painter.LoadFromXML(xml);
            

            /* Check min size */
            if (Painter.Width < 30)
            {
                Painter.Width = 30;
            }

            if (Painter.Height < 30)
            {
                Painter.Height = 30;
            }

            UpdateAnchorsPositions();
        }

        public override void WriteToXML(XElement root)
        {
            base.WriteToXML(root);
            painter.WriteToXML(root);
        }

        bool selected = false;

        public virtual void Unselect()
        {
            selected = false;
        }

        public virtual bool IsSelected()
        {
            return selected;
        }

        public virtual void Select()
        {
            selected = true;
        }
    }
}
