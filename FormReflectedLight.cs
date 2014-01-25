using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
using System.IO;

namespace ReflectedLightAnalysis
{
    public partial class FormReflectedLight : Form
    {
        public FormReflectedLight()
        {
            InitializeComponent();

            /********************************************************************
            *                               单天
             ********************************************************************/
            ///
            /// comboBoxCity
            ///
            comboBoxCity.SelectedIndex = 0; // 默认选择上海
            textBoxLongitude.Text = Convert.ToString(121.4667); 
            textBoxLatitud.Text = Convert.ToString(31.2333);
            ///
            /// comboBoxWindowsDrection
            ///
            comboBoxWindowsDrection.SelectedIndex = 0;
            ///
            /// comboBoxTime
            ///
            comboBoxTime.SelectedIndex = 0;  // 默认选择春分
            DateTime dt = new DateTime(DateTime.Now.Year, 3, 20);
            dateTimePickerSingleDay.Value = dt;
            ///
            ///textBoxTimeInterval
            ///
            textBoxTimeInterval.Text = "60";
            ///
            ///dateTimePickerStart
            ///
            dateTimePickerStart.Format = DateTimePickerFormat.Custom;
            dateTimePickerStart.CustomFormat = "HH:mm:ss";
            dateTimePickerStart.Value = new DateTime(
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            ///
            ///dateTimePickerEnd
            ///
            dateTimePickerEnd.Format = DateTimePickerFormat.Custom;
            dateTimePickerEnd.CustomFormat = "HH:mm:ss";
            dateTimePickerEnd.Value = new DateTime(
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
            ///
            /// textBoxTimeInterval
            /// 
            textBoxTimeInterval.Text = "60";
            ///
            /// comboBoxWindowsDrection2
            ///
            comboBoxWindowsDirection2.SelectedIndex = 0;

            /********************************************************************
            *                               多天
             ********************************************************************/
            ///
            ///dateTimePickerAddStart
            ///
            dateTimePickerAddStart.Format = DateTimePickerFormat.Custom;
            dateTimePickerAddStart.CustomFormat = "HH:mm:ss";
            dateTimePickerAddStart.Value = new DateTime(
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            ///
            ///dateTimePickerAddEnd
            ///
            dateTimePickerAddEnd.Format = DateTimePickerFormat.Custom;
            dateTimePickerAddEnd.CustomFormat = "HH:mm:ss";
            dateTimePickerAddEnd.Value = new DateTime(
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);

            /********************************************************************
            *                               参数编辑
             ********************************************************************/
            ///
            ///
            ///
            
        }

      
        ///////////////////////////////////////////////////////////////////////////////////

        Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

        Entity entity; // 玻璃幕墙边
        Entity influencedEntity; // 受影响的玻璃幕墙边

        Geometry geometry = new Geometry();
        Constants constantValue = new Constants();
        CADOperator cadOperator = new CADOperator();
        FormServices formServices = new FormServices();

        LineWeight lw = LineWeight.ByLineWeightDefault;


        /**********************************************************************
         *                               单天
         **********************************************************************/

        /// <summary>
        /// 选择玻璃幕墙边
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            CADOperator cadOperator = new CADOperator();
            entity = cadOperator.Single("Select one line:\n");
        }

        /// <summary>
        /// 画反射光线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButtonLine.Checked)
            {
                Line line = entity as Line;
                if (line != null)
                {
                    drawReflectLight_line(line);
                }
            }
            else if (radioButtonCurve.Checked)
            {
                Curve curve = entity as Curve;
                if (curve != null)
                {
                    drawReflectLight_curve(curve);
                }
            }

        }
        
        private void drawReflectLight_line(Line line)
        {
            double angle = geometry.Angle2X(line.StartPoint, line.EndPoint);
            double windowsDirectionAngle = geometry.getWindowsDirectionAngle(line, comboBoxWindowsDrection);

            int month = dateTimePickerSingleDay.Value.Month - 1;
            int day = dateTimePickerSingleDay.Value.Day - 1;


            double startTime = formServices.dataTimePicker2Hour(dateTimePickerStart);
            /*
                dateTimePickerStart.Value.Hour +
                dateTimePickerStart.Value.Minute / 60.0 +
                dateTimePickerStart.Value.Second / 3600.0;
             */
            double endTime = formServices.dataTimePicker2Hour(dateTimePickerEnd);
            /*
                dateTimePickerEnd.Value.Hour +
                dateTimePickerEnd.Value.Minute / 60.0 +
                dateTimePickerEnd.Value.Second / 3600.0;
            */
            double interval = Convert.ToDouble(textBoxTimeInterval.Text) / 60.0;
            double latitudeAngle, longitudeAngle;

            latitudeAngle = getLatitudeAngle();
            longitudeAngle = getLongitudeAngle();


            drawLines(line, 
                windowsDirectionAngle, 
                GetWindowsDownHeight(), 
                GetWindowsUpHeight(),
                constantValue.angleSun[month, day], 
                latitudeAngle, 
                longitudeAngle, 
                startTime, 
                endTime, 
                interval, 
                lw);
        }

        private void drawReflectLight_curve(Curve curve)
        {
            ed.WriteMessage("\n This is a curve !");

            Point3d startPt = curve.StartPoint;
            ed.WriteMessage("\n start point : " + startPt.ToString());
            Point3d nextPt = curve.GetClosestPointTo(startPt, false);
            ed.WriteMessage("  next point : " + nextPt.ToString());
            Vector3d vector = curve.GetFirstDerivative(startPt);
            Point3d extendP = startPt + vector * 1;
            ed.WriteMessage("\n " + extendP.ToString());
            Point3d closestP = curve.GetClosestPointTo(extendP, true);

            Line startP2extendP = new Line(startPt, extendP);
            startP2extendP.ColorIndex = 2;
            Line startP2closestP = new Line(startPt, closestP);
            startP2closestP.ColorIndex = 3;

           CADOperator cadOperator = new CADOperator();
           cadOperator.drawLine(startP2extendP);
           cadOperator.drawLine(startP2closestP);
        }


        private void addLegend(string title, Point3d position, double tableWidth, double tableHeight)
        {
            CADOperator cadOperator = new CADOperator();
            //添加文字
            DBText dbtext = cadOperator.NewDBText(title, position, tableHeight / 7, 0, false);
            cadOperator.addEntity(dbtext);
            
            //添加表格
            Table table = new Table();
            table.SetSize(8, 4);
            table.Position = new Point3d(position.X, position.Y - 20, position.Z);
            table.Width = tableWidth;
            table.Height = tableHeight;
            table.Cells.TextHeight = tableHeight / 10;
           

            table.Cells[0, 0].TextString = "颜色";
            table.Cells[0, 1].TextString = "时间";
            table.Cells[0, 2].TextString = "颜色";
            table.Cells[0, 3].TextString = "时间";

            string[] cellString = constantValue.clocks;

            for (int i = 1; i < 8; i++)
            {
                table.Cells[i, 0].BackgroundColor = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByAci, (short)constantValue.colorArray[i - 1 + 6]);
                table.Cells[i, 1].TextString = cellString[i - 1];
            }

            for (int i = 1; i < 7; i++)
            {
                table.Cells[i, 2].BackgroundColor = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                    Autodesk.AutoCAD.Colors.ColorMethod.ByAci, (short)constantValue.colorArray[i + 6 + 6]);
                table.Cells[i, 3].TextString = cellString[i + 6];
            }

