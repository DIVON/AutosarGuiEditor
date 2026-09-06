using System;
using System.Globalization;
using System.Xml.Linq;
using AutosarGuiEditor.Source.SystemInterfaces;
using AutosarGuiEditor.Source.Utility;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutosarGuiEditor.Source.Render
{
    /// <summary>
    /// Comment on the diagram - plain rectangular note.
    /// Inherits ResizableRectangleElement for anchor-based resize and translate-based move.
    /// </summary>
    public class CommentInstance : ResizableRectangleElement
    {
        public string Text { get; set; } = "123";

        // Default comment size
        private const double DefaultWidth = 200;
        private const double DefaultHeight = 80;

        public CommentInstance()
        {
            Painter.BackgroundColor = Colors.Beige;
            UpdateAnchorsPositions();
        }

        public CommentInstance(double centerX, double centerY, string text)
        {
            Painter.BackgroundColor = Colors.Beige;
            // Set size
            Painter.Width = DefaultWidth;
            Painter.Height = DefaultHeight;
            // Set position: painter.TopLeft = center - half-size
            double halfW = DefaultWidth / 2.0;
            double halfH = DefaultHeight / 2.0;
            Painter.Left = centerX - halfW;
            Painter.Top = centerY - halfH;
            Painter.Right = Painter.Left + DefaultWidth;
            Painter.Bottom = Painter.Top + DefaultHeight;
            UpdateAnchorsPositions();
            Text = text ?? "123";
        }

        public override void Render(RenderContext context)
        {
            // Draw base shape (filled with beige, borders drawn by painter)
            base.Render(context);

            // Draw text centered in the comment bounds
            string displayText = Text;

            // Use the same scaled font as components
            PortableFontDesc font = AutosarApplication.GetInstance().ComponentNameFont;
            GlyphFont glyphFont = LetterGlyphTool.GetFont(font);
            int textWidth = glyphFont.GetTextWidth(displayText);
            int textHeight = glyphFont.GetTextHeight(displayText);

            if (textWidth > 0)
            {
                // Center text in the comment bounds
                double textX = Painter.Left + Painter.Width / 2.0;
                double textY = Painter.Top + Painter.Height / 2.0;

                Point imageCoord = context.GetImageCoordinate(new Point(textX, textY));
                int drawX = (int)imageCoord.X - textWidth / 2;
                int drawY = (int)imageCoord.Y;

                context.Bitmap.DrawString(drawX, drawY, Colors.Black, font, displayText);
            }
        }

        public override void LoadFromXML(XElement xml)
        {
            // Delegate size loading to base class (Size element: Left/Right/Top/Bottom)
            // Handle old format (X/Y/Width/Height) for backward compatibility
            if (xml.Element("Size") == null)
            {
                double xVal, yVal, wVal, hVal;
                if (double.TryParse(XmlUtilits.GetFieldValue(xml, "X", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out xVal) &&
                    double.TryParse(XmlUtilits.GetFieldValue(xml, "Y", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out yVal) &&
                    double.TryParse(XmlUtilits.GetFieldValue(xml, "Width", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out wVal) &&
                    double.TryParse(XmlUtilits.GetFieldValue(xml, "Height", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out hVal))
                {
                    // Convert center-based format to Left/Right/Top/Bottom for base class
                    Painter.Left = xVal - wVal / 2.0;
                    Painter.Width = wVal;
                    Painter.Right = Painter.Left + Painter.Width;
                    Painter.Top = yVal - hVal / 2.0;
                    Painter.Height = hVal;
                    Painter.Bottom = Painter.Top + Painter.Height;
                }
            }

            base.LoadFromXML(xml);

            // Load comment-specific field
            String textVal = XmlUtilits.GetFieldValue(xml, "CommentText", "123");
            if (!string.IsNullOrEmpty(textVal))
                Text = textVal;

            // Update anchor positions to match painter bounds
            UpdateAnchorsPositions();
        }

        public override void WriteToXML(XElement root)
        {
            // Wrap in <CommentInstance> element (matches pattern used by other instance types)
            XElement xmlElement = new XElement("CommentInstance");
            base.WriteToXML(xmlElement);
            xmlElement.Add(new XElement("CommentText", Text));
            root.Add(xmlElement);
        }
    }
}
