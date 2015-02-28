using System;
using System.Collections.Generic;
using System.Linq;
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

        private Tracker tracker;

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
        private void ButtonSnapShot_Click(object sender, RoutedEventArgs e)
        {
            tracker.TakeSnapshot();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// intitialize objects and camera.
        /// </summary>
        private void Initialize()
        {
            tracker = new Tracker(labelFrameCounter);
            tracker.StartProcessing();
        }

        #endregion
    }
}