            cadOperator.addEntity(table);
        }
        

        private void drawLines(
            Line line, 
            double windowsDirectionAngle, 
            double windowsDownHeight, 
            double windowsUpHeight,
            double angleSun, 
            double latitudeAngle, 
            double longitudeAngle, 
            double startTime, 
            double endTime, 
            double interval, 
            LineWeight lw)
        {
            Point3d startPoint, endPoint;
            startPoint = line.StartPoint;
            endPoint = line.EndPoint;
            
            for (double time = startTime; time <= endTime; time += interval)
            {
                double h0 = geometry.getSolorHighPosition(angleSun,
                    latitudeAngle, longitudeAngle, time);

                double alpha = geometry.getAlphaPosition(angleSun,latitudeAngle,
                    longitudeAngle, h0, time);
           
                if (h0 <= 0 ||
                    (alpha + geometry.angle2Rad(windowsDirectionAngle) >= Math.PI / 2 && windowsDirectionAngle != 180) ||
                    (alpha + geometry.angle2Rad(windowsDirectionAngle) <= -1 * Math.PI / 2 && windowsDirectionAngle != 180) ||
                        (windowsDirectionAngle == 180 && 
                        (alpha + geometry.angle2Rad(windowsDirectionAngle) <= 1.50 * Math.PI) && 
                        (alpha + geometry.angle2Rad(windowsDirectionAngle) >= Math.PI / 2.0)
                    ))
                {
                    continue;
                }

                Point3d p1 = geometry.getReflectedLinePoint(
                    startPoint, 
                    windowsDownHeight,
                    geometry.rad2Angle(h0), 
                    geometry.rad2Angle(alpha), 
                    windowsDirectionAngle);

                Point3d p2 = geometry.getReflectedLinePoint(
                    startPoint, 
                    windowsUpHeight,
                    geometry.rad2Angle(h0), 
                    geometry.rad2Angle(alpha), 
                    windowsDirectionAngle);

                Point3d p3 = geometry.getReflectedLinePoint(
                    endPoint, 
                    windowsUpHeight,
                    geometry.rad2Angle(h0), 
                    geometry.rad2Angle(alpha),
                    windowsDirectionAngle);
                Point3d p4 = geometry.getReflectedLinePoint(
                    endPoint,
                    windowsDownHeight,
                    geometry.rad2Angle(h0), 
                    geometry.rad2Angle(alpha),
                    windowsDirectionAngle);

                cadOperator.drawReflectedLine(p1, p2, p3, p4, constantValue.colorArray[(int)time], lw);

            }
        }

