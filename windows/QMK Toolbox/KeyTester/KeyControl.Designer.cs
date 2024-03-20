namespace QMK_Toolbox.KeyTester
{
    partial class KeyControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblLegend = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lblLegend
            // 
            lblLegend.BackColor = System.Drawing.SystemColors.ControlLight;
            lblLegend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            lblLegend.Location = new System.Drawing.Point(0, 0);
            lblLegend.Margin = new System.Windows.Forms.Padding(0);
            lblLegend.Name = "lblLegend";
            lblLegend.Size = new System.Drawing.Size(47, 46);
            lblLegend.TabIndex = 0;
            lblLegend.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // KeyControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lblLegend);
            Enabled = false;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "KeyControl";
            Size = new System.Drawing.Size(47, 46);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblLegend;
    }
}
