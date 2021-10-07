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
            this.lblLegend = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblLegend
            // 
            this.lblLegend.BackColor = System.Drawing.SystemColors.ControlLight;
            this.lblLegend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLegend.Location = new System.Drawing.Point(0, 0);
            this.lblLegend.Margin = new System.Windows.Forms.Padding(0);
            this.lblLegend.Name = "lblLegend";
            this.lblLegend.Size = new System.Drawing.Size(40, 40);
            this.lblLegend.TabIndex = 0;
            this.lblLegend.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // KeyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblLegend);
            this.Enabled = false;
            this.Name = "KeyControl";
            this.Size = new System.Drawing.Size(40, 40);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblLegend;
    }
}
