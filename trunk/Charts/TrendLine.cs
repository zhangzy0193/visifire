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
using Visifire.Commons;

namespace Visifire.Charts
{
    public class TrendLine : Canvas
    {
        #region Public Methods

        public TrendLine()
        {
            _line = new Line();
            _shadow = new Line();
            Children.Add(_line);
            Children.Add(_shadow);

            SetDefaults();
        }

        public String TextParser(String unParsed)
        {
            String str = new String(unParsed.ToCharArray());
            if (str.Contains("##XValue"))
                str = str.Replace("##XValue", "#XValue");
            else
            {
                if (!Double.IsNaN(XValue))
                    str = str.Replace("#XValue", _parent.AxisX.GetFormattedText(XValue));
            }
            if (str.Contains("##YValue"))
                str = str.Replace("##YValue", "#YValue");
            else
            {
                if (!Double.IsNaN(YValue))
                    str = str.Replace("#YValue", _parent.AxisYPrimary.GetFormattedText(YValue));
            }
            return str;
        }

        #endregion Public Methods

        #region Public Properties

        public AxisType AxisYType
        {
            get;
            set;
        }

        public String ToolTipText
        {
            get;
            set;
        }

        public Double XValue
        {
            get
            {
                return _xValue;
            }
            set
            {
                _xValue = value;
            }
        }

        public Double YValue
        {
            get
            {
                return _yValue;
            }
            set
            {
                _yValue = value;
            }
        }

        public Boolean Enabled
        {
            get;
            set;
        }

        public Boolean ShadowEnabled
        {
            get;
            set;
        }

        #region Line Properties

        public String LineColor
        {
            set
            {
                _lineColor = Parser.ParseColor(value);
            }
        }

        public Double LineThickness
        {
            get
            {
                return _lineThickness;
            }
            set
            {
                _lineThickness = value;
            }
        }

        public String LineStyle
        {
            get
            {
                return _lineStyle;
            }

            set
            {
                _lineStyle = value;
            }
        }

        #endregion Line Properties

        #endregion Public Properties

        #region Internal Properties
        internal Brush Color
        {
            get
            {
                if (_lineColor == null)
                    return new SolidColorBrush(Colors.Orange);
                return _lineColor;
            }

            set
            {
                _lineColor = value;
            }
        }
        #endregion Internal Properties

        #region Internal Methods
        internal void InitAndDraw()
        {
            Init();

            SetDimensions();

            AttachToolTip();
        }

        internal void AttachToolTip()
        {
            String str;

            if (String.IsNullOrEmpty(ToolTipText)) return;


            str = TextParser(ToolTipText);

            this.MouseEnter += delegate(object sender, MouseEventArgs e)
            {
                _parent.ToolTip.Text = str;

                _parent.ToolTip.Visibility = Visibility.Visible;

                _parent.ToolTip.SetTop(e.GetPosition(_parent.ToolTip.Parent as UIElement).Y - (Double)_parent.ToolTip.GetValue(HeightProperty) * 1.5);
                _parent.ToolTip.SetLeft(e.GetPosition(_parent.ToolTip.Parent as UIElement).X - (Double)_parent.ToolTip.GetValue(WidthProperty) / 2);
            };

            this.MouseLeave += delegate(object sender, MouseEventArgs e)
            {
                _parent.ToolTip.Visibility = Visibility.Collapsed;

            };
        }

        internal void Init()
        {
            ValidateParent();

            SetName();

            _line.Tag = this.Name;
            _shadow.Tag = this.Name;
        }

