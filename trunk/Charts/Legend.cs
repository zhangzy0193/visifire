﻿/*
    Copyright (C) 2008 Webyog Softworks Private Limited

    This file is a part of Visifire Charts.
 
    Visifire is a free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
      
    You should have received a copy of the GNU General Public License
    along with Visifire Charts.  If not, see <http://www.gnu.org/licenses/>.
  
    If GPL is not suitable for your products or company, Webyog provides Visifire 
    under a flexible commercial license designed to meet your specific usage and 
    distribution requirements. If you have already obtained a commercial license 
    from Webyog, you can use this file under those license terms.
 
*/


using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Visifire.Commons;

namespace Visifire.Charts
{
    public class Legend : LegendBase
    {
        #region Public Methods

        public Legend()
        {

            
        }

        public override String TextParser(String str)
        {
            return str;
        }

        public override void SetHeight()
        {
            if (Enabled == true)
            {
                this.SetValue(HeightProperty, 15);
            }
            else
            {
                this.SetValue(HeightProperty, 0);
            }

        }

        public override void SetWidth()
        {
            if (Enabled == true)
            {
                this.SetValue(WidthProperty, 20);
            }
            else
            {
                this.SetValue(WidthProperty, 0);
            }

        }

        public override void Init()
        {
            base.Init();
            ValidateParent();

            SetName();
            if (BorderColor == null)
            {
                BorderColor = GetDefaultBorderColor();
            }

            AttachToolTip();
            AttachHref();

        }

        public override void SetLeft()
        {
            Double left = _parent._innerTitleBounds.Left;

            switch (AlignmentX)
            {
                case AlignmentX.Center:
                    if (!DockInsidePlotArea)
                    {
                        left = _parent.Width / 2 - this.Width / 2;
                    }
                    else
                    {
                        left = (Double)_parent.PlotArea.GetValue(LeftProperty) + _parent.PlotArea.Width / 2 - this.Width / 2;
                    }
                    break;
                case AlignmentX.Left:
                    if (!DockInsidePlotArea)
                    {
                        if (AlignmentY == AlignmentY.Center)
                            left = _parent._innerTitleBounds.Left;
                        else
                            left = 0;
                        foreach (Legend child in _parent.Legends)
                        {
                            if (this == child) break;
                            if (AlignmentX == child.AlignmentX && child.AlignmentY == AlignmentY.Center && AlignmentY == AlignmentY.Center)
                            {
                                if (left < (Double)child.GetValue(LeftProperty) + (Double)child.Width)
                                    left = (Double)child.GetValue(LeftProperty) + (Double)child.Width;
                            }
                        }
                        left += _parent.Padding;
                        if (left + Width > _parent._innerBounds.Left && AlignmentY == AlignmentY.Center)
                        {
                            _parent._innerBounds.X = left + Width;
                            _parent._innerBounds.Width -= Width;
                        }
                    }
                    else
                    {
                        left = (Double)_parent.PlotArea.GetValue(LeftProperty) + _parent.Padding;
                    }
                    break;
                case AlignmentX.Right:
                    if (!DockInsidePlotArea)
                    {
                        if (AlignmentY == AlignmentY.Center)
                            left = _parent._innerTitleBounds.Right;
                        else
                            left = _parent.Width;

                        foreach (Legend child in _parent.Legends)
                        {
                            if (this == child) break;
                            if (AlignmentX == child.AlignmentX && child.AlignmentY == AlignmentY.Center && AlignmentY == AlignmentY.Center)
                            {
                                if (left > (Double)child.GetValue(LeftProperty))
                                    left = (Double)child.GetValue(LeftProperty);
                            }
                        }
                        left -= (Width + _parent.Padding);
                        if (left < _parent._innerBounds.Right && AlignmentY == AlignmentY.Center) _parent._innerBounds.Width = left - _parent._innerBounds.Left;
                    }
                    else
                    {
                        left = (Double)_parent.PlotArea.GetValue(LeftProperty) + _parent.PlotArea.Width - Width - _parent.Padding;
                    }
                    break;
            }
            SetValue(LeftProperty, left);
        }

