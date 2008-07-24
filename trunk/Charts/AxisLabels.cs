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
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Visifire.Commons;

namespace Visifire.Charts
{
    public class AxisLabels : VisualObject
    {
        #region Public Methods

        public AxisLabels()
        {
        }

        public override void Render()
        {
        }

        public override void Init()
        {
            base.Init();

            ValidateParent();

            SetName();

            MaxLabelWidth = (_parent.Parent as Chart).Width * (TextWrap <= 0 ? 0.3 : TextWrap);

            AttachHref();
            AttachToolTip();
        }

        public override void SetWidth()
        {
            Double maxWidth = 0;

            if (_parent.AxisOrientation == AxisOrientation.Bar)
            {

                foreach (AxisLabel lbl in this.Children)
                {
                    maxWidth = Math.Max(maxWidth, lbl.ActualWidth);
                }
                maxWidth += _parent._parent.Padding;
            }
            else if (_parent.AxisOrientation == AxisOrientation.Column)
            {
                maxWidth = _parent.Width;

            }
            this.SetValue(WidthProperty, maxWidth);
        }

        public override void SetHeight()
        {
            Double maxHeight = 0;

            if (_parent.AxisOrientation == AxisOrientation.Bar)
            {
                maxHeight = _parent.Height;
            }
            else if (_parent.AxisOrientation == AxisOrientation.Column)
            {
                maxHeight = _rowHeight * _numberOfCalculatedRows + _parent._parent.Padding;
            }
            this.SetValue(HeightProperty, maxHeight);
        }

        public override void SetLeft()
        {
            if (_parent.AxisOrientation == AxisOrientation.Bar)
            {
                if ((_parent.Parent as Chart).View3D)
                {
                    if (_parent.GetType().Name == "AxisX")
                    {
                        if (!String.IsNullOrEmpty(_parent.Title))
                            this.SetValue(LeftProperty, (Double) _parent._titleTextBlock.ActualHeight);
                        else
                            this.SetValue(LeftProperty, (Double) 0);
                    }
                    else
                    {
                        if (_parent.AxisType == AxisType.Primary)
                        {
                            if (!String.IsNullOrEmpty(_parent.Title))
                                this.SetValue(LeftProperty, (Double) _parent._titleTextBlock.ActualHeight);
                            else
                                this.SetValue(LeftProperty, (Double) 0);
                        }
                        else
                        {
                            this.SetValue(LeftProperty, (Double) 0);
                        }
                    }
                }
                else
                {
                    if (_parent.AxisType == AxisType.Primary)
                    {
                        if (!String.IsNullOrEmpty(_parent.Title))
                            this.SetValue(LeftProperty, (Double) _parent._titleTextBlock.ActualHeight);
                        else
                            this.SetValue(LeftProperty, (Double) 0);
                    }
                    else
                    {
                        this.SetValue(LeftProperty, (Double) 0);
                    }
                }

            }
            else if (_parent.AxisOrientation == AxisOrientation.Column)
            {
                if ((_parent.Parent as Chart).View3D)
                {
                    this.SetValue(LeftProperty, (Double) ( -_parent.MajorTicks.TickLength));
                }
                else
                {
                    this.SetValue(LeftProperty, (Double) 0);
                }
            }
        }

        public override void SetTop()
        {
            if (_parent.AxisOrientation == AxisOrientation.Bar)
            {
                if ((_parent.Parent as Chart).View3D && _parent.AxisType== AxisType.Primary)
                {
                    this.SetValue(TopProperty, (Double) _parent.MajorTicks.TickLength);
                }
                else
                {
                    
                    this.SetValue(TopProperty, (Double) 0);
                    
                }
            }
            else if (_parent.AxisOrientation == AxisOrientation.Column)
            {
                if ((_parent.Parent as Chart).View3D && _parent.AxisType == AxisType.Primary)
                {
                    if (_parent.GetType().Name == "AxisX")
                        this.SetValue(TopProperty, (Double) _parent.MajorTicks.Height);
                    else
                        this.SetValue(TopProperty, (Double) ( (Double)_parent.MajorTicks.Height + _parent._parent.AxisX.MajorTicks.TickLength));
                }
                else
                {
                    if (_parent.AxisType == AxisType.Primary)
                        this.SetValue(TopProperty, (Double) _parent.MajorTicks.Height);
                    else
                    {
                        if(_parent._titleTextBlock.Text.Length != 0)
                            this.SetValue(TopProperty, (Double) _parent._titleTextBlock.ActualHeight);
                        else
                            this.SetValue(TopProperty, (Double) 0);
                    }
                }
            }
        }

