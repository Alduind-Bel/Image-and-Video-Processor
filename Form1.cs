using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace Image_and_Video_Processor
{
    public partial class Form1 : Form
    {
        private Bitmap imageBitmap;
        private Bitmap backgroundImage;
        private VideoCapture videoCapture;
        private CameraDevice[] devices;
        private Timer videoTimer;
        private bool videoSubtractionEnabled = false;
        private FilterType currentVideoFilter = FilterType.None; // Added

        private enum FilterType { None, Grayscale, Invert, Sepia, Subtract, Histogram }

        public Form1()
        {
            InitializeComponent();
            LoadDevices();

            videoTimer = new Timer();
            videoTimer.Interval = 33;
            videoTimer.Tick += VideoTimer_Tick;
        }

        #region ---------------- Device Manager ----------------

        private void LoadDevices()
        {
            devices = DeviceManager.GetAllDevices();
            if (devices.Length == 0)
                MessageBox.Show("No camera devices found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region ---------------- Image File Processing ----------------

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            StopCamera();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageBitmap = new Bitmap(ofd.FileName);
                pictureBox1.Image = imageBitmap;
            }
        }

        private void CopyImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap != null)
            {
                pictureBox3.Image?.Dispose();
                pictureBox3.Image = (Bitmap)imageBitmap.Clone();
            }
            else
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void HistogramImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap == null)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int[] counts = new int[256];
            for (int y = 0; y < imageBitmap.Height; y++)
                for (int x = 0; x < imageBitmap.Width; x++)
                {
                    Color c = imageBitmap.GetPixel(x, y);
                    int gray = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    counts[gray]++;
                }

            DrawHistogram(counts);
        }

        private void DrawHistogram(int[] counts)
        {
            int max = 0;
            for (int i = 0; i < counts.Length; i++)
                if (counts[i] > max) max = counts[i];

            Bitmap histogram = new Bitmap(256, 100);
            using (Graphics g = Graphics.FromImage(histogram))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int height = (int)(counts[i] * 100.0 / max);
                    g.DrawLine(Pens.Black, new System.Drawing.Point(i, 100), new System.Drawing.Point(i, 100 - height));
                }
            }

            pictureBox3.Image?.Dispose();
            pictureBox3.Image = histogram;
        }

        private void GrayscaleImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap == null)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ApplyImageFilter(FilterType.Grayscale);
        }

        private void InvertImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap == null)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ApplyImageFilter(FilterType.Invert);
        }

        private void SepiaImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap == null)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ApplyImageFilter(FilterType.Sepia);
        }

        private void SubtractImageButton_Click(object sender, EventArgs e)
        {
            if (imageBitmap == null)
            {
                MessageBox.Show("No image loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (backgroundImage == null)
            {
                MessageBox.Show("No background loaded. Please load a background first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            EnsureBackgroundSize(imageBitmap.Width, imageBitmap.Height);

            Bitmap result = new Bitmap(imageBitmap.Width, imageBitmap.Height);
            for (int y = 0; y < imageBitmap.Height; y++)
                for (int x = 0; x < imageBitmap.Width; x++)
                {
                    Color fg = imageBitmap.GetPixel(x, y);
                    Color bgPixel = backgroundImage.GetPixel(x, y);
                    if (fg.G > 100 && fg.G > fg.R * 1.2 && fg.G > fg.B * 1.2)
                        result.SetPixel(x, y, bgPixel);
                    else
                        result.SetPixel(x, y, fg);
                }

            pictureBox3.Image?.Dispose();
            pictureBox3.Image = result;
        }

        private void ApplyImageFilter(FilterType filter)
        {
            Bitmap result = new Bitmap(imageBitmap.Width, imageBitmap.Height);

            for (int y = 0; y < imageBitmap.Height; y++)
                for (int x = 0; x < imageBitmap.Width; x++)
                {
                    Color c = imageBitmap.GetPixel(x, y);
                    Color newColor;

                    switch (filter)
                    {
                        case FilterType.Grayscale:
                            int gray = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                            newColor = Color.FromArgb(gray, gray, gray);
                            break;
                        case FilterType.Invert:
                            newColor = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                            break;
                        case FilterType.Sepia:
                            int tr = Math.Min((int)(0.393 * c.R + 0.769 * c.G + 0.189 * c.B), 255);
                            int tg = Math.Min((int)(0.349 * c.R + 0.686 * c.G + 0.168 * c.B), 255);
                            int tb = Math.Min((int)(0.272 * c.R + 0.534 * c.G + 0.131 * c.B), 255);
                            newColor = Color.FromArgb(tr, tg, tb);
                            break;
                        default:
                            newColor = c;
                            break;
                    }

                    result.SetPixel(x, y, newColor);
                }

            pictureBox3.Image?.Dispose();
            pictureBox3.Image = result;
        }

        #endregion

        #region ---------------- Video Processing ----------------

        private void StartVideoButton_Click(object sender, EventArgs e)
        {
            if (videoCapture != null && videoCapture.IsOpened())
            {
                StopCamera();
                StartVideoButton.Text = "ON"; 
                pictureBox1.Image = imageBitmap;
                pictureBox3.Image?.Dispose();
            }
            else
            {
                if (devices.Length == 0)
                {
                    MessageBox.Show("No camera devices available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                videoCapture = new VideoCapture(devices[0].Index);
                if (!videoCapture.IsOpened())
                {
                    MessageBox.Show("Unable to open camera.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                videoTimer.Start();
                StartVideoButton.Text = "OFF";
            }
        }

        private void StopVideoButton_Click(object sender, EventArgs e)
        {
            StopCamera();
            pictureBox1.Image = imageBitmap;
            pictureBox3.Image?.Dispose();
        }

        private void VideoTimer_Tick(object sender, EventArgs e)
        {
            if (videoCapture == null || !videoCapture.IsOpened()) return;

            using (Mat frameMat = new Mat())
            {
                if (!videoCapture.Read(frameMat) || frameMat.Empty()) return;

                Bitmap frameBitmap = BitmapConverter.ToBitmap(frameMat);

                // Live camera feed
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = (Bitmap)frameBitmap.Clone();

                // Apply subtraction if enabled
                if (videoSubtractionEnabled && backgroundImage != null)
                {
                    EnsureBackgroundSize(frameBitmap.Width, frameBitmap.Height);
                    Bitmap result = ApplySubtraction(frameBitmap, backgroundImage);
                    pictureBox3.Image?.Dispose();
                    pictureBox3.Image = result;
                }
                // Apply live video filter
                else if (currentVideoFilter != FilterType.None)
                {
                    Bitmap result = ApplyVideoFilter(frameBitmap, currentVideoFilter);
                    pictureBox3.Image?.Dispose();
                    pictureBox3.Image = result;
                }

                frameBitmap.Dispose();
            }
        }

        private void VideoCopyButton_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;

            Bitmap clone = (Bitmap)pictureBox1.Image.Clone();
            pictureBox3.Image?.Dispose();
            pictureBox3.Image = clone;
        }

        private void VideoGrayscaleButton_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;
            currentVideoFilter = FilterType.Grayscale;
        }

        private void VideoInvertButton_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;
            currentVideoFilter = FilterType.Invert;
        }

        private void VideoSepiaButton_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;
            currentVideoFilter = FilterType.Sepia;
        }

        private void VideoSubtractButton_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;

            Bitmap currentFrame = (Bitmap)pictureBox1.Image;
            if (backgroundImage == null || backgroundImage.Width != currentFrame.Width || backgroundImage.Height != currentFrame.Height)
            {
                MessageBox.Show("Please load a background image that matches the video frame size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            videoSubtractionEnabled = true;
            MessageBox.Show("Video subtraction enabled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private unsafe Bitmap ApplyVideoFilter(Bitmap frame, FilterType filter)
        {
            if (filter == FilterType.Histogram)
            {
                return GenerateHistogram(frame);
            }

            Bitmap result = new Bitmap(frame.Width, frame.Height, PixelFormat.Format24bppRgb);

            BitmapData frameData = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height),
                                                  ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
                                                    ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideFrame = frameData.Stride;

            try
            {
                byte* ptrFrame = (byte*)frameData.Scan0;
                byte* ptrResult = (byte*)resultData.Scan0;

                for (int y = 0; y < frame.Height; y++)
                {
                    byte* rowFrame = ptrFrame + (y * strideFrame);
                    byte* rowResult = ptrResult + (y * strideFrame);

                    for (int x = 0; x < frame.Width; x++)
                    {
                        byte b = rowFrame[x * 3 + 0];
                        byte g = rowFrame[x * 3 + 1];
                        byte r = rowFrame[x * 3 + 2];

                        byte rb = r;
                        byte gb = g;
                        byte bb = b;

                        switch (filter)
                        {
                            case FilterType.Grayscale:
                                int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                                rb = gb = bb = (byte)gray;
                                break;
                            case FilterType.Invert:
                                rb = (byte)(255 - r);
                                gb = (byte)(255 - g);
                                bb = (byte)(255 - b);
                                break;
                            case FilterType.Sepia:
                                rb = (byte)Math.Min((0.393 * r + 0.769 * g + 0.189 * b), 255);
                                gb = (byte)Math.Min((0.349 * r + 0.686 * g + 0.168 * b), 255);
                                bb = (byte)Math.Min((0.272 * r + 0.534 * g + 0.131 * b), 255);
                                break;
                        }

                        rowResult[x * 3 + 0] = bb;
                        rowResult[x * 3 + 1] = gb;
                        rowResult[x * 3 + 2] = rb;
                    }
                }
            }
            finally
            {
                frame.UnlockBits(frameData);
                result.UnlockBits(resultData);
            }

            return result;
        }

        private unsafe Bitmap GenerateHistogram(Bitmap frame)
        {
            int[] counts = new int[256];

            BitmapData data = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height),
                                             ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            byte* ptr = (byte*)data.Scan0;

            for (int y = 0; y < frame.Height; y++)
            {
                byte* row = ptr + y * stride;
                for (int x = 0; x < frame.Width; x++)
                {
                    byte b = row[x * 3 + 0];
                    byte g = row[x * 3 + 1];
                    byte r = row[x * 3 + 2];
                    int gray = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                    counts[gray]++;
                }
            }

            frame.UnlockBits(data);

            int max = counts.Max();
            Bitmap histogram = new Bitmap(256, 100);
            using (Graphics g = Graphics.FromImage(histogram))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int height = (int)(counts[i] * 100.0 / max);
                    g.DrawLine(Pens.Black, new System.Drawing.Point(i, 100), new System.Drawing.Point(i, 100 - height));
                }
            }

            return histogram;
        }

        private unsafe Bitmap ApplySubtraction(Bitmap frame, Bitmap bgImage)
        {
            if (frame.Width != bgImage.Width || frame.Height != bgImage.Height)
                throw new ArgumentException("Frame and background must be the same size.");

            Bitmap result = new Bitmap(frame.Width, frame.Height, PixelFormat.Format24bppRgb);

            BitmapData frameData = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height),
                                                  ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bgData = bgImage.LockBits(new Rectangle(0, 0, bgImage.Width, bgImage.Height),
                                                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
                                                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                byte* ptrFrame = (byte*)frameData.Scan0;
                byte* ptrBg = (byte*)bgData.Scan0;
                byte* ptrResult = (byte*)resultData.Scan0;

                int strideFrame = frameData.Stride;
                int strideBg = bgData.Stride;
                int strideResult = resultData.Stride;

                for (int y = 0; y < frame.Height; y++)
                {
                    byte* rowFrame = ptrFrame + y * strideFrame;
                    byte* rowBg = ptrBg + y * strideBg;
                    byte* rowResult = ptrResult + y * strideResult;

                    for (int x = 0; x < frame.Width; x++)
                    {
                        byte b = rowFrame[x * 3 + 0];
                        byte g = rowFrame[x * 3 + 1];
                        byte r = rowFrame[x * 3 + 2];

                        if (g > 100 && g > r * 1.2 && g > b * 1.2)
                        {
                            rowResult[x * 3 + 0] = rowBg[x * 3 + 0];
                            rowResult[x * 3 + 1] = rowBg[x * 3 + 1];
                            rowResult[x * 3 + 2] = rowBg[x * 3 + 2];
                        }
                        else
                        {
                            rowResult[x * 3 + 0] = b;
                            rowResult[x * 3 + 1] = g;
                            rowResult[x * 3 + 2] = r;
                        }
                    }
                }
            }
            finally
            {
                frame.UnlockBits(frameData);
                bgImage.UnlockBits(bgData);
                result.UnlockBits(resultData);
            }

            return result;
        }

        private void EnsureBackgroundSize(int width, int height)
        {
            if (backgroundImage == null) return;

            if (backgroundImage.Width != width || backgroundImage.Height != height)
            {
                Bitmap resizedBg = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(resizedBg))
                    g.DrawImage(backgroundImage, 0, 0, width, height);

                backgroundImage.Dispose();
                backgroundImage = resizedBg;
                pictureBox2.Image = backgroundImage;
            }
        }

        private bool CheckCameraRunning()
        {
            if (videoCapture == null || !videoCapture.IsOpened())
            {
                MessageBox.Show("Camera is not running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        #endregion

        #region ---------------- Background Loading ----------------

        private void loadBackground(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("No image available to size background.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap frame = (Bitmap)pictureBox1.Image;
            loadBackground(frame.Width, frame.Height);
        }

        private Bitmap loadBackground(int width, int height)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (ofd.ShowDialog() != DialogResult.OK) return null;

            Bitmap bg = new Bitmap(ofd.FileName);
            Bitmap resizedBg = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(resizedBg))
                g.DrawImage(bg, 0, 0, width, height);

            backgroundImage?.Dispose();
            backgroundImage = resizedBg;
            pictureBox2.Image = backgroundImage;
            return resizedBg;
        }

        #endregion

        #region ---------------- Video / Image Subtraction ----------------

        private void SubtractButton_Click(object sender, EventArgs e)
        {
            if (videoCapture != null && videoCapture.IsOpened())
            {
                if (!CheckCameraRunning()) return;
                videoSubtractionEnabled = true;
                MessageBox.Show("Video subtraction enabled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                SubtractImageButton_Click(sender, e);
        }

        private void StopCamera()
        {
            videoTimer.Stop();
            videoSubtractionEnabled = false;
            currentVideoFilter = FilterType.None;

            if (videoCapture != null)
            {
                videoCapture.Release();
                videoCapture.Dispose();
                videoCapture = null;
            }
        }


        #endregion

        private void histogramToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!CheckCameraRunning()) return;
            currentVideoFilter = FilterType.Histogram;
        }
    }
}