        public override void SetTop()
        {
            Double top = 0;
            Double tempTop = _parent._innerTitleBounds.Top;
            Double childHeight = 0;
            switch (AlignmentY)
            {
                case AlignmentY.Top:
                    if (!DockInsidePlotArea)
                        tempTop = _parent._innerTitleBounds.Top + _parent.Padding;
                    else
                    {
                        tempTop = (Double)_parent.PlotArea.GetValue(TopProperty);
                        foreach (Title title in _parent.Titles)
                        {
                            if (title.DockInsidePlotArea == true && title.AlignmentY == AlignmentY.Top)
                            {
                                if (tempTop <= (Double)title.GetValue(TopProperty) + title.Height)
                                {
                                    tempTop = (Double)title.GetValue(TopProperty) + title.Height;
                                }
                            }
                        }
                    }
                    foreach (Legend child in _parent.Legends)
                    {
                        if (child == this) break;
                        if (child.AlignmentY == AlignmentY && this.DockInsidePlotArea == DockInsidePlotArea)
                        {
                            if (CheckHorizontalOverlap(this, child))
                            {
                                if (tempTop <= (Double)child.GetValue(TopProperty))
                                {
                                    tempTop = (Double)child.GetValue(TopProperty);
                                    childHeight = child.Height;
                                }
                            }
                        }
                    }
                    tempTop += childHeight;
                    if (tempTop > top) top = tempTop;
                    SetValue(TopProperty, top);
                    // Set the inner bounds
                    if (!DockInsidePlotArea && (top + Height > _parent._innerBounds.Top))
                    {
                        _parent._innerBounds.Y = top + Height;
                        _parent._innerBounds.Height -= Height;
                    }

                    break;
                case AlignmentY.Center:
                    if ((AlignmentX == AlignmentX.Right || AlignmentX == AlignmentX.Left) && !DockInsidePlotArea)
                    {
                        top = (_parent.Height - this.Height) / 2;
                    }
                    else
                    {
                        if (DockInsidePlotArea) tempTop = (_parent.PlotArea.Height) / 2;
                        else tempTop = (_parent.Height) / 2;
                        foreach (Title title in _parent.Titles)
                        {
                            if (title.AlignmentY == AlignmentY && title.AlignmentX == AlignmentX.Center)
                            {
                                if (tempTop < (Double)title.GetValue(TopProperty) + title.Height)
                                {
                                    tempTop = (Double)title.GetValue(TopProperty);

                                    childHeight = title.Height;
                                }
                            }
                        }
                        foreach (Legend child in _parent.Legends)
                        {
                            if (child == this) break;
                            if (child.AlignmentY == this.AlignmentY)
                            {

                                if (CheckHorizontalOverlap(this, child))
                                {
                                    if (tempTop < (Double)child.GetValue(TopProperty))
                                    {
                                        tempTop = (Double)child.GetValue(TopProperty);

                                        childHeight = child.Height;
                                    }
                                }
                            }
                        }
                        tempTop += childHeight;
                        if (tempTop > top) top = tempTop;
                    }
                    this.SetValue(TopProperty, (Double) top);
                    break;
                case AlignmentY.Bottom:
                    if (!DockInsidePlotArea)
                        tempTop = _parent._innerTitleBounds.Bottom - _parent.Padding;
                    else
                    {
                        tempTop = (Double)_parent.PlotArea.GetValue(TopProperty) + _parent.PlotArea.Height;
                        foreach (Title title in _parent.Titles)
                        {
                            if (title.DockInsidePlotArea == true && title.AlignmentY == AlignmentY.Bottom)
                            {
                                if (tempTop > (Double)title.GetValue(TopProperty))
                                {
                                    tempTop = (Double)title.GetValue(TopProperty);
                                }
                            }
                        }
                    }
                    childHeight = Height;
                    foreach (Legend child in _parent.Legends)
                    {
                        if (child == this) break;
                        if (child.AlignmentY == AlignmentY && this.DockInsidePlotArea == DockInsidePlotArea)
                        {
                            if (CheckHorizontalOverlap(this, child))
                            {
                                if (tempTop >= (Double)child.GetValue(TopProperty))
                                {
                                    tempTop = (Double)child.GetValue(TopProperty);
                                    childHeight = child.Height;
                                }
                            }
                        }
                    }
                    tempTop -= childHeight + _parent.Padding;
                    top = tempTop;
                    this.SetValue(Canvas.TopProperty, top);
                    //Set Inner bounds 
                    if (!DockInsidePlotArea && (top < _parent._innerBounds.Bottom)) _parent._innerBounds.Height = top - _parent._innerBounds.Top;
                    break;

            }

        }

