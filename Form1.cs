using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_and_Video_Processor
{
    public partial class Form1 : Form
    {
        private Bitmap b;
        private DeviceManager deviceManager;
        private Device[] devices;
        private Device currentDevice;
        private Bitmap backgroundImage = null;
        private enum FilterType { None, Grayscale, Invert, Sepia, Subtract }
        private FilterType lastFilter = FilterType.None;
        private Timer timer1;


        public Form1()
        {
            InitializeComponent();
            LoadDevices();
            timer1 = new Timer();
            timer1.Interval = 100;
            timer1.Tick += timer1_Tick;
        }
        private void LoadDevices()
        {
            devices = DeviceManager.GetAllDevices();
            if (devices.Length > 0)
            {
                currentDevice = devices[0];
            }
            else
            {
                MessageBox.Show("No camera devices found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // FILE IMAGE PROCESSES
        private void filseStrip_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                b = new Bitmap(ofd.FileName);
                pictureBox1.Image = b;
            }
        }
        private void copyImage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                pictureBox2.Image = b;
            }
        }
        private void greyscaleImage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Color pixel;
                int height = b.Height;
                int width = b.Width;
                Bitmap copy = new Bitmap(width, height);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        pixel = b.GetPixel(x, y);
                        int gray = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                        Color bit = Color.FromArgb(gray, gray, gray);
                        copy.SetPixel(x, y, bit);
                    }
                }
                pictureBox2.Image = copy;
            }
        }
        private void invertImage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Color pixel;
                int height = b.Height;
                int width = b.Width;
                Bitmap copy = new Bitmap(width, height);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        pixel = b.GetPixel(x, y);
                        Color bit = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                        copy.SetPixel(x, y, bit);
                    }
                }
                pictureBox2.Image = copy;
            }
        }

        private void sepiaImage_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Color pixel;
                int height = b.Height;
                int width = b.Width;
                Bitmap copy = new Bitmap(width, height);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        pixel = b.GetPixel(x, y);
                        int RED = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                        int GREEN = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                        int BLUE = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);
                        if (RED > 255) RED = 255;
                        if (GREEN > 255) GREEN = 255;
                        if (BLUE > 255) BLUE = 255;
                        Color bit = Color.FromArgb(RED, GREEN, BLUE);
                        copy.SetPixel(x, y, bit);
                    }
                }
                pictureBox2.Image = copy;
            }
        }
        private void histogramImage_Click(object sender, EventArgs e)
        {
            int[] counts = new int[256];
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                int height = b.Height;
                int width = b.Width;
                for (int y = 0; y < b.Height; y++)
                {
                    for (int x = 0; x < b.Width; x++)
                    {
                        Color pixel = b.GetPixel(x, y);
                        int gray = (int)(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                        counts[gray]++;
                    }
                }
            }
            drawHistogram(counts);
        }
        private void imageSubtractionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No file loaded", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap fg = new Bitmap(pictureBox1.Image);
            int height = fg.Height;
            int width = fg.Width;

            Bitmap bg = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                bg = new Bitmap(ofd.FileName);
                if (bg.Width != fg.Width || bg.Height != fg.Height)
                {
                    Bitmap resized = new Bitmap(fg.Width, fg.Height);
                    using (Graphics g = Graphics.FromImage(resized))
                    {
                        g.DrawImage(bg, 0, 0, fg.Width, fg.Height);
                    }
                    bg = resized;
                }
            }
            else
            {
                return;
            }

            Bitmap result = new Bitmap(width, height);

            int threshold = 100;
            double factor = 1.2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = fg.GetPixel(x, y);

                    if (pixel.G > threshold && pixel.G > pixel.R * factor && pixel.G > pixel.B * factor)
                    {
                        result.SetPixel(x, y, bg.GetPixel(x, y));
                    }
                    else
                    {
                        result.SetPixel(x, y, pixel);
                    }
                }
            }

            pictureBox2.Image = result;
        }


        private void drawHistogram(int[] counts)
        {
            int max = counts.Max();
            Bitmap histogram = new Bitmap(256, 100);
            using (Graphics g = Graphics.FromImage(histogram))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int height = (int)(counts[i] * 100.0 / max);
                    g.DrawLine(Pens.Black, new Point(i, 100), new Point(i, 100 - height));
                }
            }
            pictureBox2.Image = histogram;
        }

        // Video Functions

        private void startCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                currentDevice.ShowWindow(pictureBox1);
            }
        }
        private void stopCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                currentDevice.Stop();
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Make sure the camera is running and pictureBox1 has a frame
            if (currentDevice != null && pictureBox1.Image != null)
            {
                Bitmap frame = new Bitmap(pictureBox1.Image); // clone current frame
                Bitmap processed = null;

                // Apply selected filter
                switch (lastFilter)
                {
                    case FilterType.Grayscale:
                        processed = ApplyGrayscale(frame);
                        break;
                    case FilterType.None:
                        processed = (Bitmap)frame.Clone();
                        break;
                    case FilterType.Invert:
                        processed = ApplyInversion(frame);
                        break;
                    case FilterType.Sepia:
                        processed = ApplySepia(frame);
                        break;
                    case FilterType.Subtract:
                        processed = ApplySubtract(frame);
                        break;
                }

                // Display processed frame
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = processed;
                frame.Dispose();
            }
        }
        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                lastFilter = FilterType.None;
                timer1.Start();
            }
        }
        private void greyscaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                lastFilter = FilterType.Grayscale;
                timer1.Start();
            }
        }
        private void colorInversionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                lastFilter = FilterType.Invert;
                timer1.Start();
            }
        }
        private void sepiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                lastFilter = FilterType.Sepia;
                timer1.Start();
            }
        }

        private void imageSubtractionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentDevice != null)
            {
                lastFilter = FilterType.Subtract;
                timer1.Start();
            }
        }
        private Bitmap ApplyGrayscale(Bitmap source)
        {
            Bitmap copy = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData dstData = copy.LockBits(
                new Rectangle(0, 0, copy.Width, copy.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                int stride = srcData.Stride;

                for (int y = 0; y < source.Height; y++)
                {
                    byte* srcRow = srcPtr + (y * stride);
                    byte* dstRow = dstPtr + (y * stride);

                    for (int x = 0; x < source.Width; x++)
                    {
                        byte b = srcRow[x * 3 + 0];
                        byte g = srcRow[x * 3 + 1];
                        byte r = srcRow[x * 3 + 2];

                        int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                        if (gray > 255) gray = 255;
                        if (gray < 0) gray = 0;

                        dstRow[x * 3 + 0] = (byte)gray;
                        dstRow[x * 3 + 1] = (byte)gray;
                        dstRow[x * 3 + 2] = (byte)gray;
                    }
                }
            }

            source.UnlockBits(srcData);
            copy.UnlockBits(dstData);

            return copy;
        }
        private Bitmap ApplyInversion(Bitmap source)
        {
            Bitmap copy = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData dstData = copy.LockBits(
                new Rectangle(0, 0, copy.Width, copy.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                int stride = srcData.Stride;

                for (int y = 0; y < source.Height; y++)
                {
                    byte* srcRow = srcPtr + (y * stride);
                    byte* dstRow = dstPtr + (y * stride);

                    for (int x = 0; x < source.Width; x++)
                    {
                        byte b = srcRow[x * 3 + 0];
                        byte g = srcRow[x * 3 + 1];
                        byte r = srcRow[x * 3 + 2];

                        dstRow[x * 3 + 0] = (byte)(255 - b);
                        dstRow[x * 3 + 1] = (byte)(255 - g);
                        dstRow[x * 3 + 2] = (byte)(255 - r);
                    }
                }
            }

            source.UnlockBits(srcData);
            copy.UnlockBits(dstData);

            return copy;
        }

        private Bitmap ApplySepia(Bitmap source)
        {
            Bitmap copy = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            System.Drawing.Imaging.BitmapData dstData = copy.LockBits(
                new Rectangle(0, 0, copy.Width, copy.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* dstPtr = (byte*)dstData.Scan0;

                int stride = srcData.Stride;

                for (int y = 0; y < source.Height; y++)
                {
                    byte* srcRow = srcPtr + (y * stride);
                    byte* dstRow = dstPtr + (y * stride);

                    for (int x = 0; x < source.Width; x++)
                    {
                        byte b = srcRow[x * 3 + 0];
                        byte g = srcRow[x * 3 + 1];
                        byte r = srcRow[x * 3 + 2];

                        int tr = (int)(0.393 * r + 0.769 * g + 0.189 * b);
                        int tg = (int)(0.349 * r + 0.686 * g + 0.168 * b);
                        int tb = (int)(0.272 * r + 0.534 * g + 0.131 * b);
                        if (tr > 255) tr = 255;
                        if (tg > 255) tg = 255;
                        if (tb > 255) tb = 255;

                        dstRow[x * 3 + 0] = (byte)tb;
                        dstRow[x * 3 + 1] = (byte)tg;
                        dstRow[x * 3 + 2] = (byte)tr;
                    }
                }
            }

            source.UnlockBits(srcData);
            copy.UnlockBits(dstData);

            return copy;
        }
        private Bitmap ApplySubtract(Bitmap source)
        {
            // If background not loaded, ask once
            if (backgroundImage == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bg = new Bitmap(ofd.FileName);

                    // Resize if needed
                    if (bg.Width != source.Width || bg.Height != source.Height)
                    {
                        Bitmap resized = new Bitmap(source.Width, source.Height);
                        using (Graphics g = Graphics.FromImage(resized))
                        {
                            g.DrawImage(bg, 0, 0, source.Width, source.Height);
                        }
                        backgroundImage = resized;
                    }
                    else
                    {
                        backgroundImage = bg;
                    }
                }
                else
                {
                    // No background selected, just return original
                    return (Bitmap)source.Clone();
                }
            }

            Bitmap result = new Bitmap(source.Width, source.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int threshold = 100;
            double factor = 1.2;

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color srcPixel = source.GetPixel(x, y);
                    Color bgPixel = backgroundImage.GetPixel(x, y);

                    // If green is strong, replace with background pixel
                    if (srcPixel.G > threshold && srcPixel.G > srcPixel.R * factor && srcPixel.G > srcPixel.B * factor)
                    {
                        result.SetPixel(x, y, bgPixel);
                    }
                    else
                    {
                        result.SetPixel(x, y, srcPixel);
                    }
                }
            }

            return result;
        }


    }

}