        #endregion Public Methods

        #region Public Properties

        #region Font Properties

        public String FontFamily
        {
            get
            {
                return _fontFamily;
            }

            set
            {
                _fontFamily = value;
            }
        }

        public Double FontSize
        {
            get
            {
                if (!Double.IsNaN(_fontSize))
                    return _fontSize;
                else
                    return CalculateFontSize();
            }
            set
            {
                _fontSize = value;
            }
        }

        public Brush FontColor
        {
            get
            {
                return _fontColor;
            }

            set
            {
                _fontColor = value;
            }
        }

        public String FontStyle
        {
            get
            {
                return _fontStyle.ToString();
            }

            set
            {
                _fontStyle = Converter.StringToFontStyle(value);
            }

        }

        public String FontWeight
        {
            get
            {
                return _fontWeight.ToString();
            }

            set
            {
                _fontWeight = Converter.StringToFontWeight(value);
            }

        }

        #endregion
        
        public Double TextWrap
        {
            get;
            set;
        }

        public Int32 Rows
        {
            get;
            set;
        }

        public Double LabelAngle
        {
            get
            {
                if (!Double.IsNaN(_labelAngle))
                    return _labelAngle;
                else
                    return _parent.LabelAngle;
            }
            set
            {
                _labelAngle = value;
            }
        }

        public Double Interval
        {
            get
            {
                if (Double.IsNaN(_interval))
                    return _parent.Interval;
                else
                    return _interval;
            }
            set
            {
                _interval = value;
            }
        }
        
        #endregion Public Properties

        #region Internal Properties

        internal Double MaxLabelWidth
        {
            get;
            set;
        }

        internal System.Collections.Generic.Dictionary<Double, AxisLabel> LabelDictionary
        {
            get
            {
                return _labelDictionary;
            }
        }

        #endregion Internal properties

        #region Internal Methods

        private void CreateLabel(Double position, Double calculatedFontSize)
        {
            AxisLabel lbl = new AxisLabel();
            PlotDetails plotDetails = (_parent.Parent as Chart).PlotDetails;

            // Here the axis label text is genereted, The condition below forces the AxisY to 
            // be initialized with only formated numeric value
            String text;
            if (!plotDetails.AxisLabels.ContainsKey(position) || (_parent.GetType().Name == "AxisY"))
            {
                if (_parent.ValueFormatString != null)
                {
                    text = Parser.GetFormattedText(_parent.GetFormattedText(position));
                }
                else
                {
                    text = Parser.GetFormattedText(_parent.Prefix + position.ToString() + _parent.Suffix);
                }
            }
            else
            {
                // Here the axis labels given by the user is selected as text. only for Axis X
                text = Parser.GetFormattedText(plotDetails.AxisLabels[position]);
            }

            lbl.Text = text;

            this.Children.Add(lbl);

            lbl.Init();

            lbl.Angle = LabelAngle;

            lbl.Position = position;

            if (Double.IsNaN(_fontSize))
            {
                lbl.TextBlock.FontSize = calculatedFontSize;
            }

            lbl.UpdateSize();
        }

