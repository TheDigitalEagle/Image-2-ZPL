namespace TestZPL
{
    partial class TestZPL
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ofdTestImage = new System.Windows.Forms.OpenFileDialog();
            this.pbPicture = new System.Windows.Forms.PictureBox();
            this.tbPicturePath = new System.Windows.Forms.TextBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.tbZPL = new System.Windows.Forms.TextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.llTest = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // ofdTestImage
            // 
            this.ofdTestImage.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            // 
            // pbPicture
            // 
            this.pbPicture.Location = new System.Drawing.Point(12, 38);
            this.pbPicture.Name = "pbPicture";
            this.pbPicture.Size = new System.Drawing.Size(267, 249);
            this.pbPicture.TabIndex = 0;
            this.pbPicture.TabStop = false;
            // 
            // tbPicturePath
            // 
            this.tbPicturePath.Location = new System.Drawing.Point(12, 11);
            this.tbPicturePath.Name = "tbPicturePath";
            this.tbPicturePath.Size = new System.Drawing.Size(192, 20);
            this.tbPicturePath.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(210, 11);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(69, 21);
            this.btnOpen.TabIndex = 2;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // tbZPL
            // 
            this.tbZPL.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbZPL.Location = new System.Drawing.Point(286, 38);
            this.tbZPL.Multiline = true;
            this.tbZPL.Name = "tbZPL";
            this.tbZPL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbZPL.Size = new System.Drawing.Size(489, 276);
            this.tbZPL.TabIndex = 3;
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(12, 293);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(267, 21);
            this.btnConvert.TabIndex = 4;
            this.btnConvert.Text = "Convert To ZPLII";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // llTest
            // 
            this.llTest.AutoSize = true;
            this.llTest.Location = new System.Drawing.Point(646, 317);
            this.llTest.Name = "llTest";
            this.llTest.Size = new System.Drawing.Size(129, 13);
            this.llTest.TabIndex = 5;
            this.llTest.TabStop = true;
            this.llTest.Text = "Test out your results here!";
            this.llTest.VisitedLinkColor = System.Drawing.Color.Blue;
            this.llTest.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llTest_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(348, 317);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Make sure to put this between the ^XA and ^XZ commands.";
            // 
            // TestZPL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 338);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.llTest);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.tbZPL);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.tbPicturePath);
            this.Controls.Add(this.pbPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "TestZPL";
            this.ShowIcon = false;
            this.Text = "TestZPL";
            ((System.ComponentModel.ISupportInitialize)(this.pbPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog ofdTestImage;
        private System.Windows.Forms.PictureBox pbPicture;
        private System.Windows.Forms.TextBox tbPicturePath;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.TextBox tbZPL;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.LinkLabel llTest;
        private System.Windows.Forms.Label label1;
    }
}

