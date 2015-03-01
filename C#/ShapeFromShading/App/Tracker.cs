using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using OpenCvSharp;
using OpenCvSharp.Blob;
using System.Runtime.InteropServices;
using OpenCvSharp.CPlusPlus;
using System.Speech.Recognition;
using La = MathNet.Numerics.LinearAlgebra;
using Lad = MathNet.Numerics.LinearAlgebra.Double;

namespace ShapeFromShading
{
    /// <summary>
    /// Responsible of Image Processing.
    /// </summary>
    public class Tracker
    {
        #region Public Properties

        /// <summary>
        /// Set interval time of the main timer.
        /// </summary>
        public int TimerIntervalTime
        {
            set
            {
                timerIntervalTime = value;
                mainTimer.Interval = timerIntervalTime;
            }

            get
            {
                return timerIntervalTime;
            }
        }

        #endregion

        #region Private Variables

        private readonly int deviceID = 0;
        private readonly int screenHeight = System.Windows.Forms.SystemInformation.VirtualScreen.Height;
        private readonly int screenWidth = System.Windows.Forms.SystemInformation.VirtualScreen.Width;

        private int timerIntervalTime = 30;
        private int counter = 0;
        private Label labelFrameCounter;

        private bool timerInProgress = false;
        private System.Windows.Forms.Timer fpsTimer;
        private System.Windows.Forms.Timer mainTimer;

        private CvCapture capture;
        private IplImage cameraFrame;
        private IplImage[] images;
        private CvWindow cameraWindow;
        private List<CvWindow> windows;
        private CvSize size;
        private bool isConstructing;
        bool isVerticalApporachBusy;

        #endregion

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="labelFrameCounter"></param>
        public Tracker(Label labelFrameCounter)
        {
            this.labelFrameCounter = labelFrameCounter;
            Initialize();
            InitializeCamera();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// used to dispose any object created from this class
        /// </summary>
        public void Dispose()
        {
            if (timerInProgress)
            {
                mainTimer.Stop();
                fpsTimer.Stop();
            }

            if (mainTimer != null)
            {
                mainTimer.Dispose();
                mainTimer = null;
            }

            if (fpsTimer != null)
            {
                fpsTimer.Dispose();
                fpsTimer = null;
            }

            if (cameraWindow != null)
            {
                cameraWindow.Close();
                cameraWindow.Dispose();
                cameraWindow = null;
            }

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i] != null)
                {
                    windows[i].Close();
                    windows[i].Dispose();
                    windows[i] = null;
                }
            }

