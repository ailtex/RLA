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
    class Building
    {
        /// <summary>
        /// 建筑纬度
        /// </summary>
        private double latitude;

        public double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        /// <summary>
        /// 建筑经度
        /// </summary>
        private double longitude;

        public double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        /// <summary>
        /// 建筑线段实例
        /// </summary>
        private Line entity;

        public Line Entity
        {
            get { return entity; }
            set { entity = value; }
        }
    }
}
