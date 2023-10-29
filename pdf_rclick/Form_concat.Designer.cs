namespace pdf_rclick
{
    partial class Form_concat
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
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_concat = new System.Windows.Forms.Button();
            this.button_down = new System.Windows.Forms.Button();
            this.button_up = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.listBox_lists = new System.Windows.Forms.ListBox();
            this.label_outputfolder = new System.Windows.Forms.Label();
            this.label_action = new System.Windows.Forms.Label();
            this.textBox_outputfile = new System.Windows.Forms.TextBox();
            this.label_list = new System.Windows.Forms.Label();
            this.comboBox_action = new System.Windows.Forms.ComboBox();
            this.button_exeadd = new System.Windows.Forms.Button();
            this.button_exedelete = new System.Windows.Forms.Button();
            this.label_preview = new System.Windows.Forms.Label();
            this.label_outputfile = new System.Windows.Forms.Label();
            this.comboBox_folder = new System.Windows.Forms.ComboBox();
            this.button_folderadd = new System.Windows.Forms.Button();
            this.button_folderdelete = new System.Windows.Forms.Button();
            this.label_ext = new System.Windows.Forms.Label();
            this.checkBox_topmost = new System.Windows.Forms.CheckBox();
            this.button_save = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_cancel
            // 
            this.button_cancel.Location = new System.Drawing.Point(524, 377);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_cancel.TabIndex = 14;
            this.button_cancel.Text = "キャンセル";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_concat
            // 
            this.button_concat.Location = new System.Drawing.Point(443, 377);
            this.button_concat.Name = "button_concat";
            this.button_concat.Size = new System.Drawing.Size(75, 23);
            this.button_concat.TabIndex = 13;
            this.button_concat.Text = "結合する";
            this.button_concat.UseVisualStyleBackColor = true;
            this.button_concat.Click += new System.EventHandler(this.button_concat_Click);
            // 
            // button_down
            // 
            this.button_down.Location = new System.Drawing.Point(644, 59);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(75, 23);
            this.button_down.TabIndex = 12;
            this.button_down.Text = "↓";
            this.button_down.UseVisualStyleBackColor = true;
            this.button_down.Click += new System.EventHandler(this.button_down_Click);
            // 
            // button_up
            // 
            this.button_up.Location = new System.Drawing.Point(644, 30);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(75, 23);
            this.button_up.TabIndex = 11;
            this.button_up.Text = "↑";
            this.button_up.UseVisualStyleBackColor = true;
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_delete
            // 
            this.button_delete.Location = new System.Drawing.Point(644, 88);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(75, 23);
            this.button_delete.TabIndex = 10;
            this.button_delete.Text = "削除";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // listBox_lists
            // 
            this.listBox_lists.FormattingEnabled = true;
            this.listBox_lists.HorizontalScrollbar = true;
            this.listBox_lists.ItemHeight = 12;
            this.listBox_lists.Location = new System.Drawing.Point(319, 30);
            this.listBox_lists.Name = "listBox_lists";
            this.listBox_lists.Size = new System.Drawing.Size(319, 184);
            this.listBox_lists.TabIndex = 9;
            this.listBox_lists.SelectedIndexChanged += new System.EventHandler(this.listBox_lists_SelectedIndexChanged);
            // 
            // label_outputfolder
            // 
            this.label_outputfolder.AutoSize = true;
            this.label_outputfolder.Location = new System.Drawing.Point(317, 220);
            this.label_outputfolder.Name = "label_outputfolder";
            this.label_outputfolder.Size = new System.Drawing.Size(287, 12);
            this.label_outputfolder.TabIndex = 15;
            this.label_outputfolder.Text = "出力先フォルダ　※フォルダのドラッグ＆ドロップでも指定可能";
            // 
            // label_action
            // 
            this.label_action.AutoSize = true;
            this.label_action.Location = new System.Drawing.Point(317, 307);
            this.label_action.Name = "label_action";
            this.label_action.Size = new System.Drawing.Size(318, 12);
            this.label_action.TabIndex = 15;
            this.label_action.Text = "結合後の処理　※実行ファイルのドラッグ＆ドロップでも処理を追加";
            // 
            // textBox_outputfile
            // 
            this.textBox_outputfile.Location = new System.Drawing.Point(319, 282);
            this.textBox_outputfile.Name = "textBox_outputfile";
            this.textBox_outputfile.Size = new System.Drawing.Size(163, 19);
            this.textBox_outputfile.TabIndex = 16;
            this.textBox_outputfile.Text = "concat";
            // 
            // label_list
            // 
            this.label_list.AutoSize = true;
            this.label_list.Location = new System.Drawing.Point(317, 9);
            this.label_list.Name = "label_list";
            this.label_list.Size = new System.Drawing.Size(239, 12);
            this.label_list.TabIndex = 15;
            this.label_list.Text = "結合するファイル　※ドラッグ＆ドロップで追加可能";
            // 
            // comboBox_action
            // 
            this.comboBox_action.FormattingEnabled = true;
            this.comboBox_action.Location = new System.Drawing.Point(319, 325);
            this.comboBox_action.Name = "comboBox_action";
            this.comboBox_action.Size = new System.Drawing.Size(267, 20);
            this.comboBox_action.TabIndex = 18;
            this.comboBox_action.SelectedIndexChanged += new System.EventHandler(this.comboBox_action_SelectedIndexChanged);
            // 
            // button_exeadd
            // 
            this.button_exeadd.Location = new System.Drawing.Point(593, 323);
            this.button_exeadd.Name = "button_exeadd";
            this.button_exeadd.Size = new System.Drawing.Size(60, 23);
            this.button_exeadd.TabIndex = 17;
            this.button_exeadd.Text = "追加";
            this.button_exeadd.UseVisualStyleBackColor = true;
            this.button_exeadd.Click += new System.EventHandler(this.button_exeadd_Click);
            // 
            // button_exedelete
            // 
            this.button_exedelete.Location = new System.Drawing.Point(659, 323);
            this.button_exedelete.Name = "button_exedelete";
            this.button_exedelete.Size = new System.Drawing.Size(60, 23);
            this.button_exedelete.TabIndex = 17;
            this.button_exedelete.Text = "削除";
            this.button_exedelete.UseVisualStyleBackColor = true;
            this.button_exedelete.Click += new System.EventHandler(this.button_exedelete_Click);
            // 
            // label_preview
            // 
            this.label_preview.AutoSize = true;
            this.label_preview.Location = new System.Drawing.Point(12, 9);
            this.label_preview.Name = "label_preview";
            this.label_preview.Size = new System.Drawing.Size(117, 12);
            this.label_preview.TabIndex = 19;
            this.label_preview.Text = "プレビュー(先頭3ページ)";
            // 
            // label_outputfile
            // 
            this.label_outputfile.AutoSize = true;
            this.label_outputfile.Location = new System.Drawing.Point(317, 264);
            this.label_outputfile.Name = "label_outputfile";
            this.label_outputfile.Size = new System.Drawing.Size(75, 12);
            this.label_outputfile.TabIndex = 15;
            this.label_outputfile.Text = "出力ファイル名";
            // 
            // comboBox_folder
            // 
            this.comboBox_folder.FormattingEnabled = true;
            this.comboBox_folder.Location = new System.Drawing.Point(319, 238);
            this.comboBox_folder.Name = "comboBox_folder";
            this.comboBox_folder.Size = new System.Drawing.Size(267, 20);
            this.comboBox_folder.TabIndex = 20;
            this.comboBox_folder.SelectedIndexChanged += new System.EventHandler(this.comboBox_folder_SelectedIndexChanged);
            // 
            // button_folderadd
            // 
            this.button_folderadd.Location = new System.Drawing.Point(593, 236);
            this.button_folderadd.Name = "button_folderadd";
            this.button_folderadd.Size = new System.Drawing.Size(60, 23);
            this.button_folderadd.TabIndex = 17;
            this.button_folderadd.Text = "追加";
            this.button_folderadd.UseVisualStyleBackColor = true;
            this.button_folderadd.Click += new System.EventHandler(this.button_folderadd_Click);
            // 
            // button_folderdelete
            // 
            this.button_folderdelete.Location = new System.Drawing.Point(659, 236);
            this.button_folderdelete.Name = "button_folderdelete";
            this.button_folderdelete.Size = new System.Drawing.Size(60, 23);
            this.button_folderdelete.TabIndex = 17;
            this.button_folderdelete.Text = "削除";
            this.button_folderdelete.UseVisualStyleBackColor = true;
            this.button_folderdelete.Click += new System.EventHandler(this.button_folderdelete_Click);
            // 
            // label_ext
            // 
            this.label_ext.AutoSize = true;
            this.label_ext.Location = new System.Drawing.Point(488, 285);
            this.label_ext.Name = "label_ext";
            this.label_ext.Size = new System.Drawing.Size(23, 12);
            this.label_ext.TabIndex = 15;
            this.label_ext.Text = ".pdf";
            // 
            // checkBox_topmost
            // 
            this.checkBox_topmost.AutoSize = true;
            this.checkBox_topmost.Location = new System.Drawing.Point(593, 8);
            this.checkBox_topmost.Name = "checkBox_topmost";
            this.checkBox_topmost.Size = new System.Drawing.Size(102, 16);
            this.checkBox_topmost.TabIndex = 21;
            this.checkBox_topmost.Text = "常に手前に表示";
            this.checkBox_topmost.UseVisualStyleBackColor = true;
            this.checkBox_topmost.CheckedChanged += new System.EventHandler(this.checkBox_topmost_CheckedChanged);
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(605, 377);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(114, 23);
            this.button_save.TabIndex = 22;
            this.button_save.Text = "現在の設定を保存";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // Form_concat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 412);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.checkBox_topmost);
            this.Controls.Add(this.comboBox_folder);
            this.Controls.Add(this.label_preview);
            this.Controls.Add(this.comboBox_action);
            this.Controls.Add(this.button_folderdelete);
            this.Controls.Add(this.button_exedelete);
            this.Controls.Add(this.button_folderadd);
            this.Controls.Add(this.button_exeadd);
            this.Controls.Add(this.textBox_outputfile);
            this.Controls.Add(this.label_action);
            this.Controls.Add(this.label_list);
            this.Controls.Add(this.label_ext);
            this.Controls.Add(this.label_outputfile);
            this.Controls.Add(this.label_outputfolder);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_concat);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_up);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.listBox_lists);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form_concat";
            this.Text = "結合ダイアログ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_concat_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_concat;
        private System.Windows.Forms.Button button_down;
        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.ListBox listBox_lists;
        private System.Windows.Forms.Label label_outputfolder;
        private System.Windows.Forms.Label label_action;
        private System.Windows.Forms.TextBox textBox_outputfile;
        private System.Windows.Forms.Label label_list;
        private System.Windows.Forms.ComboBox comboBox_action;
        private System.Windows.Forms.Button button_exeadd;
        private System.Windows.Forms.Button button_exedelete;
        private System.Windows.Forms.Label label_preview;
        private System.Windows.Forms.Label label_outputfile;
        private System.Windows.Forms.ComboBox comboBox_folder;
        private System.Windows.Forms.Button button_folderadd;
        private System.Windows.Forms.Button button_folderdelete;
        private System.Windows.Forms.Label label_ext;
        private System.Windows.Forms.CheckBox checkBox_topmost;
        private System.Windows.Forms.Button button_save;
    }
}