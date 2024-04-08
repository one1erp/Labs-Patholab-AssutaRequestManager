using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ADODB;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Microsoft.Win32;
using MSXML;
using Patholab_DAL_V1;

//using Oracle.ManagedDataAccess.Client;
//using Oracle.ManagedDataAccess.Types;
using Oracle.ManagedDataAccess.Client;
using Patholab_Common;
using System.Windows;
//using Patholab_Common;

namespace AssutaRequestManager
{

    [ComVisible(true)]
    [ProgId("AssutaRequestManager.AssutaRequestManagerCls")]

    public partial class AssutaRequestManagerCls : UserControl, IExtensionWindow
    {



        #region Ctor

        public AssutaRequestManagerCls()
        {
            try
            {

                InitializeComponent();
                BackColor = Color.FromName("Control");

            }
            catch (Exception e)
            {
                //Logger.WriteLogFile(e);

            }
        }

        #endregion

        #region Private members


        private INautilusUser _ntlsUser;
        private static IExtensionWindowSite2 _ntlsSite;

        private INautilusProcessXML _processXml;

        private INautilusServiceProvider _sp;

        private OracleConnection _connection;

        private OracleCommand cmd;

        private DataLayer dal;


        private double sessionId;

        private string _connectionString;
        INautilusDBConnection _ntlsCon;


        #endregion

        #region Implementing IExtensionWindow


        // private INautilusRecordSet rs;
        public void PreDisplay()
        {

            _processXml = Utils.GetXmlProcessor(_sp);

            _ntlsUser = Utils.GetNautilusUser(_sp);


            //Initilize the MainWindow which is in WPF.
            MainWindow w = new MainWindow();//_sp, _processXml, _ntlsCon, _ntlsSite, _ntlsUser);
            elementHost1.Child = w;
            w.InitializeData(_ntlsCon, _sp, false, null);

        }

        public void SetParameters(string parameters)
        {
            try
            {

            }

            catch (Exception e)
            {

                System.Windows.MessageBox.Show("לא הוגדרו פרמטרים כראוי,לא ניתן להשתמש בתוכנית");
            }
        }

        public bool CloseQuery()
        {
            try
            {
                if (cmd != null)
                    cmd.Dispose();
                if (_connection != null)
                    _connection.Close();

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR" + ex.Message);
                return true;
            }
        }

        public void Internationalise()
        {
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("AssutaRequestManager");
            _ntlsSite.SetWindowRegistryName("AssutaRequestManager");
            _ntlsSite.SetWindowTitle("AssutaRequestManager");
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public bool SaveData()
        {
            return false;
        }

        public void SaveSettings(int hKey)
        {
        }

        public void Setup()
        {
        }

        public void refresh()
        {

        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void SetServiceProvider(object serviceProvider)
        {
            _sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(_sp);

        }

        public void RestoreSettings(int hKey)
        {

        }

        #endregion

        #region Form Events

        public int imageIndex = 0;
        private string _whereClause = "";

        private string applicationCode;




        #endregion

        #region Private methods



        private void InitControls()
        {


        }



        #endregion


        public void RunByEntityExtention(string name, INautilusServiceProvider sp)
        {
            _sp = sp;
            _processXml = Utils.GetXmlProcessor(sp);
            _ntlsCon = Utils.GetNtlsCon(sp);
            _ntlsUser = Utils.GetNautilusUser(sp);
            

            //Initilize the MainWindow which is in WPF.
            MainWindow w = new MainWindow();//_sp, _processXml, _ntlsCon, _ntlsSite, _ntlsUser);
            elementHost1.Child = w;
            w.InitializeData(_ntlsCon,_sp, true, name);
        }

        public void Close()
        {
        }
        public static event Action<bool> Closedddd;
        public static void CloseWindow(bool isChanged)
        {
            if (_ntlsSite != null)
            {
                if (isChanged)
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show("ישנם נתונים שלא נשמרו, האם ברצונך לצאת?", "יציאה מהחלון", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        _ntlsSite.CloseWindow();
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                else
                {
                    _ntlsSite.CloseWindow();
                }
            }

            else
            {
                if(Closedddd!=null)
                    Closedddd(isChanged);
            }

        }

    }

}


