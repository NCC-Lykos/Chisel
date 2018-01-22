using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chisel.Common.Mediator;

namespace Chisel.Editor.UI
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();

            VersionLabel.Text = FileVersionInfo.GetVersionInfo(typeof (Editor).Assembly.Location).FileVersion;

            LTLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://logic-and-trick.com");
            GithubLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "https://github.com/NCC-Lykos/Chisel");
            GPLLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://www.gnu.org/licenses/gpl-2.0.html");
            NCLink.Click += (s, e) => Mediator.Publish(EditorMediator.OpenWebsite, "http://www.neocron.org");
        }
    }
}
