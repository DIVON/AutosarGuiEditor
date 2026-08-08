using System.Windows;

namespace AutosarGuiEditor.Source.Composition
{
    /// <summary>
    /// Stores the viewport state (scale and offset) for a specific Composition instance.
    /// </summary>
    public class CompositionViewport
    {
        public double Scale { get; set; }
        public Point Offset { get; set; }
        public bool IsViewInitialized { get; set; }

        public CompositionViewport()
        {
            Scale = 1.0;
            Offset = new Point(0, 0);
            IsViewInitialized = false;
        }

        public CompositionViewport(double scale, Point offset, bool isViewInitialized)
        {
            Scale = scale;
            Offset = offset;
            IsViewInitialized = isViewInitialized;
        }

        /// <summary>
        /// Creates a default viewport for showing the composition from scratch (fit to view).
        /// </summary>
        public static CompositionViewport CreateDefault()
        {
            return new CompositionViewport(1.0, new Point(0, 0), false);
        }
    }
}