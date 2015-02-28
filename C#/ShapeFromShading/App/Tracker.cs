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

namespace ShapeFromShading
{
    /// <summary>
    /// Responsible of Image Processing.
    /// </summary>
    public class Tracker
    {
        #region Public Properties

        /// <summary>
        /// Use grayscale images as input or not.
        /// </summary>
        public bool IsGrayScale { get; set; }
        /// <summary>
        /// Value of Guassian smooth.
        /// </summary>
        public int GaussianSmooth { get; set; }
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
        private IplImage frame1;
        private IplImage frame2;
        private IplImage grayFrame1;
        private IplImage grayFrame2;
        private IplImage transformedFrame;
        private CvWindow window1;
        private CvWindow window2;
        private CvWindow window3;
        private CvSize size;
        private SIFT sift;
        private SURF surf;
        private BFMatcher bruteForceMatcher;
        private FlannBasedMatcher flannBasedMatcher;

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

            if (window1 != null)
            {
                window1.Close();
                window1.Dispose();
                window1 = null;
            }

            if (window2 != null)
            {
                window2.Close();
                window2.Dispose();
                window2 = null;
            }

            if (window3 != null)
            {
                window3.Close();
                window3.Dispose();
                window3 = null;
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
        public void StartProcessing()
        {
            mainTimer.Start();
            fpsTimer.Start();
            timerInProgress = true;
        }

        /// <summary>
        /// Stop mainThread, that stops tracking
        /// </summary>
        public void StopProcessing()
        {
            mainTimer.Stop();
            fpsTimer.Stop();
            timerInProgress = false;
        }

        /// <summary>
        /// Take snapshot and save it as camera 2.
        /// </summary>
        public void TakeSnapshot()
        {
            frame2 = capture.QueryFrame().Clone();
            isReady = true;
        }

        bool isReady = false;

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
            // Intialize camera
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

            frame1 = new IplImage(size, BitDepth.U8, 3);
            frame2 = new IplImage(size, BitDepth.U8, 3);
            grayFrame1 = new IplImage(size, BitDepth.U8, 1);
            grayFrame2 = new IplImage(size, BitDepth.U8, 1);
            transformedFrame = new IplImage(size, BitDepth.U8, 1);
            sift = new SIFT();
            surf = new SURF(500, 4, 2, true);
            bruteForceMatcher = new BFMatcher(NormType.L2, false);
            flannBasedMatcher = new FlannBasedMatcher();

            // windows to view what's going on
            window1 = new CvWindow("Camera 1", WindowMode.KeepRatio);
            window1.Resize(size.Width, size.Height);
            window1.Move(screenWidth - 17 - 2 * size.Width, 20);

            window2 = new CvWindow("Camera 2", WindowMode.KeepRatio);
            window2.Resize(size.Width, size.Height);
            window2.Move(screenWidth - 20 - 1 * size.Width, 20);

            window3 = new CvWindow("Result", WindowMode.KeepRatio);
            window3.Resize(size.Width * 2, size.Height);
            window3.Move(screenWidth - 20 - 2 * size.Width, 20 + size.Height);
        }

        /// <summary>
        /// Image Processing. It is done using OpenCVSharp Library.
        /// </summary>
        private void ProcessFrame(object sender, EventArgs e)
        {
            // increment counter
            counter++;

            // capture new frame
            frame1 = capture.QueryFrame();

            // show image on the separate window
            window1.Image = frame1;
            window2.Image = frame2;

            // apply some variations to the image (brightness, salt-and-pepper, ...)
            if (isReady)
            {
                IplImage image1;
                IplImage image2;

                // check if to use gray-scale or not
                if (IsGrayScale)
                {
                    // convert to grayscale image
                    Cv.CvtColor(frame1, grayFrame1, ColorConversion.BgrToGray);
                    Cv.CvtColor(frame2, grayFrame2, ColorConversion.BgrToGray);
                    image1 = grayFrame1;
                    image2 = grayFrame2;
                }
                else
                {
                    image1 = frame1;
                    image2 = frame2;
                }

                // check if to use gaussian smooth
                if (GaussianSmooth > 0)
                {
                    int gaussianValue = (GaussianSmooth % 2 == 0) ? GaussianSmooth - 1 : GaussianSmooth;
                    Cv.Smooth(image1, image1, SmoothType.Gaussian, gaussianValue);
                }

                // apply the matching
                // transformedFrame = Matching(image1, image2, FeatureExtractor);

                window3.Image = image2;
            }
        }

        #endregion
    }
}