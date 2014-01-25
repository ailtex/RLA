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
    class FormServices
    {
        Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

        public double dataTimePicker2Hour(DateTimePicker dtp)
        {
            return dtp.Value.Hour + dtp.Value.Minute / 60.0 + dtp.Value.Second / 3600.0;
        }

        public double dataTime2Hour(DateTime dt)
        {
            return dt.Hour + dt.Minute / 60.0 + dt.Second / 3600.0;
        }

        public string convertTimeDouble2String(double time)
        {
            int hours = (int)Math.Floor(time);
            int minutes = (int)Math.Floor((time - hours) * 60.0 );
            return hours.ToString("D2") + ":" + minutes.ToString("D2");
        }

        

        public Point3dCollection GetReflectedLinePoints(Windows win, Sun sun)
        {
            
            Geometry geometry = new Geometry();
            Point3dCollection points = new Point3dCollection();
            points.Add(geometry.getReflectedLinePoint_rad(
                win.Entity.StartPoint,
                win.Bottom,
                sun.Elevation,
                sun.Azimuth,
                win.Direction
                ));

            points.Add(geometry.getReflectedLinePoint_rad(
                win.Entity.StartPoint,
                win.Top,
                sun.Elevation,
                sun.Azimuth,
                win.Direction
                ));

            points.Add(geometry.getReflectedLinePoint_rad(
                win.Entity.EndPoint,
                win.Top,
                sun.Elevation,
                sun.Azimuth,
                win.Direction
                ));

            points.Add(geometry.getReflectedLinePoint_rad(
                win.Entity.EndPoint,
                win.Top,
                sun.Elevation,
                sun.Azimuth,
                win.Direction
                ));

            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anaTimePeriod"></param>
        /// <param name="win"></param>
        /// <param name="sun"></param>
        /// <param name="influencedLine"></param>
        /// <returns></returns>
        internal string AddInfluencedTime(
            AnalysisTimePeriod anaTimePeriod, 
            Windows win, 
            Sun sun, 
            IrraditedBuilding irraditedBuiding)
        {
            Dictionary<string,int> partialInfTimeTable = new Dictionary<string,int>();

            double startTime, endTime, interval;
            startTime = dataTime2Hour(anaTimePeriod.StartTime);
            endTime = dataTime2Hour(anaTimePeriod.EndTime);
            interval = anaTimePeriod.IntervalInHour;

            double now;
            for (now = startTime; now <= endTime; now += interval)
            {
                double h0 = sun.CalcElevation(win.Latitude, win.Longitude, now);
                double alpha = sun.CalcAzimuth(win.Latitude, win.Longitude, now);

                if (h0 <= 0 ||
                    (alpha + win.Direction >= Math.PI / 2 && win.Direction != Math.PI) ||
                    (alpha + win.Direction <= -1 * Math.PI / 2 && win.Direction != Math.PI) ||
                        (win.Direction == Math.PI &&
                        (alpha + win.Direction <= 1.50 * Math.PI) &&
                        (alpha + win.Direction >= Math.PI / 2.0)
                    ))
                {
                    int cnt = partialInfTimeTable.Count;
                    if (cnt == 0 || partialInfTimeTable.ElementAt(cnt - 1).Value != 0)
                    {
                        partialInfTimeTable.Add(convertTimeDouble2String(now), 0);
                    }
                }
                else
                {
                    ed.WriteMessage("\n Time : " + convertTimeDouble2String(now) + " ");
                    Point3dCollection points = GetReflectedLinePoints(win, sun);
                    //irraditedBuiding.Sun = sun;
                    double dist = irraditedBuiding.midP2LineInDirection(win.Entity, points[0] - points[1]);
                    irraditedBuiding.AngleOfBuildingGap = Math.Atan((win.Top - irraditedBuiding.Height) / dist);
                    irraditedBuiding.AngleOfIrraditedBuilding = Math.Atan(irraditedBuiding.Height / dist);
                    irraditedBuiding.IrraditedStatus = irraditedBuiding.GetIrraditedStatus(points, sun.Elevation);

                    PartialTimePeriod(points, irraditedBuiding, now, partialInfTimeTable);
                }
            }
            return convertDict2String(partialInfTimeTable, endTime );
        }

        private string convertDict2String(Dictionary<string, int> partialInfTimeTable, double endTime)
        {
            string influceceTimeString = "";
            string[] valMap = {"不被照射；", "部分照射；", "全部照射；"};
            string preKey = "";
            bool first = true;

            foreach (var item in partialInfTimeTable)
            {
                ed.WriteMessage("\n Time Table : " + item.ToString());
                if (first)
                {
                    preKey = item.Key;
                    first = false;
                }
                else
                {
                    influceceTimeString += preKey + "-" + item.Key + ": " + valMap[ partialInfTimeTable[preKey] ] ;
                    preKey = item.Key;
                }
            }
            if (partialInfTimeTable.ContainsKey(preKey))
            {
                influceceTimeString += preKey + "-" +
                    convertTimeDouble2String(endTime) + ": " +
                    valMap[partialInfTimeTable[preKey]];
            }

            return influceceTimeString;
        }

        private void PartialTimePeriod(
            Point3dCollection points,
            IrraditedBuilding irraditedBuiding, 
            double now, 
            Dictionary<string, int> partialInfTimeTable)
        {
            Geometry geo = new Geometry();
            int status = irraditedBuiding.IrraditedStatus;
            int cnt = partialInfTimeTable.Count;
            string timeInString = convertTimeDouble2String(now);

            if (cnt == 0 || partialInfTimeTable.ElementAt(cnt - 1).Value != status)
            {
                partialInfTimeTable.Add(timeInString, status);
            }
            //ed.WriteMessage(" \n Time table Add precedure: " + partialInfTimeTable.ToString());
        }

        
    }
}
