﻿#if WPF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Xml;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Globalization;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections;

#else

using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Globalization;
#endif

using Visifire.Charts;
using Visifire.Commons;


namespace Visifire.Charts
{
    public class Faces
    {
        public Faces()
        {
            VisualComponents = new List<FrameworkElement>();
        }

        /// <summary>
        /// Contains references to individual components of the elements in the visual
        /// </summary>
        public List<FrameworkElement> VisualComponents
        {
            get;
            private set;
        }

        public Panel Visual
        {
            get;
            set;
        }

        public Canvas LabelCanvas
        {
            get;
            set;
        }

    }

    public class ExtendedGraphics
    {
        public ExtendedGraphics()
        {

        }

        #region Static Methods
        /// <summary>
        /// Generates a rectangle. The shape of each of the corners can be controlled and is useful for creating single sided 
        /// curved rectangles.
        /// </summary>
        private static PathGeometry GetRectanglePathGeometry(Double width, Double height, CornerRadius xRadius, CornerRadius yRadius)
        {
            // Create a path geometry object
            PathGeometry pathGeometry = new PathGeometry();

            pathGeometry.Figures = new PathFigureCollection();

            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(xRadius.TopLeft, 0);
            pathFigure.Segments = new PathSegmentCollection();

            // Do not change the order of the lines below
            // Segmens required to create the rectangle
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(width - xRadius.TopRight, 0)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(width, yRadius.TopRight), new Size(xRadius.TopRight, yRadius.TopRight), 0, SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(width, height - yRadius.BottomRight)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(width - xRadius.BottomRight, height), new Size(xRadius.BottomRight, yRadius.BottomRight), 0, SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(xRadius.BottomLeft, height)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(0, height - yRadius.BottomLeft), new Size(xRadius.BottomLeft, yRadius.BottomLeft), 0, SweepDirection.Clockwise));
            pathFigure.Segments.Add(Graphics.GetLineSegment(new Point(0, yRadius.TopLeft)));
            pathFigure.Segments.Add(Graphics.GetArcSegment(new Point(xRadius.TopLeft, 0), new Size(xRadius.TopLeft, yRadius.TopLeft), 0, SweepDirection.Clockwise));

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private static CornerRadius GetCorrectedRadius(CornerRadius radius,Double limit)
        {
           return new CornerRadius(
                ((radius.TopLeft > limit) ? limit : radius.TopLeft),
                ((radius.TopRight > limit) ? limit : radius.TopRight),
                ((radius.BottomRight > limit) ? limit : radius.BottomRight),
                ((radius.BottomLeft > limit) ? limit : radius.BottomLeft)
                );
        }

