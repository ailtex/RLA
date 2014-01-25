using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows.ToolPalette;
using Autodesk.AutoCAD.Windows;

namespace ReflectedLightAnalysis
{
    class Windows : Building
    {
        /// <summary>
        /// 幕墙方位角
        /// </summary>
        private double direction;

        public double Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        /// <summary>
        /// 幕墙低端高度
        /// </summary>
        private double bottom;

        public double Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        /// <summary>
        /// 幕墙顶端高度
        /// </summary>
        private double top;

        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        /// <summary>
        /// 幕墙反射率
        /// </summary>
        private double reflectivity;

        public double Reflectivity
        {
            get { return reflectivity; }
            set { reflectivity = value; }
        }

        
        
        
    }
}
