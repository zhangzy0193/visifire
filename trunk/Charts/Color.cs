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
using System.Windows.Documents;
using System.Windows.Media;
using Visifire.Commons;

namespace Visifire.Charts
{
    public class Color: System.Windows.Controls.Canvas
    {
        #region Public Method

        public Color()
        {

        }

        public Color(Brush brush)
        {
            this.Background = brush;
        }

        #endregion Public Method
                
        #region Public Properties
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
        #endregion Public Propertirs

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
