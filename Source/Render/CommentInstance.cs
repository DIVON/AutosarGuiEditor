using System;
using System.Collections.Generic;
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
        private string _text = "123";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (AutosarApplication.GetInstance() != null)
                    AdjustSizeToText();
            }
        }
        public System.Windows.TextAlignment TextAlign { get; set; } = System.Windows.TextAlignment.Left;

        public CommentInstance()
        {
            Painter.BackgroundColor = Colors.Beige;
            if (AutosarApplication.GetInstance() != null)
                AdjustSizeToText();
            UpdateAnchorsPositions();
        }

        public CommentInstance(double centerX, double centerY, string text)
        {
            Painter.BackgroundColor = Colors.Beige;
            if (AutosarApplication.GetInstance() != null)
                AdjustSizeToText();
            double halfW = Painter.Width / 2.0;
            double halfH = Painter.Height / 2.0;
            Painter.Left = centerX - halfW;
            Painter.Top = centerY - halfH;
            UpdateAnchorsPositions();
            Text = text ?? "123";
        }

        private void AdjustSizeToText()
        {
            PortableFontDesc font = AutosarApplication.GetInstance().ComponentNameFont;
            GlyphFont glyphFont = LetterGlyphTool.GetFont(font);

            int lineHeight = glyphFont.TextHeight;

            // Количество строк = количество \n + 1 (без word-wrap)
            string[] lines = Text.Split(new[] { '\n' }, StringSplitOptions.None);
            int lineCount = Math.Max(1, lines.Length);

            // Ширина = ширина самой длинной строки
            double requiredWidth = 0;
            foreach (string line in lines)
            {
                int lineWidth = glyphFont.GetTextWidth(line);
                if (lineWidth > requiredWidth)
                    requiredWidth = lineWidth;
            }

            // Высота = высота всех строк + отступ сверху и снизу по одной высоте строки
            double requiredHeight = lineCount * lineHeight;
            double width = requiredWidth + lineHeight * 2;
            double height = requiredHeight + lineHeight * 2;

            Painter.Width = width;
            Painter.Height = height;
            UpdateAnchorsPositions();
        }

        public override void Render(RenderContext context)
        {
            // Draw base shape (filled with beige, borders drawn by painter)
            base.Render(context);

            string displayText = Text;
            if (string.IsNullOrEmpty(displayText)) return;

            // Use the same scaled font as components
            PortableFontDesc font = AutosarApplication.GetInstance().ComponentNameFont;
            GlyphFont glyphFont = LetterGlyphTool.GetFont(font);

            int lineHeight = glyphFont.TextHeight;

            // Split by newline only — no word-wrap (one text line per \n)
            string[] lines = displayText.Split(new[] { '\n' }, StringSplitOptions.None);
            if (lines.Length == 0) return;

            // Find widest line for centering
            double maxLineWidth = 0;
            foreach (string line in lines)
            {
                int lineWidth = glyphFont.GetTextWidth(line);
                if (lineWidth > maxLineWidth) maxLineWidth = lineWidth;
            }

            // DrawString expects bitmap/image coordinates. Painter coords are in world coords,
            // so we convert via GetImageCoordinate (applies scale + offset).
            // Padding = lineHeight on all sides.
            Point topLeftImage = context.GetImageCoordinate(
                new System.Windows.Point(Painter.Left + lineHeight, Painter.Top + lineHeight));
            double bitmapMaxWidth = maxLineWidth;

            int drawX = (int)topLeftImage.X;
            int drawY = (int)topLeftImage.Y;

            foreach (string line in lines)
            {
                int lineWidth = glyphFont.GetTextWidth(line);
                // Center line horizontally within the widest line
                int offsetX = (int)((bitmapMaxWidth - lineWidth) / 2.0);
                context.Bitmap.DrawString(drawX + offsetX, drawY, Colors.Black, font, line);
                drawY += lineHeight;
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
