using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chisel.Common.Mediator;

namespace Chisel.Editor.UI.MotionsEditor
{
    public partial class MotionsEditorDialog : Form, IMediatorListener
    {
        public MotionsEditorDialog(Documents.Document document)
        {
            InitializeComponent();
            Initialize(document);
        }

        private Documents.Document Document { get; set; }

        private void Initialize(Documents.Document d)
        {
            Document = d;

                //CustomFlags.Items.Add(keys[x], CheckState.Unchecked);
        }

        
        
        public void Notify(string message, object data) {}
    }
}
