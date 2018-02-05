using System;
using System.Windows.Forms;
using Chisel.DataStructures.Geometric;
using Chisel.Extensions;

namespace Chisel.Editor.Tools.MotionsTool
{
    public partial class KeyFrameEdit : Form
    {
        private float _id;
        public bool change = false;
        public Coordinate c;
        public Quaternion q;

        public KeyFrameEdit(float ID)
        {
            InitializeComponent();
            _id = ID;
            tX.Text = "0";
            tY.Text = "0";
            tZ.Text = "0";

            rX.Text = "0";
            rY.Text = "0";
            rZ.Text = "0";
        }

        private void OnKeyDown(object s, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return) EditClicked(null, null);
            else if (e.KeyChar == (char)Keys.Escape) CancelClicked(null, null);
        }

        private bool CheckInputs()
        {
            bool ret = true;
            decimal r;

            if (!decimal.TryParse(tX.Text, out r)) ret = false;
            if (!decimal.TryParse(tY.Text, out r)) ret = false;
            if (!decimal.TryParse(tZ.Text, out r)) ret = false;

            if (!decimal.TryParse(rX.Text, out r)) ret = false;
            if (!decimal.TryParse(rY.Text, out r)) ret = false;
            if (!decimal.TryParse(rZ.Text, out r)) ret = false;

            if (!ret) MessageBox.Show("Non numeric inputs, no edit applied.", "bad inputs", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return ret;
        }

        private void EditClicked(object s, EventArgs e)
        {
            if (!CheckInputs())
            {
                change = false;
                Hide();
            }
            c = new Coordinate(Convert.ToDecimal(tX.Text), Convert.ToDecimal(tY.Text), Convert.ToDecimal(tZ.Text));
            Coordinate rot = new Coordinate(Convert.ToDecimal(rX.Text), Convert.ToDecimal(rY.Text), Convert.ToDecimal(rZ.Text));

            q = Quaternion.EulerAngles(rot * DMath.PI / 180);
            change = true;
            Hide();
        }

        private void CancelClicked(object s, EventArgs e)
        {
            change = false;
            Hide();
        }
    }
}
