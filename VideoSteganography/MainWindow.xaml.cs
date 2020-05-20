using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

namespace VideoSteganography
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filePath = "";
        private int width, height;
        private long framecount, capacityHiding;
        private List<Bitmap> bitmaps = new List<Bitmap>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectedVideo_Loaded(object sender, RoutedEventArgs e)
        {
            selectedVideo.Pause();
            btnPlay.IsEnabled = btnPause.IsEnabled = true;
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            selectedVideo.Position = new TimeSpan(0, 0, 0);
            selectedVideo.Play();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            selectedVideo.Pause();
        }

        private void RichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtHide.Document.Blocks.Clear();
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSelectVideo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp4";
            dialog.Filter = "Video Files|*.mp4;*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3";
            dialog.InitialDirectory = @"C:\Users\deniz\OneDrive\Masaüstü\";


            if (dialog.ShowDialog() == true)
            {
                filePath = txtVideoPath.Text = dialog.FileName;
                selectedVideo.Source = new Uri($@"{filePath}");
                selectedVideo.LoadedBehavior = MediaState.Manual;
                txtName.Text = dialog.SafeFileNames[0];
                FetchPixels();
                txtWidth.Text = width.ToString();
                txtHeight.Text = height.ToString();
                txtType.Text =  System.IO.Path.GetExtension(filePath);
                txtSize.Text = (new FileInfo(filePath).Length / 1024).ToString() + "KB";
                txtStatus.Text = $"You have chance that can hide {capacityHiding} chracters.";
            }


        }

        private void FetchPixels() // Pixels are fetching.
        {
            VideoFileReader reader = new VideoFileReader();
            reader.Open(filePath);
            framecount = reader.FrameCount;

            for (int i = 0; i < framecount; i++)
            {
                Bitmap eachFrame = reader.ReadVideoFrame();
                bitmaps.Add(eachFrame);
                //videoFrame.Save(Environment.CurrentDirectory + "\\" + i + ".bmp");
                // dispose the frame when it is no longer required
                //videoFrame.Dispose();
            }

            width = bitmaps[0].Width; // Width value is defining.
            height = bitmaps[0].Height; // Height value is defining.
            capacityHiding = framecount * width * height * 3;
       
        }
    }
}