        internal void CreateLabels()
        {
            if (!Enabled || !_parent.Enabled)
                return;


            Double calculatedFontSize = CalculateFontSize();

            PlotDetails plotDetails = (_parent.Parent as Chart).PlotDetails;

            Double noOfIntervals = _parent.AxisManager.GetNoOfIntervals();

            Decimal interval = (Decimal)Interval;

            this.LabelDictionary.Clear();

            Decimal i = (Decimal)_parent.AxisMinimum;

            if (_parent.GetType().Name == "AxisX")
            {
                if ((Double)(_parent.AxisManager.GetMinimumDataValue() - interval) < _parent.AxisMinimum && _parent.GetType().Name == "AxisX")
                    i = _parent.AxisManager.GetMinimumDataValue();
                else
                    i = (Decimal)_parent.AxisMinimum;

                // To get the axis lable of the datapoint with smallest x value
                if (plotDetails.AllAxisLabels && plotDetails.AxisLabels.Count > 0)
                {
                    System.Collections.Generic.Dictionary<Double, String>.Enumerator enumerator = plotDetails.AxisLabels.GetEnumerator();
                    enumerator.MoveNext();
                    Int32 j = 0;
                    for (i = (Decimal)enumerator.Current.Key; j < plotDetails.AxisLabels.Count - 1; j++)
                    {
                        enumerator.MoveNext();
                        if (i > (Decimal)enumerator.Current.Key)
                            i = (Decimal)enumerator.Current.Key;
                    }
                    enumerator.Dispose();
                }
            }

            Int32 countIntervals = 0;
            Decimal minValue = i;


            if (_parent.GetType().Name == "AxisX")
            {
                for (; i <= (Decimal)(_parent.AxisMaximum ); i = minValue + (++countIntervals) * interval)
                {
                    if (_parent.GetType().Name == "AxisX")
                        if ((plotDetails.AllAxisLabels && plotDetails.AxisLabels.Count > 0) && i > (Decimal)plotDetails.MaxAxisXDataValue)
                            continue;

                    CreateLabel((Double)i, calculatedFontSize);
                }
            }
            else
            {
                CreateLabel((Double)minValue, calculatedFontSize);

                i = minValue + (++countIntervals) * interval;

                for (; i <= (Decimal)(_parent.AxisMaximum - (Double)(interval / 2)); i = minValue + (++countIntervals) * interval)
                {
                    CreateLabel((Double)i, calculatedFontSize);
                }

                CreateLabel((Double)_parent.AxisMaximum, calculatedFontSize);
            }
            
        }

        private void PositionLabelsVertically()
        {
            Point position;

            foreach (AxisLabel lbl in this.Children)
            {
                
                if (_parent.AxisType == AxisType.Primary)
                {
                    position = new Point(this.Width - _parent.MajorTicks.TickLength / 2, (Double)this._parent.DoubleToPixel(lbl.Position));

                    lbl.UpdatePositionLeft(position);
                }
                else
                {
                    position = new Point(_parent.MajorTicks.TickLength , (Double)this._parent.DoubleToPixel(lbl.Position)+ lbl.TextBlock.ActualHeight/2);

                    lbl.UpdatePositionRight(position);
                }
            }
            
        }

