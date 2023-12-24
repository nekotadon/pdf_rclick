/*
 * This software includes the work that is distributed in the Apache License 2.0  
 * This software includes the work that is distributed in the MIT License  
 */

using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

//sharpshell 2.7.2
//MIT license
//PM> NuGet\Install-Package SharpShell -Version 2.7.2
//構成マネージャーでRelease/x64にする
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//PdfSharp 1.50.5147
//MIT License
//PM> NuGet\Install-Package PdfSharp -Version 1.50.5147
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

//PdfiumViewer 2.13.0
//PdfiumViewer.Native.x86_64.v8 - xfa 2018.4.8.256
//Apache-2.0
//PM> NuGet\Install-Package PdfiumViewer -Version 2.13.0
//PM> NuGet\Install-Package PdfiumViewer.Native.x86_64.v8-xfa -Version 2018.4.8.256

//snkファイルをこのファイルと同じフォルダに入れる

namespace pdf_rclick
{
    [ComVisible(true)]
    [Guid("AE4E3899-1035-4D61-BA2C-81A392E6A41F")]
    //pdfファイルが関連付けされていない場合、機能が有効にならないので全ファイル対象とする。
    //そのうえでコードの中で選択ファイルが全てpdfかどうかを判定するようにしている。
    //[COMServerAssociation(AssociationType.ClassOfExtension, ".pdf")]
    [COMServerAssociation(AssociationType.AllFilesAndFolders)]
    //他のSharpShellを使用するソフトウェアとclass名が重複していると正常に機能しない
    public class NekotadonPdfRclickExtension : SharpContextMenu
    {
        int dpiSetting = 150;//画像保存時の解像度初期値
        int[] dpis = { 36, 72, 150, 200, 360, 640 };
        System.Text.Encoding encoding = null;//テキスト保存時の文字コード
        List<System.Text.Encoding> encodings = new List<System.Text.Encoding>();
        string contextName = "PDF処理(&A)";//コンテキストメニュー名

