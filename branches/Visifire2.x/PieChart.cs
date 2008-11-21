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
using System.Windows.Media.Animation;
#else
using System;
using System.Windows;
using System.Linq;
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
using System.Diagnostics;
#endif

using Visifire.Commons;

namespace Visifire.Charts
{
    internal class ElementPositionData
    {
        #region Public Methods

        public ElementPositionData()
        {
        }

        public ElementPositionData(FrameworkElement element, Double angle1, Double angle2)
        {
            Element = element;
            StartAngle = angle1;
            StopAngle = angle2;
        }

        public ElementPositionData(ElementPositionData m)
        {
            Element = m.Element;
            StartAngle = m.StartAngle;
            StopAngle = m.StopAngle;
        }

        #endregion Public Methods

        #region Static Methods

        public static Int32 CompareAngle(ElementPositionData a, ElementPositionData b)
        {
            Double angle1 = (a.StartAngle + a.StopAngle) / 2;
            Double angle2 = (b.StartAngle + b.StopAngle) / 2;
            return angle1.CompareTo(angle2);
        }

        #endregion Static Methods

        #region Public Properties

        public FrameworkElement Element
        {
            get;
            set;
        }

        public Double StartAngle
        {
            get;
            set;
        }

        public Double StopAngle
        {
            get;
            set;
        }

        #endregion
    }

    internal class SectorChartShapeParams
    {
        private Double _startAngle;
        private Double _stopAngle;
        private Double FixAngle(Double angle)
        {
            while (angle > Math.PI * 2) angle -= Math.PI;
            while (angle < 0) angle += Math.PI;
            return angle;
        }
        internal Double OuterRadius { get; set; }
        internal Double InnerRadius { get; set; }

        internal Double StartAngle
        {
            get
            {
                return _startAngle;
            }
            set
            {
                _startAngle = FixAngle(value);
            }
        }

        internal Double StopAngle
        {
            get
            {
                return _stopAngle;
            }
            set
            {
                _stopAngle = FixAngle(value);
            }
        }

        internal Point Center { get; set; }
        internal Double OffsetX { get; set; }
        internal Double OffsetY { get; set; }
        internal Boolean IsLargerArc { get; set; }
        internal Boolean Lighting { get; set; }
        internal Boolean Bevel { get; set; }
        internal Brush Background { get; set; }
        internal bool IsZero { get; set; }
        internal Double YAxisScaling
        {
            get
            {
                return Math.Sin(TiltAngle);
            }
        }
        internal Double ZAxisScaling
        {
            get
            {
                return Math.Cos(Math.PI / 2 - TiltAngle);
            }
        }
        internal Double Depth { get; set; }
        internal Double ExplodeRatio { get; set; }
        internal Double Width { get; set; }
        internal Double Height { get; set; }
        internal Double TiltAngle { get; set; }
        internal Point LabelPoint { get; set; }
        internal Brush LabelLineColor { get; set; }
        internal Double LabelLineThickness { get; set; }
        internal DoubleCollection LabelLineStyle { get; set; }
        internal Boolean LabelLineEnabled { get; set; }

        internal Double MeanAngle { get; set; }

        internal Storyboard Storyboard { get; set; }
        internal Boolean AnimationEnabled { get; set; }
    }

