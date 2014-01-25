// (C) Copyright 2013 by  Ailtex Zhang
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows.ToolPalette;
using Autodesk.AutoCAD.Windows;

// This line is not mandatory, but improves loading performances
[assembly: ExtensionApplication(typeof(ReflectedLightAnalysis.MyPlugin))]

namespace ReflectedLightAnalysis
{

    // This class is instantiated by AutoCAD once and kept alive for the 
    // duration of the session. If you don't do any one time initialization 
    // then you should remove this class.
    public class MyPlugin : IExtensionApplication
    {

        ContextMenuExtension m_ContextMenu;//定义右键菜单 
        PaletteSet palSet;//定义工具栏按钮

        void IExtensionApplication.Initialize()
        {
            // Add one time initialization here
            // One common scenario is to setup a callback function here that 
            // unmanaged code can call. 
            // To do this:
            // 1. Export a function from unmanaged code that takes a function
            //    pointer and stores the passed in value in a global variable.
            // 2. Call this exported function in this function passing delegate.
            // 3. When unmanaged code needs the services of this managed module
            //    you simply call acrxLoadApp() and by the time acrxLoadApp 
            //    returns  global function pointer is initialized to point to
            //    the C# delegate.
            // For more info see: 
            // http://msdn2.microsoft.com/en-US/library/5zwkzwf4(VS.80).aspx
            // http://msdn2.microsoft.com/en-us/library/44ey4b32(VS.80).aspx
            // http://msdn2.microsoft.com/en-US/library/7esfatk4.aspx
            // as well as some of the existing AutoCAD managed apps.

            // Initialize your plug-in application here

            AddContextMenu();//添加右键菜单
            //AddPalette();//添加面板工具栏
        }

        void IExtensionApplication.Terminate()
        {
            RemoveContextMenu();
        }

        #region 添加一个右键菜单，选择玻璃幕墙边，输入对应的幕墙高度，添加对应的反射光线
        /// <summary>
        /// 点击响应事件，添加反射光线
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void MyMenuItem_OnClick(object o, EventArgs e)
        {
            FormReflectedLight reflectedLightForm = new FormReflectedLight();
            Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(reflectedLightForm);

        }

        /// <summary>
        /// 添加右键菜单项
        /// </summary>
        private void AddContextMenu()
        {
            m_ContextMenu = new ContextMenuExtension();
            m_ContextMenu.Title = "玻璃幕墙分析";

            Autodesk.AutoCAD.Windows.MenuItem mi;
            mi = new Autodesk.AutoCAD.Windows.MenuItem("玻璃幕墙分析");
            //关联菜单项的处理函数
            mi.Click += MyMenuItem_OnClick;
            m_ContextMenu.MenuItems.Add(mi);

            Application.AddDefaultContextMenuExtension(m_ContextMenu);
        }
        /// <summary>移除菜单项
        /// 
        /// </summary>
        private void RemoveContextMenu()
        {
            if (m_ContextMenu != null)
            {
                Application.RemoveDefaultContextMenuExtension(m_ContextMenu);
                m_ContextMenu = null;
            }
        }
        #endregion





        [CommandMethod("AddPalette")]
        public void AddPalette()
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (palSet == null)
                {
                    palSet = new Autodesk.AutoCAD.Windows.PaletteSet("我的面板集");

                    palSet.Style = PaletteSetStyles.ShowTabForSingle;
                    palSet.Style = PaletteSetStyles.NameEditable;
                    palSet.Style = PaletteSetStyles.ShowPropertiesMenu;
                    palSet.Style = PaletteSetStyles.ShowAutoHideButton;
                    palSet.Style = PaletteSetStyles.ShowCloseButton;
                    palSet.Opacity = 90;
                    palSet.MinimumSize = new System.Drawing.Size(300, 300);
                    System.Windows.Forms.UserControl myPageCtrl = new ModelessForm();//注意这里是加载自己写的用户控件
                    //myPageCtrl.Dock = System.Windows.Forms.DockStyle.Fill; 
                    palSet.Add("我的页面", myPageCtrl);
                    palSet.Visible = true;
                }
            }

            catch
            {
                ed.WriteMessage("创建面板集错误");
            }


        }
    }

}
