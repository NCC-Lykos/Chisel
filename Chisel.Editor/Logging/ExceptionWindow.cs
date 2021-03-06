﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Chisel.Editor.Logging
{
    public partial class ExceptionWindow : Form
    {
        public ExceptionInfo ExceptionInfo { get; set; }

        public ExceptionWindow(ExceptionInfo info)
        {
            ExceptionInfo = info;
            InitializeComponent();
            FrameworkVersion.Text = info.RuntimeVersion;
            OperatingSystem.Text = info.OperatingSystem;
            ChiselVersion.Text = info.ApplicationVersion;
            FullError.Text = info.FullStackTrace;
        }

        private void SubmitButtonClicked(object sender, EventArgs e)
        {
            Submit();
            Close();
        }

        private void Submit()
        {
            try
            {
                ExceptionInfo.UserEnteredInformation = InfoTextBox.Text;
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["Message"] = ExceptionInfo.Message;
                    values["Runtime"] = ExceptionInfo.RuntimeVersion;
                    values["OS"] = ExceptionInfo.OperatingSystem;
                    values["Version"] = ExceptionInfo.ApplicationVersion;
                    values["StackTrace"] = ExceptionInfo.FullStackTrace;
                    values["UserInfo"] = ExceptionInfo.UserEnteredInformation;
                    values["Source"] = ExceptionInfo.Source;
                    values["Date"] = ExceptionInfo.Date.ToUniversalTime().ToString("yyyy-MM-ddThh:mm:ssZ");
                    client.UploadValues("http://bugs.Chisel-editor.com/Bug/AutoSubmit", "POST", values);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending bug report: " + ex.Message);
            }
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