        private static Brush GetCornerShadowGradientBrush(Corners corner)
        {
            RadialGradientBrush gradBrush = new RadialGradientBrush();
            gradBrush.GradientStops = new GradientStopCollection();
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(191, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform() { ScaleX = 2, ScaleY = 2, CenterX = 0.5, CenterY = 0.5 };
            TranslateTransform tt = null;
            switch (corner)
            {
                case Corners.TopLeft:
                    tt = new TranslateTransform() { X = 0.5, Y = 0.5 };
                    break;
                case Corners.TopRight:
                    tt = new TranslateTransform() { X = -0.5, Y = 0.5 };
                    break;
                case Corners.BottomLeft:
                    tt = new TranslateTransform() { X = 0.5, Y = -0.5 };
                    break;
                case Corners.BottomRight:
                    tt = new TranslateTransform() { X = -0.5, Y = -0.5 };
                    break;
            }
            tg.Children.Add(st);
            tg.Children.Add(tt);
            gradBrush.RelativeTransform = tg;
            return gradBrush;
        }
        private static Brush GetSideShadowGradientBrush(Directions direction)
        {
            LinearGradientBrush gradBrush = new LinearGradientBrush();
            gradBrush.GradientStops = new GradientStopCollection();
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(191, 0, 0, 0), 0));
            gradBrush.GradientStops.Add(Graphics.GetGradientStop(Color.FromArgb(0, 0, 0, 0), 1));
            switch (direction)
            {
                case Directions.Top:
                    gradBrush.StartPoint = new Point(0.5, 1);
                    gradBrush.EndPoint = new Point(0.5, 0);
                    break;
                case Directions.Right:
                    gradBrush.StartPoint = new Point(0, 0.5);
                    gradBrush.EndPoint = new Point(1, 0.5);
                    break;
                case Directions.Left:
                    gradBrush.StartPoint = new Point(1, 0.5);
                    gradBrush.EndPoint = new Point(0, 0.5);
                    break;
                case Directions.Bottom:
                    gradBrush.StartPoint = new Point(0.5, 0);
                    gradBrush.EndPoint = new Point(0.5, 1);
                    break;
            }
            return gradBrush;
        }

        public static DoubleCollection CloneCollection(DoubleCollection collection)
        {
            DoubleCollection newCollection = new DoubleCollection();
            foreach (Double value in collection)
                newCollection.Add(value);

            return newCollection;
        }

        /// <summary>
        /// Creates and returns a rectangle based on the given params
        /// </summary>
        public static Canvas Get2DRectangle(Double width, Double height, Double strokeThickness, DoubleCollection strokeDashArray, Brush stroke, Brush fill, CornerRadius xRadius, CornerRadius yRadius)
        {
            Canvas canvas = new Canvas();

            Path rectangle = new Path();

            canvas.Width = width;
            canvas.Height = height;

            rectangle.StrokeThickness = strokeThickness;
            rectangle.StrokeDashArray = strokeDashArray != null ? CloneCollection(strokeDashArray) : strokeDashArray;
            rectangle.StrokeDashCap = PenLineCap.Flat;
            rectangle.StrokeEndLineCap = PenLineCap.Flat;
            rectangle.StrokeMiterLimit = 1;
            rectangle.StrokeStartLineCap = PenLineCap.Flat;
            rectangle.StrokeLineJoin = PenLineJoin.Bevel;
            rectangle.Stroke = stroke;

            rectangle.Fill = fill;

            rectangle.Data = GetRectanglePathGeometry(
                width,
                height,
                GetCorrectedRadius(xRadius, width),
                GetCorrectedRadius(yRadius, height)
                );

            rectangle.SetValue(Canvas.TopProperty, (Double)0);
            rectangle.SetValue(Canvas.LeftProperty, (Double)0);

            canvas.Children.Add(rectangle);

            return canvas;
        }

        public static Canvas Get2DRectangleBevel(Double width, Double height, Double bevelX, Double bevelY, Brush topBrush, Brush leftBrush, Brush rightBrush, Brush bottomBrush, Double[] Opacities)
        {
            Canvas canvas = Get2DRectangleBevel(width, height, bevelX, bevelY, topBrush, leftBrush, rightBrush, bottomBrush);

            canvas.Children[0].Opacity = Opacities[0];
            canvas.Children[1].Opacity = Opacities[1];
            canvas.Children[2].Opacity = Opacities[2];
            canvas.Children[3].Opacity = Opacities[3];
            return canvas;
        }

        public static Canvas Get2DRectangleBevel(Double width, Double height, Double bevelX,Double bevelY, Brush topBrush, Brush leftBrush, Brush rightBrush, Brush bottomBrush)
        {
            Canvas canvas = new Canvas();

            canvas.Width = width;
            canvas.Height = height;

            Polygon topBevel = new Polygon();
            topBevel.Points = new PointCollection();
            topBevel.Points.Add(new Point(0, 0));
            topBevel.Points.Add(new Point(width, 0));
            topBevel.Points.Add(new Point(width - bevelX , bevelY));
            topBevel.Points.Add(new Point(bevelX, bevelY));
            topBevel.Fill = topBrush;
            canvas.Children.Add(topBevel);

            Polygon leftBevel = new Polygon();
            leftBevel.Points = new PointCollection();
            leftBevel.Points.Add(new Point(0, 0));
            leftBevel.Points.Add(new Point(bevelX, bevelY));
            leftBevel.Points.Add(new Point(bevelX, height - bevelY));
            leftBevel.Points.Add(new Point(0, height));
            leftBevel.Fill = leftBrush;
            canvas.Children.Add(leftBevel);

            Polygon rightBevel = new Polygon();
            rightBevel.Points = new PointCollection();
            rightBevel.Points.Add(new Point(width, 0));
            rightBevel.Points.Add(new Point(width, height));
            rightBevel.Points.Add(new Point(width - bevelX, height - bevelY));
            rightBevel.Points.Add(new Point(width - bevelX, bevelY));
            rightBevel.Fill = rightBrush;
            canvas.Children.Add(rightBevel);

            Polygon bottomBevel = new Polygon();
            bottomBevel.Points = new PointCollection();
            bottomBevel.Points.Add(new Point(0, height));
            bottomBevel.Points.Add(new Point(bevelX,height - bevelY));
            bottomBevel.Points.Add(new Point(width - bevelX, height - bevelY));
            bottomBevel.Points.Add(new Point(width, height));
            bottomBevel.Fill = bottomBrush;
            canvas.Children.Add(bottomBevel);

            return canvas;
        }

        public static Canvas Get2DRectangleGradiance(Double width, Double height, Brush brush1, Brush brush2,Orientation orientation)
        {
            Canvas canvas = new Canvas();

            canvas.Width = width;
            canvas.Height = height;

            if (orientation == Orientation.Vertical)
            {
                Rectangle rectLeft = new Rectangle();
                rectLeft.Width = width / 2 ;
                rectLeft.Height = height;
                rectLeft.SetValue(Canvas.TopProperty, (Double)0);
                rectLeft.SetValue(Canvas.LeftProperty, (Double)0);
                rectLeft.Fill = brush1;
                canvas.Children.Add(rectLeft);

                Rectangle rectRight = new Rectangle();
                rectRight.Width = width / 2;
                rectRight.Height = height;
                rectRight.SetValue(Canvas.TopProperty, (Double)0);
                rectRight.SetValue(Canvas.LeftProperty, (Double)width / 2);
                rectRight.Fill = brush2;
                canvas.Children.Add(rectRight);
            }
            else
            {
                Rectangle rectTop = new Rectangle();
                rectTop.Width = width;
                rectTop.Height = height / 2;
                rectTop.SetValue(Canvas.TopProperty, (Double)0);
                rectTop.SetValue(Canvas.LeftProperty, (Double)0);
                rectTop.Fill = brush1;
                canvas.Children.Add(rectTop);

                Rectangle rectBottom = new Rectangle();
                rectBottom.Width = width;
                rectBottom.Height = height / 2;
                rectBottom.SetValue(Canvas.TopProperty, (Double)height / 2);
                rectBottom.SetValue(Canvas.LeftProperty, (Double)0);
                rectBottom.Fill = brush2;
                canvas.Children.Add(rectBottom);
            }

            return canvas;
        }

        public static PathGeometry Get2DRectangleClip(Double width, Double height, CornerRadius xRadius, CornerRadius yRadius)
        {
            return GetRectanglePathGeometry(width, height, GetCorrectedRadius(xRadius, width), GetCorrectedRadius(yRadius, height));
        }

        public static Grid Get2DRectangleShadow(Double width, Double height, CornerRadius xRadius, CornerRadius yRadius,Double minCurvature)
        {
            CornerRadius tempXRadius = new CornerRadius(Math.Max(xRadius.TopLeft, minCurvature), Math.Max(xRadius.TopRight, minCurvature), Math.Max(xRadius.BottomRight, minCurvature), Math.Max(xRadius.BottomLeft, minCurvature));
            CornerRadius tempYRadius = new CornerRadius(Math.Max(yRadius.TopLeft, minCurvature), Math.Max(yRadius.TopRight, minCurvature), Math.Max(yRadius.BottomRight, minCurvature), Math.Max(yRadius.BottomLeft, minCurvature));

            CornerRadius radiusX = GetCorrectedRadius(tempXRadius, width/2);
            CornerRadius radiusY = GetCorrectedRadius(tempYRadius, height/2);

            Grid visual = new Grid();
            visual.Height = height;
            visual.Width = width;

            for (Int32 index = 0; index < 3; index++)
            {
                visual.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                visual.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
            }

            Rectangle topLeft = new Rectangle() { Width = radiusX.TopLeft, Height = radiusY.TopLeft,Fill = GetCornerShadowGradientBrush(Corners.TopLeft) };
            Rectangle topRight = new Rectangle() { Width = radiusX.TopRight, Height = radiusY.TopRight, Fill = GetCornerShadowGradientBrush(Corners.TopRight) };
            Rectangle bottomLeft = new Rectangle() { Width = radiusX.BottomLeft, Height = radiusY.BottomLeft, Fill = GetCornerShadowGradientBrush(Corners.BottomLeft) };
            Rectangle bottomRight = new Rectangle() { Width = radiusX.BottomRight, Height = radiusY.BottomRight, Fill = GetCornerShadowGradientBrush(Corners.BottomRight) };
            Rectangle center = new Rectangle() { Width = width - radiusX.TopLeft - radiusX.TopRight, Height = height - radiusY.TopLeft - radiusY.BottomLeft, Fill = new SolidColorBrush(Color.FromArgb((Byte)191, (Byte)0, (Byte)0, (Byte)0)) };
            Rectangle top = new Rectangle() { Width = width - radiusX.TopLeft - radiusX.TopRight, Height = Math.Max(radiusY.TopLeft, radiusY.TopRight), Fill = GetSideShadowGradientBrush(Directions.Top) };
            Rectangle right = new Rectangle() { Width = Math.Max(radiusX.TopRight, radiusX.BottomRight), Height = height - radiusY.TopRight - radiusY.BottomRight, Fill = GetSideShadowGradientBrush(Directions.Right) };
            Rectangle left = new Rectangle() { Width = Math.Max(radiusX.TopLeft, radiusX.BottomLeft), Height = height - radiusY.TopLeft - radiusY.BottomLeft, Fill = GetSideShadowGradientBrush(Directions.Left) };
            Rectangle bottom = new Rectangle() { Width = width - radiusX.BottomLeft - radiusX.BottomRight, Height = Math.Max(radiusY.BottomLeft, radiusY.BottomRight), Fill = GetSideShadowGradientBrush(Directions.Bottom) };

            topLeft.SetValue(Grid.RowProperty, (Int32)0); topLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            top.SetValue(Grid.RowProperty, (Int32)0); top.SetValue(Grid.ColumnProperty, (Int32)1);
            topRight.SetValue(Grid.RowProperty, (Int32)0); topRight.SetValue(Grid.ColumnProperty, (Int32)2);
            left.SetValue(Grid.RowProperty, (Int32)1); topLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            center.SetValue(Grid.RowProperty, (Int32)1); center.SetValue(Grid.ColumnProperty, (Int32)1);
            right.SetValue(Grid.RowProperty, (Int32)1); right.SetValue(Grid.ColumnProperty, (Int32)2);
            bottomLeft.SetValue(Grid.RowProperty, (Int32)2); bottomLeft.SetValue(Grid.ColumnProperty, (Int32)0);
            bottom.SetValue(Grid.RowProperty, (Int32)2); bottom.SetValue(Grid.ColumnProperty, (Int32)1);
            bottomRight.SetValue(Grid.RowProperty, (Int32)2); bottomRight.SetValue(Grid.ColumnProperty, (Int32)2);

            visual.Children.Add(topLeft);
            visual.Children.Add(top);
            visual.Children.Add(topRight);
            visual.Children.Add(left);
            visual.Children.Add(center);
            visual.Children.Add(right);
            visual.Children.Add(bottomLeft);
            visual.Children.Add(bottom);
            visual.Children.Add(bottomRight);

            return visual;
        }

        private enum Corners { TopLeft, TopRight, BottomLeft, BottomRight };
        private enum Directions { Top, Left, Right, Bottom };
        #endregion
    }
}

