using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReflectedLightAnalysis
{
    public partial class ModelessForm : UserControl
    {
        public ModelessForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Form1 modalForm = new Form1();
            //Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(modalForm);
        }
    }
}
