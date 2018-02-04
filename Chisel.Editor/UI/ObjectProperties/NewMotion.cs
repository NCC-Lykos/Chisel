using System;
using System.Windows.Forms;

namespace Chisel.Editor.UI.ObjectProperties
{
    public partial class NewMotion : Form
    {
        public string Name;
        public NewMotion(long ID)
        {
            InitializeComponent();
            txtName.Text = "Motion_" + ID.ToString();
            SetName();
        }
        private void SetName(bool hide = false, bool blank = false)
        {
            if (!blank) Name = new string(txtName.Text.ToCharArray());
            else Name = null;
            Hide();
        }
        private void OnKeyDown(object s, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                if (txtName.Text.Length == 0) MessageBox.Show("Name Empty");
                else
                {
                    SetName(true);
                }
            }
            else if (e.KeyChar == (char)Keys.Escape) ; 
        }
        private void AddClicked(object s, EventArgs e)
        {
            SetName(true);
        }

        private void CancelClicked(object s, EventArgs e)
        {
            SetName(true, true);
        }
    }
}