        protected override bool CanShowMenu()
        {
            //全てpdfファイルかどうか確認
            bool pdfonly = true;
            try
            {
                foreach (var filePath in SelectedItemPaths)
                {
                    if (Directory.Exists(filePath))//フォルダが選択されている場合
                    {
                        pdfonly = false;
                        break;
                    }
                    else if (File.Exists(filePath))
                    {
                        if (Path.GetExtension(filePath).ToLower() != ".pdf")//pdfファイル以外
                        {
                            pdfonly = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                pdfonly = false;
            }

            //文字コード
            encodings = new List<System.Text.Encoding>
            {
                TextLib.EncodeLib.UTF8,//0
                TextLib.EncodeLib.UTF8withBOM,//1
                TextLib.EncodeLib.SJIS//2
            };

            try
            {
                //設定ファイル読み込み
                TextLib.IniFile iniFile = new TextLib.IniFile();
                dpiSetting = iniFile.GetKeyValueInt("setting", "dpi", 150, dpis.Min(), dpis.Max(), true);
                contextName = iniFile.GetKeyValueStringWithoutEmpty("setting", "name", "PDF処理(&A)", true);
                int encodingNum = iniFile.GetKeyValueInt("setting", "encode", 0, 0, encodings.Count - 1, true);
                encoding = encodings[encodingNum];
            }
            catch (Exception)
            {

            }

            return pdfonly;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            //メニュー
            ToolStripMenuItem mainMenu = new ToolStripMenuItem { Text = contextName };

            //結合
            if (SelectedItemPaths.Count() != 1)//選択ファイルが1つの場合は結合メニューを表示しない
            {
                ToolStripMenuItem subMenu_concat = new ToolStripMenuItem { Text = "結合(&C)" };
                subMenu_concat.Click += (s, e) =>
                {
                    logOpen();

                    //選択されたファイルをlistにストック
                    List<string> paths = new List<string>();
                    foreach (string path in SelectedItemPaths)
                    {
                        paths.Add(path);
                    }

                    //ソート
                    StringSort.Sort(ref paths);

                    //結合
                    Task.Run(() => pdf_concat(paths));
                };
                mainMenu.DropDownItems.Add(subMenu_concat);
            }

            //結合ダイアログを開く
            ToolStripMenuItem subMenu_concat_dialog = new ToolStripMenuItem { Text = "結合ダイアログを開く(&D)" };
            subMenu_concat_dialog.Click += (s, e) => pdf_concat_dialog();
            mainMenu.DropDownItems.Add(subMenu_concat_dialog);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //分割
            ToolStripMenuItem subMenu_split = new ToolStripMenuItem { Text = "分割(&S)" };
            subMenu_split.Click += (s, e) => { logOpen(); Task.Run(() => pdf_split(false)); };
            mainMenu.DropDownItems.Add(subMenu_split);

            //サブフォルダに分割
            ToolStripMenuItem subMenu_split_subfolder = new ToolStripMenuItem { Text = "サブフォルダに分割" };
            subMenu_split_subfolder.Click += (s, e) => { logOpen(); Task.Run(() => pdf_split(true)); };
            mainMenu.DropDownItems.Add(subMenu_split_subfolder);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //90°回転
            ToolStripMenuItem subMenu_rotate90 = new ToolStripMenuItem { Text = "↷90°回転(&R)" };
            subMenu_rotate90.Click += (s, e) => { logOpen(); Task.Run(() => pdf_rotate(90)); };
            mainMenu.DropDownItems.Add(subMenu_rotate90);

            //-90°回転
            ToolStripMenuItem subMenu_rotate270 = new ToolStripMenuItem { Text = "↶90°回転(&L)" };
            subMenu_rotate270.Click += (s, e) => { logOpen(); Task.Run(() => pdf_rotate(-90)); };
            mainMenu.DropDownItems.Add(subMenu_rotate270);

            //180°回転
            ToolStripMenuItem subMenu_rotate180 = new ToolStripMenuItem { Text = "180°回転(&I)" };
            subMenu_rotate180.Click += (s, e) => { logOpen(); Task.Run(() => pdf_rotate(180)); };
            mainMenu.DropDownItems.Add(subMenu_rotate180);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //1ページを左右2分割
            ToolStripMenuItem subMenu_page_split_left_right = new ToolStripMenuItem { Text = "1ページを左右2分割" };
            subMenu_page_split_left_right.Click += (s, e) => { logOpen(); Task.Run(() => pdf_page_split_left_right()); };
            mainMenu.DropDownItems.Add(subMenu_page_split_left_right);

            //1ページを上下2分割
            ToolStripMenuItem subMenu_page_split_top_bottom = new ToolStripMenuItem { Text = "1ページを上下2分割" };
            subMenu_page_split_top_bottom.Click += (s, e) => { logOpen(); Task.Run(() => pdf_page_split_top_bottom()); };
            mainMenu.DropDownItems.Add(subMenu_page_split_top_bottom);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //ページレイアウト変更
            ToolStripMenuItem subMenu_layout = new ToolStripMenuItem { Text = "ページレイアウト変更(&L)" };

            ToolStripMenuItem subMenu_layout1 = new ToolStripMenuItem { Text = "連続 単一ページ" };
            ToolStripMenuItem subMenu_layout2 = new ToolStripMenuItem { Text = "連続 見開き（表紙なし）" };
            ToolStripMenuItem subMenu_layout3 = new ToolStripMenuItem { Text = "連続 見開き（表紙あり）" };
            ToolStripMenuItem subMenu_layout4 = new ToolStripMenuItem { Text = "非連続 単一ページ" };
            ToolStripMenuItem subMenu_layout5 = new ToolStripMenuItem { Text = "非連続 見開き（表紙なし）" };
            ToolStripMenuItem subMenu_layout6 = new ToolStripMenuItem { Text = "非連続 見開き（表紙あり）" };

            subMenu_layout1.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(1)); };
            subMenu_layout2.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(2)); };
            subMenu_layout3.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(3)); };
            subMenu_layout4.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(4)); };
            subMenu_layout5.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(5)); };
            subMenu_layout6.Click += (s, e) => { logOpen(); Task.Run(() => pdf_layout(6)); };

            subMenu_layout.DropDownItems.Add(subMenu_layout1);
            subMenu_layout.DropDownItems.Add(subMenu_layout2);
            subMenu_layout.DropDownItems.Add(subMenu_layout3);
            subMenu_layout.DropDownItems.Add(new ToolStripSeparator());
            subMenu_layout.DropDownItems.Add(subMenu_layout4);
            subMenu_layout.DropDownItems.Add(subMenu_layout5);
            subMenu_layout.DropDownItems.Add(subMenu_layout6);

            mainMenu.DropDownItems.Add(subMenu_layout);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //先頭に白紙ページを挿入
            ToolStripMenuItem subMenu_insert_blank_page = new ToolStripMenuItem { Text = "先頭に白紙ページを挿入(&B)" };
            subMenu_insert_blank_page.Click += (s, e) => { logOpen(); Task.Run(() => pdf_insert_blank_page()); };
            mainMenu.DropDownItems.Add(subMenu_insert_blank_page);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //画像として保存
            ToolStripMenuItem subMenu_image = new ToolStripMenuItem { Text = "画像として保存(&G)" };
            subMenu_image.Click += (s, e) => { logOpen(); Task.Run(() => pdf_image(dpiSetting, false)); };
            mainMenu.DropDownItems.Add(subMenu_image);

            //画像としてサブフォルダに保存
            ToolStripMenuItem subMenu_image_subfolder = new ToolStripMenuItem { Text = "画像としてサブフォルダに保存" };
            subMenu_image_subfolder.Click += (s, e) => { logOpen(); Task.Run(() => pdf_image(dpiSetting, true)); };
            mainMenu.DropDownItems.Add(subMenu_image_subfolder);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //全テキストの抽出
            ToolStripMenuItem subMenu_all_texts = new ToolStripMenuItem { Text = "全テキストの抽出(&T)" };
            subMenu_all_texts.Click += (s, e) => { logOpen(); Task.Run(() => pdf_all_texts()); };
            mainMenu.DropDownItems.Add(subMenu_all_texts);

            //メニューに追加
            menu.Items.Add(mainMenu);

            //区切り
            mainMenu.DropDownItems.Add(new ToolStripSeparator());

            //その他
            ToolStripMenuItem subMenu_other = new ToolStripMenuItem { Text = "その他" };

            ToolStripMenuItem subMenu_setting = new ToolStripMenuItem { Text = "設定" };
            subMenu_other.DropDownItems.Add(subMenu_setting);
            TextLib.IniFile iniFile = new TextLib.IniFile();

            //画像保存時の解像度
            ToolStripMenuItem subMenu_setting_dpi = new ToolStripMenuItem { Text = "画像として保存時の解像度" };
            subMenu_setting.DropDownItems.Add(subMenu_setting_dpi);

            //画像保存時の解像度サブメニュー
            int dpiCurrent = iniFile.GetKeyValueInt("setting", "dpi", 150, dpis.Min(), dpis.Max(), true);

            bool dpiExist = false;
            foreach (var dpi in dpis)
            {
                if (!dpiExist && dpiCurrent == dpi)
                {
                    dpiExist = true;
                }
                ToolStripMenuItem subMenu = new ToolStripMenuItem { Text = dpi.ToString() + "dpi", Checked = dpiCurrent == dpi };
                subMenu.Click += (s, e) =>
                {
                    TextLib.IniFile iniFilebuf = new TextLib.IniFile();
                    iniFilebuf.SetKeyValueInt("setting", "dpi", dpi);
                    dpiSetting = dpi;
                };
                subMenu_setting_dpi.DropDownItems.Add(subMenu);
            }
            if (!dpiExist)
            {
                ToolStripMenuItem subMenu_dpi_custom = new ToolStripMenuItem { Text = "カスタム", Checked = true };
                subMenu_setting_dpi.DropDownItems.Add(subMenu_dpi_custom);
            }

            //テキスト保存時の文字コード
            ToolStripMenuItem subMenu_setting_code = new ToolStripMenuItem { Text = "テキスト保存時の文字コード" };
            subMenu_setting.DropDownItems.Add(subMenu_setting_code);

            //テキスト保存時の文字コードサブメニュー
            ToolStripMenuItem subMenu_utf8 = new ToolStripMenuItem { Text = "UTF8" };
            ToolStripMenuItem subMenu_utf8_with_bom = new ToolStripMenuItem { Text = "UTF8 with BOM" };
            ToolStripMenuItem subMenu_sjis = new ToolStripMenuItem { Text = "Shift-JIS" };

            subMenu_setting_code.DropDownItems.Add(subMenu_utf8);
            subMenu_setting_code.DropDownItems.Add(subMenu_utf8_with_bom);
            subMenu_setting_code.DropDownItems.Add(subMenu_sjis);

            int encodingNum = iniFile.GetKeyValueInt("setting", "encode", 0, 0, encodings.Count - 1, true);
            subMenu_utf8.Checked = encodingNum == 0;
            subMenu_utf8_with_bom.Checked = encodingNum == 1;
            subMenu_sjis.Checked = encodingNum == 2;

            Action<object, int> action_encode = (s, num) =>
            {
                TextLib.IniFile iniFilebuf = new TextLib.IniFile();
                if (num < 0 || encodings.Count <= num)
                {
                    num = 0;
                }
                iniFilebuf.SetKeyValueInt("setting", "encode", num);
                encoding = encodings[num];
            };

            subMenu_utf8.Click += (s, e) => action_encode(s, 0);
            subMenu_utf8_with_bom.Click += (s, e) => action_encode(s, 1);
            subMenu_sjis.Click += (s, e) => action_encode(s, 2);

            //結合ダイアログでのプレビュー
            ToolStripMenuItem subMenu_setting_preview = new ToolStripMenuItem { Text = "結合ダイアログでのプレビュー" };
            subMenu_setting.DropDownItems.Add(subMenu_setting_preview);

            //結合ダイアログでのプレビューサブメニュー
            int currentPreviewSize = iniFile.GetKeyValueInt("setting", "previewSize", 2, 0, 1000, true);
            int[] previewSizes = { 0, 1, 2, 5, 10, 20, 50, 1000 };

            bool previewSizeExist = false;
            foreach (var previewSize in previewSizes)
            {
                if (!previewSizeExist && currentPreviewSize == previewSize)
                {
                    previewSizeExist = true;
                }
                ToolStripMenuItem subMenu = new ToolStripMenuItem { Text = previewSize.ToString() + "MB以下のファイルのみ", Checked = currentPreviewSize == previewSize };
                if (previewSize == 0)
                {
                    subMenu.Text = "プレビューしない";
                }

                subMenu.Click += (s, e) =>
                {
                    TextLib.IniFile iniFilebuf = new TextLib.IniFile();
                    iniFilebuf.SetKeyValueInt("setting", "previewSize", previewSize);
                };
                subMenu_setting_preview.DropDownItems.Add(subMenu);
            }
            if (currentPreviewSize != 0 && !previewSizeExist)
            {
                ToolStripMenuItem subMenu_preview_custom = new ToolStripMenuItem { Text = "カスタム", Checked = true };
                subMenu_setting_preview.DropDownItems.Add(subMenu_preview_custom);
            }

            //区切り
            subMenu_other.DropDownItems.Add(new ToolStripSeparator());

            //バージョン情報
            ToolStripMenuItem subMenu_version = new ToolStripMenuItem { Text = "バージョン情報" };
            subMenu_other.DropDownItems.Add(subMenu_version);
            subMenu_version.Click += (s, e) =>
            {
                MessageBox.Show("pdf_rclick ver."
                 + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
                 + Environment.NewLine
                 + Environment.NewLine
                 + "Copyright (c) 2023 nekotadon", "バージョン情報");
            };

            mainMenu.DropDownItems.Add(subMenu_other);
            return menu;
        }

        ///結合
        public void pdf_concat(List<string> filelists, string outputfilepath = "", string exe = "")
        {
            logOpen();
            Task.Run(() => pdf_concat_action(filelists, outputfilepath, exe));
        }

        //結合の実処理
        private void pdf_concat_action(List<string> filelists, string outputfilepath, string exe)
        {
            try
            {
                logMessage("結合処理の開始");

                //結合後の出力ファイル
                string outputfile = "";
                var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                bool first = true;
                foreach (string file in filelists)
                {
                    //ファイル1個目の場合は出力ファイルパスを設定
                    if (first)
                    {
                        first = false;
                        //出力ファイルの指定がない場合は1個目と同じフォルダ、指定ある場合は指示に従う
                        outputfile = outputfilepath == "" ? Path.GetDirectoryName(file) + @"\concat.pdf" : outputfilepath;
                    }

                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //1ページずつ個別に保存
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                logMessage((i + 1).ToString() + "/" + num.ToString() + "ページ目を出力処理中...");

                                outputPdfDocument.AddPage(inputPdfDocument.Pages[i]);
                            }
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }

                        //ファイルを閉じる
                        inputPdfDocument.Close();
                        inputPdfDocument.Dispose();
                    }

                    logMessage("処理終了");
                }

                //出力
                if (isContinued() && outputPdfDocument.PageCount != 0)
                {
                    outputPdfDocument.Save(outputfile);
                    if (!global_error && exe != "" && File.Exists(outputfile))
                    {
                        if (exe == "default")
                        {
                            Process.Start(outputfile);
                        }
                        else if (File.Exists(exe))
                        {
                            Process p = new Process();
                            p.StartInfo.FileName = exe;
                            p.StartInfo.Arguments = outputfile;
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(outputfile);
                            p.Start();
                        }
                    }
                }
                outputPdfDocument.Dispose();
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //結合ダイアログを開く
        Form_concat form_concat = null;
        private void pdf_concat_dialog()
        {
            //ソート
            List<string> filelists = new List<string>();
            foreach (string file in SelectedItemPaths)
            {
                filelists.Add(file);
            }
            StringSort.Sort(ref filelists);

            //ログの表示
            if (form_concat == null || form_concat.IsDisposed)
            {
                form_concat = new Form_concat(filelists);
                form_concat.Show();
            }
        }

        //分割
        private void pdf_split(bool subfolder)
        {
            try
            {
                logMessage("分割処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //サブフォルダ作成
                        string outputdir = folder + @"\" + filename + "_";
                        if (subfolder)
                        {
                            outputdir = folder + @"\" + filename + @"_pdf\";
                            if (!Directory.Exists(outputdir) && isContinued())
                            {
                                try
                                {
                                    Directory.CreateDirectory(outputdir);
                                }
                                catch (Exception)
                                {
                                    logError("サブフォルダが作成できませんでした。");

                                    //ファイルを閉じる
                                    inputPdfDocument.Close();
                                    inputPdfDocument.Dispose();
                                    continue;
                                }
                            }
                        }

                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //0の個数
                            int kazu = Math.Max((int)Math.Log10(num) + 1, 5);

                            //1ページずつ個別に保存
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                logMessage((i + 1).ToString() + "/" + num.ToString() + "ページ目を出力処理中...");

                                //出力するファイル
                                var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                                //ページ追加
                                outputPdfDocument.AddPage(inputPdfDocument.Pages[i]);

                                //保存
                                outputPdfDocument.Save(outputdir + (i + 1).ToString().PadLeft(kazu, '0') + ".pdf");
                                outputPdfDocument.Dispose();
                            }
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }

                        //ファイルを閉じる
                        inputPdfDocument.Close();
                        inputPdfDocument.Dispose();
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //回転
        private void pdf_rotate(int deg)
        {
            try
            {
                logMessage("回転処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + "_" + deg.ToString() + "deg.pdf";

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //出力ファイル
                            var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                logMessage((i + 1).ToString() + "/" + num.ToString() + "ページ目を回転処理中...");

                                PdfPage page = inputPdfDocument.Pages[i];
                                int deg_next;

                                //角度設定
                                deg_next = (page.Rotate + deg) % 360;
                                deg_next += deg_next < 0 ? 360 : 0;
                                page.Rotate = deg_next;

                                outputPdfDocument.AddPage(page);
                            }
                            if (isContinued())
                            {
                                outputPdfDocument.Save(outputfile);
                            }
                            outputPdfDocument.Dispose();
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }

                        //ファイルを閉じる
                        inputPdfDocument.Close();
                        inputPdfDocument.Dispose();
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //1ページを左右2分割
        private void pdf_page_split_left_right()
        {
            try
            {
                logMessage("左右2分割処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + "_left_right.pdf";

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //出力ファイル
                            var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                            //元ファイルのページを出力ファイルへコピー
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                PdfPage page1 = inputPdfDocument.Pages[i];

                                //同じページを2回追加
                                outputPdfDocument.AddPage(page1);
                                outputPdfDocument.AddPage(page1);
                            }

                            //元ファイルのレイアウト確認
                            if (inputPdfDocument.PageLayout == PdfPageLayout.OneColumn ||
                                inputPdfDocument.PageLayout == PdfPageLayout.TwoColumnLeft ||
                                inputPdfDocument.PageLayout == PdfPageLayout.TwoColumnRight)
                            {
                                //連続
                                outputPdfDocument.PageLayout = PdfPageLayout.TwoColumnLeft;
                            }
                            else
                            {
                                //非連続
                                outputPdfDocument.PageLayout = PdfPageLayout.TwoPageLeft;
                            }

                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            //必要箇所のみ抜粋 PDFは左下が座標(0,0)
                            for (int i = 0; i < outputPdfDocument.PageCount; i++)
                            {
                                if (!isContinued()) break;

                                if (i % 2 == 0)
                                {
                                    logMessage((i / 2 + 1).ToString() + "/" + num.ToString() + "ページ目を処理中...");
                                }

                                PdfPage page = outputPdfDocument.Pages[i];

                                //回転角度確認
                                int deg = page.Rotate % 360;
                                deg += deg < 0 ? 360 : 0;

                                if (deg == 90 || deg == 270)
                                {
                                    if (i % 2 == 0)//奇数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(0, deg == 90 ? 0 : page.Height / 2, page.Width, page.Height / 2));
                                    }
                                    else//偶数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(0, deg == 90 ? page.Height / 2 : 0, page.Width, page.Height / 2));
                                    }
                                }
                                else
                                {
                                    if (i % 2 == 0)//奇数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(deg == 180 ? page.Width / 2 : 0, 0, page.Width / 2, page.Height));
                                    }
                                    else//偶数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(deg == 180 ? 0 : page.Width / 2, 0, page.Width / 2, page.Height));
                                    }
                                }
                            }

                            //保存
                            if (isContinued())
                            {
                                outputPdfDocument.Save(outputfile);
                            }
                            outputPdfDocument.Dispose();
                        }
                        else
                        {
                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            logError("ページ数が0です");
                        }
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }
        //1ページを上下2分割
        private void pdf_page_split_top_bottom()
        {
            try
            {
                logMessage("上下2分割処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + "_top_bottom.pdf";

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //出力ファイル
                            var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                            //元ファイルのページを出力ファイルへコピー
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;

                                PdfPage page1 = inputPdfDocument.Pages[i];

                                //同じページを2回追加
                                outputPdfDocument.AddPage(page1);
                                outputPdfDocument.AddPage(page1);
                            }

                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            //必要箇所のみ抜粋 PDFは左下が座標(0,0)
                            for (int i = 0; i < outputPdfDocument.PageCount; i++)
                            {
                                if (!isContinued()) break;

                                if (i % 2 == 0)
                                {
                                    logMessage((i / 2 + 1).ToString() + "/" + num.ToString() + "ページ目を処理中...");
                                }

                                PdfPage page = outputPdfDocument.Pages[i];

                                //回転角度確認
                                int deg = page.Rotate % 360;
                                deg += deg < 0 ? 360 : 0;

                                if (deg == 90 || deg == 270)
                                {
                                    if (i % 2 == 0)//奇数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(deg == 90 ? 0 : page.Width / 2, 0, page.Width / 2, page.Height));
                                    }
                                    else//偶数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(deg == 90 ? page.Width / 2 : 0, 0, page.Width / 2, page.Height));
                                    }
                                }
                                else
                                {
                                    if (i % 2 == 0)//奇数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(0, deg == 180 ? 0 : page.Height / 2, page.Width, page.Height / 2));
                                    }
                                    else//偶数ページ
                                    {
                                        page.MediaBox = new PdfSharp.Pdf.PdfRectangle(new PdfSharp.Drawing.XRect(0, deg == 180 ? page.Height / 2 : 0, page.Width, page.Height / 2));
                                    }
                                }
                            }

                            //保存
                            if (isContinued())
                            {
                                outputPdfDocument.Save(outputfile);
                            }
                            outputPdfDocument.Dispose();
                        }
                        else
                        {
                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            logError("ページ数が0です");
                        }
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //レイアウト変更
        private void pdf_layout(int layout_num)
        {
            try
            {
                logMessage("レイアウト変更処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + "_layout_";

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //出力ファイルを作成し元ファイルをコピー
                            var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                outputPdfDocument.AddPage(inputPdfDocument.Pages[i]);
                            }
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            if (1 <= layout_num && layout_num <= 6)
                            {
                                string addname = "";
                                if (layout_num == 1)
                                {
                                    //連続、単一ページとして保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.OneColumn;
                                    addname = "continuous_one.pdf";
                                }
                                else if (layout_num == 2)
                                {
                                    //連続、見開きとして保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.TwoColumnLeft;
                                    addname = "continuous_two.pdf";
                                }
                                else if (layout_num == 3)
                                {
                                    //連続、見開き(1ページ目表紙)として保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.TwoColumnRight;
                                    addname = "continuous_two_cover.pdf";
                                }
                                else if (layout_num == 4)
                                {
                                    //非連続、単一ページとして保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.SinglePage;
                                    addname = "non_continuous_one.pdf";
                                }
                                else if (layout_num == 5)
                                {
                                    //非連続、見開きとして保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.TwoPageLeft;
                                    addname = "non_continuous_two.pdf";
                                }
                                else if (layout_num == 6)
                                {
                                    //非連続、見開き(1ページ目表紙)として保存
                                    outputPdfDocument.PageLayout = PdfPageLayout.TwoPageRight;
                                    addname = "non_continuous_two_cover.pdf";
                                }
                                if (isContinued())
                                {
                                    outputPdfDocument.Save(outputfile + addname);
                                }
                            }

                            outputPdfDocument?.Dispose();
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //先頭に白紙ページを挿入
        private void pdf_insert_blank_page()
        {
            try
            {
                logMessage("先頭への白紙ページ挿入処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + "_insert_blank_page.pdf";

                    //ファイルを開く
                    var inputPdfDocument = logPdfSharpFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //出力ファイル作成
                            var outputPdfDocument = new PdfSharp.Pdf.PdfDocument();

                            //白紙追加
                            logMessage("白紙PDFを作成中...");
                            outputPdfDocument.AddPage(new PdfPage());

                            if (inputPdfDocument.Pages[0].CropBox.Height >= 200 && inputPdfDocument.Pages[0].CropBox.Width >= 200)
                            {
                                //CropBoxのサイズがある場合はCropBoxのサイズで設定
                                outputPdfDocument.Pages[0].Width = getUint(inputPdfDocument.Pages[0].CropBox.Width);
                                outputPdfDocument.Pages[0].Height = getUint(inputPdfDocument.Pages[0].CropBox.Height);
                            }
                            else
                            {
                                //CropBoxのサイズがない場合はMediaBoxのサイズで設定
                                outputPdfDocument.Pages[0].Width = getUint(inputPdfDocument.Pages[0].MediaBox.Width);
                                outputPdfDocument.Pages[0].Height = getUint(inputPdfDocument.Pages[0].MediaBox.Height);
                            }

                            //元ファイル結合
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;
                                logMessage("元ファイルの" + (i + 1).ToString() + "/" + num.ToString() + "ページ目を追加中...");
                                outputPdfDocument.AddPage(inputPdfDocument.Pages[i]);
                            }

                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            //保存
                            if (isContinued())
                            {
                                outputPdfDocument.Save(outputfile);
                            }
                            outputPdfDocument.Dispose();
                        }
                        else
                        {
                            //元ファイルを閉じる
                            inputPdfDocument.Close();
                            inputPdfDocument.Dispose();

                            logError("ページ数が0です");
                        }
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //区切りのよい整数に変換
        private XUnit getUint(double x)
        {
            //用紙サイズ(72dpi)
            int[] sizes = { 420, 595, 842, 1191, 1684, 2384, 3370, 516, 729, 1032, 1460, 2064, 2920, 4127 };

            foreach (int size in sizes)
            {
                if (Math.Abs(x - size) <= 1.0)
                {
                    return new XUnit(size);
                }
            }

            return new XUnit(x);
        }

        //画像として保存
        private void pdf_image(int dpi, bool subfolder)
        {
            try
            {
                logMessage("画像化処理の開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);

                    //ファイルを開く
                    var inputPdfDocument = logPdfiumViewerFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //サブフォルダ作成
                        string outputdir = folder + @"\" + filename + "_";
                        if (subfolder)
                        {
                            outputdir = folder + @"\" + filename + @"_pdf\";
                            if (!Directory.Exists(outputdir) && isContinued())
                            {
                                try
                                {
                                    Directory.CreateDirectory(outputdir);
                                }
                                catch (Exception)
                                {
                                    logError("サブフォルダが作成できませんでした。");

                                    //ファイルを閉じる
                                    inputPdfDocument.Dispose();
                                    continue;
                                }
                            }
                        }

                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //0の個数
                            int kazu = Math.Max((int)Math.Log10(num) + 1, 5);

                            //1ページずつ個別に保存
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;

                                logMessage((i + 1).ToString() + "/" + num.ToString() + "ページ目を出力処理中...");

                                string outfile = outputdir + (i + 1).ToString().PadLeft(kazu, '0') + ".png";
                                int w = (int)inputPdfDocument.PageSizes[i].Width * dpi / 72;
                                int h = (int)inputPdfDocument.PageSizes[i].Height * dpi / 72;
                                using (System.Drawing.Image img = inputPdfDocument.Render(i, w, h, dpi, dpi, true))
                                {
                                    img.Save(outfile, System.Drawing.Imaging.ImageFormat.Png);
                                }
                            }
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }

                        //ファイルを閉じる
                        inputPdfDocument.Dispose();
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //全テキストの抽出
        private void pdf_all_texts()
        {
            try
            {
                logMessage("全テキストの抽出処理を開始");

                foreach (string file in SelectedItemPaths)
                {
                    //処理開始メッセージ＆ファイルの存在チェック
                    if (!logFileCheck(file)) continue;
                    if (!isContinued()) break;

                    //フォルダ、名前
                    string folder = Path.GetDirectoryName(file);
                    string filename = Path.GetFileNameWithoutExtension(file);
                    string outputfile = folder + @"\" + filename + ".txt";

                    //抽出したテキスト
                    var sb = new System.Text.StringBuilder();

                    //ファイルを開く
                    var inputPdfDocument = logPdfiumViewerFileOpen(file);

                    //ファイルが開ける場合の処理
                    if (inputPdfDocument != null)
                    {
                        //ページ数確認
                        int num = inputPdfDocument.PageCount;

                        if (num > 0)
                        {
                            //1ページずつ抽出
                            for (int i = 0; i < num; i++)
                            {
                                if (!isContinued()) break;

                                logMessage((i + 1).ToString() + "/" + num.ToString() + "ページ目を抽出中...");
                                try
                                {
                                    sb.Append(inputPdfDocument.GetPdfText(i));
                                }
                                catch (Exception)
                                {
                                    LogError((i + 1).ToString() + "/" + num.ToString() + "ページ目を抽出できませんでした...");
                                }
                            }
                        }
                        else
                        {
                            logError("ページ数が0です");
                        }

                        //ファイルを閉じる
                        inputPdfDocument.Dispose();
                    }

                    //ファイルに出力
                    StreamWriter sw = null;
                    try
                    {
                        if (isContinued())
                        {
                            //ファイルを作成
                            sw = new StreamWriter(outputfile, false, encoding ?? TextLib.EncodeLib.UTF8);
                            sw.Write(sb.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        logError(ex.Message);
                        logError("ファイル出力ができませんでした。");
                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Close();
                        }
                    }

                    logMessage("処理終了");
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }

            logClose();
        }

        //ログフォーム
        Form_log form_log = null;

        //処理中にエラーがあったかどうか
        bool global_error = false;

        //ログフォームを開く
        private void logOpen()
        {
            //初期化
            global_error = false;

            //ログの表示
            if (form_log == null || form_log.IsDisposed)
            {
                form_log = new Form_log();
                form_log.Show();
            }
        }

        //ファイルの存在確認を実施しログ出力
        private bool logFileCheck(string file)
        {
            logMessage(Environment.NewLine + file + "の処理を開始");

            if (!File.Exists(file))//ファイルが存在しない場合
            {
                logError("ファイルが存在しません。");
                return false;
            }
            else if (new FileInfo(file).Length == 0)//ファイルサイズ0の場合
            {
                logError("ファイルサイズが0です。");
                return false;
            }

            return true;
        }

        //ファイルオープン確認を実施しログ出力
        private PdfSharp.Pdf.PdfDocument logPdfSharpFileOpen(string file)
        {
            logMessage("ファイルを開いています...");

            PdfSharp.Pdf.PdfDocument inputPdfDocument;

            try
            {
                inputPdfDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                return inputPdfDocument;
            }
            catch (Exception ex)
            {
                logError(ex.Message);
                logError("ファイルが開けません。セキュリティ設定(編集権限)を確認してください。");
            }

            return null;
        }

        private PdfiumViewer.PdfDocument logPdfiumViewerFileOpen(string file)
        {
            logMessage("ファイルを開いています...");

            PdfiumViewer.PdfDocument inputPdfDocument;

            try
            {
                inputPdfDocument = PdfiumViewer.PdfDocument.Load(file);
                return inputPdfDocument;
            }
            catch (Exception ex)
            {
                logError(ex.Message);
                logError("ファイルが開けません。セキュリティ設定(編集権限)を確認してください。");
            }

            return null;
        }

        //ログを追加
        private void logMessage(string text)
        {
            if (form_log != null && !form_log.IsDisposed)
            {
                if (!form_log.interruption)
                {
                    form_log.Message(text);
                }
            }
        }

        //エラーログを追加
        private void logError(string text)
        {
            if (form_log != null && !form_log.IsDisposed)
            {
                global_error = true;

                if (!form_log.interruption)
                {
                    form_log.Message("エラー : " + text);
                }
            }
        }

        //ログフォームを閉じる
        private void logClose()
        {
            if (form_log != null && !form_log.IsDisposed)
            {
                //処理完了
                form_log.finished = true;

                if (!global_error || form_log.interruption)
                {
                    form_log.Close();
                    form_log.Dispose();
                }
                else
                {
                    form_log.Message("全ての処理終了。途中でエラーが発生しています。");
                }
            }
        }

        //処理を継続するか
        private bool isContinued()
        {
            if (form_log != null && !form_log.IsDisposed)
            {
                if (form_log.interruption)
                {
                    return false;
                }
            }

            return true;
        }
    }

    //ファイル名でソート
    public static class StringSort
    {
        internal static class NativeMethods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            internal static extern int StrCmpLogicalW(string str1, string str2);
        }

        private static int StringComparer(string s1, string s2)
        {
            try
            {
                string name1 = Path.GetFileNameWithoutExtension(s1);
                string name2 = Path.GetFileNameWithoutExtension(s2);

                return NativeMethods.StrCmpLogicalW(name1, name2);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static void Sort(ref List<string> lists)
        {
            lists.Sort(StringComparer);
        }
        public static void Sort(ref string[] lists)
        {
            List<string> list = new List<string>(lists);
            list.Sort(StringComparer);
            lists = list.ToArray();
        }
    }

    public static class AppInfo
    {
        public static string filepath => System.Reflection.Assembly.GetExecutingAssembly().Location;//dllのフルパス
        public static string folder => Path.GetDirectoryName(filepath);//dllのあるフォルダ
        public static string actionfile => folder + @"\action.ini";//処理設定ファイル(.ini)のフルパス
        public static string folderfile => folder + @"\folder.ini";//フォルダ設定ファイル(.ini)のフルパス
    }
}

