using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace MLMathImageProcess
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files|*.jpg;*.png;*.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(openFileDialog1.FileName);
                pictureBox1.Image = originalImage;
            }
        }
        Bitmap originalImage;

        private void btnGray_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show("Önce bir resim seç!");
                return;
            }

            Bitmap grayImage = new Bitmap(originalImage.Width, originalImage.Height);

            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixel = originalImage.GetPixel(x, y);

                    int gray = (int)((pixel.R + pixel.G + pixel.B) / 3);

                    Color grayColor = Color.FromArgb(gray, gray, gray);
                    grayImage.SetPixel(x, y, grayColor);
                }
            }

            pictureBox1.Image = grayImage;
        }

        private void btnEqualize_Click(object sender, EventArgs e)
        {

            if (originalImage == null)
            {
                MessageBox.Show("Önce bir resim seç!");
                return;
            }

            // 1) Önce griye çevir
            Bitmap gray = ToGrayscale(originalImage);

            // 2) Histogram çıkar
            int[] hist = new int[256];
            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    int val = gray.GetPixel(x, y).R; // gri olduğu için R=G=B
                    hist[val]++;
                }
            }

            // 3) CDF (kümülatif dağılım) hesapla
            int total = gray.Width * gray.Height;
            int[] cdf = new int[256];
            cdf[0] = hist[0];
            for (int i = 1; i < 256; i++)
                cdf[i] = cdf[i - 1] + hist[i];

            // 4) İlk sıfır olmayan CDF'yi bul (cdfMin)
            int cdfMin = 0;
            for (int i = 0; i < 256; i++)
            {
                if (cdf[i] != 0)
                {
                    cdfMin = cdf[i];
                    break;
                }
            }

            // 5) Yeni değerler için LUT oluştur
            byte[] lut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double mapped = ((double)(cdf[i] - cdfMin) / (total - cdfMin)) * 255.0;
                if (mapped < 0) mapped = 0;
                if (mapped > 255) mapped = 255;
                lut[i] = (byte)mapped;
            }

            // 6) LUT'u uygula
            Bitmap eq = new Bitmap(gray.Width, gray.Height);
            for (int y = 0; y < gray.Height; y++)
            {
                for (int x = 0; x < gray.Width; x++)
                {
                    int val = gray.GetPixel(x, y).R;
                    int newVal = lut[val];
                    eq.SetPixel(x, y, Color.FromArgb(newVal, newVal, newVal));
                }
            }

            pictureBox1.Image = eq;
        }
        private Bitmap ToGrayscale(Bitmap source)
        {
            Bitmap grayImage = new Bitmap(source.Width, source.Height);

            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    Color pixel = source.GetPixel(x, y);
                    int gray = (int)((pixel.R + pixel.G + pixel.B) / 3);
                    grayImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            return grayImage;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Kaydedilecek resim yok!");
                return;
            }

            saveFileDialog1.Filter = "PNG Image|*.png|JPG Image|*.jpg|Bitmap Image|*.bmp";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap img = new Bitmap(pictureBox1.Image);
                img.Save(saveFileDialog1.FileName);
                MessageBox.Show("Resim kaydedildi.");
            }
        }
    }
}