namespace Visifire.Commons
{
    public class Graphics
    {
        public Graphics()
        {

        }

        #region Static Methods
        public static LineSegment GetLineSegment(Point point)
        {
            LineSegment lineSegment = new LineSegment();
            lineSegment.Point = point;
            return lineSegment;
        }

        public static ArcSegment GetArcSegment(Point point, Size size, Double rotation, SweepDirection sweep)
        {
            ArcSegment arcSegment = new ArcSegment();
            arcSegment.Point = point;
            arcSegment.Size = size;
            arcSegment.RotationAngle = 0;
            arcSegment.SweepDirection = SweepDirection.Clockwise;
            return arcSegment;
        }

        public static Double ConvertScale(Double fromScaleMin, Double fromScaleMax, Double fromValue, Double toScaleMin, Double toScaleMax)
        {
            return ((fromValue - fromScaleMin) * (toScaleMax - toScaleMin) / (fromScaleMax - fromScaleMin)) + toScaleMin;
        }
        
        /// <summary>
        /// converts value to pixel position
        /// </summary>
        public static Double ValueToPixelPosition(Double positionMin, Double positionMax, Double valueMin, Double valueMax, Double value)
        {
            return ((value - valueMin) / (valueMax - valueMin)) * (positionMax - positionMin) + positionMin;
        }

        
        /// <summary>
        /// Converts pixel position to value
        /// </summary>
        public static Double PixelPositionToValue(Double positionMin, Double positionMax, Double valueMin, Double valueMax, Double position)
        {
            return ((position - positionMin) / (positionMax - positionMin) * (valueMax - valueMin)) + valueMin;
        }

