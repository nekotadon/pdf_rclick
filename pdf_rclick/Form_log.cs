using System.Windows.Forms;

namespace pdf_rclick
{
    public partial class Form_log : Form
    {
        public bool interruption { get; set; }//中断
        public bool finished { get; set; }//処理が完了しているか
        public Form_log()
        {
            InitializeComponent();

            //フォント設定
            textBox_log.Font = SystemInformation.MenuFont;

            //初期化
            interruption = false;//中断なし
            finished = false;//処理未完了

            Icon = Properties.Resources.appico;
        }

        //ログ出力
        public void Message(string text)
        {
            //処理中断中でなければログ出力
            if (!interruption)
            {
                textBox_log.AppendText(text);
                textBox_log.AppendText(System.Environment.NewLine);
            }
        }

        //処理中断確認
        private void Form_log_FormClosing(object sender, FormClosingEventArgs e)
        {
            //処理中の場合
            if (!finished)
            {
                //中断確認
                if (MessageBox.Show("処理を中断しますか？", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    interruption = true;
                    textBox_log.AppendText("中断しています...");
                    textBox_log.AppendText(System.Environment.NewLine);
                }

                //処理終了で自動で終了するのでフォームを閉じるのをキャンセル
                e.Cancel = true;
            }
        }
    }
}
