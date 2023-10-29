namespace pdf_rclick
{
    partial class Form_log
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
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox_log
            // 
            this.textBox_log.BackColor = System.Drawing.SystemColors.Control;
            this.textBox_log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_log.ForeColor = System.Drawing.SystemColors.WindowText;
            this.textBox_log.Location = new System.Drawing.Point(0, 0);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_log.Size = new System.Drawing.Size(566, 273);
            this.textBox_log.TabIndex = 0;
            // 
            // Form_log
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 273);
            this.Controls.Add(this.textBox_log);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form_log";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "進捗";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_log_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_log;
    }
}