        public static GradientStop GetGradientStop(Color color,Double stop)
        {
            GradientStop gradStop = new GradientStop();
            gradStop.Color = color;
            gradStop.Offset = stop;
            return gradStop;
        }

        public static Brush CreateLinearGradientBrush(Double angle,Point start,Point end,List<Color> colors,List<Double> stops)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            if (colors.Count != stops.Count)
                throw new Exception("Colors and Stops arrays don't match");

            brush.StartPoint = start;
            brush.EndPoint = end;
            brush.GradientStops = new GradientStopCollection();

            for (Int32 i = 0; i < colors.Count; i++)
            {
                brush.GradientStops.Add(GetGradientStop(colors[i],stops[i]));
            }

            RotateTransform rt = new RotateTransform();
            rt.Angle = angle;
            rt.CenterX = 0.5;
            rt.CenterY = 0.5;
            brush.RelativeTransform = rt;

            return brush;
        }

        public static Brush CreateRadialGradientBrush(List<Color> colors, List<Double> stops)
        {
            RadialGradientBrush brush = new RadialGradientBrush();
            if (colors.Count != stops.Count)
                throw new Exception("Colors and Stops arrays don't match");

            brush.GradientStops = new GradientStopCollection();

            for (Int32 i = 0; i < colors.Count; i++)
            {
                brush.GradientStops.Add(GetGradientStop(colors[i], stops[i]));
            }

            return brush;
        }

