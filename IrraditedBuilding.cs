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
    class IrraditedBuilding : Building
    {
        Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

        /// <summary>
        /// 受照建筑高度
        /// </summary>
        private double height;

        public double Height
        {
            get { return height; }
            set { height = value; }
        }

        private double h0c;

        public double AngleOfBuildingGap
        {
            get { return h0c; }
            set { h0c = value; }
        }

        private double h01;

        public double AngleOfIrraditedBuilding
        {
            get { return h01; }
            set { h01 = value; }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="dirt"></param>
        /// <returns></returns>
        public double midP2LineInDirection(Line line, Vector3d dirt)
        {
            Geometry geo = new Geometry();
            Line myLine = this.Entity;
            
            // Get mid point of itself
            Point3d midP = geo.GetMidPoint(myLine);
            
            //construct assistant line
            Point3d assisP = midP + dirt;
            Line guides = new Line(midP, assisP);

            // Get Intersection point
            Point3dCollection insectPoints = new Point3dCollection();
            guides.IntersectWith(line, Intersect.ExtendBoth, insectPoints, 0, 0);

            CADOperator ope = new CADOperator();
            ope.addEntity(new Line(midP, insectPoints[0]));

            return midP.DistanceTo(insectPoints[0]);
        }

        public bool isCoverdAll(double angleOfSun)
        {
            if (angleOfSun < h01 && angleOfSun > h0c)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int irraditedStatus;

        public int IrraditedStatus
        {
            get { return irraditedStatus; }
            set { irraditedStatus = value; }
        }

        public int GetIrraditedStatus(Point3dCollection points, double angleOfSun)
        {
            Geometry geo = new Geometry();
            if (geo.isLineInPolygons(points, Entity))
            {
                ed.WriteMessage("\n angleofSun : " + angleOfSun + "  h01 :" + h01 + "  h0c :" + h0c);
                if (isCoverdAll(angleOfSun))
                {
                    irraditedStatus = 2;
                }
                else
                {
                    irraditedStatus = 1;
                }
            }
            else if (geo.isLineInOrIntersectPolygons(points, Entity))
            {
                irraditedStatus = 1;
                
            }
            else
            {
                irraditedStatus = 0;
            }
            return irraditedStatus;
        }
        
    }
}
