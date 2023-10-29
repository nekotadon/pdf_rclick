using System.Windows.Forms;
using System.Drawing;

namespace pdf_rclick
{
    public class PreviewPanel : Panel
    {
        public string filepath { get; set; }//pdfファイルのフルパス
        private PictureBox[] pages = new PictureBox[] { null, null, null };//pdfの1~3ページ目表示用PictureBox
        public int PageCount//PictureBoxの数
        {
            get
            {
                return pages.Length;
            }
        }

        public PreviewPanel(string _filepath)
        {
            filepath = _filepath;

            //サイズ設定
            int width = (int)(210 * 1.15);
            int height = (int)(297 * 1.15);
            int margin = 3;

            //Panelのプロパティ設定
            AutoScroll = true;
            Location = new Point(14, 30);
            Size = new Size(width + margin * 2 + 30, height + margin * 2);
            Visible = false;

            //PictureBoxの初期化
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i] = new PictureBox
                {
                    Location = new Point(margin, margin + (height + margin) * i),
                    Name = "page1",
                    Size = new Size(width, height),
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                setLoading(i);
                Controls.Add(pages[i]);
            }
        }

        //指定画像を設定
        public void setImage(Image newImage, int page)
        {
            if (0 <= page && page < pages.Length)
            {
                if (newImage != null)
                {
                    var oldImg = pages[page].Image;
                    pages[page].Image = newImage;
                    if (oldImg != null)
                    {
                        oldImg.Dispose();
                    }
                }
            }
        }

        //読み込み中画像を設定
        private void setLoading(int page)
        {
            setImage(Properties.Resources.loading, page);
        }

        //ページなし画像を設定
        public void setNoPage(int page)
        {
            setImage(Properties.Resources.nopage, page);
        }

        //Dispose
        public void allPictureDispose()
        {
            foreach (var page in pages)
            {
                if (page.Image != null)
                {
                    page.Image.Dispose();
                }
            }
            filepath = "";
        }
    }
}