        private double GetWindowsDownHeight()
        {
            if (textBoxWindowsDown.Text == "")
            {
                return 0.0;
            }
            else
            {
                return Convert.ToDouble(textBoxWindowsDown.Text);
            }
        }

        private double GetWindowsUpHeight()
        {
            if (textBoxWindowsUP.Text == "")
            {
                MessageBox.Show("请输入玻璃幕墙顶端高度");
                return 0.0;         // Error
            }
            else
            {
                return Convert.ToDouble(textBoxWindowsUP.Text);
            }
        }

        private double getLongitudeAngle()
        {
            if (textBoxLongitude.Text == "")
            {
                MessageBox.Show("请输入地区经度");
            }
           
            return Convert.ToDouble(textBoxLongitude.Text);
        }

        private double getLatitudeAngle()
        {
            if (textBoxLatitud.Text == "")
            {
                MessageBox.Show("请输入地区纬度");
            }
            
            return Convert.ToDouble(textBoxLatitud.Text);
        }


        private void comboBoxTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime dt;
            string selectItemString = comboBoxTime.SelectedItem.ToString();
            switch (selectItemString)
            {
                case "春分":
                    dt = new DateTime(DateTime.Now.Year, 3, 20);
                    dateTimePickerSingleDay.Value = dt;
                    break;
                case "夏至":
                    dt = new DateTime(DateTime.Now.Year, 6, 21);
                    dateTimePickerSingleDay.Value = dt;
                    break;
                case "秋分":
                    dt = new DateTime(DateTime.Now.Year, 9, 22);
                    dateTimePickerSingleDay.Value = dt;
                    break;
                case "冬至":
                    dt = new DateTime(DateTime.Now.Year, 12, 21);
                    dateTimePickerSingleDay.Value = dt;
                    break;
            }
            dateTimePickerSingleDay.Update();
            this.Refresh();
        }

        private void comboBoxCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectItemString = comboBoxCity.SelectedItem.ToString();
            switch (selectItemString)
            {
                case "上海": 
                    textBoxLongitude.Text = Convert.ToString(121.4667);
                    textBoxLatitud.Text = Convert.ToString(31.2333);
                    break;
                case "北京": 
                    textBoxLongitude.Text = Convert.ToString(116.28);
                    textBoxLatitud.Text = Convert.ToString(39.54);
                    break;
                case "广州": 
                    textBoxLongitude.Text = Convert.ToString(113.18);
                    textBoxLatitud.Text = Convert.ToString(23.10);
                    break;
                case "成都": 
                    textBoxLongitude.Text = Convert.ToString(104.04);
                    textBoxLatitud.Text = Convert.ToString(30.39);
                    break;
                case "南京": 
                    textBoxLongitude.Text = Convert.ToString(118.46);
                    textBoxLatitud.Text = Convert.ToString(32.03);
                    break;
                case "其他":
                    textBoxLongitude.Text = "";
                    textBoxLatitud.Text = "";
                    break;
            }
            this.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void dateTimePickerSingleDay_ValueChanged(object sender, EventArgs e)
        {
            DateTime dt = dateTimePickerSingleDay.Value;
            int month = dt.Month;
            int day = dt.Day;
            if (month == 3 && day == 20)
            {
                comboBoxTime.SelectedIndex = 0;
            }
            else if (month == 6 && day == 21)
            {
                comboBoxTime.SelectedIndex = 1;
            }
            else if (month == 9 && day == 22)
            {
                comboBoxTime.SelectedIndex = 2;
            }
            else if (month == 12 && day == 21)
            {
                comboBoxTime.SelectedIndex = 3;
            }
            else
            {
                comboBoxTime.SelectedIndex = 4;
            }

            this.Refresh();
        }

        /**********************************************************************
         *                               多天
         **********************************************************************/

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            RedoSerialNo();
        }

        private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            RedoSerialNo();
        }

        /// <summary>
        /// 自动添加编号
        /// </summary>
        private void RedoSerialNo()
        {
            for (int i = 0; i < dataGridView1.RowCount; ++i)
            {
                dataGridView1[0, i].Value = i + 1;
            }
        }

        
        /// <summary>
        /// 多天 - 添加日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //////////////////////  Input Test   /////////////////////
            if (entity == null)
            {
                MessageBox.Show("选择幕墙边");
                return;
            }
            if (textBoxSurfaceReflectivity.Text == "")
            {
                MessageBox.Show("输入材料表面发射率");
                return;
            }


            //////////////////////////////////////////////////////////

            Geometry geometry = new Geometry();
            string[] row = new string[dataGridView1.ColumnCount];
            row[0] = "";
            row[1] = dateTimePickerAddDate.Value.ToString("M");
            row[2] = dateTimePickerAddStart.Value.ToString("HH:mm") + "-" +
                dateTimePickerAddEnd.Value.ToString("HH:mm");

            /////////////////////////////////////////////////////////
            AnalysisTimePeriod anaTimePeriod = new AnalysisTimePeriod();
            anaTimePeriod.Date = dateTimePickerAddDate.Value;
            

            anaTimePeriod.StartTime = dateTimePickerAddStart.Value;
            anaTimePeriod.EndTime = dateTimePickerAddEnd.Value;
            anaTimePeriod.IntervalInHour = 5.0 / 3600;
            //ed.WriteMessage("Analysis Time Period - Month :" + anaTimePeriod.Month);

            int month = dateTimePickerAddDate.Value.Month - 1;
            int day = dateTimePickerAddDate.Value.Day - 1;
           
            double startTime = formServices.dataTimePicker2Hour(dateTimePickerAddStart);
            double endTime = formServices.dataTimePicker2Hour(dateTimePickerAddEnd);
            double interval = 5.0 / 3600; // time interval related to prcision

            /////////////////////////////////////////////////////////
            Windows win = new Windows();
            win.Reflectivity = Convert.ToDouble(textBoxSurfaceReflectivity.Text);
            win.Entity = (Line)entity;
            win.Direction = geometry.angle2Rad(geometry.getWindowsDirectionAngle((Line)entity, comboBoxWindowsDirection2));
            win.Bottom = GetWindowsDownHeight();
            win.Top = GetWindowsUpHeight();
            win.Latitude = geometry.angle2Rad(getLatitudeAngle());
            win.Longitude = geometry.angle2Rad(getLongitudeAngle());

            double surfaceReflectivity = Convert.ToDouble(textBoxSurfaceReflectivity.Text);
            double windowsDirectionAngle = geometry.getWindowsDirectionAngle((Line)entity, comboBoxWindowsDirection2);

            row[3] = addWindowsDirectionName();

            /////////////
            Sun sun = new Sun();
            sun.Angle = geometry.angle2Rad(
                constantValue.angleSun[anaTimePeriod.Date.Month - 1, anaTimePeriod.Date.Day - 1]
                );


            Line influencedLine = (Line)influencedEntity;
            Line windowsLine = (Line)entity;

            //row[4] = geometry.AddInfluencedTime2(anaTimePeriod, win, sun, influencedLine);

            IrraditedBuilding irraditedBuiding = new IrraditedBuilding();
            if (influencedEntity != null)
            {

                irraditedBuiding.Entity = (Line)influencedEntity;
                irraditedBuiding.Height = Convert.ToDouble(textBoxInfluencedBuildingHeight.Text);

                row[4] = formServices.AddInfluencedTime(anaTimePeriod, win, sun, irraditedBuiding);
            }
            /*
            row[4] = geometry.AddInfluencedTime(
                month,
                day,
                startTime,
                endTime,
                interval,
                geometry.angle2Rad(constantValue.angleSun[month, day]),
                geometry.angle2Rad(getLatitudeAngle()),
                geometry.angle2Rad(getLongitudeAngle()),
                geometry.angle2Rad(windowsDirectionAngle),
                getWindowsDownHeight(),
                getWindowsUpHeight(),
                windowsLine,
                influencedLine);
            */


            

            row[5] = geometry.getSunReflectedLightAngleInterval(
                month,
                day,
                startTime,
                endTime,
                interval,
                constantValue.angleSun[month, day], 
                getLatitudeAngle(), 
                getLongitudeAngle(), 
                windowsDirectionAngle);

            row[6] = geometry.getReflectedLightAngleInterval(
                month,
                day,
                startTime,
                endTime,
                interval,
                constantValue.angleSun[month, day], 
                getLatitudeAngle(), 
                getLongitudeAngle());

            row[7] = geometry.getLuminanceInterval(
                month, 
                day, 
                startTime, 
                endTime, 
                interval, 
                surfaceReflectivity,
                constantValue.angleSun[month, day], 
                getLatitudeAngle(), 
                getLongitudeAngle());

            dataGridView1.Rows.Add(row);
        }

        private string addWindowsDirectionName()
        {
            string face = comboBoxWindowsDirection2.Text.ToString();
            face = face.Substring(1);
            face += "立面";
            return face;
        }

        
        

        /// <summary>
        /// 多天 - 选择玻璃幕墙
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            CADOperator cadOperator = new CADOperator();
            entity = cadOperator.Single("Select one line\n");
        }

        /// <summary>
        /// 选择受影响建筑边
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            CADOperator cadOperator = new CADOperator();
            influencedEntity = cadOperator.Single("Select one line\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            CADOperator cadOperator = new CADOperator();
            cadOperator.dataGridViewExportToExcel(dataGridView1);
        }

        /// <summary>
        /// Exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
            /*
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(1000, 0, 0);
            Point3d p3 = new Point3d(1000, 1000, 0);
            Point3d p4 = new Point3d(0, 800, 0);

            Point3dCollection points = new Point3dCollection();
            points.Add(p1); points.Add(p2); points.Add(p3); points.Add(p4);

            Point3d p5 = new Point3d(500, 0, 0);
            Point3d p6 = new Point3d(500, 300, 0);
            Point3d p7 = new Point3d(1500, 0, 0);
            Point3d p8 = new Point3d(500, 1500, 0);
            
            Line line1 = new Line(p5, p6);
            Line line2 = new Line(p7, p8);
            Line line3 = new Line(p5, p7);
            Line line4 = new Line(p1,p2);
            if (geometry.isLineInOrIntersectPolygons(points, line1))
            {
                ed.WriteMessage("line1 is in Polygon");
            }
            if (geometry.isLineInOrIntersectPolygons(points, line2))
            {
                ed.WriteMessage("line2 is in Polygon");
            }
            if (geometry.isLineInOrIntersectPolygons(points, line3))
            {
                ed.WriteMessage("line3 is in Polygon");
            }

            

            /*
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(1000, 0, 0);
            Point3d p3 = new Point3d(1500, 0, 0);
            Point3d p4 = new Point3d(500, 100, 0);
            Point3d p5 = new Point3d(500, 0, 0);
            Line line = new Line(p1,p2);
            if (geometry.isPointOnLine(p3, line))
            {
                ed.WriteMessage("p3 is on line\n");
            }

            if (geometry.isPointOnLine(p4, line))
            {
                ed.WriteMessage("p4 is on line\n");
            }

            if (geometry.isPointOnLine(p5, line))
            {
                ed.WriteMessage("p5 is on line\n");
            }
            /*
            Line l1 = (Line)entity;
            Line l2 = (Line)influencedEntity;

            Point3dCollection points = geometry.segmentsIntersect(l1,l2);
            if (points.Count > 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    ed.WriteMessage(points[i].ToString() + "\n ");
                }
            }
            else
            {
                ed.WriteMessage("no interseted \n");
            }
            */
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            this.Refresh();
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// 画出连续天数的反射光
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            Geometry geometry = new Geometry();
            if (entity is Line)
            {
                double windowsDirectionAngle = geometry.getWindowsDirectionAngle(((Line)entity), comboBoxWindowsDirection2);
                int month, day;
                double startTime,endTime,interval;
                double latitudeAngle, longitudeAngle;

                latitudeAngle = getLatitudeAngle();
                longitudeAngle = getLongitudeAngle();

                interval = Convert.ToDouble(textBoxTimeInterval.Text) / 60.0;

                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    DateTime date = new DateTime();
                    
                    date = DateTime.ParseExact(dataGridView1.Rows[i].Cells[1].Value.ToString(), "M", System.Globalization.CultureInfo.CurrentCulture);

                    month = date.Month - 1;
                    day = date.Day - 1;

                    string[] time = dataGridView1.Rows[i].Cells[2].Value.ToString().Split('-');
                    date = DateTime.ParseExact(time[0], "HH:ss", System.Globalization.CultureInfo.CurrentCulture);

                    startTime = date.Hour + date.Minute / 60.0 + date.Second / 3600;

                    date = DateTime.ParseExact(time[1], "HH:ss", System.Globalization.CultureInfo.CurrentCulture);
 
                    endTime = date.Hour + date.Minute / 60.0 + date.Second / 3600;

                    drawLines((Line)entity, 
                        windowsDirectionAngle, 
                        GetWindowsDownHeight(), 
                        GetWindowsUpHeight(),
                        constantValue.angleSun[month, day], 
                        latitudeAngle, 
                        longitudeAngle, 
                        startTime, 
                        endTime, 
                        interval, 
                        lw);
                    
                }
            }
            
        }

        /**********************************************************************
         *                               参数编辑
         **********************************************************************/

        Extents3d ext;

        private void button9_Click(object sender, EventArgs e)
        {
            if (textBoxNewAngleSun.Text == "")
            {
                MessageBox.Show("请输入修改值");
                return;
            }
            double newAngleSun = Convert.ToDouble(textBoxNewAngleSun.Text);

            int month = dateTimePickerNewAngleSun.Value.Month;
            int day = dateTimePickerNewAngleSun.Value.Day;
            constantValue.angleSun[month, day] = newAngleSun;

        }

        private void button10_Click(object sender, EventArgs e)
        {
            textBoxNewAngleSun.Text = "";
        }

        /// <summary>
        /// 选择图例位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            CADOperator cadOperator = new CADOperator();
            ext = cadOperator.GetCorner("baseword", "targetword");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Point3d minpt = ext.MinPoint;
            Point3d maxpt = ext.MaxPoint;

            double width = Math.Abs(minpt.X - maxpt.X);
            double height = Math.Abs(minpt.Y - maxpt.Y);
            
            Point3d posiont = new Point3d(minpt.X, maxpt.Y, 0);
            addLegend(textBoxLegendTitle.Text, posiont, width, height);
        }

        /// <summary>
        /// 反射光线粗细设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLineWeight_Click(object sender, EventArgs e)
        {
            LineWeightDialog lwd = new LineWeightDialog();
            lwd.ShowDialog();
            lw = lwd.LineWeight;
            ed.WriteMessage(lwd.LineWeight.ToString());

            this.Refresh();

        }

    }
}
