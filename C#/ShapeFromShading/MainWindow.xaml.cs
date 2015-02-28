using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShapeFromShading
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Variables

        /// <summary>
        /// Object ot manage camera and computer vision stuff.
        /// </summary>
        private Tracker tracker;
        /// <summary>
        /// Manage voice recognition.
        /// </summary>
        private SpeechRecognitionEngine speechRecEngine;
        /// <summary>
        /// List of the names of the numbers from 1 to 10.
        /// </summary>
        private string[] numbers;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// intitialize objects and camera.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        /// <summary>
        /// Flip horizontal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxFlipHorizontal_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Change feature extractor to Sift.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonSift_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Change feature extractor to Surf.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonSurf_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Change the value of the Brightness.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (tracker != null)
            {

            }
        }

        /// <summary>
        /// Button snapshot is clicked.
        /// </summary>
        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// intitialize objects and camera.
        /// </summary>
        private void Initialize()
        {
            numbers = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
                        
            // initialize camera object
            tracker = new Tracker(labelFrameCounter);
            tracker.StartCamera();

            // initialize speech recogintion
            InitializeSpeechRecognition();
            
            // start speech recognition
            speechRecEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        /// <summary>
        /// Intialize objects and grammars required for speech recognition 
        /// using Microsoft Speeach API (SAPI).
        /// </summary>
        private void InitializeSpeechRecognition()
        {
            //speechRec = new SpeechRecognizer();
            speechRecEngine = new SpeechRecognitionEngine();

            // Train the system to recognize these sentences            
            Choices c = new Choices();
            c.Add(numbers);
            c.Add("Process");

            var grammarBuilder = new GrammarBuilder(c);
            Grammar grammar = new Grammar(grammarBuilder);

            speechRecEngine.LoadGrammar(grammar);
            speechRecEngine.SetInputToDefaultAudioDevice();
            speechRecEngine.SpeechRecognized += Rec_SpeechRecognized;
        }

        /// <summary>
        /// Event handler for voice recognition. Called when a new voice is recognized.
        /// </summary>        
        private void Rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {   
            if (e.Result.Confidence < 0.935)
            {
                return;
            }

            string resultText = e.Result.Text;
            int index = Array.IndexOf(numbers, resultText);
            if (index > -1)
            {
                tracker.TakeSnapshot(index);
            }
            else if (this.IsEqualText(resultText, "Process"))
            {
                tracker.ConstructImage();
            }
        }

        private bool IsEqualText(string text1, string text2)
        {
            return string.Compare(text1, text2, true) == 0;
        }
        #endregion
    }
}