        private void PositionLabelsHorizontally()
        {
            Double textSize;
            Double rowHeight = 0;
            Int32 numberOfRows = 1;

            Double totalLabelWidth = 0;
            Double axisWidth = (_parent.DoubleToPixel(_parent.AxisMaximum) - _parent.DoubleToPixel(_parent.AxisMinimum));
            Double labelHeight = 0, tempHeight;

            Double prevLabelRight = 0, curLabelLeft;
            Boolean overlapOccured = false;

            Double gapBetweenText = 0;
            TextBlock textBlock = new TextBlock() { Text = "AB" };

            Double textMinimum = 8;

            if (Double.IsNaN(_fontSize))
            {
                textSize = CalculateFontSize();
            }
            else
            {
                textSize = _fontSize;
                textMinimum = _fontSize;
            }

            for (; textSize >= textMinimum; textSize -= 2)
            {
                totalLabelWidth = 0;
                labelHeight = 0;
                overlapOccured = false;
                textBlock.FontSize = textSize;
                gapBetweenText = textBlock.ActualWidth;

                foreach (AxisLabel lbl in this.Children)
                {
                    lbl.TextBlock.FontSize = textSize;
                    lbl.Angle = LabelAngle;
                    lbl.UpdateSize();

                    curLabelLeft = (Double)this._parent.DoubleToPixel(lbl.Position) - lbl.ActualWidth / 2;

                    // if there is a overlap then add the overlap region to the label list

                    if (((prevLabelRight - curLabelLeft) > 0) && totalLabelWidth > 0)
                        overlapOccured = true;

                    totalLabelWidth += (lbl.ActualWidth + gapBetweenText);
                    tempHeight = lbl.ActualHeight;

                    labelHeight = Math.Max(labelHeight, tempHeight);
                    prevLabelRight = (Double)this._parent.DoubleToPixel(lbl.Position) + lbl.ActualWidth / 2;
                }

                // This is to number of rows required to display the labels
                if (overlapOccured == false)
                {
                    numberOfRows = (Int32)Math.Ceiling(totalLabelWidth / axisWidth);
                    if (numberOfRows == 1) break;
                }
            }

            //if overlap occured then fore change of angle
            if (overlapOccured)
            {
                numberOfRows = (Int32)Math.Ceiling(totalLabelWidth / axisWidth);
                Double[] prevLR = new Double[numberOfRows];
                Double[] totalLW = new Double[numberOfRows];
                Double curLL;
                Int32 k, j = 0;
                prevLR.Initialize();
                totalLW.Initialize();

                overlapOccured = false;
                foreach (AxisLabel lbl in this.Children)
                {
                    k = j++ % numberOfRows;
                    curLL = (Double)this._parent.DoubleToPixel(lbl.Position) - lbl.ActualWidth / 2;

                    // if there is a overlap then add the overlap region to the label list

                    if (((prevLR[k] - curLL) > 0) && totalLW[k] > 0)
                        overlapOccured = true;

                    totalLW[k] += (lbl.ActualWidth + gapBetweenText);
                    prevLR[k] = (Double)this._parent.DoubleToPixel(lbl.Position) + lbl.ActualWidth / 2;
                }
            }

            // If Number of rows is given by the user then overide the previous setting
            if (Rows > 0)
            {
                numberOfRows = Rows;
            }
            else if ((numberOfRows > 2 || overlapOccured) && Double.IsNaN(_labelAngle) && Double.IsNaN(_parent._labelAngle))
            {
                labelHeight = 0;
                totalLabelWidth = 0;
                numberOfRows = 1;
                foreach (AxisLabel lbl in this.Children)
                {
                    lbl.Angle = -45;
                    lbl.UpdateSize();

                    totalLabelWidth += lbl.ActualWidth;
                    tempHeight = lbl.ActualHeight;
                    labelHeight = Math.Max(labelHeight, tempHeight);
                }
            }
            // this is the height of each row
            rowHeight = labelHeight;

            //These variables will be used to update the height of the row
            _rowHeight = rowHeight;
            _numberOfCalculatedRows = numberOfRows;

            SetHeight();

            // label index decides which label goes into which row
            Int32 index = 0;
            Point position;
            foreach (AxisLabel lbl in this.Children)
            {
                if (_parent.AxisType == AxisType.Primary)
                {
                    if (lbl.Angle > 0 && lbl.Angle <= 90)
                        position = new Point((Double)this._parent.DoubleToPixel(lbl.Position), rowHeight * (index % numberOfRows) + lbl.TextBlock.ActualHeight / 2 * Math.Abs(Math.Cos(lbl.Angle * Math.PI / 180)));
                    else
                        position = new Point((Double)this._parent.DoubleToPixel(lbl.Position), rowHeight * (index % numberOfRows));
                    index++;

                    lbl.UpdatePositionBottom(position);
                }
                else
                {
                    position = new Point((Double)this._parent.DoubleToPixel(lbl.Position),this.Height -( rowHeight * (index % numberOfRows)) - _parent.MajorTicks.TickLength/2);
                    index++;

                    lbl.UpdatePositionTop(position);
                }
            }
            

        }

