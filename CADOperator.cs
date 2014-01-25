using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    class CADOperator
    {
        /// <summary>
        /// 选择单个实体
        /// </summary>
        /// <param name="message">选择提示</param>
        /// <returns>实体对象</returns>
        public Entity Single(String message)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Entity entity = null;
            PromptEntityResult ent = ed.GetEntity(message);
            if (ent.Status == PromptStatus.OK)
            {
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    entity = (Entity)transaction.GetObject(ent.ObjectId, OpenMode.ForRead, true);
                    transaction.Commit();
                }
            }
            return entity;
        }

        /// <summary>
        /// 指定基点与目标点复制实体
        /// </summary>
        /// <param name="ent">实体对象</param>
        /// <param name="sourcePt">基点</param>
        /// <param name="targetPt">目标点</param>
        /// <returns>复制的实体对象</returns>
        public Entity copyTo(Entity ent, Point3d sourcePt, Point3d targetPt)
        {
            Matrix3d mt = Matrix3d.Displacement(targetPt - sourcePt);
            Entity entCopy = ent.GetTransformedCopy(mt);
            return entCopy;
        }

        public void drawLine(Line line)
        {
            using (DocumentLock doclock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    btr.AppendEntity(line);
                    trans.AddNewlyCreatedDBObject(line, true);
                    trans.Commit();
                }
            }
        }

        public void addEntity(Entity entity)
        {
            using (DocumentLock doclock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    btr.AppendEntity(entity);
                    trans.AddNewlyCreatedDBObject(entity, true);
                    trans.Commit();
                }
            }
        }

        public DBText NewDBText(string textString, Point3d position, double height, double rot, bool isfield)
        {
            DBText txt = new DBText();
            txt.Position = position;
            txt.Height = height;
            txt.Rotation = rot;
            if (isfield)
            {
                Field field = new Field(textString);
                txt.SetField(field);
            }
            else
            {
                txt.TextString = textString;
            }
            return txt;
        }

        public Point3d GetPoint(string message)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptPointResult pt = ed.GetPoint(message);
            if (pt.Status == PromptStatus.OK)
            {
                return (Point3d)pt.Value;
            }
            else
            {
                return new Point3d();
            }
        }

        public Extents3d GetCorner(string baseword, string targetword)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Point3d basept = GetPoint(baseword);
            Point3d targetpt = new Point3d();
            PromptCornerOptions options = new PromptCornerOptions(targetword, basept);
            PromptPointResult i = ed.GetCorner(options);
            if (i.Status == PromptStatus.OK)
            {
                targetpt = i.Value;
            }
            Autodesk.AutoCAD.DatabaseServices.Extents3d ext = new Autodesk.AutoCAD.DatabaseServices.Extents3d();
            ext.AddPoint(targetpt);
            ext.AddPoint(basept);

            return ext;
        }

        /// <summary> 
        /// 画出一个反射光线区域
        /// </summary>
        public void drawReflectedLine(Point3d p1, Point3d p2, Point3d p3, Point3d p4, int colorIndex, LineWeight lw)
        {
            using (DocumentLock doclock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    //画4条直线的平行四边形
                    List<Line> lins = new List<Line>();
                    lins.Add(new Line(p1, p2));
                    lins.Add(new Line(p2, p3));
                    lins.Add(new Line(p3, p4));
                    lins.Add(new Line(p4, p1));
                    
                    foreach (Line line in lins)
                    {
                        line.ColorIndex = colorIndex; // set color of the line
                        line.LineWeight = lw;         // set thickness of the line  
                        
                        btr.AppendEntity(line);
                        trans.AddNewlyCreatedDBObject(line, true);
                    }

                    trans.Commit();
                }
                db.LineWeightDisplay = true;
            }
        }

        public void dataGridViewExportToExcel(DataGridView dateGridView)
        {

            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "Execl files (*.xls)|*.xls";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.CreatePrompt = true;
            saveFileDialog.Title = "保存为Excel文件";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName.IndexOf(":") < 0) return; //被点了"取消"

            Stream myStream;
            myStream = saveFileDialog.OpenFile();
            StreamWriter sw = new StreamWriter(myStream, System.Text.Encoding.GetEncoding(-0));
            string columnTitle = "";
            try
            {
                //写入列标题
                for (int i = 0; i < dateGridView.ColumnCount; i++)
                {
                    if (i > 0)
                    {
                        columnTitle += "\t";
                    }
                    columnTitle += dateGridView.Columns[i].HeaderText;
                }
                sw.WriteLine(columnTitle);

                //写入列内容

                for (int j = 0; j < dateGridView.Rows.Count - 1; j++)
                {
                    string columnValue = "";
                    for (int k = 0; k < dateGridView.Columns.Count; k++)
                    {
                        if (k > 0)
                        {
                            columnValue += "\t";
                        }

                        if (dateGridView.Rows[j].Cells[k].Value == null)
                            columnValue += "";
                        else
                            columnValue += dateGridView.Rows[j].Cells[k].Value.ToString().Trim();
                    }
                    sw.WriteLine(columnValue);
                }
                sw.Close();
                myStream.Close();
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sw.Close();
                myStream.Close();
            }

        }
    }
}