    internal struct Point3D
    {
        public Double X;
        public Double Y;
        public Double Z;

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString() + "," + Z.ToString();
        }
    }

    internal class PieDoughnut2DPoints
    {
        public PieDoughnut2DPoints()
        {
        }

        public Point Center { get; set; }
        public Point InnerArcStart { get; set; }
        public Point InnerArcMid { get; set; }
        public Point InnerArcEnd { get; set; }
        public Point OuterArcStart { get; set; }
        public Point OuterArcMid { get; set; }
        public Point OuterArcEnd { get; set; }
        public Point LabelLineStartPoint { get; set; }
        public Point LabelLineMidPoint { get; set; }
        public Point LabelLineEndPoint { get; set; }
        public Point LabelPosition { get; set; }
    }

    internal class PieDoughnut3DPoints
    {
        public PieDoughnut3DPoints()
        {
        }
        public Point LabelLineStartPoint { get; set; }
        public Point LabelLineMidPoint { get; set; }
        public Point LabelLineEndPoint { get; set; }
        public Point LabelPosition { get; set; }
    }

    internal class PieChart
    {
        private static Double FixAngle(Double angle)
        {
            while (angle > Math.PI * 2) angle -= Math.PI * 2;
            while (angle < 0) angle += Math.PI * 2;
            return angle;
        }
        private static List<ElementPositionData> _elementPositionData;

        private static Grid CreateLabel(DataPoint dataPoint)
        {
            Grid visual = new Grid() { Background = dataPoint.LabelBackground };
            TextBlock labelText = new TextBlock()
            {
                FontFamily = dataPoint.LabelFontFamily,
                FontSize = (Double)dataPoint.LabelFontSize,
                FontStyle = (FontStyle)dataPoint.LabelFontStyle,
                FontWeight = (FontWeight)dataPoint.LabelFontWeight,
                Text = dataPoint.TextParser(dataPoint.LabelText)
            };

            visual.Children.Add(labelText);

            visual.Measure(new Size(Double.MaxValue, Double.MaxValue));

            dataPoint.LabelVisual = visual;

            return visual;
        }

        internal class PostionData
        {
            public Int32 Index;
            public Double yPosition;
            public Double xPosition;
            public Double MeanAngle;

            public static Int32 CompareYPosition(PostionData a, PostionData b)
            {
                return a.yPosition.CompareTo(b.yPosition);
            }
        }

        private static void PositionLabels(Double totalSum, List<DataPoint> dataPoints, Dictionary<DataPoint, Grid> labels, Size pieSize, Size referenceEllipseSize, Size visualCanvasSize, Double scaleY, Boolean is3D)
        {
            Double hOuterEllipseRadius = referenceEllipseSize.Width / (is3D ? 1 : 2);
            Double vOuterEllipseRadius = referenceEllipseSize.Height / (is3D ? 1 : 2) * scaleY;
            Double hInnerEllipseRadius = (pieSize.Width / (is3D ? 1 : 2)) * 0.7;
            Double vInnerEllipseRadius = (pieSize.Height / (is3D ? 1 : 2)) * 0.7 * scaleY;
            Double hPieRadius = pieSize.Width / (is3D ? 1 : 2);
            Double vPieRadius = pieSize.Height / (is3D ? 1 : 2) * scaleY;

            Dictionary<Int32, PostionData> rightPositionData = new Dictionary<int, PostionData>();
            Dictionary<Int32, PostionData> leftPositionData = new Dictionary<int, PostionData>();
            Dictionary<Int32, PostionData> tempPositionData = new Dictionary<int, PostionData>();

            Int32 index = 0;
            Int32 rightIndex = 0;
            Int32 leftIndex = 0;

            Double startAngle = FixAngle(dataPoints[0].Parent.StartAngle);
            Double stopAngle = 0;
            Double meanAngle = 0;

            Double xPos = 0;
            Double yPos = 0;

            Double centerX = visualCanvasSize.Width / 2;
            Double centerY = visualCanvasSize.Height / 2;

            Double gapLeft = 0;
            Double gapRight = 0;


            foreach (DataPoint dataPoint in dataPoints)
            {
                stopAngle = startAngle + Math.PI * 2 * Math.Abs(dataPoint.YValue) / totalSum;
                meanAngle = (startAngle + stopAngle) / 2;

                centerX = visualCanvasSize.Width / 2;
                centerY = visualCanvasSize.Height / 2;

                if (dataPoint.LabelStyle == LabelStyles.Inside)
                {

                    xPos = centerX + hInnerEllipseRadius * Math.Cos(meanAngle) - labels[dataPoint].DesiredSize.Width / 2;
                    yPos = centerY + vInnerEllipseRadius * Math.Sin(meanAngle) - labels[dataPoint].DesiredSize.Height / 2;
                    labels[dataPoint].SetValue(Canvas.TopProperty, yPos);
                    labels[dataPoint].SetValue(Canvas.LeftProperty, xPos);
                }
                else
                {
                    xPos = centerX + hOuterEllipseRadius * Math.Cos(meanAngle);
                    yPos = centerY + vOuterEllipseRadius * Math.Sin(meanAngle);

                    //Debug.WriteLine("Y: " + yPos + " A: " + meanAngle);

                    if (xPos < centerX)
                    {
                        xPos -= labels[dataPoint].DesiredSize.Width + 10;
                        leftPositionData.Add(leftIndex++, new PostionData() { Index = index, xPosition = xPos, yPosition = yPos, MeanAngle = meanAngle });
                        gapLeft = Math.Max(gapLeft, labels[dataPoint].DesiredSize.Height);
                    }
                    else
                    {
                        xPos += 10;
                        rightPositionData.Add(rightIndex++, new PostionData() { Index = index, xPosition = xPos, yPosition = yPos, MeanAngle = meanAngle });
                        gapRight = Math.Max(gapRight, labels[dataPoint].DesiredSize.Height);
                    }
                }

                startAngle = stopAngle;
                index++;

            }



            PostionData tempData;

            Double minimumY;
            Double maximumY;
            Double extent;
            if (is3D)
            {
                minimumY = centerY - vOuterEllipseRadius;
                maximumY = centerY + vOuterEllipseRadius;
            }
            else
            {
                minimumY = gapLeft / 2;
                maximumY = visualCanvasSize.Height - gapLeft / 2;
            }

            Double maxGapBetweenLabels = ((maximumY - minimumY) - (gapLeft * leftPositionData.Count)) / leftPositionData.Count;
            PositionLabels(minimumY, maximumY, 0, maxGapBetweenLabels, leftIndex, leftPositionData, false);
            for (Int32 i = 0; i < leftIndex; i++)
            {
                leftPositionData.TryGetValue(i, out tempData);

                centerX = visualCanvasSize.Width / 2;
                centerY = visualCanvasSize.Height / 2;

                extent = Math.Max(centerY - minimumY, maximumY - centerY);
                if (is3D)
                {
                    tempData.xPosition = centerX - Math.Sqrt((1 - Math.Pow((tempData.yPosition - centerY) / extent, 2)) * Math.Pow(hOuterEllipseRadius, 2)) - labels[dataPoints[tempData.Index]].DesiredSize.Width - 10;
                }
                else
                    tempData.xPosition = centerX - hOuterEllipseRadius * Math.Cos(Math.Asin(Math.Abs(tempData.yPosition - centerY) / hOuterEllipseRadius)) - labels[dataPoints[tempData.Index]].DesiredSize.Width - 10;

                if (tempData.xPosition < 0)
                    tempData.xPosition = 2;
                if (tempData.yPosition + labels[dataPoints[tempData.Index]].DesiredSize.Height > visualCanvasSize.Height)
                    tempData.yPosition = visualCanvasSize.Height - labels[dataPoints[tempData.Index]].DesiredSize.Height / 2;
                if (tempData.yPosition < labels[dataPoints[tempData.Index]].DesiredSize.Height / 2)
                    tempData.yPosition = labels[dataPoints[tempData.Index]].DesiredSize.Height / 2;

                if ((bool)dataPoints[tempData.Index].LabelEnabled)
                {
                    labels[dataPoints[tempData.Index]].SetValue(Canvas.TopProperty, tempData.yPosition - labels[dataPoints[tempData.Index]].DesiredSize.Height / 2);

                    labels[dataPoints[tempData.Index]].SetValue(Canvas.LeftProperty, tempData.xPosition);

                }
            }


            PostionData[] dataForSorting = rightPositionData.Values.ToArray();
            Array.Sort(dataForSorting, PostionData.CompareYPosition);
            rightPositionData.Clear();
            for (int i = 0; i < dataForSorting.Length; i++)
                rightPositionData.Add(i, dataForSorting[i]);

            if (is3D)
            {
                minimumY = centerY - vOuterEllipseRadius;
                maximumY = centerY + vOuterEllipseRadius;

            }
            else
            {
                minimumY = gapRight / 2;
                maximumY = visualCanvasSize.Height - gapRight / 2;
            }
            maxGapBetweenLabels = ((maximumY - minimumY) - (gapRight * rightPositionData.Count)) / rightPositionData.Count;
            PositionLabels(minimumY, maximumY, 0, maxGapBetweenLabels, rightIndex, rightPositionData, true);

            for (Int32 i = 0; i < rightIndex; i++)
            {
                rightPositionData.TryGetValue(i, out tempData);

                centerX = visualCanvasSize.Width / 2;
                centerY = visualCanvasSize.Height / 2;

                extent = Math.Max(centerY - minimumY, maximumY - centerY);
                if (is3D)
                {
                    tempData.xPosition = centerX + Math.Sqrt((1 - Math.Pow((tempData.yPosition - centerY) / extent, 2)) * Math.Pow(hOuterEllipseRadius, 2)) + 10;
                }
                else
                    tempData.xPosition = centerX + hOuterEllipseRadius * Math.Cos(Math.Asin(Math.Abs(tempData.yPosition - centerY) / hOuterEllipseRadius)) + 10;

                if (tempData.xPosition + labels[dataPoints[tempData.Index]].DesiredSize.Width > visualCanvasSize.Width)
                    tempData.xPosition = visualCanvasSize.Width - labels[dataPoints[tempData.Index]].DesiredSize.Width;
                if (tempData.yPosition + labels[dataPoints[tempData.Index]].DesiredSize.Height > visualCanvasSize.Height)
                    tempData.yPosition = visualCanvasSize.Height - labels[dataPoints[tempData.Index]].DesiredSize.Height;
                if (tempData.yPosition < labels[dataPoints[tempData.Index]].DesiredSize.Height / 2)
                    tempData.yPosition = labels[dataPoints[tempData.Index]].DesiredSize.Height / 2;

                if ((bool)dataPoints[tempData.Index].LabelEnabled)
                {
                    labels[dataPoints[tempData.Index]].SetValue(Canvas.TopProperty, tempData.yPosition - labels[dataPoints[tempData.Index]].DesiredSize.Height / 2);

                    labels[dataPoints[tempData.Index]].SetValue(Canvas.LeftProperty, tempData.xPosition);
                }
            }
        }

        private static void PositionLabels(Double minY, Double maxY, Double gap, Double maxGap, Double labelCount, Dictionary<Int32, PostionData> labelPositions, Boolean isRight)
        {
            Double limit = (isRight) ? minY : maxY;
            Double sign = (isRight) ? -1 : 1;
            Int32 iterationCount = 0;
            Boolean isOverlap = false;
            Double previousY;
            Double currentY;
            PostionData point;
            //Double offsetFactor = sign * ((gap > maxGap) ? maxGap / 2 : gap / 2);
            Double offsetFactor = sign * ((gap > maxGap) ? maxGap / 10 : gap / 10);
            do
            {
                previousY = limit;
                isOverlap = false;

                for (Int32 i = 0; i < labelCount; i++)
                {
                    labelPositions.TryGetValue(i, out point);
                    currentY = point.yPosition;

                    if (Math.Abs(previousY - currentY) < gap && i != 0)
                    {

                        point.yPosition = previousY - offsetFactor;
                        if (isRight)
                        {
                            if (point.yPosition > maxY) point.yPosition = (previousY + maxY - gap) / 2;
                        }
                        else
                        {
                            if (point.yPosition < minY) point.yPosition = (minY + previousY) / 2;
                        }
                        currentY = point.yPosition;

                        labelPositions.Remove(i);
                        //Debug.WriteLine("Y: " + point.yPosition + " A: " + point.MeanAngle);
                        labelPositions.Add(i, new PostionData() { Index = point.Index, MeanAngle = point.MeanAngle, xPosition = point.xPosition, yPosition = point.yPosition });

                        labelPositions.TryGetValue(i - 1, out point);
                        point.yPosition = previousY + offsetFactor;

                        if (isRight)
                        {
                            if (point.yPosition < minY) point.yPosition = (minY + previousY) / 2;
                        }
                        else
                        {
                            if (point.yPosition > maxY) point.yPosition = (previousY + maxY - gap) / 2;
                        }
                        //Debug.WriteLine("Y: " + point.yPosition + " A: " + point.MeanAngle);
                        labelPositions.Remove(i - 1);
                        labelPositions.Add(i - 1, new PostionData() { Index = point.Index, MeanAngle = point.MeanAngle, xPosition = point.xPosition, yPosition = point.yPosition });
                        isOverlap = true;
                        if (isRight)
                        {
                            if (previousY < currentY) isOverlap = true;
                        }
                        else
                        {
                            if (previousY > currentY) isOverlap = true;
                        }
                        break;
                    }

                    previousY = currentY;
                }
                iterationCount++;

            } while (isOverlap && iterationCount < 128);

            if (isOverlap)
            {
                Double stepSize = (maxY - minY) / labelCount;
                for (Int32 i = 0; i < labelCount; i++)
                {
                    labelPositions.TryGetValue(i, out point);
                    if (isRight)
                    {
                        point.yPosition = minY + stepSize * i;
                    }
                    else
                    {
                        point.yPosition = maxY - stepSize * (i + 1);
                    }
                    //Debug.WriteLine("Y: " + point.yPosition + " A: " + point.MeanAngle);
                    labelPositions.Remove(i);
                    labelPositions.Add(i, new PostionData() { Index = point.Index, MeanAngle = point.MeanAngle, xPosition = point.xPosition, yPosition = point.yPosition });
                }
            }
        }

        private static Canvas CreateAndPositionLabels(Double totalSum, List<DataPoint> dataPoints, Double width, Double height, Double scaleY, Boolean is3D, ref Size size)
        {
            Canvas visual = new Canvas();

            Dictionary<DataPoint, Grid> labels = new Dictionary<DataPoint, Grid>();

            Double labelLineLength = 30;

            Boolean isLabelEnabled = false;
            Boolean isLabelOutside = false;
            Double maxLabelWidth = 0;
            Double maxLabelHeight = 0;

            foreach (DataPoint dataPoint in dataPoints)
            {
                Grid label = CreateLabel(dataPoint);
                if ((bool)dataPoint.LabelEnabled)
                {
                    maxLabelWidth = Math.Max(maxLabelWidth, label.DesiredSize.Width);
                    maxLabelHeight = Math.Max(maxLabelHeight, label.DesiredSize.Height);

                    isLabelEnabled = true;

                    if (dataPoint.LabelStyle == LabelStyles.OutSide)
                        isLabelOutside = true;
                }

                labels.Add(dataPoint, label);

                if (isLabelEnabled)
                    visual.Children.Add(label);
            }

            //this is to offset the label to draw label line
            maxLabelWidth += 10;

            Double pieCanvasWidth = 0;
            Double pieCanvasHeight = 0;

            Double labelEllipseWidth = 0;
            Double labelEllipseHeight = 0;

            Double minLength = (width - maxLabelWidth) * scaleY;
            if ((width - maxLabelWidth) > height)
            {
                minLength = Math.Min(minLength, height);
            }




            if (isLabelEnabled)
            {
                if (isLabelOutside)
                {
                    //pieCanvasWidth = minLength - 4 - labelLineLength ;
                    //pieCanvasHeight = minLength - 4 - labelLineLength ;
                    pieCanvasWidth = minLength - labelLineLength * 2;
                    pieCanvasHeight = pieCanvasWidth;

                    labelEllipseWidth = minLength;
                    labelEllipseHeight = labelEllipseWidth;
                    //labelEllipseWidth = pieCanvasWidth + maxLabelWidth;
                    //labelEllipseHeight = pieCanvasHeight + maxLabelHeight;

                    PositionLabels(totalSum, dataPoints, labels, new Size(Math.Abs(pieCanvasWidth), Math.Abs(pieCanvasHeight)), new Size(Math.Abs(labelEllipseWidth), Math.Abs(labelEllipseHeight)), new Size(width, height), scaleY, is3D);
                }
                else
                {
                    pieCanvasWidth = minLength;
                    pieCanvasHeight = minLength;

                    labelEllipseWidth = pieCanvasWidth;
                    labelEllipseHeight = pieCanvasHeight;

                    PositionLabels(totalSum, dataPoints, labels, new Size(Math.Abs(pieCanvasWidth), Math.Abs(pieCanvasHeight)), new Size(Math.Abs(labelEllipseWidth), Math.Abs(labelEllipseHeight)), new Size(width, height), scaleY, is3D);
                }
            }
            else
            {
                pieCanvasWidth = minLength;
                pieCanvasHeight = minLength;
            }

            size = new Size(Math.Abs(pieCanvasWidth), Math.Abs(pieCanvasHeight));

            if (isLabelEnabled)
                return visual;
            else
                return null;
        }

        internal static Canvas GetVisualObjectForPieChart(Double width, Double height, PlotDetails plotDetails, List<DataSeries> seriesList, Chart chart, bool animationEnabled)
        {
            if (Double.IsNaN(width) || Double.IsNaN(height) || width <= 0 || height <= 0) return null;

            Debug.WriteLine("PieStart: " + DateTime.Now.ToLongTimeString());

            Canvas visual = new Canvas();
            visual.Width = width;
            visual.Height = height;
            DataSeries series = seriesList[0];

            if (series.Enabled == false)
                return visual;

            List<DataPoint> enabledDataPoints = (from datapoint in series.DataPoints where datapoint.Enabled == true select datapoint).ToList();
            Double absoluteSum = plotDetails.GetAbsoluteSumOfDataPoints(enabledDataPoints);
            absoluteSum = (absoluteSum == 0) ? 1 : absoluteSum;

            Double centerX = width / 2;
            Double centerY = height / 2;

            Double offsetX = 0;
            Double offsetY = 0;
            Boolean IsLabelEnabled;

            Size pieCanvas = new Size();
            Canvas labelCanvas = CreateAndPositionLabels(absoluteSum, enabledDataPoints, width, height, ((chart.View3D) ? 0.4 : 1), chart.View3D, ref pieCanvas);

            Debug.WriteLine("Labels Positioning over: " + DateTime.Now.ToLongTimeString());

            if (labelCanvas == null)
                IsLabelEnabled = false;
            else
                IsLabelEnabled = true;

            Double radius = Math.Min(pieCanvas.Width, pieCanvas.Height) / (chart.View3D ? 1 : 2);
            Double startAngle = series.StartAngle;
            Double endAngle = 0;
            Double angle;
            Double absoluteYValue;
            Double meanAngle = 0;
            Int32 zindex = 0;

            if (chart.View3D)
                _elementPositionData = new List<ElementPositionData>();

            if (series.Storyboard == null)
                series.Storyboard = new Storyboard();

            DataSeriesRef = series;

            foreach (DataPoint dataPoint in enabledDataPoints)
            {
                DataPointRef = dataPoint;

                if (Double.IsNaN(dataPoint.YValue))
                    continue;

                absoluteYValue = Math.Abs(dataPoint.YValue);

                angle = (absoluteYValue / absoluteSum) * Math.PI * 2;

                endAngle = startAngle + angle;
                meanAngle = (startAngle + endAngle) / 2;

                SectorChartShapeParams pieParams = new SectorChartShapeParams();
                pieParams.Storyboard = series.Storyboard;
                pieParams.AnimationEnabled = animationEnabled;
                pieParams.Center = new Point(centerX, centerY);
                pieParams.ExplodeRatio = 0.2;
                pieParams.InnerRadius = 0;
                pieParams.OuterRadius = radius;
                pieParams.StartAngle = (startAngle) % (Math.PI * 2);
                pieParams.StopAngle = (endAngle) % (Math.PI * 2);
                pieParams.Lighting = (Boolean)dataPoint.LightingEnabled;
                pieParams.Bevel = series.Bevel;
                pieParams.IsLargerArc = (angle / (Math.PI)) > 1;
                pieParams.Background = dataPoint.Color;
                pieParams.Width = width;
                pieParams.Height = height;
                pieParams.TiltAngle = Math.Asin(0.4);
                pieParams.Depth = 20 / pieParams.YAxisScaling;

                pieParams.MeanAngle = meanAngle;
                pieParams.LabelLineEnabled = (Boolean)dataPoint.LabelLineEnabled;
                pieParams.LabelLineColor = dataPoint.LabelLineColor;
                pieParams.LabelLineThickness = (Double)dataPoint.LabelLineThickness;
                pieParams.LabelLineStyle = dataPoint.GetDashArray((LineStyles)dataPoint.LabelLineStyle);
                pieParams.IsZero = (dataPoint.YValue == 0);

                offsetX = radius * pieParams.ExplodeRatio * Math.Cos(meanAngle);
                offsetY = radius * pieParams.ExplodeRatio * Math.Sin(meanAngle);
                pieParams.OffsetX = offsetX;
                pieParams.OffsetY = offsetY * (chart.View3D ? pieParams.YAxisScaling : 1);

                if (dataPoint.LabelVisual != null)
                {
                    if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                    {
                        pieParams.LabelPoint = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) + dataPoint.LabelVisual.DesiredSize.Width, (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty) + dataPoint.LabelVisual.DesiredSize.Height / 2);
                    }
                    else
                    {
                        pieParams.LabelPoint = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty) + dataPoint.LabelVisual.DesiredSize.Height / 2);
                    }

                    // apply animation to the labels
                    if (animationEnabled)
                    {
                        series.Storyboard = CreateOpacityAnimation(series.Storyboard, dataPoint.LabelVisual, 2, 1, 0.5);
                        dataPoint.LabelVisual.Opacity = 0;
                    }
                }

                Faces faces = new Faces();

                if (chart.View3D)
                {
                    PieDoughnut3DPoints unExplodedPoints = new PieDoughnut3DPoints();
                    PieDoughnut3DPoints explodedPoints = new PieDoughnut3DPoints();
                    List<Path> pieFaces = GetPie3D(pieParams, ref zindex, ref unExplodedPoints, ref explodedPoints, ref dataPoint._labelLine);

                    foreach (Path path in pieFaces)
                    {
                        if (path != null)
                        {
                            visual.Children.Add(path);
                            faces.VisualComponents.Add(path);
                            path.RenderTransform = new TranslateTransform();
                            // apply animation to the 3D sections
                            if (animationEnabled)
                            {
                                series.Storyboard = CreateOpacityAnimation(series.Storyboard, path, 1.0 / (series.DataPoints.Count) * (series.DataPoints.IndexOf(dataPoint)), dataPoint.Opacity, 0.5);
                                path.Opacity = 0;
                            }
                        }

                    }
                    if (dataPoint._labelLine != null)
                    {
                        dataPoint._labelLine.RenderTransform = new TranslateTransform();
                        visual.Children.Add(dataPoint._labelLine);
                        faces.VisualComponents.Add(dataPoint._labelLine);
                    }
                    faces.Visual = visual;
                    if (dataPoint.LabelVisual != null)
                    {
                        unExplodedPoints.LabelPosition = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty));
                        if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                        else
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                    }

                    dataPoint.ExplodeAnimation = new Storyboard();
                    dataPoint.ExplodeAnimation = CreateExplodingOut3DAnimation(dataPoint.ExplodeAnimation, pieFaces, dataPoint.LabelVisual, dataPoint._labelLine, unExplodedPoints, explodedPoints, pieParams.OffsetX, pieParams.OffsetY);
                    dataPoint.UnExplodeAnimation = new Storyboard();
                    dataPoint.UnExplodeAnimation = CreateExplodingIn3DAnimation(dataPoint.UnExplodeAnimation, pieFaces, dataPoint.LabelVisual, dataPoint._labelLine, unExplodedPoints, explodedPoints, pieParams.OffsetX, pieParams.OffsetY);
                }
                else
                {
                    PieDoughnut2DPoints unExplodedPoints = new PieDoughnut2DPoints();
                    PieDoughnut2DPoints explodedPoints = new PieDoughnut2DPoints();

                    Canvas pieVisual = GetPie2D(pieParams, ref unExplodedPoints, ref explodedPoints, ref dataPoint._labelLine);

                    if (dataPoint.LabelVisual != null)
                    {
                        unExplodedPoints.LabelPosition = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty));
                        if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                        else
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                    }
                    TranslateTransform translateTransform = new TranslateTransform();
                    pieVisual.RenderTransform = translateTransform;
                    dataPoint.ExplodeAnimation = new Storyboard();
                    dataPoint.ExplodeAnimation = CreateExplodingOut2DAnimation(dataPoint.ExplodeAnimation, pieVisual, dataPoint.LabelVisual, dataPoint._labelLine, translateTransform, unExplodedPoints, explodedPoints, offsetX, offsetY);
                    dataPoint.UnExplodeAnimation = new Storyboard();
                    dataPoint.UnExplodeAnimation = CreateExplodingIn2DAnimation(dataPoint.UnExplodeAnimation, pieVisual, dataPoint.LabelVisual, dataPoint._labelLine, translateTransform, unExplodedPoints, explodedPoints, offsetX, offsetY);


                    pieVisual.SetValue(Canvas.TopProperty, height / 2 - pieVisual.Height / 2);
                    pieVisual.SetValue(Canvas.LeftProperty, width / 2 - pieVisual.Width / 2);
                    visual.Children.Add(pieVisual);
                    faces.VisualComponents.Add(pieVisual);
                    faces.Visual = pieVisual;
                }

                Debug.WriteLine("Datapoint" + enabledDataPoints.IndexOf(dataPoint) + ": " + DateTime.Now.ToLongTimeString());

                dataPoint.Faces = faces;

                startAngle = endAngle;
            }

            if (chart.View3D)
            {
                Int32 zindex1, zindex2;

                _elementPositionData.Sort(ElementPositionData.CompareAngle);
                zindex1 = 1000;
                zindex2 = -1000;
                
                for (Int32 i = 0; i < _elementPositionData.Count; i++)
                {
                    SetZIndex(_elementPositionData[i].Element, ref zindex1, ref zindex2, _elementPositionData[i].StartAngle);
                }
            }


            if (IsLabelEnabled)
                visual.Children.Add(labelCanvas);

            return visual;
        }

        internal static Canvas GetVisualObjectForDoughnutChart(Double width, Double height, PlotDetails plotDetails, List<DataSeries> seriesList, Chart chart, bool animationEnabled)
        {
            if (Double.IsNaN(width) || Double.IsNaN(height) || width <= 0 || height <= 0) return null;

            Canvas visual = new Canvas();
            visual.Width = width;
            visual.Height = height;

            DataSeries series = seriesList[0];
            if (series.Enabled == false)
                return visual;

            List<DataPoint> enabledDataPoints = (from datapoint in series.DataPoints where datapoint.Enabled == true select datapoint).ToList();
            Double absoluteSum = plotDetails.GetAbsoluteSumOfDataPoints(enabledDataPoints);

            absoluteSum = (absoluteSum == 0) ? 1 : absoluteSum;

            Double centerX = width / 2;
            Double centerY = height / 2;

            Double offsetX = 0;
            Double offsetY = 0;

            Size pieCanvas = new Size();
            Canvas labelCanvas = CreateAndPositionLabels(absoluteSum, enabledDataPoints, width, height, ((chart.View3D) ? 0.4 : 1), chart.View3D, ref pieCanvas);

            Double radius = Math.Min(pieCanvas.Width, pieCanvas.Height) / (chart.View3D ? 1 : 2);
            Double startAngle = series.StartAngle;
            Double endAngle = 0;
            Double angle;
            Double meanAngle;
            Double absoluteYValue;
            Double radiusDiff = 0;

            var explodedDataPoints = (from datapoint in series.DataPoints where datapoint.Exploded == true select datapoint);
            radiusDiff = (explodedDataPoints.Count() > 0) ? radius * 0.3 : 0;

            //radius -= radiusDiff;
            if (chart.View3D)
                _elementPositionData = new List<ElementPositionData>();


            if (series.Storyboard == null)
                series.Storyboard = new Storyboard();

            DataSeriesRef = series;

            foreach (DataPoint dataPoint in enabledDataPoints)
            {
                DataPointRef = dataPoint;

                if (Double.IsNaN(dataPoint.YValue))
                    continue;

                absoluteYValue = Math.Abs(dataPoint.YValue);

                angle = (absoluteYValue / absoluteSum) * Math.PI * 2;

                endAngle = startAngle + angle;
                meanAngle = (startAngle + endAngle) / 2;

                SectorChartShapeParams pieParams = new SectorChartShapeParams();
                pieParams.AnimationEnabled = animationEnabled;
                pieParams.Storyboard = series.Storyboard;
                pieParams.ExplodeRatio = 0.2;
                pieParams.Center = new Point(centerX, centerY);

                pieParams.InnerRadius = radius / 2;
                pieParams.OuterRadius = radius;
                pieParams.StartAngle = (startAngle) % (Math.PI * 2);
                pieParams.StopAngle = (endAngle) % (Math.PI * 2);
                pieParams.Lighting = (Boolean)dataPoint.LightingEnabled;
                pieParams.Bevel = series.Bevel;
                pieParams.IsLargerArc = (angle / (Math.PI)) > 1;
                pieParams.Background = dataPoint.Color;
                pieParams.Width = width;
                pieParams.Height = height;
                pieParams.TiltAngle = Math.Asin(0.4);
                pieParams.Depth = 20 / pieParams.YAxisScaling;

                pieParams.MeanAngle = meanAngle;
                pieParams.LabelLineEnabled = (Boolean)dataPoint.LabelLineEnabled;
                pieParams.LabelLineColor = dataPoint.LabelLineColor;
                pieParams.LabelLineThickness = (Double)dataPoint.LabelLineThickness;
                pieParams.LabelLineStyle = dataPoint.GetDashArray((LineStyles)dataPoint.LabelLineStyle);

                offsetX = radius * pieParams.ExplodeRatio * Math.Cos(meanAngle);
                offsetY = radius * pieParams.ExplodeRatio * Math.Sin(meanAngle);
                pieParams.OffsetX = offsetX;
                pieParams.OffsetY = offsetY * (chart.View3D ? pieParams.YAxisScaling : 1);

                if (dataPoint.LabelVisual != null)
                {
                    if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                    {
                        pieParams.LabelPoint = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) + dataPoint.LabelVisual.DesiredSize.Width, (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty) + dataPoint.LabelVisual.DesiredSize.Height / 2);
                    }
                    else
                    {
                        pieParams.LabelPoint = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty) + dataPoint.LabelVisual.DesiredSize.Height / 2);
                    }
                    // apply animation to the labels
                    if (animationEnabled)
                    {
                        series.Storyboard = CreateOpacityAnimation(series.Storyboard, dataPoint.LabelVisual, 2, 1, 0.5);
                        dataPoint.LabelVisual.Opacity = 0;
                    }
                }

                Faces faces = new Faces();
                if (chart.View3D)
                {
                    PieDoughnut3DPoints unExplodedPoints = new PieDoughnut3DPoints();
                    PieDoughnut3DPoints explodedPoints = new PieDoughnut3DPoints();
                    List<Path> pieFaces = GetDoughnut3D(pieParams, ref unExplodedPoints, ref explodedPoints, ref dataPoint._labelLine);

                    foreach (Path path in pieFaces)
                    {
                        if (path != null)
                        {
                            visual.Children.Add(path);
                            faces.VisualComponents.Add(path);
                            path.RenderTransform = new TranslateTransform();
                            // apply animation to the 3D sections
                            if (animationEnabled)
                            {
                                series.Storyboard = CreateOpacityAnimation(series.Storyboard, path, 1.0 / (series.DataPoints.Count) * (series.DataPoints.IndexOf(dataPoint)), dataPoint.Opacity, 0.5);
                                path.Opacity = 0;
                            }
                        }

                    }
                    if (dataPoint._labelLine != null)
                    {
                        dataPoint._labelLine.RenderTransform = new TranslateTransform();
                        visual.Children.Add(dataPoint._labelLine);
                        faces.VisualComponents.Add(dataPoint._labelLine);
                    }
                    faces.Visual = visual;

                    if (dataPoint.LabelVisual != null)
                    {
                        unExplodedPoints.LabelPosition = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty));
                        if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                        else
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                    }

                    dataPoint.ExplodeAnimation = new Storyboard();
                    dataPoint.ExplodeAnimation = CreateExplodingOut3DAnimation(dataPoint.ExplodeAnimation, pieFaces, dataPoint.LabelVisual, dataPoint._labelLine, unExplodedPoints, explodedPoints, pieParams.OffsetX, pieParams.OffsetY);
                    dataPoint.UnExplodeAnimation = new Storyboard();
                    dataPoint.UnExplodeAnimation = CreateExplodingIn3DAnimation(dataPoint.UnExplodeAnimation, pieFaces, dataPoint.LabelVisual, dataPoint._labelLine, unExplodedPoints, explodedPoints, pieParams.OffsetX, pieParams.OffsetY);
                }
                else
                {
                    PieDoughnut2DPoints unExplodedPoints = new PieDoughnut2DPoints();
                    PieDoughnut2DPoints explodedPoints = new PieDoughnut2DPoints();

                    Canvas pieVisual = GetDoughnut2D(pieParams, ref unExplodedPoints, ref explodedPoints, ref dataPoint._labelLine);

                    if (dataPoint.LabelVisual != null)
                    {
                        unExplodedPoints.LabelPosition = new Point((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty), (Double)dataPoint.LabelVisual.GetValue(Canvas.TopProperty));
                        if ((Double)dataPoint.LabelVisual.GetValue(Canvas.LeftProperty) < width / 2)
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                        else
                        {
                            explodedPoints.LabelPosition = new Point(unExplodedPoints.LabelPosition.X + offsetX, unExplodedPoints.LabelPosition.Y);
                        }
                    }
                    TranslateTransform translateTransform = new TranslateTransform();
                    pieVisual.RenderTransform = translateTransform;
                    dataPoint.ExplodeAnimation = new Storyboard();
                    dataPoint.ExplodeAnimation = CreateExplodingOut2DAnimation(dataPoint.ExplodeAnimation, pieVisual, dataPoint.LabelVisual, dataPoint._labelLine, translateTransform, unExplodedPoints, explodedPoints, offsetX, offsetY);
                    dataPoint.UnExplodeAnimation = new Storyboard();
                    dataPoint.UnExplodeAnimation = CreateExplodingIn2DAnimation(dataPoint.UnExplodeAnimation, pieVisual, dataPoint.LabelVisual, dataPoint._labelLine, translateTransform, unExplodedPoints, explodedPoints, offsetX, offsetY);


                    pieVisual.SetValue(Canvas.TopProperty, height / 2 - pieVisual.Height / 2);
                    pieVisual.SetValue(Canvas.LeftProperty, width / 2 - pieVisual.Width / 2);
                    visual.Children.Add(pieVisual);
                    faces.VisualComponents.Add(pieVisual);
                    faces.Visual = pieVisual;
                }

                dataPoint.Faces = faces;

                startAngle = endAngle;
            }

            if (chart.View3D)
            {
                Int32 zindex1, zindex2;
                _elementPositionData.Sort(ElementPositionData.CompareAngle);
                zindex1 = 1000;
                zindex2 = -1000;
                for (Int32 i = 0; i < _elementPositionData.Count; i++)
                {
                    SetZIndex(_elementPositionData[i].Element, ref zindex1, ref zindex2, _elementPositionData[i].StartAngle);
                }
            }

            visual.Children.Add(labelCanvas);

            return visual;
        }

        private static Canvas GetPie2D(SectorChartShapeParams pieParams, ref PieDoughnut2DPoints unExplodedPoints, ref PieDoughnut2DPoints explodedPoints, ref Path labelLinePath)
        {
            Canvas visual = new Canvas();

            Double width = pieParams.OuterRadius * 2;
            Double height = pieParams.OuterRadius * 2;

            visual.Width = width;
            visual.Height = height;

            Point center = new Point(width / 2, height / 2);
            Double xOffset = pieParams.OuterRadius * pieParams.ExplodeRatio * Math.Cos(pieParams.MeanAngle);
            Double yOffset = pieParams.OuterRadius * pieParams.ExplodeRatio * Math.Sin(pieParams.MeanAngle);

            #region PieSlice
            if (pieParams.StartAngle != pieParams.StopAngle || !pieParams.IsZero)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.Fill = pieParams.Lighting ? GetLightingEnabledBrush(pieParams.Background) : pieParams.Background;

                Point start = new Point();
                Point end = new Point();
                Point arcMidPoint = new Point();

                start.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                start.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle);

                end.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                end.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle);

                arcMidPoint.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.MeanAngle);
                arcMidPoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.MeanAngle);

                List<PathGeometryParams> clipPathGeometry = new List<PathGeometryParams>();
                clipPathGeometry.Add(new LineSegmentParams(start));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius), 0, false, SweepDirection.Clockwise, pieParams.AnimationEnabled ? start : arcMidPoint));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius), 0, false, SweepDirection.Clockwise, pieParams.AnimationEnabled ? start : end));
                clipPathGeometry.Add(new LineSegmentParams(center));
                ellipse.Clip = GetPathGeometryFromList(FillRule.Nonzero, center, clipPathGeometry);
                PathSegmentCollection segments = (ellipse.Clip as PathGeometry).Figures[0].Segments;

                // apply animation to the individual points that for the pie slice
                if (pieParams.AnimationEnabled)
                {
                    // if the stop angle is zero then animation weill not be applies to that point 
                    // hence during animation the shape of the pie will get distorted
                    Double stopAngle = 0;
                    if (pieParams.StopAngle == 0)
                        stopAngle = pieParams.StartAngle + Math.Abs(pieParams.MeanAngle - pieParams.StartAngle) * 2;
                    else
                        stopAngle = pieParams.StopAngle;

                    // apply animation to the points
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[1], center, pieParams.OuterRadius, 0, pieParams.MeanAngle);
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[2], center, pieParams.OuterRadius, 0, stopAngle);
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[0], center, pieParams.OuterRadius, 0, pieParams.StartAngle);
                }

                visual.Children.Add(ellipse);

                // set the un exploded points for interactivity
                unExplodedPoints.Center = center;
                unExplodedPoints.OuterArcStart = start;
                unExplodedPoints.OuterArcMid = arcMidPoint;
                unExplodedPoints.OuterArcEnd = end;

                // set the exploded points for interactivity
                explodedPoints.Center = new Point(center.X + xOffset, center.Y + yOffset);
                explodedPoints.OuterArcStart = new Point(start.X + xOffset, start.Y + yOffset);
                explodedPoints.OuterArcMid = new Point(arcMidPoint.X + xOffset, arcMidPoint.Y + yOffset);
                explodedPoints.OuterArcEnd = new Point(end.X + xOffset, end.Y + yOffset);
            }
            #endregion PieSlice

            #region Lighting

            if (pieParams.Lighting && (pieParams.StartAngle != pieParams.StopAngle || !pieParams.IsZero))
            {
                Ellipse lightingEllipse = new Ellipse();
                lightingEllipse.Width = width;
                lightingEllipse.Height = height;
                lightingEllipse.IsHitTestVisible = false;
                lightingEllipse.Fill = GetPieGradianceBrush();

                Point start = new Point();
                Point end = new Point();
                Point arcMidPoint = new Point();

                start.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                start.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle);

                end.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                end.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle);

                arcMidPoint.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.MeanAngle);
                arcMidPoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.MeanAngle);

                List<PathGeometryParams> clipPathGeometry = new List<PathGeometryParams>();
                clipPathGeometry.Add(new LineSegmentParams(start));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius), 0, false, SweepDirection.Clockwise, pieParams.AnimationEnabled ? start : arcMidPoint));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius), 0, false, SweepDirection.Clockwise, pieParams.AnimationEnabled ? start : end));
                clipPathGeometry.Add(new LineSegmentParams(center));
                lightingEllipse.Clip = GetPathGeometryFromList(FillRule.Nonzero, center, clipPathGeometry);
                PathSegmentCollection segments = (lightingEllipse.Clip as PathGeometry).Figures[0].Segments;

                // apply animation to the individual points that for the shape that
                // gives the lighting effect to the pie slice
                if (pieParams.AnimationEnabled)
                {
                    // if the stop angle is zero then animation weill not be applies to that point 
                    // hence during animation the shape of the pie will get distorted
                    Double stopAngle = 0;
                    if (pieParams.StopAngle == 0)
                        stopAngle = pieParams.StartAngle + Math.Abs(pieParams.MeanAngle - pieParams.StartAngle) * 2;
                    else
                        stopAngle = pieParams.StopAngle;

                    // apply animation to the points
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[1], center, pieParams.OuterRadius, 0, pieParams.MeanAngle);
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[2], center, pieParams.OuterRadius, 0, stopAngle);
                    pieParams.Storyboard = CreatePathSegmentAnimation(pieParams.Storyboard, segments[0], center, pieParams.OuterRadius, 0, pieParams.StartAngle);
                }
                visual.Children.Add(lightingEllipse);
            }
            #endregion Lighting

            #region LabelLine
            if (pieParams.LabelLineEnabled)
            {
                Path labelLine = new Path();
                Double meanAngle = pieParams.MeanAngle;

                Point piePoint = new Point();
                piePoint.X = center.X + pieParams.OuterRadius * Math.Cos(meanAngle);
                piePoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(meanAngle);

                Point labelPoint = new Point();
                labelPoint.X = center.X + pieParams.LabelPoint.X - pieParams.Width / 2;
                labelPoint.Y = center.Y + pieParams.LabelPoint.Y - pieParams.Height / 2;

                Point midPoint = new Point();
                midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
                midPoint.Y = labelPoint.Y;

                List<PathGeometryParams> labelLinePathGeometry = new List<PathGeometryParams>();
                labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : midPoint));
                labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : labelPoint));
                labelLine.Data = GetPathGeometryFromList(FillRule.Nonzero, piePoint, labelLinePathGeometry);
                PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                PathSegmentCollection segments = figure.Segments;
                figure.IsClosed = false;
                figure.IsFilled = false;

                // animate the label lines of the individual pie slices
                if (pieParams.AnimationEnabled)
                {
                    pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[0], piePoint, midPoint);
                    pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[1], piePoint, midPoint, labelPoint);
                }

                labelLine.Stroke = pieParams.LabelLineColor;
                labelLine.StrokeDashArray = pieParams.LabelLineStyle;
                labelLine.StrokeThickness = pieParams.LabelLineThickness;

                labelLinePath = labelLine;

                visual.Children.Add(labelLine);

                // set the un exploded points for interactivity
                unExplodedPoints.LabelLineEndPoint = labelPoint;
                unExplodedPoints.LabelLineMidPoint = midPoint;
                unExplodedPoints.LabelLineStartPoint = piePoint;

                // set the exploded points for interactivity
                explodedPoints.LabelLineEndPoint = new Point(labelPoint.X, labelPoint.Y - yOffset);
                explodedPoints.LabelLineMidPoint = new Point(midPoint.X, midPoint.Y - yOffset);
                explodedPoints.LabelLineStartPoint = new Point(piePoint.X + xOffset, piePoint.Y + yOffset);
            }

            #endregion LabelLine

            #region Bevel

            if (pieParams.Bevel && Math.Abs(pieParams.StartAngle - pieParams.StopAngle) > 0.03 && (pieParams.StartAngle != pieParams.StopAngle))
            {
                Point start = new Point();
                Point end = new Point();

                start.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                start.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle);

                end.X = center.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                end.Y = center.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle);

                Point bevelCenter = new Point();
                Point bevelStart = new Point();
                Point bevelEnd = new Point();
                Double bevelLength = 4;
                Double bevelOuterRadius = Math.Abs(pieParams.OuterRadius - bevelLength);

                bevelCenter.X = center.X + bevelLength * Math.Cos(pieParams.MeanAngle);
                bevelCenter.Y = center.Y + bevelLength * Math.Sin(pieParams.MeanAngle);

                bevelStart.X = center.X + bevelOuterRadius * Math.Cos(pieParams.StartAngle + 0.03);
                bevelStart.Y = center.Y + bevelOuterRadius * Math.Sin(pieParams.StartAngle + 0.03);

                bevelEnd.X = center.X + bevelOuterRadius * Math.Cos(pieParams.StopAngle - 0.03);
                bevelEnd.Y = center.Y + bevelOuterRadius * Math.Sin(pieParams.StopAngle - 0.03);

                List<PathGeometryParams> pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(center));
                pathGeometry.Add(new LineSegmentParams(start));
                pathGeometry.Add(new LineSegmentParams(bevelStart));
                pathGeometry.Add(new LineSegmentParams(bevelCenter));

                Path path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelCenter, pathGeometry);
                if (pieParams.StartAngle > Math.PI * 0.5 && pieParams.StartAngle <= Math.PI * 1.5)
                {
                    path.Fill = GetDarkerBevelBrush(pieParams.Background, pieParams.StartAngle * 180 / Math.PI + 135);
                }
                else
                {
                    path.Fill = GetLighterBevelBrush(pieParams.Background, -pieParams.StartAngle * 180 / Math.PI);
                }
                // Apply animation to the beveling path
                if (pieParams.AnimationEnabled)
                {
                    pieParams.Storyboard = CreateOpacityAnimation(pieParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

                pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(center));
                pathGeometry.Add(new LineSegmentParams(end));
                pathGeometry.Add(new LineSegmentParams(bevelEnd));
                pathGeometry.Add(new LineSegmentParams(bevelCenter));

                path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelCenter, pathGeometry);
                if (pieParams.StopAngle > Math.PI * 0.5 && pieParams.StopAngle <= Math.PI * 1.5)
                {
                    path.Fill = GetLighterBevelBrush(pieParams.Background, pieParams.StopAngle * 180 / Math.PI + 135);
                }
                else
                {
                    path.Fill = GetDarkerBevelBrush(pieParams.Background, -pieParams.StopAngle * 180 / Math.PI);
                }
                // Apply animation to the beveling path
                if (pieParams.AnimationEnabled)
                {
                    pieParams.Storyboard = CreateOpacityAnimation(pieParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

                pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(end));
                pathGeometry.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius), 0, pieParams.IsLargerArc, SweepDirection.Counterclockwise, start));
                pathGeometry.Add(new LineSegmentParams(bevelStart));
                pathGeometry.Add(new ArcSegmentParams(new Size(bevelOuterRadius, bevelOuterRadius), 0, pieParams.IsLargerArc, SweepDirection.Clockwise, bevelEnd));

                path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelEnd, pathGeometry);
                if (pieParams.MeanAngle > 0 && pieParams.MeanAngle < Math.PI)
                {
                    path.Fill = GetCurvedBevelBrush(pieParams.Background, pieParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(-0.745, -0.85), GetDoubleCollection(0, 1));
                }
                else
                {
                    path.Fill = GetCurvedBevelBrush(pieParams.Background, pieParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(0.745, -0.99), GetDoubleCollection(0, 1));
                }
                // Apply animation to the beveling path
                if (pieParams.AnimationEnabled)
                {
                    pieParams.Storyboard = CreateOpacityAnimation(pieParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }

                visual.Children.Add(path);
            }

            #endregion LabelLine

            return visual;
        }

        private static Canvas GetDoughnut2D(SectorChartShapeParams doughnutParams, ref PieDoughnut2DPoints unExplodedPoints, ref PieDoughnut2DPoints explodedPoints, ref Path labelLinePath)
        {
            Canvas visual = new Canvas();

            Double width = doughnutParams.OuterRadius * 2;
            Double height = doughnutParams.OuterRadius * 2;

            visual.Width = width;
            visual.Height = height;

            Point center = new Point(width / 2, height / 2);
            Double xOffset = doughnutParams.OuterRadius * doughnutParams.ExplodeRatio * Math.Cos(doughnutParams.MeanAngle);
            Double yOffset = doughnutParams.OuterRadius * doughnutParams.ExplodeRatio * Math.Sin(doughnutParams.MeanAngle);

            #region Doughnut Slice
            if (doughnutParams.StartAngle != doughnutParams.StopAngle || !doughnutParams.IsZero)
            {

                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.Fill = doughnutParams.Lighting ? GetLightingEnabledBrush(doughnutParams.Background) : doughnutParams.Background;


                Point start = new Point();
                Point end = new Point();
                Point arcMidPoint = new Point();
                Point innerstart = new Point();
                Point innerend = new Point();
                Point innerArcMidPoint = new Point();

                start.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StartAngle);
                start.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StartAngle);

                end.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StopAngle);
                end.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StopAngle);

                arcMidPoint.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.MeanAngle);
                arcMidPoint.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.MeanAngle);

                innerstart.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StartAngle);
                innerstart.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StartAngle);

                innerend.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StopAngle);
                innerend.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StopAngle);

                innerArcMidPoint.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.MeanAngle);
                innerArcMidPoint.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.MeanAngle);

                List<PathGeometryParams> clipPathGeometry = new List<PathGeometryParams>();
                clipPathGeometry.Add(new LineSegmentParams(start));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.OuterRadius, doughnutParams.OuterRadius), 0, false, SweepDirection.Clockwise, doughnutParams.AnimationEnabled ? start : arcMidPoint));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.OuterRadius, doughnutParams.OuterRadius), 0, false, SweepDirection.Clockwise, doughnutParams.AnimationEnabled ? start : end));
                clipPathGeometry.Add(new LineSegmentParams(doughnutParams.AnimationEnabled ? innerstart : innerend));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.InnerRadius, doughnutParams.InnerRadius), 0, false, SweepDirection.Counterclockwise, doughnutParams.AnimationEnabled ? innerstart : innerArcMidPoint));
                clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.InnerRadius, doughnutParams.InnerRadius), 0, false, SweepDirection.Counterclockwise, doughnutParams.AnimationEnabled ? innerstart : innerstart));

                ellipse.Clip = GetPathGeometryFromList(FillRule.Nonzero, innerstart, clipPathGeometry);

                PathFigure figure = (ellipse.Clip as PathGeometry).Figures[0];
                PathSegmentCollection segments = figure.Segments;

                // Apply animation to the doughnut slice
                if (doughnutParams.AnimationEnabled)
                {
                    // if stop angle is zero the animation creates a distorted doughnut while animating
                    // so we need adjust the value such that the doughnutis not distorted
                    Double stopAngle = 0;
                    if (doughnutParams.StopAngle == 0)
                        stopAngle = doughnutParams.StartAngle + Math.Abs(doughnutParams.MeanAngle - doughnutParams.StartAngle) * 2;
                    else
                        stopAngle = doughnutParams.StopAngle;

                    // animate the outer points
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[0], center, doughnutParams.OuterRadius, 0, doughnutParams.StartAngle);
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[1], center, doughnutParams.OuterRadius, 0, doughnutParams.MeanAngle);
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[2], center, doughnutParams.OuterRadius, 0, stopAngle);

                    // animate the inner points
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[3], center, doughnutParams.InnerRadius, 0, stopAngle);
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[4], center, doughnutParams.InnerRadius, 0, doughnutParams.MeanAngle);
                    doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[5], center, doughnutParams.InnerRadius, 0, doughnutParams.StartAngle);

                    doughnutParams.Storyboard = CreatePathFigureAnimation(doughnutParams.Storyboard, figure, center, doughnutParams.InnerRadius, 0, doughnutParams.StartAngle);
                }
                visual.Children.Add(ellipse);

                // set the un exploded points for interactivity
                unExplodedPoints.Center = center;
                unExplodedPoints.OuterArcStart = start;
                unExplodedPoints.OuterArcMid = arcMidPoint;
                unExplodedPoints.OuterArcEnd = end;
                unExplodedPoints.InnerArcStart = innerstart;
                unExplodedPoints.InnerArcMid = innerArcMidPoint;
                unExplodedPoints.InnerArcEnd = innerend;


                // set the exploded points for interactivity
                explodedPoints.Center = new Point(center.X + xOffset, center.Y + yOffset);
                explodedPoints.OuterArcStart = new Point(start.X + xOffset, start.Y + yOffset);
                explodedPoints.OuterArcMid = new Point(arcMidPoint.X + xOffset, arcMidPoint.Y + yOffset);
                explodedPoints.OuterArcEnd = new Point(end.X + xOffset, end.Y + yOffset);
                explodedPoints.InnerArcStart = new Point(innerstart.X + xOffset, innerstart.Y + yOffset);
                explodedPoints.InnerArcMid = new Point(innerArcMidPoint.X + xOffset, innerArcMidPoint.Y + yOffset);
                explodedPoints.InnerArcEnd = new Point(innerend.X + xOffset, innerend.Y + yOffset);
            }
            #endregion Doughnut Slice

            #region Lighting
            if (doughnutParams.Lighting)
            {
                Ellipse lightingEllipse = new Ellipse();
                lightingEllipse.Width = width;
                lightingEllipse.Height = height;
                lightingEllipse.IsHitTestVisible = false;
                lightingEllipse.Fill = GetDoughnutGradianceBrush();

                if (doughnutParams.StartAngle != doughnutParams.StopAngle || !doughnutParams.IsZero)
                {
                    Point start = new Point();
                    Point end = new Point();
                    Point arcMidPoint = new Point();
                    Point innerstart = new Point();
                    Point innerend = new Point();
                    Point innerArcMidPoint = new Point();

                    start.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StartAngle);
                    start.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StartAngle);

                    end.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StopAngle);
                    end.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StopAngle);

                    arcMidPoint.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.MeanAngle);
                    arcMidPoint.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.MeanAngle);

                    innerstart.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StartAngle);
                    innerstart.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StartAngle);

                    innerend.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StopAngle);
                    innerend.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StopAngle);

                    innerArcMidPoint.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.MeanAngle);
                    innerArcMidPoint.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.MeanAngle);

                    List<PathGeometryParams> clipPathGeometry = new List<PathGeometryParams>();
                    clipPathGeometry.Add(new LineSegmentParams(start));
                    clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.OuterRadius, doughnutParams.OuterRadius), 0, false, SweepDirection.Clockwise, doughnutParams.AnimationEnabled ? start : arcMidPoint));
                    clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.OuterRadius, doughnutParams.OuterRadius), 0, false, SweepDirection.Clockwise, doughnutParams.AnimationEnabled ? start : end));
                    clipPathGeometry.Add(new LineSegmentParams(doughnutParams.AnimationEnabled ? innerstart : innerend));
                    clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.InnerRadius, doughnutParams.InnerRadius), 0, false, SweepDirection.Counterclockwise, doughnutParams.AnimationEnabled ? innerstart : innerArcMidPoint));
                    clipPathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.InnerRadius, doughnutParams.InnerRadius), 0, false, SweepDirection.Counterclockwise, doughnutParams.AnimationEnabled ? innerstart : innerstart));

                    lightingEllipse.Clip = GetPathGeometryFromList(FillRule.Nonzero, innerstart, clipPathGeometry);

                    PathFigure figure = (lightingEllipse.Clip as PathGeometry).Figures[0];
                    PathSegmentCollection segments = figure.Segments;

                    // Apply animation to the doughnut slice
                    if (doughnutParams.AnimationEnabled)
                    {
                        // if stop angle is zero the animation creates a distorted doughnut while animating
                        // so we need adjust the value such that the doughnutis not distorted
                        Double stopAngle = 0;
                        if (doughnutParams.StopAngle == 0)
                            stopAngle = doughnutParams.StartAngle + Math.Abs(doughnutParams.MeanAngle - doughnutParams.StartAngle) * 2;
                        else
                            stopAngle = doughnutParams.StopAngle;

                        // animate the outer points
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[0], center, doughnutParams.OuterRadius, 0, doughnutParams.StartAngle);
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[1], center, doughnutParams.OuterRadius, 0, doughnutParams.MeanAngle);
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[2], center, doughnutParams.OuterRadius, 0, stopAngle);

                        // animate the inner points
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[3], center, doughnutParams.InnerRadius, 0, stopAngle);
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[4], center, doughnutParams.InnerRadius, 0, doughnutParams.MeanAngle);
                        doughnutParams.Storyboard = CreatePathSegmentAnimation(doughnutParams.Storyboard, segments[5], center, doughnutParams.InnerRadius, 0, doughnutParams.StartAngle);

                        doughnutParams.Storyboard = CreatePathFigureAnimation(doughnutParams.Storyboard, figure, center, doughnutParams.InnerRadius, 0, doughnutParams.StartAngle);
                    }
                }
                visual.Children.Add(lightingEllipse);
            }
            #endregion Lighting

            #region LabelLine
            if (doughnutParams.LabelLineEnabled)
            {
                Path labelLine = new Path();
                Double meanAngle = doughnutParams.MeanAngle;

                Point doughnutPoint = new Point();
                doughnutPoint.X = center.X + doughnutParams.OuterRadius * Math.Cos(meanAngle);
                doughnutPoint.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(meanAngle);

                Point labelPoint = new Point();
                labelPoint.X = center.X + doughnutParams.LabelPoint.X - doughnutParams.Width / 2;
                labelPoint.Y = center.Y + doughnutParams.LabelPoint.Y - doughnutParams.Height / 2;

                Point midPoint = new Point();
                midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
                midPoint.Y = labelPoint.Y;

                List<PathGeometryParams> labelLinePathGeometry = new List<PathGeometryParams>();
                labelLinePathGeometry.Add(new LineSegmentParams(doughnutParams.AnimationEnabled ? doughnutPoint : midPoint));
                labelLinePathGeometry.Add(new LineSegmentParams(doughnutParams.AnimationEnabled ? doughnutPoint : labelPoint));
                labelLine.Data = GetPathGeometryFromList(FillRule.Nonzero, doughnutPoint, labelLinePathGeometry);
                PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                PathSegmentCollection segments = figure.Segments;
                figure.IsClosed = false;
                figure.IsFilled = false;

                // apply animation to the label line
                if (doughnutParams.AnimationEnabled)
                {
                    doughnutParams.Storyboard = CreateLabelLineAnimation(doughnutParams.Storyboard, segments[0], doughnutPoint, midPoint);
                    doughnutParams.Storyboard = CreateLabelLineAnimation(doughnutParams.Storyboard, segments[1], doughnutPoint, midPoint, labelPoint);
                }

                labelLine.Stroke = doughnutParams.LabelLineColor;
                labelLine.StrokeDashArray = doughnutParams.LabelLineStyle;
                labelLine.StrokeThickness = doughnutParams.LabelLineThickness;
                visual.Children.Add(labelLine);

                labelLinePath = labelLine;

                // set the un exploded points for interactivity
                unExplodedPoints.LabelLineEndPoint = labelPoint;
                unExplodedPoints.LabelLineMidPoint = midPoint;
                unExplodedPoints.LabelLineStartPoint = doughnutPoint;

                // set the exploded points for interactivity
                explodedPoints.LabelLineEndPoint = new Point(labelPoint.X, labelPoint.Y - yOffset);
                explodedPoints.LabelLineMidPoint = new Point(midPoint.X, midPoint.Y - yOffset);
                explodedPoints.LabelLineStartPoint = new Point(doughnutPoint.X + xOffset, doughnutPoint.Y + yOffset);
            }
            #endregion LabelLine

            #region Bevel
            if (doughnutParams.Bevel && Math.Abs(doughnutParams.StartAngle - doughnutParams.StopAngle) > 0.03)
            {
                Point start = new Point();
                Point end = new Point();
                Point innerstart = new Point();
                Point innerend = new Point();

                start.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StartAngle);
                start.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StartAngle);

                end.X = center.X + doughnutParams.OuterRadius * Math.Cos(doughnutParams.StopAngle);
                end.Y = center.Y + doughnutParams.OuterRadius * Math.Sin(doughnutParams.StopAngle);

                innerstart.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StartAngle);
                innerstart.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StartAngle);

                innerend.X = center.X + doughnutParams.InnerRadius * Math.Cos(doughnutParams.StopAngle);
                innerend.Y = center.Y + doughnutParams.InnerRadius * Math.Sin(doughnutParams.StopAngle);

                Point bevelCenter = new Point();
                Point bevelStart = new Point();
                Point bevelEnd = new Point();
                Point bevelInnerStart = new Point();
                Point bevelInnerEnd = new Point();
                Double bevelLength = 4;
                Double bevelOuterRadius = Math.Abs(doughnutParams.OuterRadius - bevelLength);
                Double bevelInnerRadius = Math.Abs(doughnutParams.InnerRadius + bevelLength);

                bevelCenter.X = center.X + bevelLength * Math.Cos(doughnutParams.MeanAngle);
                bevelCenter.Y = center.Y + bevelLength * Math.Sin(doughnutParams.MeanAngle);

                bevelStart.X = center.X + bevelOuterRadius * Math.Cos(doughnutParams.StartAngle + 0.03);
                bevelStart.Y = center.Y + bevelOuterRadius * Math.Sin(doughnutParams.StartAngle + 0.03);

                bevelEnd.X = center.X + bevelOuterRadius * Math.Cos(doughnutParams.StopAngle - 0.03);
                bevelEnd.Y = center.Y + bevelOuterRadius * Math.Sin(doughnutParams.StopAngle - 0.03);

                bevelInnerStart.X = center.X + bevelInnerRadius * Math.Cos(doughnutParams.StartAngle + 0.03);
                bevelInnerStart.Y = center.Y + bevelInnerRadius * Math.Sin(doughnutParams.StartAngle + 0.03);

                bevelInnerEnd.X = center.X + bevelInnerRadius * Math.Cos(doughnutParams.StopAngle - 0.03);
                bevelInnerEnd.Y = center.Y + bevelInnerRadius * Math.Sin(doughnutParams.StopAngle - 0.03);

                List<PathGeometryParams> pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(innerstart));
                pathGeometry.Add(new LineSegmentParams(start));
                pathGeometry.Add(new LineSegmentParams(bevelStart));
                pathGeometry.Add(new LineSegmentParams(bevelInnerStart));

                Path path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelInnerStart, pathGeometry);
                if (doughnutParams.StartAngle > Math.PI * 0.5 && doughnutParams.StartAngle <= Math.PI * 1.5)
                {
                    path.Fill = GetDarkerBevelBrush(doughnutParams.Background, doughnutParams.StartAngle * 180 / Math.PI + 135);
                }
                else
                {
                    path.Fill = GetLighterBevelBrush(doughnutParams.Background, -doughnutParams.StartAngle * 180 / Math.PI);
                }
                // Apply animation to the beveling path
                if (doughnutParams.AnimationEnabled)
                {
                    doughnutParams.Storyboard = CreateOpacityAnimation(doughnutParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

                pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(innerend));
                pathGeometry.Add(new LineSegmentParams(end));
                pathGeometry.Add(new LineSegmentParams(bevelEnd));
                pathGeometry.Add(new LineSegmentParams(bevelInnerEnd));

                path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelInnerEnd, pathGeometry);
                if (doughnutParams.StopAngle > Math.PI * 0.5 && doughnutParams.StopAngle <= Math.PI * 1.5)
                {
                    path.Fill = GetLighterBevelBrush(doughnutParams.Background, doughnutParams.StopAngle * 180 / Math.PI + 135);
                }
                else
                {
                    path.Fill = GetDarkerBevelBrush(doughnutParams.Background, -doughnutParams.StopAngle * 180 / Math.PI);
                }
                // Apply animation to the beveling path
                if (doughnutParams.AnimationEnabled)
                {
                    doughnutParams.Storyboard = CreateOpacityAnimation(doughnutParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

                pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(end));
                pathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.OuterRadius, doughnutParams.OuterRadius), 0, doughnutParams.IsLargerArc, SweepDirection.Counterclockwise, start));
                pathGeometry.Add(new LineSegmentParams(bevelStart));
                pathGeometry.Add(new ArcSegmentParams(new Size(bevelOuterRadius, bevelOuterRadius), 0, doughnutParams.IsLargerArc, SweepDirection.Clockwise, bevelEnd));

                path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelEnd, pathGeometry);
                if (doughnutParams.MeanAngle > 0 && doughnutParams.MeanAngle < Math.PI)
                {
                    path.Fill = GetCurvedBevelBrush(doughnutParams.Background, doughnutParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(-0.745, -0.85), GetDoubleCollection(0, 1));
                }
                else
                {
                    path.Fill = GetCurvedBevelBrush(doughnutParams.Background, doughnutParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(0.745, -0.99), GetDoubleCollection(0, 1));
                }
                // Apply animation to the beveling path
                if (doughnutParams.AnimationEnabled)
                {
                    doughnutParams.Storyboard = CreateOpacityAnimation(doughnutParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

                pathGeometry = new List<PathGeometryParams>();
                pathGeometry.Add(new LineSegmentParams(innerend));
                pathGeometry.Add(new ArcSegmentParams(new Size(doughnutParams.InnerRadius, doughnutParams.InnerRadius), 0, doughnutParams.IsLargerArc, SweepDirection.Counterclockwise, innerstart));
                pathGeometry.Add(new LineSegmentParams(bevelInnerStart));
                pathGeometry.Add(new ArcSegmentParams(new Size(bevelInnerRadius, bevelInnerRadius), 0, doughnutParams.IsLargerArc, SweepDirection.Clockwise, bevelInnerEnd));

                path = new Path();
                path.Data = GetPathGeometryFromList(FillRule.Nonzero, bevelInnerEnd, pathGeometry);
                if (doughnutParams.MeanAngle > 0 && doughnutParams.MeanAngle < Math.PI)
                {

                    path.Fill = GetCurvedBevelBrush(doughnutParams.Background, doughnutParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(0.745, -0.99), GetDoubleCollection(0, 1));
                }
                else
                {
                    path.Fill = GetCurvedBevelBrush(doughnutParams.Background, doughnutParams.MeanAngle * 180 / Math.PI + 90, GetDoubleCollection(-0.745, -0.85), GetDoubleCollection(0, 1));
                }

                // Apply animation to the beveling path
                if (doughnutParams.AnimationEnabled)
                {
                    doughnutParams.Storyboard = CreateOpacityAnimation(doughnutParams.Storyboard, path, 1, 1, 1);
                    path.Opacity = 0;
                }
                visual.Children.Add(path);

            }
            #endregion Bevel
            return visual;
        }

        private static List<Path> GetPie3D(SectorChartShapeParams pieParams, ref Int32 zindex, ref PieDoughnut3DPoints unExplodedPoints, ref PieDoughnut3DPoints explodedPoints, ref Path labelLinePath)
        {

            List<Path> pieFaces = new List<Path>();
            if (pieParams.StartAngle == pieParams.StopAngle && pieParams.IsLargerArc)
            {
                // draw singleton pie here
            }
            else
            {
                Point center = new Point();
                center.X = pieParams.Width / 2;
                center.Y = pieParams.Height / 2;

                // calculate 3d offsets
                Double yOffset = -pieParams.Depth / 2 * pieParams.ZAxisScaling;

                // calculate all points
                Point3D topFaceCenter = new Point3D();
                topFaceCenter.X = center.X;
                topFaceCenter.Y = center.Y + yOffset;
                topFaceCenter.Z = pieParams.OffsetY * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D topArcStart = new Point3D();
                topArcStart.X = topFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                topArcStart.Y = topFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                topArcStart.Z = (topFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D topArcStop = new Point3D();
                topArcStop.X = topFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                topArcStop.Y = topFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                topArcStop.Z = (topFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomFaceCenter = new Point3D();
                bottomFaceCenter.X = center.X;
                bottomFaceCenter.Y = center.Y - yOffset;
                bottomFaceCenter.Z = pieParams.OffsetY * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomArcStart = new Point3D();
                bottomArcStart.X = bottomFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                bottomArcStart.Y = bottomFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                bottomArcStart.Z = (bottomFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomArcStop = new Point3D();
                bottomArcStop.X = bottomFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                bottomArcStop.Y = bottomFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                bottomArcStop.Z = (bottomFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D centroid = GetCentroid(topFaceCenter, topArcStart, topArcStop, bottomFaceCenter, bottomArcStart, bottomArcStop);

                Path topFace = GetPieFace(pieParams, centroid, topFaceCenter, topArcStart, topArcStop);
                pieFaces.Add(topFace);

                Path bottomFace = GetPieFace(pieParams, centroid, bottomFaceCenter, bottomArcStart, bottomArcStop);
                pieFaces.Add(bottomFace);

                Path rightFace = GetPieSide(pieParams, centroid, topFaceCenter, bottomFaceCenter, topArcStart, bottomArcStart);
                pieFaces.Add(rightFace);

                Path leftFace = GetPieSide(pieParams, centroid, topFaceCenter, bottomFaceCenter, topArcStop, bottomArcStop);
                pieFaces.Add(leftFace);

                List<Path> curvedSurface = GetPieOuterCurvedFace(pieParams, centroid, topFaceCenter, bottomFaceCenter);
                pieFaces.InsertRange(pieFaces.Count, curvedSurface);

                Path labelLine = new Path();
                if (pieParams.LabelLineEnabled)
                {

                    Double meanAngle = pieParams.MeanAngle;

                    Point piePoint = new Point();
                    piePoint.X = center.X + pieParams.OuterRadius * Math.Cos(meanAngle);
                    piePoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(meanAngle) * pieParams.YAxisScaling;

                    Point labelPoint = new Point();
                    labelPoint.X = center.X + pieParams.LabelPoint.X - pieParams.Width / 2;
                    labelPoint.Y = center.Y + pieParams.LabelPoint.Y - pieParams.Height / 2;

                    Point midPoint = new Point();
                    midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
                    midPoint.Y = labelPoint.Y;

                    List<PathGeometryParams> labelLinePathGeometry = new List<PathGeometryParams>();
                    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : midPoint));
                    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : labelPoint));
                    labelLine.Data = GetPathGeometryFromList(FillRule.Nonzero, piePoint, labelLinePathGeometry);
                    PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                    PathSegmentCollection segments = figure.Segments;
                    figure.IsClosed = false;
                    figure.IsFilled = false;

                    // apply animation to the label line
                    if (pieParams.AnimationEnabled)
                    {
                        pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[0], piePoint, midPoint);
                        pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[1], piePoint, midPoint, labelPoint);
                    }
                    labelLine.Stroke = pieParams.LabelLineColor;
                    labelLine.StrokeDashArray = pieParams.LabelLineStyle;
                    labelLine.StrokeThickness = pieParams.LabelLineThickness;

                    labelLinePath = labelLine;

                    // set the un exploded points for interactivity
                    unExplodedPoints.LabelLineEndPoint = labelPoint;
                    unExplodedPoints.LabelLineMidPoint = midPoint;
                    unExplodedPoints.LabelLineStartPoint = piePoint;

                    // set the exploded points for interactivity
                    explodedPoints.LabelLineEndPoint = new Point(labelPoint.X, labelPoint.Y - pieParams.OffsetY);
                    explodedPoints.LabelLineMidPoint = new Point(midPoint.X, midPoint.Y - pieParams.OffsetY);
                    explodedPoints.LabelLineStartPoint = new Point(piePoint.X + pieParams.OffsetX, piePoint.Y + pieParams.OffsetY);
                }

                //Top face ZIndex
                topFace.SetValue(Canvas.ZIndexProperty, (Int32)(50000));

                //BottomFace ZIndex
                bottomFace.SetValue(Canvas.ZIndexProperty, (Int32)(-50000));

                // ZIndex of curved face
                if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2 && pieParams.IsLargerArc)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[0],pieParams.StartAngle,0));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[1], 0, Math.PI));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[2],Math.PI,pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], 0, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, 0, Math.PI));
                }
                else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI && pieParams.IsLargerArc)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[1], 0, Math.PI));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[2], 0, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[1], 0, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        labelLine.SetValue(Canvas.ZIndexProperty, -50000);
                }
                else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, Math.PI));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[1], Math.PI,pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                }
                else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, 0));
                    //_elementPositionData.Add(new ElementPositionData(curvedSurface[1], 0, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], 0, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StopAngle, pieParams.StopAngle));
                }
                else
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    if (pieParams.StartAngle >= 0 && pieParams.StartAngle < Math.PI / 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle < Math.PI / 2)
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StopAngle, pieParams.StopAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= 0 && pieParams.StartAngle < Math.PI / 2 && pieParams.StopAngle >= Math.PI / 2 && pieParams.StopAngle < Math.PI)
                    {
                        if (labelLine != null)
                            labelLine.SetValue(Canvas.ZIndexProperty, 40000);
                        curvedSurface[0].SetValue(Canvas.ZIndexProperty, 35000);
                        //_elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI / 2 && pieParams.StartAngle < Math.PI && pieParams.StopAngle >= Math.PI / 2 && pieParams.StopAngle < Math.PI)
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle < Math.PI * 1.5 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle < Math.PI * 1.5)
                    {
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI * 1.5 && pieParams.StartAngle < Math.PI * 2 && pieParams.StopAngle >= Math.PI * 1.5 && pieParams.StopAngle < Math.PI * 2)
                    {
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                }


            }

            return pieFaces;

        }

        private static List<Path> GetDoughnut3D(SectorChartShapeParams pieParams, ref PieDoughnut3DPoints unExplodedPoints, ref PieDoughnut3DPoints explodedPoints, ref Path labelLinePath)
        {
            List<Path> pieFaces = new List<Path>();
            if (pieParams.StartAngle == pieParams.StopAngle && pieParams.IsLargerArc)
            {
                // draw singleton pie here
            }
            else
            {
                Point center = new Point();
                center.X += pieParams.Width / 2;
                center.Y += pieParams.Height / 2;

                // calculate 3d offsets
                Double yOffset = -pieParams.Depth / 2 * pieParams.ZAxisScaling;

                // calculate all points
                Point3D topFaceCenter = new Point3D();
                topFaceCenter.X = center.X;
                topFaceCenter.Y = center.Y + yOffset;
                topFaceCenter.Z = pieParams.OffsetY * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);


                Point3D topOuterArcStart = new Point3D();
                topOuterArcStart.X = topFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                topOuterArcStart.Y = topFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                topOuterArcStart.Z = (topFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D topOuterArcStop = new Point3D();
                topOuterArcStop.X = topFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                topOuterArcStop.Y = topFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                topOuterArcStop.Z = (topFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D topInnerArcStart = new Point3D();
                topInnerArcStart.X = topFaceCenter.X + pieParams.InnerRadius * Math.Cos(pieParams.StartAngle);
                topInnerArcStart.Y = topFaceCenter.Y + pieParams.InnerRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                topInnerArcStart.Z = (topFaceCenter.Y + pieParams.InnerRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D topInnerArcStop = new Point3D();
                topInnerArcStop.X = topFaceCenter.X + pieParams.InnerRadius * Math.Cos(pieParams.StopAngle);
                topInnerArcStop.Y = topFaceCenter.Y + pieParams.InnerRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                topInnerArcStop.Z = (topFaceCenter.Y + pieParams.InnerRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);


                Point3D bottomFaceCenter = new Point3D();
                bottomFaceCenter.X = center.X;
                bottomFaceCenter.Y = center.Y - yOffset;
                bottomFaceCenter.Z = pieParams.OffsetY * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomOuterArcStart = new Point3D();
                bottomOuterArcStart.X = bottomFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StartAngle);
                bottomOuterArcStart.Y = bottomFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                bottomOuterArcStart.Z = (bottomFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomOuterArcStop = new Point3D();
                bottomOuterArcStop.X = bottomFaceCenter.X + pieParams.OuterRadius * Math.Cos(pieParams.StopAngle);
                bottomOuterArcStop.Y = bottomFaceCenter.Y + pieParams.OuterRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                bottomOuterArcStop.Z = (bottomFaceCenter.Y + pieParams.OuterRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomInnerArcStart = new Point3D();
                bottomInnerArcStart.X = bottomFaceCenter.X + pieParams.InnerRadius * Math.Cos(pieParams.StartAngle);
                bottomInnerArcStart.Y = bottomFaceCenter.Y + pieParams.InnerRadius * Math.Sin(pieParams.StartAngle) * pieParams.YAxisScaling;
                bottomInnerArcStart.Z = (bottomFaceCenter.Y + pieParams.InnerRadius) * Math.Sin(pieParams.StartAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

                Point3D bottomInnerArcStop = new Point3D();
                bottomInnerArcStop.X = bottomFaceCenter.X + pieParams.InnerRadius * Math.Cos(pieParams.StopAngle);
                bottomInnerArcStop.Y = bottomFaceCenter.Y + pieParams.InnerRadius * Math.Sin(pieParams.StopAngle) * pieParams.YAxisScaling;
                bottomInnerArcStop.Z = (bottomFaceCenter.Y + pieParams.InnerRadius) * Math.Sin(pieParams.StopAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);


                Point3D centroid = GetCentroid(topInnerArcStart, topInnerArcStop, topOuterArcStart, topOuterArcStop, bottomInnerArcStart, bottomInnerArcStop, bottomOuterArcStart, bottomOuterArcStop);

                Path topFace = GetDoughnutFace(pieParams, centroid, topInnerArcStart, topInnerArcStop, topOuterArcStart, topOuterArcStop, true);
                pieFaces.Add(topFace);


                Path bottomFace = GetDoughnutFace(pieParams, centroid, bottomInnerArcStart, bottomInnerArcStop, bottomOuterArcStart, bottomOuterArcStop, false);
                pieFaces.Add(bottomFace);

                Path rightFace = GetPieSide(pieParams, centroid, topInnerArcStart, bottomInnerArcStart, topOuterArcStart, bottomOuterArcStart);
                pieFaces.Add(rightFace);

                Path leftFace = GetPieSide(pieParams, centroid, topInnerArcStop, bottomInnerArcStop, topOuterArcStop, bottomOuterArcStop);
                pieFaces.Add(leftFace);

                List<Path> curvedSurface = GetDoughnutCurvedFace(pieParams, centroid, topFaceCenter, bottomFaceCenter);
                pieFaces.InsertRange(pieFaces.Count, curvedSurface);

                Path labelLine = new Path();
                if (pieParams.LabelLineEnabled)
                {

                    Double meanAngle = pieParams.MeanAngle;

                    Point piePoint = new Point();
                    piePoint.X = center.X + pieParams.OuterRadius * Math.Cos(meanAngle);
                    piePoint.Y = center.Y + pieParams.OuterRadius * Math.Sin(meanAngle) * pieParams.YAxisScaling;

                    Point labelPoint = new Point();
                    labelPoint.X = center.X + pieParams.LabelPoint.X - pieParams.Width / 2;
                    labelPoint.Y = center.Y + pieParams.LabelPoint.Y - pieParams.Height / 2;

                    Point midPoint = new Point();
                    midPoint.X = (labelPoint.X < center.X) ? labelPoint.X + 10 : labelPoint.X - 10;
                    midPoint.Y = labelPoint.Y;

                    List<PathGeometryParams> labelLinePathGeometry = new List<PathGeometryParams>();
                    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : midPoint));
                    labelLinePathGeometry.Add(new LineSegmentParams(pieParams.AnimationEnabled ? piePoint : labelPoint));
                    labelLine.Data = GetPathGeometryFromList(FillRule.Nonzero, piePoint, labelLinePathGeometry);
                    PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                    PathSegmentCollection segments = figure.Segments;
                    figure.IsClosed = false;
                    figure.IsFilled = false;

                    // apply animation to the label line
                    if (pieParams.AnimationEnabled)
                    {
                        pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[0], piePoint, midPoint);
                        pieParams.Storyboard = CreateLabelLineAnimation(pieParams.Storyboard, segments[1], piePoint, midPoint, labelPoint);
                    }

                    labelLine.Stroke = pieParams.LabelLineColor;
                    labelLine.StrokeDashArray = pieParams.LabelLineStyle;
                    labelLine.StrokeThickness = pieParams.LabelLineThickness;

                    labelLinePath = labelLine;

                    // set the un exploded points for interactivity
                    unExplodedPoints.LabelLineEndPoint = labelPoint;
                    unExplodedPoints.LabelLineMidPoint = midPoint;
                    unExplodedPoints.LabelLineStartPoint = piePoint;

                    // set the exploded points for interactivity
                    explodedPoints.LabelLineEndPoint = new Point(labelPoint.X, labelPoint.Y - pieParams.OffsetY);
                    explodedPoints.LabelLineMidPoint = new Point(midPoint.X, midPoint.Y - pieParams.OffsetY);
                    explodedPoints.LabelLineStartPoint = new Point(piePoint.X + pieParams.OffsetX, piePoint.Y + pieParams.OffsetY);

                }

                //Top face ZIndex
                topFace.SetValue(Canvas.ZIndexProperty, (Int32)(50000));

                //BottomFace ZIndex
                bottomFace.SetValue(Canvas.ZIndexProperty, (Int32)(-50000));

                // ZIndex of curved face
                if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2 && pieParams.IsLargerArc)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], 0, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[1], pieParams.StartAngle, 0));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[2], Math.PI, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, 0, Math.PI));
                }
                else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI && pieParams.IsLargerArc)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[1], Math.PI * 2, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[2], Math.PI, Math.PI * 2));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        labelLine.SetValue(Canvas.ZIndexProperty, -50000);
                }
                else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, Math.PI));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[1], Math.PI, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                }
                else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI)
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[0], 0, pieParams.StopAngle));
                    _elementPositionData.Add(new ElementPositionData(curvedSurface[1], pieParams.StartAngle, 0));
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                    if (labelLine != null)
                        _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StopAngle, pieParams.StopAngle));
                }
                else
                {
                    _elementPositionData.Add(new ElementPositionData(rightFace, pieParams.StartAngle, pieParams.StartAngle));
                    if (pieParams.StartAngle >= 0 && pieParams.StartAngle < Math.PI / 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle < Math.PI / 2)
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StopAngle, pieParams.StopAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= 0 && pieParams.StartAngle < Math.PI / 2 && pieParams.StopAngle >= Math.PI / 2 && pieParams.StopAngle < Math.PI)
                    {
                        if (labelLine != null)
                            labelLine.SetValue(Canvas.ZIndexProperty, 40000);
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI / 2 && pieParams.StartAngle < Math.PI && pieParams.StopAngle >= Math.PI / 2 && pieParams.StopAngle < Math.PI)
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle < Math.PI * 1.5 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle < Math.PI * 1.5)
                    {
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                    }
                    else if (pieParams.StartAngle >= Math.PI * 1.5 && pieParams.StartAngle < Math.PI * 2 && pieParams.StopAngle >= Math.PI * 1.5 && pieParams.StopAngle < Math.PI * 2)
                    {
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StopAngle));
                    }
                    else
                    {
                        if (labelLine != null)
                            _elementPositionData.Add(new ElementPositionData(labelLine, pieParams.StartAngle, pieParams.StartAngle));
                        _elementPositionData.Add(new ElementPositionData(curvedSurface[0], pieParams.StartAngle, pieParams.StopAngle));
                    }
                    _elementPositionData.Add(new ElementPositionData(leftFace, pieParams.StopAngle, pieParams.StopAngle));
                }
            }

            return pieFaces;
        }


        private static void SetZIndex(FrameworkElement element, ref Int32 zindex1, ref Int32 zindex2, Double angle)
        {
            if (angle >= 0 && angle <= Math.PI / 2)
            {
                element.SetValue(Canvas.ZIndexProperty, ++zindex1);
            }
            else if (angle > Math.PI / 2 && angle <= Math.PI)
            {
                element.SetValue(Canvas.ZIndexProperty, --zindex1);
            }
            else if (angle > Math.PI && angle <= Math.PI * 3 / 2)
            {
                element.SetValue(Canvas.ZIndexProperty, --zindex2);
            }
            else
            {
                element.SetValue(Canvas.ZIndexProperty, ++zindex2);
            }
        }

        private static List<Path> GetPieOuterCurvedFace(SectorChartShapeParams pieParams, Point3D centroid, Point3D topFaceCenter, Point3D bottomFaceCenter)
        {
            List<Path> curvedFaces = new List<Path>();

            if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2 && pieParams.IsLargerArc)
            {
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, 0, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI && pieParams.IsLargerArc)
            {
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
                curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, Math.PI * 2, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2)
            {
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI)
            {
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, 0, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
            }
            else
            {
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
            }

            return curvedFaces;
        }

        private static Path GetCurvedSegment(SectorChartShapeParams pieParams, Double radius, Double startAngle, Double stopAngle, Point3D topFaceCenter, Point3D bottomFaceCenter, Point3D centroid, Boolean isOuterSide)
        {

            Point3D topArcStart = new Point3D();
            topArcStart.X = topFaceCenter.X + radius * Math.Cos(startAngle);
            topArcStart.Y = topFaceCenter.Y + radius * Math.Sin(startAngle) * pieParams.YAxisScaling;
            topArcStart.Z = (topFaceCenter.Z + radius) * Math.Sin(startAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

            Point3D topArcStop = new Point3D();
            topArcStop.X = topFaceCenter.X + radius * Math.Cos(stopAngle);
            topArcStop.Y = topFaceCenter.Y + radius * Math.Sin(stopAngle) * pieParams.YAxisScaling;
            topArcStop.Z = (topFaceCenter.Z + radius) * Math.Sin(stopAngle) * Math.Cos(pieParams.TiltAngle) + pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

            Point3D bottomArcStart = new Point3D();
            bottomArcStart.X = bottomFaceCenter.X + radius * Math.Cos(startAngle);
            bottomArcStart.Y = bottomFaceCenter.Y + radius * Math.Sin(startAngle) * pieParams.YAxisScaling;
            bottomArcStart.Z = (bottomFaceCenter.Z + radius) * Math.Sin(startAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

            Point3D bottomArcStop = new Point3D();
            bottomArcStop.X = bottomFaceCenter.X + radius * Math.Cos(stopAngle);
            bottomArcStop.Y = bottomFaceCenter.Y + radius * Math.Sin(stopAngle) * pieParams.YAxisScaling;
            bottomArcStop.Z = (bottomFaceCenter.Z + radius) * Math.Sin(stopAngle) * Math.Cos(pieParams.TiltAngle) - pieParams.Depth * Math.Cos(Math.PI / 2 - pieParams.TiltAngle);

            Path pieFace = new Path();

            pieFace.Fill = pieParams.Lighting ? GetLightingEnabledBrush(pieParams.Background) : pieParams.Background;

            List<PathGeometryParams> pathGeometryList = new List<PathGeometryParams>();

            Boolean isLargeArc = (Math.Abs(stopAngle - startAngle) > Math.PI) ? true : false;
            if (stopAngle < startAngle)
                isLargeArc = (Math.Abs((stopAngle + Math.PI * 2) - startAngle) > Math.PI) ? true : false;

            pathGeometryList.Add(new LineSegmentParams(new Point(topArcStop.X, topArcStop.Y)));
            pathGeometryList.Add(new ArcSegmentParams(new Size(radius, radius * pieParams.YAxisScaling), 0, isLargeArc, SweepDirection.Counterclockwise, new Point(topArcStart.X, topArcStart.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(bottomArcStart.X, bottomArcStart.Y)));
            pathGeometryList.Add(new ArcSegmentParams(new Size(radius, radius * pieParams.YAxisScaling), 0, isLargeArc, SweepDirection.Clockwise, new Point(bottomArcStop.X, bottomArcStop.Y)));

            pieFace.Data = GetPathGeometryFromList(FillRule.Nonzero, new Point(bottomArcStop.X, bottomArcStop.Y), pathGeometryList);
            PathFigure figure = (pieFace.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;
            //Point3D midPoint = GetFaceZIndex(topArcStart, topArcStop, bottomArcStop, bottomArcStart);

            //pieFace.SetValue(Canvas.ZIndexProperty,(Int32)(midPoint.Z*(isOuterSide?200:200)) );
            return pieFace;

        }

        private static Path GetPieFace(SectorChartShapeParams pieParams, Point3D centroid, Point3D center, Point3D arcStart, Point3D arcStop)
        {
            Path pieFace = new Path();

            pieFace.Fill = pieParams.Lighting ? GetLightingEnabledBrush(pieParams.Background) : pieParams.Background;

            List<PathGeometryParams> pathGeometryList = new List<PathGeometryParams>();

            pathGeometryList.Add(new LineSegmentParams(new Point(arcStop.X, arcStop.Y)));
            pathGeometryList.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius * pieParams.YAxisScaling), 0, pieParams.IsLargerArc, SweepDirection.Counterclockwise, new Point(arcStart.X, arcStart.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(center.X, center.Y)));

            pieFace.Data = GetPathGeometryFromList(FillRule.Nonzero, new Point(center.X, center.Y), pathGeometryList);

            PathFigure figure = (pieFace.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;

            //Point3D midPoint = GetFaceZIndex(centroid, center, arcStart, arcStop);
            //pieFace.SetValue(Canvas.ZIndexProperty, (Int32)(midPoint.Z*200));
            return pieFace;
        }

        private static Path GetPieSide(SectorChartShapeParams pieParams, Point3D centroid, Point3D centerTop, Point3D centerBottom, Point3D outerTop, Point3D outerBottom)
        {
            Path pieFace = new Path();

            pieFace.Fill = pieParams.Lighting ? GetLightingEnabledBrush(pieParams.Background) : pieParams.Background;

            List<PathGeometryParams> pathGeometryList = new List<PathGeometryParams>();

            pathGeometryList.Add(new LineSegmentParams(new Point(centerTop.X, centerTop.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(outerTop.X, outerTop.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(outerBottom.X, outerBottom.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(centerBottom.X, centerBottom.Y)));

            pieFace.Data = GetPathGeometryFromList(FillRule.Nonzero, new Point(centerBottom.X, centerBottom.Y), pathGeometryList);
            PathFigure figure = (pieFace.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;

            //Point3D midPoint = GetFaceZIndex(centerTop, centerBottom, outerTop,outerBottom);
            //pieFace.SetValue(Canvas.ZIndexProperty, (Int32)(midPoint.Z * 100));
            return pieFace;
        }

        private static Path GetDoughnutFace(SectorChartShapeParams pieParams, Point3D centroid, Point3D arcInnerStart, Point3D arcInnerStop, Point3D arcOuterStart, Point3D arcOuterStop, Boolean isTopFace)
        {
            Path pieFace = new Path();

            pieFace.Fill = pieParams.Lighting ? GetLightingEnabledBrush(pieParams.Background) : pieParams.Background;

            List<PathGeometryParams> pathGeometryList = new List<PathGeometryParams>();

            pathGeometryList.Add(new LineSegmentParams(new Point(arcOuterStop.X, arcOuterStop.Y)));
            pathGeometryList.Add(new ArcSegmentParams(new Size(pieParams.OuterRadius, pieParams.OuterRadius * pieParams.YAxisScaling), 0, pieParams.IsLargerArc, SweepDirection.Counterclockwise, new Point(arcOuterStart.X, arcOuterStart.Y)));
            pathGeometryList.Add(new LineSegmentParams(new Point(arcInnerStart.X, arcInnerStart.Y)));
            pathGeometryList.Add(new ArcSegmentParams(new Size(pieParams.InnerRadius, pieParams.InnerRadius * pieParams.YAxisScaling), 0, pieParams.IsLargerArc, SweepDirection.Clockwise, new Point(arcInnerStop.X, arcInnerStop.Y)));

            pieFace.Data = GetPathGeometryFromList(FillRule.Nonzero, new Point(arcInnerStop.X, arcInnerStop.Y), pathGeometryList);
            Point3D midPoint = GetFaceZIndex(arcInnerStart, arcInnerStop, arcOuterStart, arcOuterStop);
            if (isTopFace)
                pieFace.SetValue(Canvas.ZIndexProperty, (Int32)(pieParams.Height * 200));
            else
                pieFace.SetValue(Canvas.ZIndexProperty, (Int32)(-pieParams.Height * 200));
            return pieFace;
        }

        private static List<Path> GetDoughnutCurvedFace(SectorChartShapeParams pieParams, Point3D centroid, Point3D topFaceCenter, Point3D bottomFaceCenter)
        {
            List<Path> curvedFaces = new List<Path>();

            if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2 && pieParams.IsLargerArc)
            {
                // Outer curved path
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, 0, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);

                // Inner curved paths
                curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, pieParams.StartAngle, 0, topFaceCenter, bottomFaceCenter, centroid, false);
                curvedFaces.Add(curvedSegment);

                curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, Math.PI, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, false);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI && pieParams.IsLargerArc)
            {
                // Outer curved paths
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);
                curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, Math.PI * 2, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);

                // Inner curved path
                curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, Math.PI, Math.PI * 2, topFaceCenter, bottomFaceCenter, centroid, false);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= 0 && pieParams.StartAngle <= Math.PI && pieParams.StopAngle >= Math.PI && pieParams.StopAngle <= Math.PI * 2)
            {
                // Outer curved path
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, Math.PI, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);

                // Inner curved path
                curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, Math.PI, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, false);
                curvedFaces.Add(curvedSegment);
            }
            else if (pieParams.StartAngle >= Math.PI && pieParams.StartAngle <= Math.PI * 2 && pieParams.StopAngle >= 0 && pieParams.StopAngle <= Math.PI)
            {
                // Outer curved path
                Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, 0, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                curvedFaces.Add(curvedSegment);

                // Inner curved path
                curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, pieParams.StartAngle, 0, topFaceCenter, bottomFaceCenter, centroid, false);
                curvedFaces.Add(curvedSegment);
            }
            else
            {
                if (pieParams.StartAngle >= 0 && pieParams.StopAngle <= Math.PI)
                {
                    Path curvedSegment = GetCurvedSegment(pieParams, pieParams.OuterRadius, pieParams.StartAngle, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, true);
                    curvedFaces.Add(curvedSegment);
                }
                else
                {
                    Path curvedSegment = curvedSegment = GetCurvedSegment(pieParams, pieParams.InnerRadius, pieParams.StartAngle, pieParams.StopAngle, topFaceCenter, bottomFaceCenter, centroid, false);
                    curvedFaces.Add(curvedSegment);
                }
            }

            return curvedFaces;
        }

        private static Point3D GetCentroid(params Point3D[] points)
        {
            Double sumX = 0;
            Double sumY = 0;
            Double sumZ = 0;

            foreach (Point3D point in points)
            {
                sumX += point.X;
                sumY += point.Y;
                sumZ += point.Z;
            }

            Point3D centroid = new Point3D();
            centroid.X = sumX / points.Length;
            centroid.Y = sumY / points.Length;
            centroid.Z = sumZ / points.Length;

            return centroid;
        }

        private static Point3D GetFaceZIndex(params Point3D[] points)
        {
            return GetCentroid(points);
        }

        private static PathGeometry GetPathGeometryFromList(FillRule fillRule, Point startPoint, List<PathGeometryParams> pathGeometryParams)
        {
            PathGeometry pathGeometry = new PathGeometry();

            pathGeometry.FillRule = fillRule;
            pathGeometry.Figures = new PathFigureCollection();

            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = startPoint;
            pathFigure.Segments = new PathSegmentCollection();
            pathFigure.IsClosed = true;

            foreach (PathGeometryParams param in pathGeometryParams)
            {
                switch (param.GetType().Name)
                {
                    case "LineSegmentParams":
                        LineSegment lineSegment = new LineSegment();
                        lineSegment.Point = param.EndPoint;
                        pathFigure.Segments.Add(lineSegment);
                        break;

                    case "ArcSegmentParams":
                        ArcSegment arcSegment = new ArcSegment();

                        arcSegment.Point = param.EndPoint;
                        arcSegment.IsLargeArc = (param as ArcSegmentParams).IsLargeArc;
                        arcSegment.RotationAngle = (param as ArcSegmentParams).RotationAngle;
                        arcSegment.SweepDirection = (param as ArcSegmentParams).SweepDirection;
                        arcSegment.Size = (param as ArcSegmentParams).Size;
                        pathFigure.Segments.Add(arcSegment);

                        break;
                }
            }

            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }

        private static Brush GetLightingEnabledBrush(Brush brush)
        {
            if (typeof(SolidColorBrush).Equals(brush.GetType()))
            {
                SolidColorBrush solidBrush = brush as SolidColorBrush;

                List<Color> colors = new List<Color>();
                List<Double> stops = new List<Double>();

                colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.745));
                stops.Add(1);

                colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.99));
                stops.Add(0);


                return Graphics.CreateRadialGradientBrush(colors, stops);
            }
            else
            {
                return brush;
            }
        }

        private static Brush GetPieGradianceBrush()
        {
            RadialGradientBrush brush = new RadialGradientBrush() { GradientOrigin = new Point(0.5, 0.5) };
            brush.GradientStops = new GradientStopCollection();

            brush.GradientStops.Add(new GradientStop() { Offset = 0, Color = Color.FromArgb((Byte)0, (Byte)0, (Byte)0, (Byte)0) });
            brush.GradientStops.Add(new GradientStop() { Offset = 0.7, Color = Color.FromArgb((Byte)34, (Byte)0, (Byte)0, (Byte)0) });
            brush.GradientStops.Add(new GradientStop() { Offset = 1, Color = Color.FromArgb((Byte)127, (Byte)0, (Byte)0, (Byte)0) });

            return brush;
        }

        private static Brush GetDoughnutGradianceBrush()
        {
            RadialGradientBrush brush = new RadialGradientBrush() { GradientOrigin = new Point(0.5, 0.5) };
            brush.GradientStops = new GradientStopCollection();

            brush.GradientStops.Add(new GradientStop() { Offset = 0, Color = Color.FromArgb((Byte)0, (Byte)0, (Byte)0, (Byte)0) });
            brush.GradientStops.Add(new GradientStop() { Offset = 0.5, Color = Color.FromArgb((Byte)127, (Byte)0, (Byte)0, (Byte)0) });
            brush.GradientStops.Add(new GradientStop() { Offset = 0.75, Color = Color.FromArgb((Byte)00, (Byte)0, (Byte)0, (Byte)0) });
            brush.GradientStops.Add(new GradientStop() { Offset = 1, Color = Color.FromArgb((Byte)127, (Byte)0, (Byte)0, (Byte)0) });

            return brush;
        }


        private static Brush GetLighterBevelBrush(Brush brush, Double angle)
        {
            if (typeof(SolidColorBrush).Equals(brush.GetType()))
            {
                SolidColorBrush solidBrush = brush as SolidColorBrush;
                Double r, g, b;
                List<Color> colors = new List<Color>();
                List<Double> stops = new List<Double>();

                r = ((double)solidBrush.Color.R / (double)255) * 0.9999;
                g = ((double)solidBrush.Color.G / (double)255) * 0.9999;
                b = ((double)solidBrush.Color.B / (double)255) * 0.9999;

                colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.99));
                stops.Add(0);

                colors.Add(Graphics.GetLighterColor(solidBrush.Color, 1 - r, 1 - g, 1 - b));
                stops.Add(0.2);

                colors.Add(Graphics.GetLighterColor(solidBrush.Color, 1 - r, 1 - g, 1 - b));
                stops.Add(0.6);

                colors.Add(Graphics.GetLighterColor(solidBrush.Color, 0.99));
                stops.Add(1);

                return Graphics.CreateLinearGradientBrush(angle, new Point(0, 0.5), new Point(1, 0.5), colors, stops);
            }
            else
            {
                return brush;
            }
        }

        private static Brush GetDarkerBevelBrush(Brush brush, Double angle)
        {
            if (typeof(SolidColorBrush).Equals(brush.GetType()))
            {
                SolidColorBrush solidBrush = brush as SolidColorBrush;
                List<Color> colors = new List<Color>();
                List<Double> stops = new List<Double>();

                colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.35));
                stops.Add(0);

                colors.Add(Graphics.GetDarkerColor(solidBrush.Color, 0.65));
                stops.Add(1);

                return Graphics.CreateLinearGradientBrush(angle, new Point(0, 0.5), new Point(1, 0.5), colors, stops);
            }
            else
            {
                return brush;
            }
        }

        private static Brush GetCurvedBevelBrush(Brush brush, Double angle, DoubleCollection shade, DoubleCollection offset)
        {
            if (typeof(SolidColorBrush).Equals(brush.GetType()))
            {
                SolidColorBrush solidBrush = brush as SolidColorBrush;
                List<Color> colors = new List<Color>();
                List<Double> stops = new List<double>();

                for (Int32 i = 0; i < shade.Count; i++)
                {
                    Color newShade = (shade[i] < 0 ? Graphics.GetDarkerColor(solidBrush.Color, Math.Abs(shade[i])) : Graphics.GetLighterColor(solidBrush.Color, Math.Abs(shade[i])));
                    colors.Add(newShade);
                }
                for (Int32 i = 0; i < offset.Count; i++)
                {
                    stops.Add(offset[i]);
                }

                return Graphics.CreateLinearGradientBrush(angle, new Point(0, 0.5), new Point(1, 0.5), colors, stops);
            }
            else
            {
                return brush;
            }

        }

        private static DoubleCollection GetDoubleCollection(params double[] values)
        {
            DoubleCollection collection = new DoubleCollection();
            foreach (double value in values)
            {
                collection.Add(value);
            }
            return collection;
        }

        internal class PathGeometryParams
        {
            #region Public Methods
            public PathGeometryParams(Point endPoint)
            {
                EndPoint = endPoint;
            }
            #endregion Public Methods

            #region Public Properties
            public Point EndPoint
            {
                get;
                set;
            }
            #endregion

        }

        internal class LineSegmentParams : PathGeometryParams
        {
            #region Public Methods
            public LineSegmentParams(Point endPoint)
                : base(endPoint)
            {
            }
            #endregion Public Methods
        }

        internal class ArcSegmentParams : PathGeometryParams
        {
            #region Public Methods
            public ArcSegmentParams(Size size, Double rotation, Boolean isLargeArc, SweepDirection sweepDirection, Point endPoint)
                : base(endPoint)
            {
                Size = size;
                RotationAngle = rotation;
                IsLargeArc = isLargeArc;
                SweepDirection = sweepDirection;
            }
            #endregion Public Methods

            #region Public Properties

            public Size Size
            {
                get;
                set;
            }

            public Double RotationAngle
            {
                get;
                set;
            }

            public Boolean IsLargeArc
            {
                get;
                set;
            }

            public SweepDirection SweepDirection
            {
                get;
                set;
            }

            #endregion Public


        }

        private static DoubleCollection GenerateDoubleCollection(params Double[] values)
        {
            DoubleCollection collection = new DoubleCollection();
            foreach (Double value in values)
                collection.Add(value);
            return collection;
        }

        private static PointCollection GenerateDoubleCollection(params Point[] values)
        {
            PointCollection collection = new PointCollection();
            foreach (Point value in values)
                collection.Add(value);
            return collection;
        }

        private static List<KeySpline> GenerateKeySplineList(params Point[] values)
        {
            List<KeySpline> splines = new List<KeySpline>();
            for (Int32 i = 0; i < values.Length; i += 2)
                splines.Add(GetKeySpline(values[i], values[i + 1]));

            return splines;
        }

        private static KeySpline GetKeySpline(Point controlPoint1, Point controlPoint2)
        {
            return new KeySpline() { ControlPoint1 = controlPoint1, ControlPoint2 = controlPoint2 };
        }

        private static PointAnimationUsingKeyFrames CreatePointAnimation(DependencyObject target, String property, Double beginTime, List<Double> frameTime, List<Point> values, List<KeySpline> splines)
        {
            PointAnimationUsingKeyFrames da = new PointAnimationUsingKeyFrames();
#if WPF
            target.SetValue(FrameworkElement.NameProperty, target.GetType().Name + target.GetHashCode().ToString());
            Storyboard.SetTargetName(da, target.GetValue(FrameworkElement.NameProperty).ToString());

            DataSeriesRef.RegisterName((string)target.GetValue(FrameworkElement.NameProperty), target);
            DataPointRef.RegisterName((string)target.GetValue(FrameworkElement.NameProperty), target);
#else
            Storyboard.SetTarget(da, target);
#endif
            Storyboard.SetTargetProperty(da, new PropertyPath(property));

            da.BeginTime = TimeSpan.FromSeconds(beginTime);

            for (Int32 index = 0; index < splines.Count; index++)
            {
                SplinePointKeyFrame keyFrame = new SplinePointKeyFrame();
                keyFrame.KeySpline = splines[index];
                keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(frameTime[index]));
                keyFrame.Value = values[index];
                da.KeyFrames.Add(keyFrame);
            }

            return da;
        }

        private static List<Point> GenerateAnimationPoints(Point center, Double radius, Double startAngle, Double stopAngle)
        {
            List<Point> points = new List<Point>();
            Double step = Math.Abs(startAngle - stopAngle) / 100;
            if (step <= 0) return points;
            for (Double angle = startAngle; angle <= stopAngle; angle += step)
            {
                points.Add(new Point(center.X + radius * Math.Cos(angle), center.Y + radius * Math.Sin(angle)));
            }
            points.Add(new Point(center.X + radius * Math.Cos(stopAngle), center.Y + radius * Math.Sin(stopAngle)));
            return points;

        }

        private static List<Double> GenerateAnimationFrames(int count, double maxTime)
        {
            List<double> frames = new List<double>();
            for (int i = 0; i < count; i++)
            {
                frames.Add(maxTime * i / (double)(count - 1));
            }
            return frames;
        }

        private static List<KeySpline> GenerateAnimationSplines(int count)
        {
            List<KeySpline> splines = new List<KeySpline>();
            for (int i = 0; i < count; i++)
            {
                splines.Add(GetKeySpline(new Point(0, 0), new Point(1, 1)));
            }
            return splines;
        }

        private static Storyboard CreatePathSegmentAnimation(Storyboard storyboard, PathSegment target, Point center, Double radius, Double startAngle, Double stopAngle)
        {
            List<Point> points = GenerateAnimationPoints(center, radius, startAngle, stopAngle);
            List<Double> frames = GenerateAnimationFrames(points.Count, 1);
            List<KeySpline> splines = GenerateAnimationSplines(points.Count);

            PointAnimationUsingKeyFrames pieSliceAnimation = null;

            if (typeof(ArcSegment).IsInstanceOfType(target))
            {
                pieSliceAnimation = CreatePointAnimation(target, "(ArcSegment.Point)", 0.5, frames, points, splines);
            }
            else
            {
                pieSliceAnimation = CreatePointAnimation(target, "(LineSegment.Point)", 0.5, frames, points, splines);
            }

            storyboard.Stop();
            storyboard.Children.Add(pieSliceAnimation);
            return storyboard;
        }

        private static Storyboard CreatePathFigureAnimation(Storyboard storyboard, PathFigure target, Point center, Double radius, Double startAngle, Double stopAngle)
        {
            List<Point> points = GenerateAnimationPoints(center, radius, startAngle, stopAngle);
            List<Double> frames = GenerateAnimationFrames(points.Count, 1);
            List<KeySpline> splines = GenerateAnimationSplines(points.Count);

            PointAnimationUsingKeyFrames pieSliceAnimation = CreatePointAnimation(target, "(PathFigure.StartPoint)", 0.5, frames, points, splines);

            storyboard.Stop();
            storyboard.Children.Add(pieSliceAnimation);
            return storyboard;
        }

        private static Storyboard CreateLabelLineAnimation(Storyboard storyboard, PathSegment target, params Point[] points)
        {
            List<Point> pointsList = points.ToList();
            List<Double> frames = GenerateAnimationFrames(pointsList.Count, 1);
            List<KeySpline> splines = GenerateAnimationSplines(pointsList.Count);

            PointAnimationUsingKeyFrames labelLineAnimation = CreatePointAnimation(target, "(LineSegment.Point)", 1 + 0.5, frames, pointsList, splines);

            storyboard.Stop();
            storyboard.Children.Add(labelLineAnimation);
            return storyboard;
        }

        private static DoubleAnimationUsingKeyFrames CreateDoubleAnimation(DependencyObject target, String property, Double beginTime, DoubleCollection frameTime, DoubleCollection values, List<KeySpline> splines)
        {
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
#if WPF
            target.SetValue(FrameworkElement.NameProperty, target.GetType().Name + target.GetHashCode().ToString());
            Storyboard.SetTargetName(da, target.GetValue(FrameworkElement.NameProperty).ToString());

            DataSeriesRef.RegisterName((string)target.GetValue(FrameworkElement.NameProperty), target);
            DataPointRef.RegisterName((string)target.GetValue(FrameworkElement.NameProperty), target);
#else
            Storyboard.SetTarget(da, target);
#endif
            Storyboard.SetTargetProperty(da, new PropertyPath(property));

            da.BeginTime = TimeSpan.FromSeconds(beginTime);

            for (Int32 index = 0; index < splines.Count; index++)
            {
                SplineDoubleKeyFrame keyFrame = new SplineDoubleKeyFrame();
                keyFrame.KeySpline = splines[index];
                keyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(frameTime[index]));
                keyFrame.Value = values[index];
                da.KeyFrames.Add(keyFrame);
            }

            return da;
        }

        private static Storyboard CreateOpacityAnimation(Storyboard storyboard, DependencyObject target, Double beginTime, Double opacity, Double duration)
        {
            DoubleCollection values = GenerateDoubleCollection(0, opacity);
            DoubleCollection frames = GenerateDoubleCollection(0, duration);
            List<KeySpline> splines = GenerateAnimationSplines(frames.Count);
            DoubleAnimationUsingKeyFrames opacityAnimation = CreateDoubleAnimation(target, "(UIElement.Opacity)", beginTime + 0.5, frames, values, splines);
            storyboard.Stop();
            storyboard.Children.Add(opacityAnimation);
            return storyboard;
        }

        private static Storyboard CreateLabelLineInteractivityAnimation(Storyboard storyboard, PathSegment target, params Point[] points)
        {
            List<Point> pointsList = points.ToList();
            List<Double> frames = GenerateAnimationFrames(pointsList.Count, 0.4);
            List<KeySpline> splines = GenerateAnimationSplines(pointsList.Count);

            PointAnimationUsingKeyFrames labelLineAnimation = CreatePointAnimation(target, "(LineSegment.Point)", 0, frames, pointsList, splines);
            storyboard.Stop();
            storyboard.Children.Add(labelLineAnimation);

            return storyboard;
        }

        private static Storyboard CreateExplodingOut2DAnimation(Storyboard storyboard, Panel visual, Panel label, Path labelLine, TranslateTransform translateTransform, PieDoughnut2DPoints unExplodedPoints, PieDoughnut2DPoints explodedPoints, Double xOffset, Double yOffset)
        {
            storyboard.Stop();

            #region Animating Silce
            DoubleCollection values = GenerateDoubleCollection(0, xOffset);
            DoubleCollection frames = GenerateDoubleCollection(0, 0.4);
            List<KeySpline> splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

            values = GenerateDoubleCollection(0, yOffset);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

            storyboard.Children.Add(sliceXAnimation);
            storyboard.Children.Add(sliceYAnimation);
            #endregion Animating Silce

            #region Animating Label
            values = GenerateDoubleCollection(unExplodedPoints.LabelPosition.X, explodedPoints.LabelPosition.X);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames labelXAnimation = CreateDoubleAnimation(label, "(Canvas.Left)", 0, frames, values, splines);
            storyboard.Children.Add(labelXAnimation);
            #endregion Animating Label

            #region Animating Label Line
            PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;
            storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[0], unExplodedPoints.LabelLineMidPoint, explodedPoints.LabelLineMidPoint);
            storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[1], unExplodedPoints.LabelLineEndPoint, explodedPoints.LabelLineEndPoint);
            #endregion Animating Label Line
            return storyboard;
        }

        private static Storyboard CreateExplodingIn2DAnimation(Storyboard storyboard, Panel visual, Panel label, Path labelLine, TranslateTransform translateTransform, PieDoughnut2DPoints unExplodedPoints, PieDoughnut2DPoints explodedPoints, Double xOffset, Double yOffset)
        {
            storyboard.Stop();

            #region Animating Silce
            DoubleCollection values = GenerateDoubleCollection(xOffset, 0);
            DoubleCollection frames = GenerateDoubleCollection(0, 0.4);
            List<KeySpline> splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

            values = GenerateDoubleCollection(yOffset, 0);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

            storyboard.Children.Add(sliceXAnimation);
            storyboard.Children.Add(sliceYAnimation);
            #endregion Animating Silce

            #region Animating Label
            values = GenerateDoubleCollection(explodedPoints.LabelPosition.X, unExplodedPoints.LabelPosition.X);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames labelXAnimation = CreateDoubleAnimation(label, "(Canvas.Left)", 0, frames, values, splines);
            storyboard.Children.Add(labelXAnimation);
            #endregion Animating Label

            #region Animating Label Line
            PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
            PathSegmentCollection segments = figure.Segments;
            storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[0], explodedPoints.LabelLineMidPoint, unExplodedPoints.LabelLineMidPoint);
            storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[1], explodedPoints.LabelLineEndPoint, unExplodedPoints.LabelLineEndPoint);
            #endregion Animating Label Line
            return storyboard;
        }

        private static Storyboard CreateExplodingOut3DAnimation(Storyboard storyboard, List<Path> pathElements, Panel label, Path labelLine, PieDoughnut3DPoints unExplodedPoints, PieDoughnut3DPoints explodedPoints, Double xOffset, Double yOffset)
        {
            DoubleCollection values;
            DoubleCollection frames;
            List<KeySpline> splines;

            storyboard.Stop();

            #region Animating Slice

            foreach (Path path in pathElements)
            {
                if (path == null) continue;
                TranslateTransform translateTransform = path.RenderTransform as TranslateTransform;

                values = GenerateDoubleCollection(0, xOffset);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

                values = GenerateDoubleCollection(0, yOffset);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

                storyboard.Children.Add(sliceXAnimation);
                storyboard.Children.Add(sliceYAnimation);
            }

            #endregion Animating Slice

            #region Animating Label
            values = GenerateDoubleCollection(unExplodedPoints.LabelPosition.X, explodedPoints.LabelPosition.X);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames labelXAnimation = CreateDoubleAnimation(label, "(Canvas.Left)", 0, frames, values, splines);
            storyboard.Children.Add(labelXAnimation);
            #endregion Animating Label

            #region Animating Label Line
            if (labelLine != null)
            {
                TranslateTransform translateTransform = labelLine.RenderTransform as TranslateTransform;

                values = GenerateDoubleCollection(0, xOffset);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

                values = GenerateDoubleCollection(0, yOffset);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

                storyboard.Children.Add(sliceXAnimation);
                storyboard.Children.Add(sliceYAnimation);


                PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                PathSegmentCollection segments = figure.Segments;
                storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[0], unExplodedPoints.LabelLineMidPoint, explodedPoints.LabelLineMidPoint);
                storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[1], unExplodedPoints.LabelLineEndPoint, explodedPoints.LabelLineEndPoint);
            }
            #endregion Animating Label Line

            return storyboard;
        }

        private static Storyboard CreateExplodingIn3DAnimation(Storyboard storyboard, List<Path> pathElements, Panel label, Path labelLine, PieDoughnut3DPoints unExplodedPoints, PieDoughnut3DPoints explodedPoints, Double xOffset, Double yOffset)
        {
            DoubleCollection values;
            DoubleCollection frames;
            List<KeySpline> splines;

            if (storyboard != null)
                storyboard.Stop();

            #region Animating Slice

            foreach (Path path in pathElements)
            {
                if (path == null) continue;

                TranslateTransform translateTransform = path.RenderTransform as TranslateTransform;

                values = GenerateDoubleCollection(xOffset, 0);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

                values = GenerateDoubleCollection(yOffset, 0);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

                storyboard.Children.Add(sliceXAnimation);
                storyboard.Children.Add(sliceYAnimation);
            }

            #endregion Animating Slice

            #region Animating Label
            values = GenerateDoubleCollection(explodedPoints.LabelPosition.X, unExplodedPoints.LabelPosition.X);
            frames = GenerateDoubleCollection(0, 0.4);
            splines = GenerateKeySplineList
                (
                    new Point(0, 0), new Point(1, 1),
                    new Point(0, 0), new Point(0, 1)
                );

            DoubleAnimationUsingKeyFrames labelXAnimation = CreateDoubleAnimation(label, "(Canvas.Left)", 0, frames, values, splines);
            storyboard.Children.Add(labelXAnimation);
            #endregion Animating Label

            #region Animating Label Line
            if (labelLine != null)
            {
                TranslateTransform translateTransform = labelLine.RenderTransform as TranslateTransform;

                values = GenerateDoubleCollection(xOffset, 0);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceXAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.X)", 0, frames, values, splines);

                values = GenerateDoubleCollection(yOffset, 0);
                frames = GenerateDoubleCollection(0, 0.4);
                splines = GenerateKeySplineList
                    (
                        new Point(0, 0), new Point(1, 1),
                        new Point(0, 0), new Point(0, 1)
                    );

                DoubleAnimationUsingKeyFrames sliceYAnimation = CreateDoubleAnimation(translateTransform, "(TranslateTransform.Y)", 0, frames, values, splines);

                storyboard.Children.Add(sliceXAnimation);
                storyboard.Children.Add(sliceYAnimation);


                PathFigure figure = (labelLine.Data as PathGeometry).Figures[0];
                PathSegmentCollection segments = figure.Segments;
                storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[0], explodedPoints.LabelLineMidPoint, unExplodedPoints.LabelLineMidPoint);
                storyboard = CreateLabelLineInteractivityAnimation(storyboard, segments[1], explodedPoints.LabelLineEndPoint, unExplodedPoints.LabelLineEndPoint);
            }
            #endregion Animating Label Line

            return storyboard;
        }

        private static DataSeries DataSeriesRef
        {
            get;
            set;
        }

        private static DataPoint DataPointRef
        {
            get;
            set;
        }
    }
}