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
    public class Color:Canvas
    {
        #region Public Method

        public Color()
        {

        }

        public Color(Brush brush)
        {
            this.Background = brush;
        }

        public String Value
        {
            set
            {
                this.Background = Parser.ParseColor(value);
                _color = value;
            }
            get
            {
                return _color;
            }
        }

        #endregion Public Method

        #region Internal Method
        internal Brush ColorValue
        {
            get
            {
                return this.Background;
            }
            set
            {
                this.Background = value;
            }
        }
        #endregion Internal Method

        #region Data
        String _color;
        #endregion  Data
    }
}
