using System;
using System.Windows.Forms;

namespace Chisel.Editor.Tools.MotionsTool
{
    public partial class NewKeyFrame : Form
    {
        public float Time;
        private float _id;
        public bool cancel;
        public NewKeyFrame(float ID)
        {
            InitializeComponent();
            _id = ID;
            txtName.Text = _id.ToString();
            SetName();
        }
        private void SetName(bool hide = false)
        {
            if (!cancel) Time = (float)Convert.ToDecimal(txtName.Text);
            else Time = _id;
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
            cancel = true;
            SetName(true);
        }
    }
}