        #endregion Public Methods

        #region Public Properties
        public virtual Boolean Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                _enabledByUser = true;
            }
        }
        #endregion Public Properties

        #region Internal Methods
        internal void SetMaxWidthHeight()
        {
            if (Enabled == true)
            {
                if (Double.IsNaN(_maxWidth))
                {
                    switch (AlignmentY)
                    {
                        case AlignmentY.Top:
                        case AlignmentY.Bottom:
                            if (DockInsidePlotArea)
                            {
                                _maxWidth = (Double)_parent.PlotArea.GetValue(WidthProperty) - 2 * _parent.Padding;
                            }
                            else
                            {
                                _maxWidth = (Double)_parent.GetValue(WidthProperty) - 2 * _parent.Padding;
                            }
                            break;

                        case AlignmentY.Center:
                            if (!DockInsidePlotArea)
                            {
                                _maxWidth = (Double)_parent.GetValue(WidthProperty) * .4;
                                Double tempWidth = 0;
                                foreach (Title child in _parent.Titles)
                                {
                                    if ((child.AlignmentX == AlignmentX.Left || child.AlignmentX == AlignmentX.Right) && child.AlignmentY == AlignmentY.Center)
                                    {
                                        tempWidth += _parent.Padding + child.Width;
                                    }
                                }
                                if (this.AlignmentX == AlignmentX.Left || this.AlignmentX == AlignmentX.Right)
                                {
                                    tempWidth += _parent.Padding;
                                    if (_maxWidth > tempWidth) _maxWidth -= tempWidth;
                                }
                            }
                            else
                            {
                                _maxWidth = (Double)_parent.PlotArea.GetValue(WidthProperty) * .4;
                            }
                            break;
                    }
                }
                if (Double.IsNaN(_maxHeight))
                {
                    switch (AlignmentY)
                    {
                        case AlignmentY.Top:
                        case AlignmentY.Bottom:
                            if (DockInsidePlotArea == true)
                            {
                                _maxHeight = (Double)_parent.PlotArea.GetValue(HeightProperty) * .5;
                            }
                            else
                            {
                                _maxHeight = (Double)_parent.GetValue(HeightProperty) * .5;
                            }
                            break;

                        case AlignmentY.Center:
                            if (DockInsidePlotArea == false)
                            {
                                _maxHeight = (Double)_parent.GetValue(HeightProperty);
                            }
                            else
                            {
                                _maxHeight = (Double)_parent.PlotArea.GetValue(HeightProperty);
                            }
                            break;
                    }
                }
            }
            else
            {
                _maxHeight = 0;
                _maxWidth = 0;

                this.SetValue(WidthProperty, 0);
                this.SetValue(HeightProperty, 0);
            }
        }

        internal void MarkLegend()
        {
            if (SeriesCount == 1)
            {
                MarkDataPoints();
            }
            else if (SeriesCount > 1)
            {
                MarkDataSeries();
            }
            else
            {
                //No dataseries is pointing to this legend

            }
            this.SetTitleOfLegendPosition();

        }

        internal void ApplyEffects()
        {
            if (LightingEnabled)
            {
                if(Background != null)
                    ApplyLighting();
            }
            if (ShadowEnabled)
            {
                if(Background != null)
                    ApplyShadow();
            }
            if (Bevel)
            {
                String[] type = { "Bright", "Medium", "Dark", "Medium" };
                Double[] length = { 8, 5, 8, 5 };
                Double[] Angle = { 90, 180, -90, 0 };
                ApplyBevel(type, length, Angle, 0);
            }

        }

        internal void PlaceOutsidePlotArea(Rect bounds,Double padding)
        {
            if (!DockInsidePlotArea)
                MarkAndPlaceLegend(bounds, padding);
        }

        internal void PlaceInsidePlotArea(Rect bounds, Double padding)
        {
            if (DockInsidePlotArea)
                MarkAndPlaceLegend(bounds, padding);
        }

        internal void MarkAndPlaceLegend(Rect bounds, Double padding)
        {
            SetMaxWidthHeight(bounds, padding);
            MarkLegend();
            SetTop();
            SetLeft();
            ApplyEffects();
        }
        #endregion Internal Methods

        #region Private Methods

        private Boolean CheckHorizontalOverlap(Legend legend1, Legend legend2)
        {
            Rect rect1 = new Rect((Double)legend1.GetValue(LeftProperty), (Double)legend1.GetValue(TopProperty), legend1.Width, legend1.Height);
            Rect rect2 = new Rect((Double)legend2.GetValue(LeftProperty), (Double)legend2.GetValue(TopProperty), legend2.Width, legend2.Height);

            if (rect1.Right >= rect2.Left && rect1.Right <= rect2.Right) return true;
            if (rect1.Left >= rect2.Left && rect1.Left <= rect2.Right) return true;
            if (rect1.Left >= rect2.Left && rect1.Right <= rect2.Right) return true;
            if (rect1.Left <= rect2.Left && rect1.Right >= rect1.Right) return true;
            return false;
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();

            Width = 0;
            Height = 0;


            AlignmentX = AlignmentX.Center;
            AlignmentY = AlignmentY.Bottom;

            FontStyle = "Normal";
            FontWeight = "Normal";
            FontSize = Double.NaN;
            FontColor = null;
            FontFamily = "Verdana";

            SeriesCount = 0;

            _enabled = true;
            _enabledByUser = false;

            this.SetValue(ZIndexProperty, 3);
        }

        private Brush GetDefaultBorderColor()
        {
            Brush borderColorBrush = null;

            if (DockInsidePlotArea && _parent.PlotArea.Background == null)
            {
                if (_parent.Background == null)
                    borderColorBrush = new SolidColorBrush(Colors.Black);
                else
                {
                    borderColorBrush = Parser.GetDefaultBorderColor(Parser.GetBrushIntensity(_parent.Background));
                }

            }
            else if (DockInsidePlotArea)
            {
                borderColorBrush = Parser.GetDefaultBorderColor(Parser.GetBrushIntensity(_parent.Background));
            }
            else
            {
                if (_parent.Background == null)
                    borderColorBrush = new SolidColorBrush(Colors.Black);
                else
                {
                    borderColorBrush = Parser.GetDefaultBorderColor(Parser.GetBrushIntensity(_parent.Background));
                }
            }
            return borderColorBrush;
        }

        public override System.Collections.Generic.List<Point> GetBoundingPoints()
        {
            System.Collections.Generic.List<Point> points = new System.Collections.Generic.List<Point>();
            points.Add(new Point(0, 0));
            points.Add(new Point(this.Width, 0));
            points.Add(new Point(this.Width, this.Height));
            points.Add(new Point(0, this.Height));
            return points;
        }
        
        /// <summary>
        /// Calculates marker sizes from chart size for the Legend fonts
        /// </summary>
        /// <returns></returns>
        private Int32 CalculateMarkerSize()
        {
            Int32[] fontSizes = { 6, 8, 10, 12, 14, 16, 18, 20, 22 };
            Double _parentSize = (Parent as FrameworkElement).Width * (Parent as FrameworkElement).Height;
            return fontSizes[(Int32)(Math.Ceiling(((_parentSize + 161027.5) / 163840)) % 9)];
        }

        private void MarkDataPoints()
        {
            Spacing = Padding;
            Int32 size = CalculateMarkerSize();
            Visifire.Charts.Marker marker = new Visifire.Charts.Marker();
            foreach (Plot plot in _parent.PlotDetails.Plots)
            {
                foreach (DataSeries dataSeries in plot.DataSeries)
                {
                    foreach (DataPoint dataPoint in dataSeries.DataPoints)
                    {
                        if (Name == dataSeries.Legend && dataPoint.ShowInLegend)
                        {
                            switch (dataSeries.RenderAs.ToUpper())
                            {
                                case "STACKEDAREA100":
                                case "STACKEDAREA":
                                case "AREA":
                                case "COLUMN":
                                case "STACKEDCOLUMN":
                                case "BAR":
                                case "STACKEDBAR":
                                case "STACKEDBAR100":
                                case "STACKEDCOLUMN100":
                                    Rectangle rect = new Rectangle();
                                    rect.Tag = this.Name;
                                    rect.Stroke = (dataPoint.BorderColor);
                                    rect.Fill = (dataPoint.Background);
                                    rect.StrokeThickness = dataPoint.BorderThickness;
                                    rect.Height = size;
                                    rect.Width = size;
                                    rect.StrokeDashArray = Parser.GetStrokeDashArray(dataPoint.BorderStyle);

                                    
                                    if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                        DrawEntryInFlowLayout(rect, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    else
                                        DrawEntryInGridLayout(rect, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    EntriesPresent = true;
                                    break;

                                case "LINE":
                                    Canvas linemarker = new Canvas();
                                    Line line = new Line();

                                    linemarker.Tag = this.Name;
                                    line.Tag = this.Name;

                                    line.Stroke = (dataPoint.Background);
                                    line.StrokeThickness = 5;

                                    line.X1 = 0;
                                    line.X2 = size;
                                    line.Y1 = size/2;
                                    line.Y2 = size/2;
                                    line.Height = size;
                                    line.Width = size;

                                    linemarker.Children.Add(line);

                                    marker.Tag = this.Name;

                                    linemarker.Children.Add(marker);
                                    if (Boolean.Parse(dataSeries.MarkerEnabled))
                                    {
                                        marker.BorderColor = (dataSeries.MarkerBorderColor);
                                        if (dataSeries.MarkerBackground != null)
                                            marker.Color = (dataSeries.MarkerBackground);
                                        else
                                            marker.Color = new SolidColorBrush(Colors.White);
                                        marker.BorderThickness = dataSeries.MarkerBorderThickness;
                                        marker.Style = dataSeries.MarkerStyle;
                                        marker.Width = size;
                                        marker.Height = size;
                                        marker.Size = size / 2;
                                        marker.SetValue(LeftProperty, (Double) ( marker.Size / 2));
                                        marker.SetValue(TopProperty, (Double) marker.Size / 2);
                                        marker.SetValue(ZIndexProperty, (Int32)line.GetValue(ZIndexProperty) + 1);
                                    }
                                    linemarker.Width = size;
                                    linemarker.Height = size;

                                    if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                        DrawEntryInFlowLayout(linemarker, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    else
                                        DrawEntryInGridLayout(linemarker, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    EntriesPresent = true;
                                    break;

                                case "BUBBLE":
                                    
                                    // Initial values for minimum size of the bubble
                                    Double minSize = 5;
                                    Double minZ = Double.PositiveInfinity;

                                    // Initial values of the maximum size of the bubble
                                    Double maxSize = size;
                                    Double maxZ = 0;

                                    Int32 i;

                                    // find true min and max Z
                                    for (i = 0; i < dataSeries.DataPoints.Count; i++)
                                    {
                                        if (minZ > dataSeries.DataPoints[i].ZValue) minZ = dataSeries.DataPoints[i].ZValue;
                                        if (maxZ < dataSeries.DataPoints[i].ZValue) maxZ = dataSeries.DataPoints[i].ZValue;
                                    }

                                    // Slope to calculate bubble size for datapoint values
                                    Double slope = (maxSize - minSize) / (maxZ - minZ);
                                    Double intercept = minSize - minZ * slope;

                                    if (Double.IsNaN(dataPoint.ZValue) || Double.IsInfinity(dataPoint.ZValue))
                                    {
                                        continue;
                                    }
                                    else
                                    {

                                        marker.Size = (slope * dataPoint.ZValue + intercept) * dataPoint.MarkerScale;
                                        marker.ImageScale = 1;
                                    }

                                    marker.Tag = this.Name;
                                    marker.BorderColor = (dataPoint.BorderColor);
                                    marker.Color = (dataPoint.Background);
                                    marker.BorderThickness = dataPoint.BorderThickness;
                                    marker.Style = dataPoint.MarkerStyle;
                                    marker.Width = marker.Size;
                                    marker.Height = marker.Size;
                                    if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                        DrawEntryInFlowLayout(marker, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    else
                                        DrawEntryInGridLayout(marker, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    EntriesPresent = true;
                                    break;
                                case "POINT":
                                    Visifire.Charts.Marker point = new Visifire.Charts.Marker();

                                    point.Tag = this.Name;
                                    point.Size = size;
                                    point.BorderColor = (dataPoint.BorderColor);
                                    point.Color = (dataPoint.Background);
                                    point.BorderThickness = dataPoint.BorderThickness;
                                    point.Style = dataPoint.MarkerStyle;
                                    point.Width = size;
                                    point.Height = size;
                                    if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                        DrawEntryInFlowLayout(point, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    else
                                        DrawEntryInGridLayout(point, (String.IsNullOrEmpty(dataPoint.LegendText) ? dataPoint.Name : dataPoint.LegendText));
                                    EntriesPresent = true;
                                    break;
                                case "DOUGHNUT":
                                case "PIE":
                                    
                                    Ellipse ellipse = new Ellipse();
                                    ellipse.Tag = this.Name;

                                    ellipse.Height = size;
                                    ellipse.Width = size;
                                    ellipse.Stroke = (dataPoint.BorderColor);
                                    ellipse.Fill = (dataPoint.Background);
                                    ellipse.StrokeDashArray = Parser.GetStrokeDashArray(dataPoint.BorderStyle);

                                   
                                    String text = (String.IsNullOrEmpty(dataPoint.LegendText) ? (String.IsNullOrEmpty(dataPoint.AxisLabel)?dataPoint.Name:dataPoint.AxisLabel) : dataPoint.LegendText);
                                    if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                        DrawEntryInFlowLayout(ellipse, text);
                                    else
                                        DrawEntryInGridLayout(ellipse, text);

                                    EntriesPresent = true;
                                    break;
                            }
                        }
                    }
                }
            }
            if (!EntriesPresent) { MarkDataSeries(); }
        }

        private void MarkDataSeries()
        {
            Spacing = Padding;
            Int32 size = CalculateMarkerSize();
            Visifire.Charts.Marker marker;

            foreach (Plot plot in _parent.PlotDetails.Plots)
            {
                foreach (DataSeries dataSeries in plot.DataSeries)
                {
                    if (Name == dataSeries.Legend && dataSeries.ShowInLegend)
                    {
                        switch (dataSeries.RenderAs.ToUpper())
                        {
                            case "STACKEDAREA100":
                            case "STACKEDAREA":
                            case "AREA":
                            case "COLUMN":
                            case "STACKEDCOLUMN":
                            case "BAR":
                            case "STACKEDBAR":
                            case "STACKEDBAR100":
                            case "STACKEDCOLUMN100":
                                Canvas can = new Canvas();
                                Rectangle rect = new Rectangle();
                                can.Tag = this.Name; 
                                rect.Tag = this.Name; 

                                rect.Stroke = (dataSeries.BorderColor);
                                rect.Fill = (dataSeries.Background);
                                rect.StrokeThickness = dataSeries.BorderThickness;
                                rect.Height = size;
                                rect.Width = size;
                                rect.StrokeDashArray = Parser.GetStrokeDashArray(dataSeries.BorderStyle);
                                
                                can.Children.Add(rect);
                                can.Width = rect.Width;
                                can.Height = rect.Height;
                                if (dataSeries.Bevel && !_parent.View3D)
                                {
                                    Polygon bevel = new Polygon();
                                    bevel.Tag = this.Name;
                                    bevel.IsHitTestVisible = false;

                                    Point[] points= new Point[4];
                                    points[0] = new Point(0, 0);
                                    points[1] = new Point(can.Width, 0);
                                    points[2] = new Point(can.Width-3, 3);
                                    points[3] = new Point(3, 3);
                                    bevel.Points = Converter.ArrayToCollection(points);
                                    can.Children.Add(bevel);
                                    Brush brush = dataSeries.Background;
                                    if (brush == null)
                                        brush = new SolidColorBrush(Colors.Transparent);

                                    if (brush.GetType().Name == "SolidColorBrush")
                                    {
                                        System.Windows.Media.Color C = (brush as SolidColorBrush).Color;
                                        Double intensity = (Double)(C.R + C.G + C.B) / (3 * 255);
                                        String fillString = "90;";
                                        if (intensity > 0.3)
                                        {
                                            fillString += Parser.GetLighterColor((brush as SolidColorBrush).Color, 0.35) + ",0;";
                                            fillString += Parser.GetLighterColor((brush as SolidColorBrush).Color, 0.70) + ",1";
                                        }
                                        else
                                        {
                                            fillString += Parser.GetDarkerColor((brush as SolidColorBrush).Color, 0.65) + ",1;";
                                            fillString += Parser.GetLighterColor((brush as SolidColorBrush).Color, 0.55) + ",0";
                                        }

                                        bevel.Fill = Parser.ParseLinearGradient(fillString);
                                    }
                                    else if (brush.GetType().Name == "LinearGradientBrush" || brush.GetType().Name == "RadialGradientBrush")
                                    {
                                        if (brush.GetType().Name == "LinearGradientBrush")
                                            bevel.Fill = new LinearGradientBrush();
                                        else
                                            bevel.Fill = new RadialGradientBrush();

                                        Parser.GenerateLighterGradientBrush(brush as GradientBrush, bevel.Fill as GradientBrush, 0.6);
                                    }
                                }

                                if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                    DrawEntryInFlowLayout(can, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : dataSeries.LegendText));
                                else
                                    DrawEntryInGridLayout(can, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : dataSeries.LegendText));
                                EntriesPresent = true;
                                break;

                            case "LINE":
                                Canvas linemarker = new Canvas();
                                Line line = new Line();

                                linemarker.Tag = this.Name;
                                line.Tag = this.Name;

                                line.Stroke = (dataSeries.Background);
                                line.StrokeThickness = 3;
                                Double lineLength = size * 1.5;

                                line.X1 = 0;
                                line.X2 = lineLength;
                                line.Y1 = size / 2;
                                line.Y2 = size / 2;
                                line.Height = size;
                                line.Width = lineLength;
                                marker = new Visifire.Charts.Marker();
                                marker.Tag = this.Name;

                                linemarker.Children.Add(line);
                                linemarker.Children.Add(marker);
                                if (dataSeries._markerEnabled.ToLower() != "false")
                                {
                                    marker.BorderColor = (dataSeries.MarkerBorderColor);
                                    if (marker.BorderColor == null)
                                        marker.BorderColor = (dataSeries.Background);
                                    if (dataSeries.MarkerBackground != null)
                                        marker.Color = (dataSeries.MarkerBackground);
                                    else
                                        marker.Color = new SolidColorBrush(Colors.White);
                                    marker.BorderThickness = dataSeries.MarkerBorderThickness;
                                    marker.Style = dataSeries.MarkerStyle;
                                    marker.Width = size;
                                    marker.Height = size;
                                    marker.Size = size/2;
                                    marker.SetValue(LeftProperty, (Double) ( lineLength / 2- marker.Size/2));
                                    marker.SetValue(TopProperty, (Double) ( marker.Size / 2));
                                    marker.SetValue(ZIndexProperty, (Int32)line.GetValue(ZIndexProperty) + 1);
                                }
                                linemarker.Width =lineLength;
                                linemarker.Height = size;

                                if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                    DrawEntryInFlowLayout(linemarker, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : dataSeries.LegendText));
                                else
                                    DrawEntryInGridLayout(linemarker, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : dataSeries.LegendText));
                                EntriesPresent = true;
                                break;

                            case "BUBBLE":
                            case "POINT":
                                marker = new Visifire.Charts.Marker();
                                marker.Tag = this.Name;

                                marker.Size = size;
                                marker.BorderColor = (dataSeries.BorderColor);
                                marker.Color = (dataSeries.Background);
                                marker.BorderThickness = dataSeries.BorderThickness;
                                marker.Style = dataSeries.MarkerStyle;
                                marker.Width = size;
                                marker.Height = size;
                                if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                    DrawEntryInFlowLayout(marker, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : ""));
                                else
                                    DrawEntryInGridLayout(marker, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : ""));
                                EntriesPresent = true;
                                break;
                            case "DOUGHNUT":
                            case "PIE":
                                Ellipse ellipse = new Ellipse();
                                ellipse.Tag = this.Name;

                                ellipse.Height = size;
                                ellipse.Width = size;
                                ellipse.Stroke = (dataSeries.BorderColor);
                                ellipse.Fill = (dataSeries.Background);
                                ellipse.StrokeDashArray = Parser.GetStrokeDashArray(dataSeries.BorderStyle);
                                
                                if ((this.AlignmentY == AlignmentY.Top || this.AlignmentY == AlignmentY.Bottom) && DockInsidePlotArea == false)
                                    DrawEntryInFlowLayout(ellipse, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : ""));
                                else
                                    DrawEntryInGridLayout(ellipse, (String.IsNullOrEmpty(dataSeries.LegendText) ? dataSeries.Name : ""));

                                EntriesPresent = true;
                                break;
                        }
                    }
                }
            }
            if (!EntriesPresent) { Width = 0; Height = 0; }
        }

        /// <summary>
        /// Validate Parent element and assign it to _parent.
        /// Parent element should be a Canvas element. Else throw an exception.
        /// </summary>
        private void ValidateParent()
        {
            if (this.Parent.GetType().Name == "Chart")
                _parent = this.Parent as Chart;
            else
                throw new Exception(this + "Parent should be a Chart");
        }

        
        #endregion Private Methods

        #region internal Properties

        internal Int32 SeriesCount
        {
            get;

            set;

        }

        internal Boolean EntriesPresent
        {
            get;
            set;
        }

        internal Boolean EnabledByUser
        {
            get
            {
                return _enabledByUser;
            }
        }
        #endregion internal Properties

        #region Data

        private Double _maxHeight;
        private Double _maxWidth;

        private Chart _parent;


        private Boolean _enabled;
        private Boolean _enabledByUser;
        #endregion Data
    }
}