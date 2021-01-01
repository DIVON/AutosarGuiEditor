using AutosarGuiEditor.Source.Interfaces;
using AutosarGuiEditor.Source.Painters.Boundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutosarGuiEditor.Source.Painters
{
    public enum TextDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public class TextPainter: IRender, IBoundary
    {
        public String Text
        {
            set;
            get;
        }

        public TextDirection Direction
        {
            set;
            get;
        }

        public Point Coordinates = new Point();
        

        public PortableFontDesc Font
        {
            set;
            get;
        }

        public Color TextColor
        {
            set;
            get;
        }

        public void Render(RenderContext context)
        {
            Point textCoord = context.GetImageCoordinate(Coordinates);

            GlyphFont glyphFont = LetterGlyphTool.GetFont(Font);

            int textHeight = glyphFont.GetTextHeight(Text);
            int textWidth = glyphFont.GetTextWidth(Text);

            if (Direction == TextDirection.LeftToRight)
            {
                textCoord.Y -= (double)textHeight / 2.0d;
                context.Bitmap.DrawString((int)textCoord.X, (int)textCoord.Y, TextColor, Font, Text);
            }
            else if (Direction == TextDirection.RightToLeft)
            {
                textCoord.Y -= (double)textHeight / 2.0d;
                textCoord.X -= textWidth;
                context.Bitmap.DrawString((int)textCoord.X, (int)textCoord.Y, TextColor, Font, Text);
            }
            else if (Direction == TextDirection.TopToBottom)
            {
                context.Bitmap.DrawStringVertical((int)textCoord.X, (int)textCoord.Y, TextColor, Font, Text, true);
            }
            else
            {
                context.Bitmap.DrawStringVertical((int)textCoord.X, (int)textCoord.Y, TextColor, Font, Text, false);
            }
        }

        public Boundary GetBoundary(RenderContext context)
        {
            Boundary boundary = new Boundary();

            Point imageCoord = context.GetImageCoordinate(Coordinates);

            if (Font == null)
            {
                return boundary;
            }
            GlyphFont glyphFont = LetterGlyphTool.GetFont(Font);

            int textHeight = glyphFont.GetTextHeight(Text);
            int textWidth = glyphFont.GetTextWidth(Text);

            if (Direction == TextDirection.LeftToRight)
            {
                Point topLeftImageCoord = new Point();
                topLeftImageCoord.X = imageCoord.X;
                topLeftImageCoord.Y = (double)imageCoord.Y - (double)textHeight / 2;

                Point bottomRightImageCoord = new Point();
                bottomRightImageCoord.X = (double)imageCoord.X + (double)textWidth;
                bottomRightImageCoord.Y = (double)imageCoord.Y + (double)textHeight / 2;

                Point topLeftWorldCoord = context.GetWorldCoordinate(topLeftImageCoord);
                Point bottomRightWorldCoord = context.GetWorldCoordinate(bottomRightImageCoord);

                boundary.Left = topLeftWorldCoord.X;
                boundary.Top = topLeftWorldCoord.Y;
                boundary.Right = bottomRightWorldCoord.X;
                boundary.Bottom = bottomRightWorldCoord.Y;
            }
            else if (Direction == TextDirection.RightToLeft)
            {
                Point topLeftImageCoord = new Point();
                topLeftImageCoord.X = imageCoord.X - textWidth;
                topLeftImageCoord.Y = (double)imageCoord.Y - (double)textHeight / 2;

                Point bottomRightImageCoord = new Point();
                bottomRightImageCoord.X = imageCoord.X;
                bottomRightImageCoord.Y = (double)imageCoord.Y + (double)textHeight / 2;

                Point topLeftWorldCoord = context.GetWorldCoordinate(topLeftImageCoord);
                Point bottomRightWorldCoord = context.GetWorldCoordinate(bottomRightImageCoord);

                boundary.Left = topLeftWorldCoord.X;
                boundary.Top = topLeftWorldCoord.Y;
                boundary.Right = bottomRightWorldCoord.X;
                boundary.Bottom = bottomRightWorldCoord.Y;
            }
            else if (Direction == TextDirection.TopToBottom)
            {
                Point topLeftImageCoord = new Point();
                topLeftImageCoord.X = (double)imageCoord.X - (double)textHeight / 2;
                topLeftImageCoord.Y = imageCoord.Y;

                Point bottomRightImageCoord = new Point();
                bottomRightImageCoord.X = (double)imageCoord.X + (double)textHeight / 2;
                bottomRightImageCoord.Y = imageCoord.Y + textWidth;

                Point topLeftWorldCoord = context.GetWorldCoordinate(topLeftImageCoord);
                Point bottomRightWorldCoord = context.GetWorldCoordinate(bottomRightImageCoord);

                boundary.Left = topLeftWorldCoord.X;
                boundary.Top = topLeftWorldCoord.Y;
                boundary.Right = bottomRightWorldCoord.X;
                boundary.Bottom = bottomRightWorldCoord.Y;
            }
            else
            {
                Point topLeftImageCoord = new Point();
                topLeftImageCoord.X = (double)imageCoord.X - (double)textHeight / 2;
                topLeftImageCoord.Y = imageCoord.Y - textWidth;

                Point bottomRightImageCoord = new Point();
                bottomRightImageCoord.X = (double)imageCoord.X + (double)textHeight / 2;
                bottomRightImageCoord.Y = imageCoord.Y;

                Point topLeftWorldCoord = context.GetWorldCoordinate(topLeftImageCoord);
                Point bottomRightWorldCoord = context.GetWorldCoordinate(bottomRightImageCoord);

                boundary.Left = topLeftWorldCoord.X;
                boundary.Top = topLeftWorldCoord.Y;
                boundary.Right = bottomRightWorldCoord.X;
                boundary.Bottom = bottomRightWorldCoord.Y;
            }

            return boundary;
        }
    }
}