        internal void PositionLabels()
        {
            if (!Enabled || !_parent.Enabled)
                return;

            if (_parent.AxisOrientation == AxisOrientation.Column)
                PositionLabelsHorizontally();
            else
                PositionLabelsVertically();

            CheckOutOfBounds();
        }

        #endregion Internal Method

        #region Private Methods

        /// <summary>
        /// Validate Parent element and assign it to _parent.
        /// Parent element should be a Chart element. Else throw an exception.
        /// </summary>
        private void ValidateParent()
        {
            if (this.Parent is Axes)
                _parent = this.Parent as Axes;
            else
                throw new Exception(this + "Parent should be an Axis");
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();
            MaxLabelWidth = 0;
            _labelDictionary = new System.Collections.Generic.Dictionary<Double, AxisLabel>();
            _fontSize = Double.NaN;
            _interval = Double.NaN;
            Enabled = true;
            Rows = 0;
            
            _fontColor = null;
           
            this.FontFamily = "Verdana";
            _labelAngle = Double.NaN;
            TextWrap = 0.3;
            
        }

        private Int32 CalculateFontSize()
        {
            Int32[] fontSizes = { 6, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 };
            Double _parentSize = (_parent.Parent as Chart).Width * (_parent.Parent as Chart).Height;
            Int32 i = (Int32)(Math.Ceiling(((_parentSize + 10000) / 115000)));
            i = (i >= fontSizes.Length ? fontSizes.Length - 1 : i);
            return fontSizes[i];
        }

        private void CheckOutOfBounds()
        {
            foreach (AxisLabel lbl in this.Children)
            {
                if (_parent.AxisOrientation == AxisOrientation.Bar)
                {
                    if (lbl.ActualTop < 0)
                    {
                        if (Math.Abs(lbl.ActualTop) > (_parent.Parent as Chart).LabelPaddingTop)
                            (_parent.Parent as Chart).LabelPaddingTop = Math.Abs(lbl.ActualTop);
                    }
                    else if (((lbl.ActualTop + lbl.ActualHeight) > (Double)this.GetValue(HeightProperty)))
                    {
                        Double overflow = ((lbl.ActualTop + lbl.ActualHeight) - (Double)this.GetValue(HeightProperty));

                        if (overflow > (Double)((_parent.Parent as Chart).LabelPaddingBottom))
                            ((_parent.Parent as Chart).LabelPaddingBottom) = overflow;
                    }
                }
                else if (_parent.AxisOrientation == AxisOrientation.Column)
                {
                    if ((lbl.ActualLeft + lbl.ActualWidth) > (Double)this.GetValue(WidthProperty))
                    {
                        Double overflow = ((lbl.ActualLeft + lbl.ActualWidth) - (Double)this.GetValue(WidthProperty));

                        overflow = overflow / lbl.Position * (_parent.AxisMaximum - _parent.AxisMinimum);

                        if ((_parent.Parent as Chart).LabelPaddingRight < overflow)
                            (_parent.Parent as Chart).LabelPaddingRight = overflow;

                    }
                    else if (lbl.ActualLeft < 0)
                    {
                        Double overflow = Math.Abs(lbl.ActualLeft);

                        overflow = overflow / (_parent.AxisMaximum - lbl.Position) * (_parent.AxisMaximum - _parent.AxisMinimum);

                        if (overflow > (_parent.Parent as Chart).LabelPaddingLeft)
                            (_parent.Parent as Chart).LabelPaddingLeft = overflow;

                    }
                }

            }
        }

        #endregion Private Methods

        #region Data
        
        private Double _labelAngle;
        internal Double _interval;
        private System.Collections.Generic.Dictionary<Double, AxisLabel> _labelDictionary;

        private String _fontFamily;
        private Double _fontSize;
        internal Brush _fontColor;
        private FontStyle _fontStyle;
        private FontWeight _fontWeight;
        
        private Axes _parent;
        private Double _rowHeight;
        private Int32 _numberOfCalculatedRows;


        #endregion Data
    }
}