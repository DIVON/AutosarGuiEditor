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
            }
            return needRedraw;
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
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
