﻿/*
      Copyright (C) 2008 Webyog Softworks Private Limited

     This file is part of VisifireCharts.
 
     VisifireCharts is a free software: you can redistribute it and/or modify
     it under the terms of the GNU General Public License as published by
     the Free Software Foundation, either version 3 of the License, or
     (at your option) any later version.
 
     VisifireCharts is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warranty of
     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.
 
     You should have received a copy of the GNU General Public License
     along with VisifireCharts.  If not, see <http://www.gnu.org/licenses/>.
 
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
    public class ToolTip : LabelBase
    {
        #region Public Properties
        
        #endregion Public Properties

        #region Public Methods

        public ToolTip()
        {
            //SetDefaults();
        }

        public override void Init()
        {
            base.Init();
            // To apply background from theme
            Background = Background;
            if (Background == null)
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 229, 229, 240));
        }

        public override void SetLeft()
        {
            throw new NotImplementedException();
        }

        public override void SetTop()
        {
            throw new NotImplementedException();
        }
        #endregion Public Methods
        
        #region Private Methods
        private int CalculateFontSize()
        {
            int[] fontSizes = { 6, 8, 10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40 };
            Double _parentSize = (Parent as Chart).Width * (Parent as Chart).Height;
            int i = (int)(Math.Floor(((_parentSize + 10000) / 115000)) % fontSizes.Length);
            return fontSizes[i];
            
        }
        
        protected override void SetDefaults()
        {
            base.SetDefaults();
            this.SetValue(ZIndexProperty, 9999);
        }

        #endregion Private Methods

        #region Internal Methods
        internal void FixToolTipSize()
        {
            if (TextWrap < 0 || TextWrap > 1)
            {
                System.Diagnostics.Debug.WriteLine("Text wrap value is invalid, must be between (0 - 1)");
                throw (new Exception("Invalid TextWrap value"));
            }

            _textBlock.Width = (Parent as Chart).Width * TextWrap;
            _textBlock.TextWrapping = TextWrapping.Wrap;

            if (Double.IsNaN(_fontSize))
            {
                _textBlock.FontSize = CalculateFontSize();
            }

        }
        #endregion Internal Methods


    }
    
}
