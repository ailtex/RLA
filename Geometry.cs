using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows.ToolPalette;
using Autodesk.AutoCAD.Windows;

namespace ReflectedLightAnalysis
{
    class Geometry
    {
        Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

        const double EPS = 1e-8;

        //FormServices formServices = new FormServices();
        //Constants constantValue = new Constants();
        //CADOperator cadOperator = new CADOperator();
        
        public double Angle2X(Point3d startPt, Point3d endPt)
        {
            Line L = new Line(startPt, endPt);
            return L.Angle;
        }

        public double angle2Rad(double angle)
        {
            return angle * Math.PI / 180;
        }

        public double rad2Angle(double rad)
        {
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Segments Intersect
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public Point3dCollection segmentsIntersect(Line line1, Line line2)
        {
            Point3dCollection points = new Point3dCollection();
            line1.IntersectWith(line2, Intersect.OnBothOperands, points, 0, 0);
            return points;
        }

        public bool isSegmentsIntersect(Line line1, Line line2)
        {
            Point3dCollection points = segmentsIntersect(line1, line2);
            if (points.Count > 0) return true;
            else return false;
        }

        public bool isPointOnLine(Point3d point, Line line)
        {
            if(line.GetClosestPointTo(point,false).DistanceTo(point) == 0.00 )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Cross Product
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private double cross(Point3d a, Point3d b, Point3d m, Point3d n)
        {
            return (b.X - a.X) * (n.Y - m.Y) - (n.X - m.X) * (b.Y - a.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int dcmp(double x)
        {
            if (x < -EPS) return -1;
            else if (x > EPS) return 1;
            else return 0;
        }

        public bool isPointInPolygon(Point3d cp, Point3dCollection points)
        {
            int count = points.Count;
            int wn = 0;
            for (int i = 0; i < count; i++)
            {
                if (isPointOnLine(cp, new Line(points[i], points[(i + 1) % count]))) return true;
                int k = dcmp(cross(points[i], points[(i + 1) % count], points[i], cp));
                int d1 = dcmp(points[i].Y - cp.Y);
                int d2 = dcmp(points[(i + 1) % count].Y - cp.Y);
                if (k > 0 && d1 <= 0 && d2 > 0) wn++;
                if (k < 0 && d2 <= 0 && d1 > 0) wn--;
            }
            return wn != 0;
        }
       

        //private int pointOnSegment(Point3d )

        /// <summary>
        /// 计算太阳高度角
        /// </summary>
        /// <param name="angleSunAngle">太阳倾角</param>
        /// <param name="latitudAngle">纬度</param>
        /// <param name="longitudeAngle">经度</param>
        /// <param name="time">此时时刻</param>
        /// <returns>太阳高度角</returns>
        public double getSolorHighPosition(double angleSunAngle, double latitudAngle, double longitudeAngle, double time)
        {
            double sinh0 = Math.Sin(angle2Rad(angleSunAngle)) *
                    Math.Sin(angle2Rad(latitudAngle))
                    + Math.Cos(angle2Rad(angleSunAngle)) *
                    Math.Cos(angle2Rad(latitudAngle)) *
                    Math.Cos(angle2Rad(15 * time + longitudeAngle - 300));

            return Math.Asin(sinh0);
        }

        public double GetSolorHighPosition_rad(double angleSun, double latitude, double longitude, double time)
        {
            double sinh0 = Math.Sin(angleSun) *
                    Math.Sin(latitude)
                    + Math.Cos(angleSun) *
                    Math.Cos(latitude) *
                    Math.Cos(angle2Rad(15 * time + rad2Angle(longitude) - 300));

            return Math.Asin(sinh0);
        }

        public double GetSolorElevation(Sun sun, Windows win, double time)
        {
            return Math.Asin(
                    Math.Sin(sun.Angle) *
                    Math.Sin(win.Latitude)
                    + Math.Cos(sun.Angle) *
                    Math.Cos(win.Latitude) *
                    Math.Cos(angle2Rad(15 * time + rad2Angle(win.Longitude) - 300))
                );
        }

        /// <summary>
        /// 计算太阳方位角
        /// </summary>
        /// <param name="angleSun">太阳倾角</param>
        /// <param name="latitudeAngle">纬度</param>
        /// <param name="longitudeAngle">经度</param>
        /// <param name="h0">太阳高度角</param>
        /// <param name="time">此时时刻</param>
        /// <returns>太阳方位角</returns>
        public double getAlphaPosition(double angleSunAngle, double latitudeAngle, double longitudeAngle, double h0, double time)
        {
            double sinAlpha = -1 * Math.Cos(angle2Rad(angleSunAngle)) *
                   Math.Sin(angle2Rad(15 * time + longitudeAngle - 300)) /
                   Math.Cos(h0);

            double cosAlpha = (Math.Sin(h0) * Math.Sin(angle2Rad(latitudeAngle)) - Math.Sin(angle2Rad(angleSunAngle))) 
                / Math.Cos(h0) / Math.Cos(angle2Rad(latitudeAngle));

 
            if (sinAlpha >= 0)
            {
                return Math.Acos(cosAlpha);
            }
            else
            {
                if (cosAlpha > 0)
                {
                    return Math.Asin(sinAlpha);
                }
                else
                {
                    return Math.Acos(cosAlpha) * (-1.0);
                }
            }
            
        }

        public double getAlphaPosition_rad(double angleSun, double latitude, double longitude, double h0, double time)
        {
            double sinAlpha = -1 * Math.Cos(angleSun) *
                   Math.Sin(angle2Rad(15 * time + rad2Angle(longitude) - 300)) /
                   Math.Cos(h0);

            double cosAlpha = (Math.Sin(h0) * Math.Sin(latitude) - Math.Sin(angleSun)) / Math.Cos(h0) / Math.Cos(latitude);


            if (sinAlpha >= 0)
            {
                return Math.Acos(cosAlpha);
            }
            else
            {
                if (cosAlpha > 0)
                {
                    return Math.Asin(sinAlpha);
                }
                else
                {
                    return Math.Acos(cosAlpha) * (-1.0);
                }
            }

        }

        private double getReflectedLight(double h0, double solor2WindonsAngle)
        {
            double cosSigma = Math.Cos(h0) * Math.Cos(angle2Rad(solor2WindonsAngle));
            return Math.Acos(cosSigma);
        }

        private double getReflectedLight_rad(double h0, double solor2Windons)
        {
            double cosSigma = Math.Cos(h0) * Math.Cos(solor2Windons);
            return Math.Acos(cosSigma);
        }

        /// <summary>
        /// 计算玻璃幕墙方位角
        /// </summary>
        /// <param name="line"></param>
        /// <param name="comboBoxWindowsDrection"></param>
        /// <returns></returns>
        public double getWindowsDirectionAngle(Line line, ComboBox comboBoxWindowsDrection)
        {
            double angle2X = rad2Angle(line.Angle);
            string selectItemString = comboBoxWindowsDrection.SelectedItem.ToString();
            if (selectItemString == "正南")
            {
                return 0.0;
            }
            else if (selectItemString == "正北")
            {
                return 180.0;
            }
            else if (selectItemString == "偏东")
            {
                if (angle2X >= 0 && angle2X <= 180)
                {
                    return -1 * angle2X;
                }
                else
                {
                    return 180.0 - angle2X;
                }
            }
            else if (selectItemString == "偏西")
            {
                if (angle2X >= 0 && angle2X <= 180)
                {
                    return 180 - angle2X;
                }
                else
                {
                    return 360 - angle2X;
                }
            }
            return 0.0; // 默认正南
        }


        public Point3d getReflectedLinePoint(
            Point3d p, 
            double height, 
            double solor_h0,
            double solorPostionAngle, 
            double windowsPositonAngle)
        {
            double H, L;
            double x, y, z; // 新点的坐标

            H = height / Math.Tan(angle2Rad(solor_h0)); // 阳光反射后的垂直距离

            double sigma = solorPostionAngle + windowsPositonAngle; // 反射光入射角

            L = H;

            double alpha = solorPostionAngle - sigma * 2;

            x = p.X + L * Math.Sin(angle2Rad(alpha));
            y = p.Y - L * Math.Cos(angle2Rad(alpha));
            z = p.Z;

            Point3d p1 = new Point3d(x, y, z);
            return p1;
        }

        public Point3d getReflectedLinePoint_rad(
            Point3d p,
            double height,
            double h0,
            double solorPostion,
            double windowsPositon)
        {
            double H, L;
            double x, y, z; // 新点的坐标

            H = height / Math.Tan(h0); // 阳光反射后的垂直距离

            double sigma = solorPostion + windowsPositon; // 反射光入射角

            L = H;

            double alpha = solorPostion - sigma * 2;

            x = p.X + L * Math.Sin(alpha);
            y = p.Y - L * Math.Cos(alpha);
            z = p.Z;

            Point3d p1 = new Point3d(x, y, z);
            return p1;
        }

        

        /// <summary>
        /// 得到反射光亮度范围值
        /// </summary>
        /// <returns></returns>
        public string getLuminanceInterval(
            int month, 
            int day, 
            double startTime, 
            double endTime,
            double interval, 
            double surfaceReflectivity, 
            double angleSun, 
            double latitudeAngle, 
            double longitudeAngle)
        {
            double maxLuminace = 0.0, miniLuminace = 100000000;
            Boolean first = true;

            for (double hour = startTime; hour <= endTime; hour += interval)
            {
                double h0 = getSolorHighPosition(angleSun,
                    latitudeAngle, longitudeAngle, hour);

                double E = 137000 * Math.Sin(h0) * Math.Exp(-0.223 / Math.Sin(h0));

                double luminance = surfaceReflectivity * E / Math.PI;

                if (first)
                {
                    maxLuminace = miniLuminace = luminance;
                    first = false;
                }
                else
                {
                    maxLuminace = maxLuminace < luminance ? luminance : maxLuminace;
                    miniLuminace = miniLuminace > luminance ? luminance : miniLuminace;
                }
            }

            if (maxLuminace < 2000)
            {
                return "<2000";
            }
            else
            {
                return String.Format("{0:0}", maxLuminace) +
                    "-" + String.Format("{0:0}", miniLuminace);
            }
        }

        /// <summary>
        /// The angle of incedence of sunlight within specified time interval
        /// </summary>
        /// <returns></returns>
        public string getSunReflectedLightAngleInterval(
            int month, 
            int day, 
            double startTime, 
            double endTime,
            double interval, 
            double angleSun, 
            double latitudeAngle, 
            double longitudeAngle, 
            double windowsDirectionAngle)
        {
            double maxAngle = 0.0, minAngle = 360.0;

            double timeFlag = startTime;
            for (double now = startTime; now <= endTime; now += interval)
            {
                double h0 = getSolorHighPosition(angleSun,latitudeAngle, longitudeAngle, now);

                double alpha = getAlphaPosition(angleSun,latitudeAngle, longitudeAngle, h0, now);
                double reflectLight = getReflectedLight(h0, rad2Angle(alpha) - windowsDirectionAngle);
                
                if (reflectLight > maxAngle)
                {
                    maxAngle = reflectLight;
                }
                if (reflectLight < minAngle)
                {
                    minAngle = reflectLight;
                    timeFlag = now;
                }
            }

            // angle may out of the 90 degree
            if (maxAngle >= Math.PI / 2.0) maxAngle = Math.PI / 2.0;
            if (minAngle >= Math.PI / 2.0) minAngle = Math.PI / 2.0;

            return String.Format("{0:0.00}", rad2Angle(maxAngle)) +
                "-" + String.Format("{0:0.00}", rad2Angle(minAngle));
        }
        /// <summary>
        /// The angle of incedence within specified time interval
        /// </summary>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="interval"></param>
        /// <param name="angleSun"></param>
        /// <param name="latitudeAngle"></param>
        /// <param name="longitudeAngle"></param>
        /// <returns></returns>
        public string getReflectedLightAngleInterval(
            int month, 
            int day, 
            double startTime, 
            double endTime,
            double interval, 
            double angleSun, 
            double latitudeAngle, 
            double longitudeAngle)
        {
            double maxAngle = 0.0, minAngle = 360.0;

            for (double now = startTime; now <= endTime; now += interval)
            {
                double h0 = getSolorHighPosition(angleSun,
                    latitudeAngle, longitudeAngle, now);

                double alpha = getAlphaPosition(angleSun,latitudeAngle,
                    longitudeAngle, h0, now);


                double reflectLight = getReflectedLight(h0, rad2Angle(alpha));

                if (reflectLight > maxAngle) maxAngle = reflectLight;
                if (reflectLight < minAngle) minAngle = reflectLight;
            }

            // angle may out of the 90 degree
            if (maxAngle >= Math.PI / 2.0) maxAngle = Math.PI / 2.0;
            if (minAngle >= Math.PI / 2.0) minAngle = Math.PI / 2.0;

            return String.Format("{0:0.00}", rad2Angle(maxAngle)) +
                "-" + String.Format("{0:0.00}", rad2Angle(minAngle));
        }

        public string AddInfluencedTime(
            int month,
            int day,
            double startTime,
            double endTime,
            double interval,
            double angleSun,
            double latitude,
            double longitude,
            double windowsDirection,
            double windowsDownHeight,
            double windowsUpHeight,
            Line line,
            Line influencedLine)
        {

            Point3d startPoint, endPoint;
            startPoint = line.StartPoint;
            endPoint = line.EndPoint;


            List<double> influenceTime = new List<double>();
            influenceTime.Clear();
            List<string> influenceWay = new List<string>();
            influenceWay.Clear();

            //Dictionary<string,string> periodInfTimeTable = new Dictionary<string,string>;
            

            bool influenceStart = false;
            //ed.WriteMessage("start time : " + startTime + "\n");
            //ed.WriteMessage("end time : " + endTime + "\n");

            //interval = interval * 60;

            for (double now = startTime; now <= endTime; now += interval)
            {
                double h0 = GetSolorHighPosition_rad(angleSun,
                    latitude, longitude, now);

                double alpha = getAlphaPosition_rad(angleSun, latitude,
                    longitude, h0, now);
                //ed.WriteMessage("Here1\n");
                //ed.WriteMessage("Time: " + now +"  h0 :" + rad2Angle(h0) + "  alpha : " + rad2Angle(alpha) + "\n");

                if (h0 <= 0 ||
                    (alpha + windowsDirection >= Math.PI / 2 && windowsDirection != Math.PI) ||
                    (alpha + windowsDirection <= -1 * Math.PI / 2 && windowsDirection != Math.PI) ||
                        (windowsDirection == Math.PI &&
                        (alpha + windowsDirection <= 1.50 * Math.PI) &&
                        (alpha + windowsDirection >= Math.PI / 2.0)
                    ))
                {
                    continue;
                }

                Point3d p1 = getReflectedLinePoint_rad(
                    startPoint,
                    windowsDownHeight,
                    h0,
                    alpha,
                    windowsDirection);

                Point3d p2 = getReflectedLinePoint_rad(
                    startPoint,
                    windowsUpHeight,
                    h0,
                    alpha,
                    windowsDirection);

                Point3d p3 = getReflectedLinePoint_rad(
                    endPoint,
                    windowsUpHeight,
                    h0,
                    alpha,
                    windowsDirection);

                Point3d p4 = getReflectedLinePoint_rad(
                    endPoint,
                    windowsDownHeight,
                    h0,
                    alpha,
                    windowsDirection);

                
                Point3dCollection points = new Point3dCollection();
                points.Add(p1); points.Add(p2);
                points.Add(p3); points.Add(p4);
                //cadOperator.drawLine(new Line(p1, p2));
                //cadOperator.drawLine(new Line(p2, p3));
                //cadOperator.drawLine(new Line(p3, p4));
                
                ///////////////////
                //PartialTimePeriod();
                if (isLineInOrIntersectPolygons(points, influencedLine))
                {
                    if (!influenceStart)
                    {
                        influenceTime.Add(now);
                        influenceStart = true;

                        influenceTime.Add(now);
                    }
                    else
                    {
                        influenceTime[influenceTime.Count - 1] = now;
                    }
                }
                else
                {
                    if (influenceStart)
                    { 
                        influenceTime[influenceTime.Count - 1] = now;
                        influenceStart = false;
                    }
                }
                
                points.Clear();
            }

            string influceceTimeString = "";
            bool firstFlag = true;

            FormServices formServices = new FormServices();

            ed.WriteMessage("influence time count: " + influenceTime.Count + "\n");
            for(int i=0; i < influenceTime.Count(); i+=2)
            {
                double start, end;
                start = influenceTime[i];
                end = influenceTime[i+1];
                
                if (firstFlag)
                {
                    firstFlag = false;
                }
                else
                {
                    influceceTimeString += ",";
                }

                influceceTimeString += formServices.convertTimeDouble2String(start);
                influceceTimeString += "-";
                influceceTimeString += formServices.convertTimeDouble2String(end);
            }
            
            return influceceTimeString;
        }

        public bool isLineInOrIntersectPolygons(Point3dCollection points, Line line)
        {
            if (isPointInPolygon(line.StartPoint, points)) return true;
            if (isPointInPolygon(line.EndPoint, points)) return true;
            int count = points.Count;
            for (int i = 0; i < count; i++)
            {
                Line line2 = new Line(points[i], points[(i + 1) % count]);
                if (isSegmentsIntersect(line, line2)) return true;
            }
            return false;
        }

        public bool isLineInPolygons(Point3dCollection points, Line line)
        {
            if (isPointInPolygon(line.StartPoint, points) &&
               isPointInPolygon(line.EndPoint, points))
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
        /// <param name="points"></param>
        /// <param name="influencedLine"></param>
        /// <returns>0 : outside; 1 : intersct ; 2 : in</returns>
        internal int CheckLineStatus(Point3dCollection points, IrraditedBuilding irraditedBuiding)
        {
            if (isLineInPolygons(points, irraditedBuiding.Entity))
            {   
                //if(irraditedBuiding.isCoverdAll())
                return 2;
            }
            else if (isLineInOrIntersectPolygons(points, irraditedBuiding.Entity))
            {
                return 1;
            }
            else return 0;
        }

        public Point3d GetMidPoint(Line line)
        {
            Point3d start = line.StartPoint;
            Point3d end = line.EndPoint;
            return new Point3d((start.X + end.X)/ 2.0,
                               (start.Y + end.Y)/ 2.0,
                               (start.Z + end.Z)/ 2.0
                );
        }

    }
}