        public static Double GetBrushIntensity(Brush brush)
        {
            Color color = new Color();
            Double intensity = 0;
            if (brush == null) return 1;
            if (brush.GetType().Name == "SolidColorBrush")
            {
                color = (brush as SolidColorBrush).Color;
                intensity = (Double)(color.R + color.G + color.B) / (3 * 255);
            }
            else if (brush.GetType().Name == "LinearGradientBrush" || brush.GetType().Name == "RadialGradientBrush")
            {
                foreach (GradientStop grad in (brush as GradientBrush).GradientStops)
                {
                    color = grad.Color;
                    intensity += (Double)(color.R + color.G + color.B) / (3 * 255);
                }

                intensity /= (brush as GradientBrush).GradientStops.Count;
            }
            else
            {
                intensity = 1;
            }
            return intensity;
        }

        public static Brush GetDefaultFontColor(Double intensity)
        {
            Brush brush = null;
            if (intensity < 0.5)
            {
                brush = ParseSolidColor("#EFEFEF");
            }
            else
            {
                brush = ParseSolidColor("#000000");
            }
            return brush;
        }
        
        internal static Brush ApplyLabelFontColor(Chart chart, DataPoint dataPoint, Brush labelFontColor, LabelStyles labelStyle)
        {
            Brush returnBrush = dataPoint.LabelFontColor;

            if (labelFontColor == null)
            {
                Double intensity;

                if (labelStyle == LabelStyles.Inside && dataPoint.Parent.RenderAs != RenderAs.Line)
                {
                    intensity = Graphics.GetBrushIntensity(dataPoint.Color);
                    returnBrush = Graphics.GetDefaultFontColor(intensity);
                }
                else
                {
                    if (chart.PlotArea.Color == null)
                    {
                        if (chart.Background == null)
                        {
                            dataPoint.LabelFontColor = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {   
                            intensity = Graphics.GetBrushIntensity(chart.Background);
                            returnBrush = Graphics.GetDefaultFontColor(intensity);
                        }
                    }
                    else
                    {
                        intensity = Graphics.GetBrushIntensity(chart.PlotArea.Color);
                        returnBrush = Graphics.GetDefaultFontColor(intensity);
                    }
                }
            }

            return returnBrush;
        }

        internal static bool AreBrushesEqual(Brush first, Brush second)
        {
            // If the default comparison is true, that's good enough.
            if (object.Equals(first, second))
            {
                return true;
            }

            // Do a field by field comparison if they're not the same reference
            SolidColorBrush firstSolidColorBrush = first as SolidColorBrush;
            if (firstSolidColorBrush != null)
            {
                SolidColorBrush secondSolidColorBrush = second as SolidColorBrush;
                if (secondSolidColorBrush != null)
                {
                    return object.Equals(firstSolidColorBrush.Color, secondSolidColorBrush.Color);
                }
            }

            return false;
        }

        internal static Brush ApplyAutoFontColor(Chart chart, Brush color, Boolean dockInsidePlotArea)
        {
            Brush brush = color;
            Double intensity;
            if (color == null)
            {
                if (!dockInsidePlotArea)
                {
                    if (chart != null)
                    {
                        if (AreBrushesEqual(chart.Background, new SolidColorBrush(Colors.Transparent)) || chart.Background == null)
                        {
                            brush = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            intensity = Graphics.GetBrushIntensity(chart.Background);
                            brush = Graphics.GetDefaultFontColor(intensity);
                        }
                    }
                }
                else
                {
                    if (chart.PlotArea != null)
                    {
                        if (AreBrushesEqual(chart.PlotArea.Color, new SolidColorBrush(Colors.Transparent)) || chart.PlotArea.Color == null)
                        {
                            if (AreBrushesEqual(chart.Background, new SolidColorBrush(Colors.Transparent)) || chart.Background == null)
                            {
                                brush = new SolidColorBrush(Colors.Black);
                            }
                            else
                            {
                                intensity = Graphics.GetBrushIntensity(chart.Background);
                                brush = Graphics.GetDefaultFontColor(intensity);
                            }
                        }
                        else
                        {
                            intensity = Graphics.GetBrushIntensity(chart.PlotArea.Color);
                            brush = Graphics.GetDefaultFontColor(intensity);
                        }
                    }
                }
            }
            return brush;
        }

        /// <summary>
        /// Converts a color in String form to Solid Color Brush
        /// </summary>
        /// <param name="colorCode"></param>
        /// <returns></returns>
        public static Brush ParseSolidColor(String colorCode)
        {
#if WPF
            return (Brush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(String.Format(System.Globalization.CultureInfo.InvariantCulture, @"<SolidColorBrush xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Color=""{0}""></SolidColorBrush>", colorCode))));
#else
            return (Brush)XamlReader.Load(String.Format(System.Globalization.CultureInfo.InvariantCulture, @"<SolidColorBrush xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Color=""{0}""></SolidColorBrush>", colorCode));
#endif
        }

