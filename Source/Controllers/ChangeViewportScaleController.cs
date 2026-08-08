using AutosarGuiEditor.Source.Composition;
using AutosarGuiEditor.Source.Painters.Boundaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutosarGuiEditor.Source.Controllers
{
    public class ChangeViewportScaleController
    {
        Scene scene;
        Image image;
        Point LastMiddlePoint;
        bool lastFitWasDone = false;
        public ChangeViewportScaleController(Scene scene, Image image)
        {
            this.scene = scene;
            this.image = image;
        }

        public void Viewport_MouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                LastMiddlePoint = e.GetPosition(image);
            }
        }

        public void Viewport_MouseWheel(MouseWheelEventArgs e)
        {
            //Scale image with cursor pointer stayed on last plase
            Point currentPoint = e.GetPosition(image);

            //get pointed scene coordinates
            Point firstSceneCoordinates = scene.MouseToXY(currentPoint);

            double scaleFactor = 0.1;
            if (e.Delta < 0)
            { 
                //check direction for mouse wheel
                scene.Context.Scale -= scaleFactor;
            }
            else
            {
                scene.Context.Scale += scaleFactor;
            }
            

            AutosarApplication.GetInstance().UpdateFontAccordingScale(scene.Context.Scale);

            //get new scene coordinates for last point
            Point newImageCoordForLastPoint = scene.XYtoImage(firstSceneCoordinates);

            scene.Context.Offset.X += currentPoint.X - newImageCoordForLastPoint.X;
            scene.Context.Offset.Y += currentPoint.Y - newImageCoordForLastPoint.Y;

            // Save viewport state for active composition after zoom
            SaveCurrentViewport();
        }

        public Boolean Viewport_MouseMove(MouseEventArgs e)
        {
            Boolean needRedraw = false;
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(image);
                scene.Context.Offset.X += (currentPoint.X - LastMiddlePoint.X);
                scene.Context.Offset.Y += (currentPoint.Y - LastMiddlePoint.Y);
                LastMiddlePoint = currentPoint;
                needRedraw = true;

                // Save viewport state for active composition after pan
                SaveCurrentViewport();
            }
            return needRedraw;
        }

        /// <summary>
        /// Saves the current viewport state to the active composition.
        /// </summary>
        private void SaveCurrentViewport()
        {
            CompositionInstance activeComposition = AutosarApplication.GetInstance().ActiveComposition;
            if (activeComposition != null)
            {
                AutosarApplication.GetInstance().Compositions.SaveViewport(
                    activeComposition, 
                    scene.Context.Scale, 
                    scene.Context.Offset, 
                    lastFitWasDone);
            }
        }

        public Boolean FitWorldToImage(double viewportWidth, double viewportHeight)
        {
            if (AutosarApplication.GetInstance().ActiveComposition != null)
            {
                Boundary boundary = AutosarApplication.GetInstance().ActiveComposition.GetInternalBoundary(scene.Context);

                double scaleX = 1;
                double scaleY = 1;

                if ((boundary.Width > 0.1) & (boundary.Height > 0.1))
                {
                    scaleX = viewportWidth / boundary.Width;
                    scaleY = viewportHeight / boundary.Height;
                }

                double scale = Math.Min(scaleX, scaleY);
                scene.Context.Scale = scale;

                AutosarApplication.GetInstance().UpdateFontAccordingScale(scale);

                //get new scene coordinates for last point
                Point newImageCoordForLastPoint = scene.Context.GetWorldCoordinate(new Point(viewportWidth / 2, viewportHeight / 2));
                Point delta = new Point(-boundary.MiddleX + newImageCoordForLastPoint.X, -boundary.MiddleY + newImageCoordForLastPoint.Y);
                Point deltaImage = scene.Context.GetImageCoordinate(delta);
                scene.Context.Offset.X = deltaImage.X;
                scene.Context.Offset.Y = deltaImage.Y;

                // Mark that we've fit to view for this composition
                lastFitWasDone = true;
                AutosarApplication.GetInstance().Compositions.MarkViewportInitialized(
                    AutosarApplication.GetInstance().ActiveComposition);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Restores a previously saved viewport for the active composition.
        /// If no viewport has been saved yet, fits the composition to view.
        /// Returns true if viewport was restored or fitted, false otherwise.
        /// </summary>
        public bool RestoreOrFitViewport(double viewportWidth, double viewportHeight)
        {
            CompositionInstance activeComposition = AutosarApplication.GetInstance().ActiveComposition;
            if (activeComposition == null)
            {
                return false;
            }

            // Try to get saved viewport
            CompositionViewport savedViewport = AutosarApplication.GetInstance().Compositions.GetViewport(activeComposition);

            if (savedViewport != null && savedViewport.IsViewInitialized)
            {
                // Restore the saved viewport
                scene.Context.Scale = savedViewport.Scale;
                scene.Context.Offset = savedViewport.Offset;
                lastFitWasDone = true;
                return true;
            }
            else
            {
                // First time viewing this composition - fit to view
                bool result = FitWorldToImage(viewportWidth, viewportHeight);
                if (result)
                {
                    lastFitWasDone = true;
                }
                return result;
            }
        }
    }
}
