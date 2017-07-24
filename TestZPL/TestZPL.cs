using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestZPL
{
    public partial class TestZPL : Form
    {
        private Bitmap imageToConvert;

        public TestZPL()
        {
            InitializeComponent();
        }

        private void llTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://labelary.com/viewer.html");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            DialogResult result = ofdTestImage.ShowDialog();
            if (result == DialogResult.OK)
            {
                tbPicturePath.Text = ofdTestImage.FileName;
                imageToConvert = (Bitmap)Bitmap.FromFile(tbPicturePath.Text);
                pbPicture.Image = imageToConvert;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            tbZPL.Clear();

            //In this example I statically set the POS to 20,20 for the sake of demo.
            tbZPL.Text = Image2ZPL.Convert.BitmapToZPLII(imageToConvert, 20, 20);
        }
    }
}