        public static Brush ParseColor(String colorString)
        {
            String[] splitStr = colorString.Split(';');
            if (splitStr.Length == 1)
                return ParseSolidColor(splitStr[0]);
            else
            {
                String[] str0 = splitStr[0].Split(',');
                String[] str1 = splitStr[1].Split(',');

                if (str0.Length == 1 && str1.Length == 1)
                    return ParseRadialGradient(colorString);
                else
                    return ParseLinearGradient(colorString);
            }
        }

        /// <summary>
        /// This converts a given String of X;Y;color,stop;.... String to a radial gradient brush
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Brush ParseRadialGradient(String str)
        {
            String[] colorStopSet = str.Split(';');

#if WPF
            String brushString = String.Format(@"<RadialGradientBrush xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">");
#else
            String brushString = String.Format(@"<RadialGradientBrush xmlns=""http://schemas.microsoft.com/client/2007"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >");
#endif
            
            brushString += GetGradientStopString(colorStopSet);

            brushString += "</RadialGradientBrush>";

#if WPF
            RadialGradientBrush brush = (RadialGradientBrush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(brushString)));
#else
            RadialGradientBrush brush = (RadialGradientBrush)XamlReader.Load(brushString);
#endif

            brush.GradientOrigin = new Point(Double.Parse(colorStopSet[0], CultureInfo.InvariantCulture), Double.Parse(colorStopSet[1], CultureInfo.InvariantCulture));

            return brush;

        }

        /// <summary>
        /// This converts a given String of angle;color,stop;.... String to a linear gradient brush
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Brush ParseLinearGradient(String str)
        {
            Double angle;

            String[] strSplit = str.Split(';');
            angle = Double.Parse(strSplit[0], CultureInfo.InvariantCulture);
#if WPF
            String brushString = String.Format(@"<LinearGradientBrush xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" EndPoint=""1,0"" StartPoint=""0,0"">");
#else
            String brushString = String.Format(@"<LinearGradientBrush xmlns=""http://schemas.microsoft.com/client/2007"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EndPoint=""1,0"" StartPoint=""0,0"">");
#endif
            
            brushString += GetGradientStopString(strSplit);

            brushString += "</LinearGradientBrush>";
#if WPF
            Brush brush = (Brush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(brushString)));
#else
            Brush brush = (Brush)XamlReader.Load(brushString);
#endif
            RotateTransform rt = new RotateTransform();
            rt.Angle = angle;
            rt.CenterX = .5;
            rt.CenterY = .5;

            // tg.Children.Add(rt);
            brush.RelativeTransform = rt;

            return brush;
        }

        public static String GetGradientStopString(String[] gradStops)
        {
            String stopsString = "";
            foreach (String colorOffset in gradStops)
            {
                String[] colorOffsetSplit = colorOffset.Split(',');

                if (colorOffsetSplit.Length > 1)
                {
                    stopsString += String.Format(@"<GradientStop Color=""" + colorOffsetSplit[0] + @""" Offset=""" + colorOffsetSplit[1] + @"""/>");
                }
            }

            return stopsString;
        }

        /// <summary>
        /// Returns a darker shade of the color by decreasing the brightness by the given intensity value
        /// </summary>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public static Color GetDarkerColor(Color color, Double intensity)
        {
            Color darkerShade = new Color();
            intensity = (intensity < 0 || intensity > 1) ? 1 : intensity;
            darkerShade.R = (Byte)(color.R * intensity);
            darkerShade.G = (Byte)(color.G * intensity);
            darkerShade.B = (Byte)(color.B * intensity);
            darkerShade.A = color.A;
            return darkerShade;
        }

        public static Color GetDarkerColor(Color color, Double intensityR, Double intensityG, Double intensityB)
        {
            Color darkerShade = new Color();
            intensityR = (intensityR < 0 || intensityR > 1) ? 1 : intensityR;
            intensityG = (intensityG < 0 || intensityG > 1) ? 1 : intensityG;
            intensityB = (intensityB < 0 || intensityB > 1) ? 1 : intensityB;
            darkerShade.R = (Byte)(color.R * intensityR);
            darkerShade.G = (Byte)(color.G * intensityG);
            darkerShade.B = (Byte)(color.B * intensityB);
            darkerShade.A = color.A;
            return darkerShade;
        }


        /// <summary>
        /// Returns a lighter shade of the color by increasing the brightness by the given intensity value
        /// </summary>
        /// <param name="color"></param>
        /// <param name="intensity"></param>
        /// <returns></returns>
        public static Color GetLighterColor(Color color, Double intensity)
        {
            Color lighterShade = new Color();
            intensity = (intensity < 0 || intensity > 1) ? 1 : intensity;
            lighterShade.R = (Byte)(256 - ((256 - color.R) * intensity));
            lighterShade.G = (Byte)(256 - ((256 - color.G) * intensity));
            lighterShade.B = (Byte)(256 - ((256 - color.B) * intensity));
            lighterShade.A = color.A;
            return lighterShade;
        }
        public static Color GetLighterColor(Color color, Double intensityR, Double intensityG, Double intensityB)
        {
            Color lighterShade = new Color();
            intensityR = (intensityR < 0 || intensityR > 1) ? 1 : intensityR;
            intensityG = (intensityG < 0 || intensityG > 1) ? 1 : intensityG;
            intensityB = (intensityB < 0 || intensityB > 1) ? 1 : intensityB;
            lighterShade.R = (Byte)(256 - ((256 - color.R) * intensityR));
            lighterShade.G = (Byte)(256 - ((256 - color.G) * intensityG));
            lighterShade.B = (Byte)(256 - ((256 - color.B) * intensityB));
            lighterShade.A = color.A;
            return lighterShade;
        }

        public static DoubleCollection SolidStrokeDashArray = null;
        public static DoubleCollection DashedStrokeDashArray = new DoubleCollection() { 4, 2 };
        public static DoubleCollection DottedStrokeDashArray = new DoubleCollection() { 2, 2 };
        public static DoubleCollection BorderStyleToStrokeDashArray(BorderStyles borderStyle)
        {
            switch (borderStyle)
            {
                case BorderStyles.Solid:
                    return SolidStrokeDashArray;
                case BorderStyles.Dotted:
                    return DottedStrokeDashArray;
                case BorderStyles.Dashed:
                    return DashedStrokeDashArray;
            }

            return SolidStrokeDashArray;
        }

        public static Brush LightingBrush(Boolean lightingEnabled)
        {
            Brush brush;

            if (lightingEnabled)
            {
                String xaml = String.Format(@"<LinearGradientBrush EndPoint=""0.5,1"" StartPoint=""0.5,0"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                                                <GradientStop Color=""#A0FFFFFF"" Offset=""0""/>
                                                <GradientStop Color=""#00FFFFFF"" Offset=""1""/>
                                          </LinearGradientBrush>");

#if WPF
                brush = (Brush)XamlReader.Load(new XmlTextReader(new System.IO.StringReader(xaml)));
#else
                brush = System.Windows.Markup.XamlReader.Load(xaml) as Brush;
#endif
            }
            else
                brush = new SolidColorBrush(Colors.Transparent);

            return brush;
        }

        #endregion

        #region Constants
        public static Double[] DefaultFontSizes = { 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 };
        #endregion
    }
}


