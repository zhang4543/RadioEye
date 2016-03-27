﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using CSRegexStringLayer;
using CSProcessLayer;

namespace RadioEye
{
    public partial class Start : Form
    {
        StringBuilder szDevice;
        string sPath = System.IO.Path.GetTempPath() + @"nfc\";

        public Start(ref StringBuilder szDeviceTmp)
        {
            szDevice = szDeviceTmp;
            InitializeComponent();
        }

        private void Start_Load(object sender, EventArgs e)
        {
            timer.Start();
            lab_Tip.Text = "Checking Libnfc Components...";
            lab_Ret.Text = "";
        }

        int TimerCount = 0;
        bool bOk = true;

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!bOk)
            {
                timer.Dispose();
                Thread.Sleep(3000);
                System.Environment.Exit(0);
            }

            string szRet = "";

            TimerCount++;
            switch (TimerCount)
            {
                case 1:
                    timer.Stop();
                    bOk = Set_LabelRet(Chk_Component(ref szRet), szRet);
                    timer.Start();
                    break;
                case 2:
                    lab_Tip.Text = "Checking RFID Device...";
                     lab_Ret.Text = "";
                    break;
                case 3:
                    timer.Stop();
                    bOk = Set_LabelRet(Chk_Device(ref szRet), szRet);
                    timer.Start();
                    break;
                case 4:
                    lab_Tip.Text = "Welcome to the RadioEye...";
                    lab_Ret.Text = "";
                    break;
                case 5:
                    break;
                case 6:
                    timer.Dispose();
                    this.Close();
                    break;
            }
        }

        private bool Set_LabelRet(bool bStatus, string szRet)
        {
            if (bStatus)
            {
                lab_Ret.ForeColor = Color.Green;
                lab_Ret.Text = szRet;

            }
            else
            {
                lab_Ret.ForeColor = Color.Red;
                lab_Ret.Text = szRet;
            }
            return bStatus;
        }

        private bool Chk_Component(ref string szRet)
        {
           
            FileStream fs;
            byte[] buffer;
            


            if (!Directory.Exists(sPath))
            {
                Directory.CreateDirectory(sPath);
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory+"key"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory+"key");
            }
            string[] szFileList = new string[]
            {
                "libnfc.dll",
                "libusb0.dll",
                "nfc-list.exe",
                "nfc-mfsetuid.exe",
                "nfc-mfclassic.exe",
                "mfoc.exe",
            }; 

            foreach (string szFileItem in szFileList)
            {
                if (!File.Exists(sPath +szFileItem))
                {
                       fs = new FileStream(sPath  + szFileItem, FileMode.CreateNew, FileAccess.Write);
                    buffer =(byte[]) Properties.Resources.ResourceManager.GetObject(szFileItem.Substring(0, szFileItem.IndexOf('.')).Replace("-","_"));//Properties.Resources.libusb0;
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Close();
                    if (!File.Exists(sPath + szFileItem))
                    {
                        szRet = "Error! " + szFileItem + " does not exist";
                        return false;
                    }
                }
            }
            szRet = "OK!";
            return true;
        }

        private bool Chk_Device(ref string szRet)
        {
            ProcessLayer pl = new ProcessLayer();
            bool bSuccess = pl.SyncStart(sPath+"nfc-list.exe", null);

            if (!bSuccess)
            {
                szRet = "Error! Can not connect to the device";
                return false;
            }
            string[] RegexRet = RegexStringLayer.GetSubString(pl.m_RetOutputMsg.ToString(), "NFC device:\\s*(.+)\\s*/");

            bool bRet;
            if (string.IsNullOrWhiteSpace(RegexRet[0]))
            {
                bRet = false;
                szRet = "Error! Can not connect to the device";
            }
            else
            {
                bRet = true;
                szRet = "Device: " + RegexRet[0];
                szDevice.Append(RegexRet[0]);
            }

            return bRet;
        }
    }
}