        internal void SetDimensions()
        {
            _line.StrokeThickness = LineThickness;
            _line.Stroke = (Color);
            _shadow.StrokeThickness = LineThickness;
            _shadow.Stroke = Parser.ParseSolidColor("#7f7f7f");
            _line.StrokeDashArray = Parser.GetStrokeDashArray(LineStyle);

            Axes axis;
            Orientation orientation = Orientation.Horizontal;
            Double value = 0;
            if (!Double.IsNaN(_yValue))
            {
                if (AxisYType == AxisType.Primary)
                {
                    axis = _parent.AxisYPrimary;
                }
                else
                {
                    axis = _parent.AxisYSecondary;
                }
                value = _yValue;
                if(_parent.PlotDetails.AxisOrientation == AxisOrientation.Column)
                    orientation = Orientation.Horizontal;
                else
                    orientation = Orientation.Vertical;
            }
            else if (!Double.IsNaN(_xValue))
            {
                axis = _parent.AxisX;
                value = _xValue;
                if (_parent.PlotDetails.AxisOrientation == AxisOrientation.Column)
                    orientation = Orientation.Vertical;
                else
                    orientation = Orientation.Horizontal;
            }
            else
            {
                return;
            }

            if (value <= axis.AxisMinimum || value >= axis.AxisMaximum)
            {
                this.Width = 0;
                this.Height = 0;
            }
            else if (orientation == Orientation.Horizontal)
            {
                this.Width = _parent.PlotArea.Width;
                this.Height = LineThickness + 3;

                this.SetValue(TopProperty, (Double)(axis.DoubleToPixel(value) + (Double)_parent.PlotArea.GetValue(TopProperty) - Height / 2));

                this.SetValue(LeftProperty, (Double)_parent.PlotArea.GetValue(LeftProperty));

                _line.X1 = 0;
                _line.Y1 = Height / 2;
                _line.X2 = Width;
                _line.Y2 = Height / 2;

                _shadow.X1 = _line.X1;
                _shadow.X2 = _line.X2;
                _shadow.Y1 = _line.Y1 + LineThickness / 2;
                _shadow.Y2 = _line.Y2 + LineThickness / 2;
            }
            else if (orientation == Orientation.Vertical)
            {
                this.Height = _parent.PlotArea.Height;
                this.Width = LineThickness + 3;

                this.SetValue(LeftProperty, (Double)(axis.DoubleToPixel(value) + (Double)_parent.PlotArea.GetValue(LeftProperty) - Width / 2));

                this.SetValue(TopProperty, (Double)_parent.PlotArea.GetValue(TopProperty));

                _line.Y1 = 0;
                _line.X1 = Width / 2;
                _line.Y2 = Height;
                _line.X2 = Width / 2;

                _shadow.X1 = _line.X1 + LineThickness / 2;
                _shadow.X2 = _line.X2 + LineThickness / 2;
                _shadow.Y1 = _line.Y1;
                _shadow.Y2 = _line.Y2;
            }
            else
            {
                this.Width = 0;
                this.Height = 0;
            }

            if (!ShadowEnabled)
                _shadow.Opacity = 0;
        }

        internal void SetLeft()
        {
        }
        internal void SetTop()
        {
        }
        #endregion Internal Methods

        #region Private Methods

        /// <summary>
        /// Set a default Name. This is usefull if user has not specified this object in data XML and it has been 
        /// created by default.
        /// </summary>
        private void SetName()
        {
            Generic.SetNameAndTag(this);
        }

        /// <summary>
        /// Validate Parent element and assign it to _parent.
        /// Parent element should be a Canvas element. Else throw an exception.
        /// </summary>
        private void ValidateParent()
        {
            if (this.Parent is Chart)
                _parent = this.Parent as Chart;
            else
                throw new Exception(this + "Parent should be an Chart");
        }

        private void SetDefaults()
        {
            Color = new SolidColorBrush(Colors.Red);
            LineThickness = 2;
            LineStyle = "Solid";

            _xValue = Double.NaN;
            _yValue = Double.NaN;
            Enabled = true;

            AxisYType = AxisType.Primary;

            this.SetValue(ZIndexProperty, 3);
            _line.SetValue(ZIndexProperty, 10);
            _shadow.SetValue(ZIndexProperty, 1);
            this.Opacity = .5;
        }

        #endregion Private Methods

        #region Data

        Double _xValue;
        Double _yValue;

        Chart _parent;
        Line _line;
        Line _shadow;

        Double _lineThickness;
        String _lineStyle;
        Brush _lineColor;



        #endregion Data
    }
}