            if (capture != null)
            {
                capture.Dispose();
                capture = null;
            }
        }

        /// <summary>
        /// Start mainThread, that starts tracking
        /// </summary>
        public void StartCamera()
        {
            mainTimer.Start();
            fpsTimer.Start();
            timerInProgress = true;
        }

        /// <summary>
        /// Stop mainThread, that stops tracking
        /// </summary>
        public void StopCamera()
        {
            mainTimer.Stop();
            fpsTimer.Stop();
            timerInProgress = false;
        }

        /// <summary>
        /// Take snapshot and save it in the array according to the given index.
        /// </summary>
        /// <param name="index"></param>
        public void TakeSnapshot(int index)
        {
            if (isConstructing || index < 0 || index > 5)
            {
                return;
            }

            // create image if not created
            IplImage image = images[index];
            if (image == null)
            {
                image = new IplImage(size, BitDepth.U8, 1);
                images[index] = image;
            }

            // capture new image, convert it to grayscale
            // and set it to our image
            Cv.CvtColor(capture.QueryFrame(), image, ColorConversion.BgrToGray);

            // show it on the window
            windows[index].Image = image;
        }

        /// <summary>
        /// Start constructing the 3D image out of the taken snapshots.
        /// </summary>
        public void ConstructImage()
        {
            string location = @"..\..\..\..\Matlab\Images";
            for (int i = 0; i < 6; i++)
            {
                images[i].SaveImage(System.IO.Path.Combine(location, string.Format("person{0}.jpg", (i + 1))));
            }

            return;

            Utils.DebugLine("Start ConstructImage");
            isConstructing = true;

            double[] light1 = new double[] { 0, 0, 40 };
            double[] light2 = new double[] { 20, 5, 40 };
            double[] light3 = new double[] { -5, 15, 40 };
            double[] light4 = new double[] { -10, -10, 40 };
            double[] light5 = new double[] { 5, -10, 40 };
            double[] light6 = new double[] { 5, 20, 40 };

            Lad.DenseVector l1 = Lad.DenseVector.OfArray(light1);
            Lad.DenseVector l2 = Lad.DenseVector.OfArray(light2);
            Lad.DenseVector l3 = Lad.DenseVector.OfArray(light3);
            Lad.DenseVector l4 = Lad.DenseVector.OfArray(light4);
            Lad.DenseVector l5 = Lad.DenseVector.OfArray(light5);
            Lad.DenseVector l6 = Lad.DenseVector.OfArray(light6);

            Lad.DenseVector[] lights = new Lad.DenseVector[] { l1, l2, l3, l4, l5, l6 };
            for (int i = 0; i < 6; i++)
            {
                lights[i] = lights[i] / lights[i].Norm(2);
            }

            double[][] s = new double[6][];
            for (int y = 0; y < 6; y++)
            {
                s[y] = lights[y].ToArray();
            }
            Lad.DenseMatrix _s = Lad.DenseMatrix.OfRowArrays(s);

            double[][][] b = new double[240][][];
            for (int y = 0; y < 240; y++)
            {
                b[y] = new double[320][];
                for (int x = 0; x < 320; x++)
                {
                    b[y][x] = new double[3] { 1, 1, 1 };
                }
            }

            double[][] p = new double[240][];
            for (int y = 0; y < 240; y++)
            {
                p[y] = new double[320];
                for (int x = 0; x < 320; x++)
                {
                    p[y][x] = 1;
                }
            }

            double[][] q = (double[][])p.Clone();
            double[][] z = (double[][])p.Clone();
            double[] e = new double[6];
            Lad.DenseMatrix _e;
            Lad.DenseMatrix tb;
            Lad.DenseMatrix _sInv;

            for (int y = 0; y < 240; y++)
            {
                for (int x = 0; x < 320; x++)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        e[i] = images[i][y, x].Val0;
                    }
                    _e = Lad.DenseMatrix.OfColumnArrays(new double[][] { e });
                    _sInv = Lad.DenseMatrix.OfMatrix(_s.Inverse());
                    tb = Lad.DenseMatrix.OfMatrix(((_sInv * _s).Inverse()) * _sInv * _e);

                    //tb= (inv(S'*S))*S'*E;
                }
            }

            //
            //E=[img1(i,j) img2(i,j) img3(i,j) img4(i,j) img5(i,j) img6(i,j)];
            //E=double(E');
            //
            //tb= (inv(S'*S))*S'*E;
            //
            //nbm = norm(tb);
            //if( nbm == 0)
            //    b(i,j,:) = 0; 
            //else
            //    b(i,j,:) = tb / nbm;
            //end

            //
            //tM = [b(i,j,1) b(i,j,2) b(i,j,3)];
            //nbm = norm(tM);
            //if( nbm == 0)
            //    tM = [0 0 0];
            //else
            //    tM = tM / nbm; 
            //end        
            //p(i,j)=tM(1,1);
            //q(i,j)=tM(1,2);


            //Lad.Matrix s = Lad.DenseMatrix.OfRowVectors(lights);
            //Lad.Matrix b = Lad.DenseMatrix.OfArray() ;// .OfArray(new double[] { 240, 320, 3 });
            //Lad.Vector p = Lad.DenseVector.OfArray(new double[] { 240, 320, 3 });            
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialize Camera, timer and some objects.
        /// </summary>
        private void Initialize()
        {
            // initialize mainTimer
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Interval = timerIntervalTime;
            mainTimer.Tick += ProcessFrame;

            // initialize timer used to count frames per seconds of the camera
            fpsTimer = new System.Windows.Forms.Timer();
            fpsTimer.Interval = 1000;
            fpsTimer.Tick += new EventHandler((object obj, EventArgs eventArgs) =>
            {
                labelFrameCounter.Content = counter.ToString();
                counter = 0;
            });
        }

        /// <summary>
        /// Initialize camera input, frame window and other image objects required.
        /// This is done after getting the settings of the tracker object of this class.
        /// </summary>
        private void InitializeCamera()
        {
            // intialize camera
            try
            {
                capture = new CvCapture(CaptureDevice.Any, deviceID);
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Failed to initialize the camera, the program will be closed." +
                    "\n\nThis is the internal error:\n" + exception.Message, "Notify", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            // small frame to decrease computational complexity
            size = new CvSize(320, 240);

            capture.SetCaptureProperty(CaptureProperty.FrameHeight, size.Height);
            capture.SetCaptureProperty(CaptureProperty.FrameWidth, size.Width);
            capture.SetCaptureProperty(CaptureProperty.FrameCount, 15);

            images = new IplImage[6];

            // windows to view what's going on
            cameraWindow = new CvWindow("Camera", WindowMode.KeepRatio);
            cameraWindow.Resize(size.Width, size.Height);
            cameraWindow.Move(screenWidth - 40 - 3 * size.Width, 20 + 0 * size.Height);

            windows = new List<CvWindow>(6);

            windows.Add(new CvWindow("Picture 1", WindowMode.KeepRatio));
            windows[0].Resize(size.Width, size.Height);
            windows[0].Move(screenWidth - 20 - 2 * size.Width, 0 + 0 * size.Height);

            windows.Add(new CvWindow("Picture 2", WindowMode.KeepRatio));
            windows[1].Resize(size.Width, size.Height);
            windows[1].Move(screenWidth - 20 - 1 * size.Width, 0 + 0 * size.Height);

            windows.Add(new CvWindow("Picture 3", WindowMode.KeepRatio));
            windows[2].Resize(size.Width, size.Height);
            windows[2].Move(screenWidth - 20 - 2 * size.Width, 33 + 1 * size.Height);

            windows.Add(new CvWindow("Picture 4", WindowMode.KeepRatio));
            windows[3].Resize(size.Width, size.Height);
            windows[3].Move(screenWidth - 20 - 1 * size.Width, 33 + 1 * size.Height);

            windows.Add(new CvWindow("Picture 5", WindowMode.KeepRatio));
            windows[4].Resize(size.Width, size.Height);
            windows[4].Move(screenWidth - 20 - 2 * size.Width, 66 + 2 * size.Height);

            windows.Add(new CvWindow("Picture 6", WindowMode.KeepRatio));
            windows[5].Resize(size.Width, size.Height);
            windows[5].Move(screenWidth - 20 - 1 * size.Width, 66 + 2 * size.Height);
        }

        /// <summary>
        /// Image Processing. It is done using OpenCVSharp Library.
        /// </summary>
        private void ProcessFrame(object sender, EventArgs e)
        {
            // increment counter
            counter++;

            // capture new frame
            cameraFrame = capture.QueryFrame();

            // show th one camera frames
            cameraWindow.Image = cameraFrame;

            //if (!isVerticalApporachBusy)
            //{
            //    windows[1].Image = VariationalApproach(cameraFrame, 100, 0.4);
            //}
        }

        /// <summary>
        /// The variational approach to shape-from-shading.
        /// </summary>
        private IplImage VariationalApproach(IplImage image, int iterations, double lambda)
        {
            isVerticalApporachBusy = true;

            IplImage grayScale_ = new IplImage(image.Width, image.Height, BitDepth.U8, 1);

            // convert to grayscale
            Cv.CvtColor(image, grayScale_, ColorConversion.BgrToGray);
            IplImage grayScale = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            Cv.Convert(grayScale_, grayScale);

            // normalize image
            Cv.Normalize(grayScale, grayScale);

            // multibly by the filter
            float[,] arrayX = new float[1, 3];
            float[,] arrayY = new float[3, 1];
            arrayX[0, 0] = arrayY[0, 0] = -1;
            arrayX[0, 1] = arrayY[1, 0] = 0;
            arrayX[0, 2] = arrayY[2, 0] = 1;
            CvMat kernelX = CvMat.FromArray(arrayX);
            CvMat kernelY = CvMat.FromArray(arrayY);
            IplImage zX = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage zY = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            Cv.Filter2D(grayScale, zX, kernelX);
            Cv.Filter2D(grayScale, zY, kernelY);
            zX *= 0.5;
            zY *= 0.5;
            double maxX = 0; double minX = 0; double maxY = 0; double minY = 0;

            // get max values
            Cv2.MinMaxIdx(new Mat(zX), out minX, out maxX);
            Cv2.MinMaxIdx(new Mat(zY), out minY, out maxY);

            IplImage gt = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage ft = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage r = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage fs = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage gs = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    ft[y, x] = 0;
                    if (zX[y, x] >= 0.5 * maxX)
                    {
                        ft[y, x] = -2;
                    }
                    else if (zX[y, x] < 0.5 * minX)
                    {
                        ft[y, x] = 2;
                    }

                    gt[y, x] = 0;
                    if (zX[y, x] >= 0.5 * maxY)
                    {
                        gt[y, x] = -2;
                    }
                    else if (zX[y, x] < 0.5 * minY)
                    {
                        gt[y, x] = 2;
                    }

                    r[y, x] = Math.Pow(ft[y, x].Val0, 2) + Math.Pow(gt[y, x].Val0, 2);
                    if (r[y, x] == 4)
                    {
                        fs[y, x] = ft[y, x];
                        gs[y, x] = gt[y, x];
                    }
                }
            }

            float[,] array = new float[3, 3];
            array[0, 0] = array[0, 1] = array[0, 2] = 1;
            array[1, 0] = array[1, 2] = 1;
            array[1, 1] = 0;
            array[2, 0] = array[2, 1] = array[2, 2] = 1;
            CvMat kernel = CvMat.FromArray(array);
            kernel = (1 / 8) * kernel;
            IplImage depth = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage fs2 = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage gs2 = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage ones = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage rNorm = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage fMid = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage gMid = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage diff = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage f = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            IplImage g = new IplImage(image.Width, image.Height, BitDepth.F32, 1);
            ones.Set(Scalar.FromRgb(1, 1, 1));

            for (int i = 0; i < iterations; i++)
            {
                Cv.Mul(fs, fs, fs2);
                Cv.Mul(gs, gs, gs2);

                Cv.Div((-fs2 - gs2 + 4), (fs2 + gs2 + 4), depth);
                Cv.Div(ones, (fs2 + gs2 + 4), rNorm);

                Cv.Filter2D(fs, fMid, kernel);
                Cv.Filter2D(gs, gMid, kernel);

                diff = grayScale - depth;
               
                Cv.Mul(fs, rNorm, f);
                Cv.Mul(f, diff, f);
                f = f * (1 / lambda);

                Cv.Mul(gs, rNorm, g);
                Cv.Mul(g, diff, g);
                f = f * (1 / lambda);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        if (ft[y, x] == 0)
                        {
                            fs[y, x] = fMid[y, x] - f[y, x];
                        }

                        if (gt[y, x] == 0)
                        {
                            gs[y, x] = gMid[y, x] - g[y, x];
                        }
                    }
                }
            }

            isVerticalApporachBusy = false;

            return depth;
        }

        #endregion
    }
}