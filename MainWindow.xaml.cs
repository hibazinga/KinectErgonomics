//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Diagnostics;
    using System;
    using IronPython.Hosting;
    using Microsoft.Scripting.Hosting;
    using Microsoft.Scripting;
    using SpeechLib;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        private static double[] count = { 0 };
        private static bool voice = false;
  

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        
        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;
            SpVoice Voice = new SpVoice();
            Voice.Speak("Hello! Welcome to Kinect with Ergonomics!");
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    //
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.Image2.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
                
                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }
        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }
        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                //ImageSourceConverter c = new ImageSourceConverter();
                //ImageSource x = (ImageSource)c.ConvertFrom(colorBitmap);
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
        /*
        private void ExecutePython(string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "C:\\Python27\\python.exe";

            start.Arguments = "python 1+2";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }*/
        private void RecordPosition1(Skeleton skeleton, double[] count)
        {
            if (count[0] % 10 != 0)
            {
                count[0]++; return;
            }
            else
            {
                count[0] = 0;
                count[0]++;
            }
            string text = "";
            string fn = @"D:\kinect.txt";
            //HEAD
            text = "Head: " + " X: " + skeleton.Joints[JointType.Head].Position.X + " ,Y: " + skeleton.Joints[JointType.Head].Position.Y + " ,Z: " + skeleton.Joints[JointType.Head].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //SHOULDER CENTER
            text = "Shoulder Center: " + " X: " + skeleton.Joints[JointType.ShoulderCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderCenter].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //SHOULDER LEFT
            text = "Shoulder Left: " + " X: " + skeleton.Joints[JointType.ShoulderLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //SHOULDER RIGHT
            text = "Shoulder Right: " + " X: " + skeleton.Joints[JointType.ShoulderRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //SPINE
            text = "Spine: " + " X: " + skeleton.Joints[JointType.Spine].Position.X + " ,Y: " + skeleton.Joints[JointType.Spine].Position.Y + " ,Z: " + skeleton.Joints[JointType.Spine].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //HIP CENTER
            text = "Hip Center: " + " X: " + skeleton.Joints[JointType.HipCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.HipCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipCenter].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //HIP LEFT
            text = "Hip Left: " + " X: " + skeleton.Joints[JointType.HipLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HipLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //HIP RIGHT
            text = "Hip Right: " + " X: " + skeleton.Joints[JointType.HipRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HipRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //ELBOW LEFT
            text = "Elbow Left: " + " X: " + skeleton.Joints[JointType.ElbowLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //WRIST LEFT
            text = "Wrist Left: " + " X: " + skeleton.Joints[JointType.WristLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.WristLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //HAND LEFT
            text = "Hand Left: " + " X: " + skeleton.Joints[JointType.HandLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HandLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //ELBOW RIGHT
            text = "Elbow Right: " + " X: " + skeleton.Joints[JointType.ElbowRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //WRIST RIGHT
            text = "Wrist Right: " + " X: " + skeleton.Joints[JointType.WristRight].Position.X + " ,Y: " + skeleton.Joints[JointType.WristRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //HAND RIGHT
            text = "Hand Right: " + " X: " + skeleton.Joints[JointType.HandRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HandRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            text = "Knee Left: " + " X: " + skeleton.Joints[JointType.KneeLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Ankle Left: " + " X: " + skeleton.Joints[JointType.AnkleLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Knee Right: " + " X: " + skeleton.Joints[JointType.KneeRight].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Ankle Right: " + " X: " + skeleton.Joints[JointType.AnkleRight].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            File.AppendAllText(fn, "--------------------------------------\n");

        }

        private void RecordPosition0(Skeleton skeleton, double[] count)
        {
            /*if (count[0] % 90 != 0)
            {
                count[0]++; return;
            }
            else
            {
                count[0] = 0;
                count[0]++;
            }*/
            string fn = @"data.txt";
            //File.WriteAllText(fn, String.Empty);
            //StreamWriter sw = new StreamWriter(fn);
            //JointType[] recordAll = {JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ShoulderRight, JointType.Spine, JointType.HipCenter, JointType.HipLeft, JointType.HipRight, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft, JointType.ElbowRight, JointType.WristRight, JointType.HandRight}; 
            string text = "------------------------\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HEAD
            text = "Head: " + " X: " + skeleton.Joints[JointType.Head].Position.X + " ,Y: " + skeleton.Joints[JointType.Head].Position.Y + " ,Z: " + skeleton.Joints[JointType.Head].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //SHOULDER CENTER
            text = "Shoulder Center: " + " X: " + skeleton.Joints[JointType.ShoulderCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderCenter].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //SHOULDER LEFT
            text = "Shoulder Left: " + " X: " + skeleton.Joints[JointType.ShoulderLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //SHOULDER RIGHT
            text = "Shoulder Right: " + " X: " + skeleton.Joints[JointType.ShoulderRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //SPINE
            text = "Spine: " + " X: " + skeleton.Joints[JointType.Spine].Position.X + " ,Y: " + skeleton.Joints[JointType.Spine].Position.Y + " ,Z: " + skeleton.Joints[JointType.Spine].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HIP CENTER
            text = "Hip Center: " + " X: " + skeleton.Joints[JointType.HipCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.HipCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipCenter].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HIP LEFT
            text = "Hip Left: " + " X: " + skeleton.Joints[JointType.HipLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HipLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HIP RIGHT
            text = "Hip Right: " + " X: " + skeleton.Joints[JointType.HipRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HipRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //ELBOW LEFT
            text = "Elbow Left: " + " X: " + skeleton.Joints[JointType.ElbowLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //WRIST LEFT
            text = "Wrist Left: " + " X: " + skeleton.Joints[JointType.WristLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.WristLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HAND LEFT
            text = "Hand Left: " + " X: " + skeleton.Joints[JointType.HandLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HandLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandLeft].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //ELBOW RIGHT
            text = "Elbow Right: " + " X: " + skeleton.Joints[JointType.ElbowRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //WRIST RIGHT
            text = "Wrist Right: " + " X: " + skeleton.Joints[JointType.WristRight].Position.X + " ,Y: " + skeleton.Joints[JointType.WristRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //HAND RIGHT
            text = "Hand Right: " + " X: " + skeleton.Joints[JointType.HandRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HandRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandRight].Position.Z + "\n";
            File.AppendAllText(fn, text);
            text = "Knee Left: " + " X: " + skeleton.Joints[JointType.KneeLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Ankle Left: " + " X: " + skeleton.Joints[JointType.AnkleLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Knee Right: " + " X: " + skeleton.Joints[JointType.KneeRight].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            text = "Ankle Right: " + " X: " + skeleton.Joints[JointType.AnkleRight].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            File.AppendAllText(fn, text);
            //sw.Write(text);
            //sw.Close();
            File.AppendAllText(fn, "--------------------------------------\n");
            //run_test();


        }
        private void RecordPosition (Skeleton skeleton, double[] count){
            if (count[0] % 90 != 0)
            {
                count[0]++; return;
            }
            else {
                count[0] = 0;
                count[0]++;
            }
            string fn = @"test.txt";
            //File.WriteAllText(fn, String.Empty);
            StreamWriter sw = new StreamWriter(fn);
            //JointType[] recordAll = {JointType.Head, JointType.ShoulderCenter, JointType.ShoulderLeft, JointType.ShoulderRight, JointType.Spine, JointType.HipCenter, JointType.HipLeft, JointType.HipRight, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft, JointType.ElbowRight, JointType.WristRight, JointType.HandRight}; 
            string text="------------------------\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //HEAD
            text = "Head: " + " X: " + skeleton.Joints[JointType.Head].Position.X + " ,Y: " + skeleton.Joints[JointType.Head].Position.Y + " ,Z: " + skeleton.Joints[JointType.Head].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //SHOULDER CENTER
            text = "Shoulder Center: " + " X: " + skeleton.Joints[JointType.ShoulderCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderCenter].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //SHOULDER LEFT
            text = "Shoulder Left: " + " X: " + skeleton.Joints[JointType.ShoulderLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //SHOULDER RIGHT
            text = "Shoulder Right: " + " X: " + skeleton.Joints[JointType.ShoulderRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ShoulderRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ShoulderRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //SPINE
            text = "Spine: " + " X: " + skeleton.Joints[JointType.Spine].Position.X + " ,Y: " + skeleton.Joints[JointType.Spine].Position.Y + " ,Z: " + skeleton.Joints[JointType.Spine].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //HIP CENTER
            text = "Hip Center: " + " X: " + skeleton.Joints[JointType.HipCenter].Position.X + " ,Y: " + skeleton.Joints[JointType.HipCenter].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipCenter].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //HIP LEFT
			text = "Hip Left: " + " X: " + skeleton.Joints[JointType.HipLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HipLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //HIP RIGHT
			text = "Hip Right: " + " X: " + skeleton.Joints[JointType.HipRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HipRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HipRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //ELBOW LEFT
			text = "Elbow Left: " + " X: " + skeleton.Joints[JointType.ElbowLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //WRIST LEFT
			text = "Wrist Left: " + " X: " + skeleton.Joints[JointType.WristLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.WristLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //HAND LEFT
			text = "Hand Left: " + " X: " + skeleton.Joints[JointType.HandLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.HandLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //ELBOW RIGHT
			text = "Elbow Right: " + " X: " + skeleton.Joints[JointType.ElbowRight].Position.X + " ,Y: " + skeleton.Joints[JointType.ElbowRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.ElbowRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            //WRIST RIGHT
			text = "Wrist Right: " + " X: " + skeleton.Joints[JointType.WristRight].Position.X + " ,Y: " + skeleton.Joints[JointType.WristRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.WristRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text); 
            //HAND RIGHT
			text = "Hand Right: " + " X: " + skeleton.Joints[JointType.HandRight].Position.X + " ,Y: " + skeleton.Joints[JointType.HandRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.HandRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            text = "Knee Left: " + " X: " + skeleton.Joints[JointType.KneeLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            text = "Ankle Left: " + " X: " + skeleton.Joints[JointType.AnkleLeft].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleLeft].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleLeft].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            text = "Knee Right: " + " X: " + skeleton.Joints[JointType.KneeRight].Position.X + " ,Y: " + skeleton.Joints[JointType.KneeRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.KneeRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            text = "Ankle Right: " + " X: " + skeleton.Joints[JointType.AnkleRight].Position.X + " ,Y: " + skeleton.Joints[JointType.AnkleRight].Position.Y + " ,Z: " + skeleton.Joints[JointType.AnkleRight].Position.Z + "\n";
            //File.AppendAllText(fn, text);
            sw.Write(text);
            sw.Close();
            //File.AppendAllText(fn, "--------------------------------------\n");
            run_test();


        }
        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
            
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
            RecordPosition1(skeleton, count);
            //System.Diagnostics.Process.Start(@"speak.vbs");
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }            
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    voice = true;
                }
                else
                {
                    voice = false;
                }
            }
        }
        private static void doPython()
        {
            ScriptEngine engine = Python.CreateEngine();
            engine.ExecuteFile(@"classify.py");
            //engine.Runtime.IO.RedirectToConsole();
            //System.IO.TextWriter writeFile = new StreamWriter("D:\\tmp.txt");
            //Console.SetOut(writeFile);
            //writeFile.Flush();
            //writeFile.Close();
            //ScriptRuntime pyruntime = Python.CreateRuntime();
            //dynamic obj = pyruntime.UseFile(@"C:\Users\user\Desktop\cs239\classify.py");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            doPython();
            textbox.Text += "\n";
            StreamReader streamReader = new StreamReader(@"result.txt");
            textbox.Text += streamReader.ReadToEnd();
            //MessageBox.Show("good posture!\n");
        }
        private void run_test() {
            /*
            doPython();
            textbox.Text += "\n";
            StreamReader streamReader = new StreamReader(@"result.txt");
            textbox.Text += streamReader.ReadToEnd();
            streamReader.Close();
            textbox.ScrollToEnd();
            if (voice) {
                //SpeechSynthesizer reader = new SpeechSynthesizer();
            }
            */
            doPython();
            textbox.Text += "\n ---------------------- \n";
            StreamReader streamReader = new StreamReader(@"result.txt");
            String tmp = streamReader.ReadToEnd();
            streamReader.Close();
            String speak_text="";
            SpVoice Voice = new SpVoice();
            int errno = Convert.ToInt32(tmp);
            if (errno == 0)
            {
                if(voice) Voice.Speak("Perfect Posture!");
                textbox.Text += "You have a perfect posture!\n";
            }
            else {
                while (errno > 0) {
                    if (errno >= 4)
                    {
                        if (voice) Voice.Speak("Hunching back!");
                        textbox.Text += "You are hunching your back!\n";
                        errno -= 4;
                    }
                    else if (errno >= 2)
                    {
                        if (voice) Voice.Speak("Arms too close!");
                        textbox.Text += "You arms are too close to your body!\n";
                        errno -= 2;
                    }
                    else if (errno >= 1)
                    {
                        if (voice) Voice.Speak("Crossing legs!");
                        textbox.Text += "You are crossing your legs!\n";
                        errno -= 1;
                    }
                    else break;
                }

            }
            textbox.ScrollToEnd();
        }
    }
}

