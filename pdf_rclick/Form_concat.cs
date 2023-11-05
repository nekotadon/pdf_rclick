/*
 * This software includes the work that is distributed in the Apache License 2.0  
 * This software includes the work that is distributed in the MIT License  
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using TextLib;

namespace pdf_rclick
{
    public partial class Form_concat : Form
    {
        //結合するファイル
        List<FileClass> files = new List<FileClass>();

        //結合後の処理リスト(何もしない、開くを除く)
        List<string> actions = new List<string>();

        //出力先フォルダリスト
        List<string> folders = new List<string>();

        //PDFプレビュー
        List<Task> tasks = new List<Task>();//プレビュー用画像作成Task
        bool taskcancel = false;//プレビュー処理キャンセルフラグ
        int previewSize = 0;//プレビューを実行するファイルサイズ上限MB

        public Form_concat(List<string> lists)//結合するファイルパスリスト
        {
            InitializeComponent();

            //アイコン設定
            Icon = Properties.Resources.appico;

            //画面中央に表示
            StartPosition = FormStartPosition.CenterScreen;

            //フォント変更
            foreach (Control c in Controls)
            {
                c.Font = SystemFonts.MenuFont;
            }

            //ListBox
            listBox_lists.AllowDrop = true;
            listBox_lists.DragEnter += (sender, e) => listBox_lists_DragEnter(sender, e);
            listBox_lists.DragDrop += (sender, e) => listBox_lists_DragDrop(sender, e);

            //出力先フォルダリスト
            readFolderFile();
            setFolders();
            comboBox_folder.AllowDrop = true;
            comboBox_folder.DragEnter += (sender, e) => comboBox_folder_DragEnter(sender, e);
            comboBox_folder.DragDrop += (sender, e) => comboBox_folder_DragDrop(sender, e);
            comboBox_folder.SelectedIndex = 0;

            //結合後の処理リスト
            readActionFile();
            setActions();
            comboBox_action.AllowDrop = true;
            comboBox_action.DragEnter += (sender, e) => comboBox_action_DragEnter(sender, e);
            comboBox_action.DragDrop += (sender, e) => comboBox_action_DragDrop(sender, e);
            comboBox_action.SelectedIndex = 0;

            //設定ファイルを読み込んで設定
            try
            {
                //設定ファイル読み込み
                IniFile iniFile = new IniFile();

                //プレビューを実行するファイルサイズ上限MB
                previewSize = iniFile.GetKeyValueInt("setting", "previewSize", 2, 0, 1000, true);
                if (previewSize == 0)
                {
                    label_preview.Text = "プレビューなし";
                }
                else
                {
                    label_preview.Text += " ファイルサイズ" + previewSize.ToString() + "MB以下限定";
                }

                //最前面表示設定
                TopMost = checkBox_topmost.Checked = iniFile.GetKeyValueBool("output", "topmost", false, true);

                //出力先フォルダ
                int folderindex = iniFile.GetKeyValueInt("output", "folder", 0, 0, 100, true);
                if (folderindex < comboBox_folder.Items.Count)
                {
                    comboBox_folder.SelectedIndex = folderindex;
                }

                //出力ファイル名
                string outputfilename = iniFile.GetKeyValueStringWithoutEmpty("output", "filename", "concat", true);
                if (isCorrectFilename(outputfilename))
                {
                    textBox_outputfile.Text = outputfilename;
                }

                //処理
                int actionindex = iniFile.GetKeyValueInt("output", "action", 0, 0, 100, true);
                if (actionindex < comboBox_action.Items.Count)
                {
                    comboBox_action.SelectedIndex = actionindex;
                }
            }
            catch (Exception)
            {

            }

            //結合するファイル
            foreach (string s in lists)
            {
                //ファイルが存在する場合は
                if (File.Exists(s))
                {
                    //ファイルリストに追加
                    files.Add(new FileClass(s));

                    //プレビュー画像作成
                    if (previewSize != 0)//プレビューするファイルのサイズ上限が0の場合はプレビューしない
                    {
                        if (new FileInfo(s).Length <= previewSize * 1024 * 1024)
                        {
                            //プレビュー作成
                            createPanel(s);
                        }
                    }
                }
            }

            //ListBoxの表示を更新
            table_refresh();

            //先頭のファイルを選択
            if (listBox_lists.Items.Count >= 0)
            {
                listBox_lists.SelectedIndex = 0;
            }
        }

        ///////////////////////////////////////////////////////////////////////// 共通

        //ファイル名に使えない文字がないか
        private bool isCorrectFilename(string name)
        {
            string[] badString = { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };

            bool check = true;
            foreach (var s in badString)
            {
                if (name.Contains(s))
                {
                    check = false;
                    break;
                }
            }

            return check;
        }

        //ComboBoxのサイズを自動調整
        private void setComboBoxWidth(ComboBox comboBox)
        {
            //Comboboxの幅
            int width = comboBox.Width;

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                //ComboBoxに表示されている項目名
                string text = comboBox.Items[i].ToString();

                //その項目名でLabel作成
                Label label = new Label
                {
                    AutoSize = true,
                    Text = text,
                    Font = comboBox.Font,
                    Visible = false,
                };

                //LabelのAutoSizeを利用して幅を取得
                Controls.Add(label);
                if (label.Width > width)
                {
                    width = label.Width;
                }
                Controls.Remove(label);
            }

            //ComboboxのDropDownWidthを更新
            comboBox.DropDownWidth = width + (width == comboBox.Width ? 0 : 20);
        }


        ///////////////////////////////////////////////////////////////////////// ListBox

        //ファイルをListBoxに表示するためのクラス
        private class FileClass
        {
            public string path { get; set; }//ファイルのフルパス
            public string name { get; set; }//ファイル名(ListBox表示用)
            public string folder { get; set; }//フォルダ(同名のファイルがある場合カッコつきでListBoxに表示)

            public bool isExist//ファイルが存在するか
            {
                get
                {
                    return File.Exists(path);
                }
            }

            public FileClass()
            {
                path = name = folder = "";
            }
            public FileClass(string _filepath)
            {
                path = _filepath;
                try
                {
                    name = Path.GetFileName(_filepath);
                    folder = Path.GetDirectoryName(_filepath);
                }
                catch (Exception)
                {
                    name = "";
                    folder = "";
                }
            }

            //ListBoxでの上下位置入れ替え用
            public void copy_from(FileClass scbuf)
            {
                path = scbuf.path;
                name = scbuf.name;
                folder = scbuf.folder;
            }
        };

        //ListBoxの表示を更新
        private void table_refresh()
        {
            //リスト初期化
            listBox_lists.Items.Clear();

            //ファイルがある場合
            if (files != null && files.Count > 0)
            {
                foreach (var f in files)
                {
                    string name = f.name;

                    //同名ファイルがあるかどうか
                    int counter = 0;
                    foreach (var tmp in files)
                    {
                        if (tmp.name == name)
                        {
                            counter++;
                        }
                    }

                    if (counter == 1)//同名ファイルがない場合
                    {
                        listBox_lists.Items.Add(f.name);
                    }
                    else//同名ファイルがある場合
                    {
                        //ファイル名に続けてフォルダ名を記載
                        listBox_lists.Items.Add(f.name + " (" + f.folder + ")");
                    }
                }
            }

            //ファイルが2個以上ない場合は結合処理不可
            button_concat.Enabled = files != null && files.Count >= 2;
        }

        //項目選択でプレビュー画面を非表示から表示に切り替え
        private void listBox_lists_SelectedIndexChanged(object sender, EventArgs e)
        {
            hideAllPreviewPanels();

            int index = listBox_lists.SelectedIndex;
            if (0 <= index && index < files.Count)
            {
                displayPanel(files[index].path);
            }
        }

        //D&D
        private void listBox_lists_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string d in drags)
                {
                    if (!File.Exists(d))//ファイルでない
                    {
                        return;
                    }
                    else if (Path.GetExtension(d).ToLower() != ".pdf")//pdfでない
                    {
                        return;
                    }
                }
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void listBox_lists_DragDrop(object sender, DragEventArgs e)
        {
            string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            foreach (var d in drags)
            {
                //.pdfファイルの場合
                if (File.Exists(d) && Path.GetExtension(d).ToLower() == ".pdf")
                {
                    //ファイルリストに追加
                    files.Add(new FileClass(d));

                    //プレビュー画像作成
                    if (previewSize != 0)
                    {
                        if (new FileInfo(d).Length <= previewSize * 1024 * 1024)
                        {
                            createPanel(d);
                        }
                    }
                }
            }

            //ListBoxの表示を更新
            table_refresh();
        }


        //上へボタン
        private void button_up_Click(object sender, EventArgs e)
        {
            int idx = listBox_lists.SelectedIndex;

            if (idx > 0)//選択されている場合で最初でない場合
            {
                //入れ替え
                FileClass buf1 = new FileClass();
                FileClass buf2 = new FileClass();

                buf1.copy_from(files[idx - 1]);
                buf2.copy_from(files[idx]);

                files[idx].copy_from(buf1);
                files[idx - 1].copy_from(buf2);

                //ListBoxの表示を更新
                table_refresh();

                //選択位置変更
                listBox_lists.SelectedIndex = idx - 1;
            }
        }

        //下へボタン
        private void button_down_Click(object sender, EventArgs e)
        {
            int idx = listBox_lists.SelectedIndex;

            if (0 <= idx && idx < listBox_lists.Items.Count - 1)//選択されている場合で最後でない場合
            {
                //入れ替え
                FileClass buf1 = new FileClass();
                FileClass buf2 = new FileClass();

                buf1.copy_from(files[idx]);
                buf2.copy_from(files[idx + 1]);

                files[idx].copy_from(buf2);
                files[idx + 1].copy_from(buf1);

                //ListBoxの表示を更新
                table_refresh();

                //選択位置変更
                listBox_lists.SelectedIndex = idx + 1;
            }
        }

        //削除ボタン
        private void button_delete_Click(object sender, EventArgs e)
        {
            int idx = listBox_lists.SelectedIndex;

            if (0 <= idx && idx < files.Count)//選択されている場合
            {
                //選択されたファイル用のPreviewPanel削除
                deletePanel(files[idx].path);

                //ファイルリストから削除
                files.RemoveAt(idx);

                //ListBoxの表示を更新
                table_refresh();

                //選択位置変更
                if (listBox_lists.Items.Count > 0)
                {
                    //一番下のファイルを削除した場合のみ
                    if (idx >= listBox_lists.Items.Count - 1)
                    {
                        //1つ上のファイルを選択
                        idx = listBox_lists.Items.Count - 1;
                    }

                    //それ以外は選択位置を変更しない
                    listBox_lists.SelectedIndex = idx;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////// PreviewPanel

        //ファイルの数だけPreview用のPanelを作成し、コントロールに追加。そこに最初の3ページ分を画像化して表示
        //選択されたファイル用のPanelだけ Visible = true にすることで、選択ファイルだけプレビューされているように見せている

        //フォームを閉じるとき
        bool finished = false;
        private void Form_concat_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!finished)//すべてのTaskの完了が未確認の場合
            {
                if (!taskcancel)
                {
                    //Taskのキャンセルを実施
                    taskcancel = true;
                    Text = "結合ダイアログ - プレビュー処理キャンセル中...";

                    //すべてのTaskが終わるまで待つ
                    Task.Run(() =>
                    {
                        Task.WaitAll(tasks.ToArray());
                        finished = true;//Task完了確認済み
                        Close();
                    });
                }
                e.Cancel = true;
            }
        }

        //プレビューパネル新作
        private void createPanel(string filepath)
        {
            //現在あるPreviewPanelがどのpdfファイルのものか確認
            List<string> filepaths = new List<string>();
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(PreviewPanel))
                {
                    filepaths.Add(((PreviewPanel)c).filepath);
                }
            }

            //プレビューを作成したいpdfファイル用のPreviewPanelがない場合
            if (!filepaths.Contains(filepath))
            {
                //プレビューパネル作成
                PreviewPanel panel = new PreviewPanel(filepath);

                //フォームに追加
                Controls.Add(panel);

                //pdfの最初の3ページを画像化するTask
                var task = new Task(() =>
                {
                    try
                    {
                        if (!taskcancel)
                        {
                            using (PdfiumViewer.PdfDocument inputPdfDocument = PdfiumViewer.PdfDocument.Load(filepath))
                            {
                                if (!taskcancel)
                                {
                                    int num = inputPdfDocument.PageCount;

                                    //ページがある場合
                                    if (num > 0)
                                    {
                                        //ページ数がプレビュー数以下の場合はページなし画像を設定
                                        if (num < panel.PageCount)
                                        {
                                            for (int i = num; i < panel.PageCount; i++)
                                            {
                                                panel.setNoPage(i);
                                            }
                                        }

                                        //プレビュー用画像作成
                                        for (int i = 0; i < panel.PageCount; i++)
                                        {
                                            if (taskcancel)
                                            {
                                                break;
                                            }

                                            if (i < num)
                                            {
                                                //pdfを画像化
                                                int dpi = 72;
                                                int w = (int)inputPdfDocument.PageSizes[i].Width * dpi / 72;
                                                int h = (int)inputPdfDocument.PageSizes[i].Height * dpi / 72;
                                                Image img = inputPdfDocument.Render(i, w, h, dpi, dpi, true);

                                                //PreviewPanelに画像を設定
                                                panel.setImage(img, i);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        ;
                    }
                });

                //Taskキャンセル出なければTask開始
                if (!taskcancel)
                {
                    tasks.Add(task);
                    task.Start();
                }
            }
        }

        //プレビューパネル削除
        private void deletePanel(string filepath)
        {
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(PreviewPanel))
                {
                    PreviewPanel panel = (PreviewPanel)c;

                    //対象のPreviewPanelの場合
                    if (panel.filepath == filepath)
                    {
                        //メモリ解放しコントロールから削除
                        panel.Visible = false;
                        panel.allPictureDispose();
                        Controls.Remove(panel);
                        break;
                    }
                }
            }
        }

        //プレビューパネル非表示
        private void hideAllPreviewPanels()
        {
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(PreviewPanel))
                {
                    ((PreviewPanel)c).Visible = false;
                }
            }
        }

        //選択されたファイルのプレビューパネル表示
        private void displayPanel(string filepath)
        {
            foreach (Control c in this.Controls)
            {
                if (c.GetType() == typeof(PreviewPanel))
                {
                    if (((PreviewPanel)c).filepath == filepath)
                    {
                        ((PreviewPanel)c).Visible = true;
                        break;
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////// 出力先フォルダ

        //出力先フォルダを追加ボタン
        private void button_folderadd_Click(object sender, EventArgs e)
        {
            //フォルダ選択ダイアログ
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop
            };

            //OKを押された場合
            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                addFolder(folderBrowserDialog.SelectedPath);
            }
        }

        //出力先フォルダを削除ボタン
        private void button_folderdelete_Click(object sender, EventArgs e)
        {
            int index = comboBox_folder.SelectedIndex;
            int maxccount = comboBox_folder.Items.Count - 1;

            if (index >= 1)//1個目は固定
            {
                //ComboBoxから削除
                comboBox_folder.Items.RemoveAt(index);

                //フォルダリストから削除
                folders.RemoveAt(index - 1);

                //フォルダ設定ファイル(.ini)の更新
                writeFolderFile();

                //ComboBoxの幅の自動調整
                setComboBoxWidth(comboBox_folder);

                //ComboBoxの選択項目を変更
                if (index == maxccount)
                {
                    comboBox_folder.SelectedIndex = index - 1;
                }
                else
                {
                    comboBox_folder.SelectedIndex = index;
                }
            }
        }

        //D&D
        private void comboBox_folder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                //1個でない場合は処理しない
                if (drags.Length != 1) return;

                if (!Directory.Exists(drags[0]))
                {
                    return;
                }

                e.Effect = DragDropEffects.Copy;
            }
        }
        private void comboBox_folder_DragDrop(object sender, DragEventArgs e)
        {
            string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //1個でない場合は処理しない
            if (drags.Length != 1) return;

            if (Directory.Exists(drags[0]))
            {
                addFolder(drags[0]);
            }
        }

        //選択が変更された場合
        private void comboBox_folder_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_folderdelete.Enabled = comboBox_folder.SelectedIndex >= 1;
        }

        //comboBoxにフォルダを設定
        private void setFolders()
        {
            //初期化
            comboBox_folder.Items.Clear();
            comboBox_folder.Items.Add("(結合するファイルの先頭のファイルと同じフォルダ)");

            foreach (var folder in folders)
            {
                try
                {
                    comboBox_folder.Items.Add(folder);
                }
                catch (Exception)
                {
                    ;
                }
            }

            //ComboBoxの幅の自動調整
            setComboBoxWidth(comboBox_folder);
        }

        //フォルダを追加
        private void addFolder(string path)
        {
            try
            {
                //フォルダが存在する場合で
                if (Directory.Exists(path))
                {
                    //現在のリストにない場合は
                    if (!folders.Contains(path))
                    {
                        //フォルダリストに追加
                        folders.Add(path);

                        //フォルダ設定ファイル(.ini)の更新
                        writeFolderFile();

                        //comboBoxにフォルダを設定
                        setFolders();

                        //最後の項目（今回追加したフォルダ）を選択
                        comboBox_folder.SelectedIndex = comboBox_folder.Items.Count - 1;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        //フォルダ設定ファイル(.ini)読み込み
        private void readFolderFile()
        {
            //ファイル読み込み
            string str = TextFile.Read(AppInfo.folderfile);

            //フォルダリスト初期化
            folders.Clear();

            //1行ごとに処理
            foreach (string folder in str.Replace("\r\n", "\n").Split('\n'))
            {
                try
                {
                    if (Directory.Exists(folder))
                    {
                        folders.Add(folder);
                    }
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        //フォルダ設定ファイル(.ini)書き込み
        private void writeFolderFile()
        {
            string str = string.Join(Environment.NewLine, folders.ToArray());
            TextFile.Write(AppInfo.folderfile, str);
        }

        ///////////////////////////////////////////////////////////////////////// 結合後の処理


        //結合後の処理を追加ボタン
        private void button_exeadd_Click(object sender, EventArgs e)
        {
            //ファイル選択ダイアログ
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Filter = "実行ファイル(*.exe;*.bat)|*.exe;*.bat"
            };

            //OKを押された場合
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                addAction(openFileDialog.FileName);
            }
        }

        //結合後の処理を削除ボタン
        private void button_exedelete_Click(object sender, EventArgs e)
        {
            int index = comboBox_action.SelectedIndex;
            int maxccount = comboBox_action.Items.Count - 1;

            if (index >= 2)//1,2個目は固定
            {
                //ComboBoxから削除
                comboBox_action.Items.RemoveAt(index);

                //結合後の処理リストから削除
                actions.RemoveAt(index - 2);

                //処理設定ファイル(.ini)の更新
                writeActionFile();

                //ComboBoxの幅の自動調整
                setComboBoxWidth(comboBox_action);

                //ComboBoxの選択項目を変更
                if (index == maxccount)
                {
                    comboBox_action.SelectedIndex = index - 1;
                }
                else
                {
                    comboBox_action.SelectedIndex = index;
                }
            }
        }

        //D&D
        private void comboBox_action_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                //1個でない場合は処理しない
                if (drags.Length != 1) return;

                //存在するファイルで.exe/.batでない場合は処理しない
                if (File.Exists(drags[0]))
                {
                    string ext = Path.GetExtension(drags[0]).ToLower();
                    if (ext != ".exe" && ext != ".bat")
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

                e.Effect = DragDropEffects.Copy;
            }
        }
        private void comboBox_action_DragDrop(object sender, DragEventArgs e)
        {
            string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //1個でない場合は処理しない
            if (drags.Length != 1) return;

            if (File.Exists(drags[0]))
            {
                string ext = Path.GetExtension(drags[0]).ToLower();
                if (ext == ".exe" || ext == ".bat")
                {
                    addAction(drags[0]);
                }
            }
        }

        //選択が変更された場合
        private void comboBox_action_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_exedelete.Enabled = comboBox_action.SelectedIndex >= 2;
        }

        //comboBoxに結合後の処理を設定
        private void setActions()
        {
            //初期化
            comboBox_action.Items.Clear();
            comboBox_action.Items.Add("何もしない");
            comboBox_action.Items.Add("開く");

            foreach (var action in actions)
            {
                try
                {
                    string ext = Path.GetExtension(action).ToLower();

                    //.exe .batの場合
                    if (ext == ".exe" || ext == ".bat")
                    {
                        //ファイル名
                        string name = Path.GetFileName(action);

                        //同名ファイルがあるかどうか
                        int counter = 0;
                        foreach (var item in actions)
                        {
                            if (Path.GetFileName(item) == name)
                            {
                                counter++;
                            }
                        }

                        if (counter == 1)//同名ファイルがない場合
                        {
                            comboBox_action.Items.Add(name + "で開く");
                        }
                        else//同名ファイルがある場合
                        {
                            //ファイル名に続けてフォルダ名を記載
                            comboBox_action.Items.Add(name + "で開く (" + action + ")");
                        }
                    }
                }
                catch (Exception)
                {
                    ;
                }
            }

            //ComboBoxの幅の自動調整
            setComboBoxWidth(this.comboBox_action);
        }

        //結合後の処理を追加
        private void addAction(string path)
        {
            try
            {
                string ext = Path.GetExtension(path).ToLower();

                //.exeまたは.batの場合で
                if (ext == ".exe" || ext == ".bat")
                {
                    //現在のリストにない場合は
                    if (!actions.Contains(path))
                    {
                        //結合後の処理リストに追加
                        actions.Add(path);

                        //処理設定ファイル(.ini)の更新
                        writeActionFile();

                        //comboBoxに結合後の処理を設定
                        setActions();

                        //最後の項目（今回追加した結合後の処理）を選択
                        comboBox_action.SelectedIndex = comboBox_action.Items.Count - 1;
                    }
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        //処理設定ファイル(.ini)読み込み
        private void readActionFile()
        {
            //ファイル読み込み
            string str = TextFile.Read(AppInfo.actionfile);

            //結合後の処理リスト初期化
            actions.Clear();

            //1行ごとに処理
            foreach (string file in str.Replace("\r\n", "\n").Split('\n'))
            {
                try
                {
                    string ext = Path.GetExtension(file).ToLower();

                    //.exeまたは.batの場合
                    if (ext == ".exe" || ext == ".bat")
                    {
                        actions.Add(file);
                    }
                }
                catch (Exception)
                {
                    ;
                }
            }
        }

        //処理設定ファイル(.ini)書き込み
        private void writeActionFile()
        {
            string str = String.Join(Environment.NewLine, actions.ToArray());
            TextFile.Write(AppInfo.actionfile, str);
        }

        ///////////////////////////////////////////////////////////////////////// 現在の設定を保存

        private void button_save_Click(object sender, EventArgs e)
        {
            //出力ファイル名の中にファイルに使えない文字が入っていない場合
            if (isCorrectFilename(textBox_outputfile.Text))
            {
                IniFile iniFile = new TextLib.IniFile();

                //TopMost
                iniFile.SetKeyValueBool("output", "topmost", checkBox_topmost.Checked);

                //出力先フォルダのIndex
                if (comboBox_folder.SelectedIndex >= 0)
                {
                    iniFile.SetKeyValueInt("output", "folder", comboBox_folder.SelectedIndex);
                }

                //出力ファイル名
                iniFile.SetKeyValueString("output", "filename", this.textBox_outputfile.Text);

                //結合後の処理のIndex
                if (comboBox_action.SelectedIndex >= 0)
                {
                    iniFile.SetKeyValueInt("output", "action", comboBox_action.SelectedIndex);
                }

                MessageBox.Show("保存しました。");
            }
            else
            {
                MessageBox.Show("ファイル名に使えない文字が含まれています。", "出力ファイル名エラー");
            }
        }

        ///////////////////////////////////////////////////////////////////////// TopMost

        private void checkBox_topmost_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = checkBox_topmost.Checked;
        }

        ///////////////////////////////////////////////////////////////////////// 結合処理

        //キャンセル
        private void button_cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        //結合
        private void button_concat_Click(object sender, EventArgs e)
        {
            //出力先フォルダの設定
            string folder;
            if (comboBox_folder.SelectedIndex >= 1)//出力先フォルダが選択されている場合
            {
                //Comboboxの値を取得
                folder = comboBox_folder.SelectedItem.ToString();

                //最後に\をつける
                if (!folder.EndsWith(@"\"))
                {
                    folder += @"\";
                }
            }
            else//出力先フォルダが選択されていない場合は結合ファイルの先頭のファイルのあるフォルダとする
            {
                folder = files[0].folder + @"\";
            }

            //出力先フォルダが存在するかどうか
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("出力先フォルダが存在しません", "確認");
                return;
            }

            //出力ファイル名の中にファイルに使えない文字が入っていないか確認
            string filename = textBox_outputfile.Text;
            if (filename == "" || !isCorrectFilename(filename))
            {
                MessageBox.Show("出力ファイル名を設定してください。", "確認");
                return;
            }

            //出力ファイルパス
            string outputfile = folder + filename + ".pdf";

            //上書き確認
            if (File.Exists(outputfile))
            {
                DialogResult result = MessageBox.Show("ファイルが存在します。上書きしますか？", "確認", MessageBoxButtons.YesNoCancel);

                //はい以外は中止
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            //ListBoxのファイルのうち存在するファイルを確認
            List<string> paths = new List<string>();
            foreach (var file in files)
            {
                if (file.isExist)
                {
                    paths.Add(file.path);
                }
            }

            if (paths.Count >= 1)
            {
                //結合後の処理を決定
                string action = "";//何もしない
                int index = comboBox_action.SelectedIndex;//現在選択されている結合後の処理
                if (index == 1)//1個目の場合はデフォルトで開く
                {
                    action = "default";
                }
                else if (index >= 2)//それ以外は実行ファイルで開く
                {
                    action = actions[index - 2];
                }

                //結合処理実施
                new ClassNekotadon_pdf_rclick().pdf_concat(paths, outputfile, action);

                //閉じる
                Close();
            }
        }
    }
}