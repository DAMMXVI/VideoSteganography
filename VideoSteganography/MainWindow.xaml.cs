using Accord.Math;
using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        #region declare variables
        private string filePath = "";
        private int width, height;
        private long framecount, capacityHiding, capacityPerFrame;
        private List<Bitmap> bitmaps = new List<Bitmap>();
        private List<Bitmap> bitmapsEmbedded = new List<Bitmap>();
        private bool isEnded = false, isEnded2 = false;
        private string InfoHiding, InfoSolved;
        private int countFrame, bitRate;
        private double frameRate;
        private VideoCodec codecName;
        private string videoDirectory = @"C:\Users\deniz\OneDrive\Masaüstü\SCHOOL REMOTE\Stenografi\VideoSteganography\VideoSteganography\Videos";
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectedVideo_Loaded(object sender, RoutedEventArgs e)
        {
            selectedVideo.Pause();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            selectedVideo.Play();
            btnPlay.IsEnabled = false;
        }

        private void BtnReplay_Click(object sender, RoutedEventArgs e)
        {
            selectedVideo.Position = new TimeSpan(0, 0, 0);
            selectedVideo.Play();
            isEnded = false;
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (GetMediaState(selectedVideo) == MediaState.Play && !isEnded)
            {
                selectedVideo.Pause();
                btnPlay.IsEnabled = true;
            }
        }

        private void BtnPlay2_Click(object sender, RoutedEventArgs e)
        {
            newVideo.Play();
            btnPlay2.IsEnabled = false;
        }

        private void BtnReplay2_Click(object sender, RoutedEventArgs e)
        {
            newVideo.Position = new TimeSpan(0, 0, 0);
            newVideo.Play();
            isEnded2 = false;
        }

        private void BtnPause2_Click(object sender, RoutedEventArgs e)
        {
            if (GetMediaState(newVideo) == MediaState.Play && !isEnded2)
            {
                newVideo.Pause();
                btnPlay2.IsEnabled = true;
            }
        }

        private void RichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtHide.Document.Blocks.Clear();
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            InfoHiding = new TextRange(txtHide.Document.ContentStart, txtHide.Document.ContentEnd).Text;
            string firstPart = String.Empty, elapsedPart = String.Empty;
            countFrame = 0;
            bitmapsEmbedded = bitmaps;
            try
            {
                do
                {
                    bool isBigger = InfoHiding.Length > capacityPerFrame;
                    firstPart = InfoHiding.Substring(0, isBigger ? (int)capacityPerFrame : InfoHiding.Length);
                    InfoHiding = isBigger ? InfoHiding.Substring((int)capacityPerFrame, InfoHiding.Length - (int)capacityPerFrame) : String.Empty;
                    bitmapsEmbedded[countFrame] = (Steganography.embedText(firstPart, bitmaps[countFrame]));
                    countFrame++;
                } while (!String.IsNullOrEmpty(InfoHiding));
                txtStatus.Text = "Congrulations! You concealed your information successfully!";
                createVideo(bitmapsEmbedded);

            }
            catch (Exception)
            {

                txtStatus.Text = "Something is wrong! You didn't conceal your information successfully!";
            }
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            InfoSolved = String.Empty;
            foreach (var bitmap in bitmapsEmbedded)
            {
                InfoSolved += Steganography.extractText(bitmap);
            }
            txtStatus.Text = $"Your concealed information is solved successfully!";
            txtSolved.IsEnabled = true;
            txtSolved.Document.Blocks.Clear();
            txtSolved.Document.Blocks.Add(new Paragraph(new Run(InfoSolved)));
        }

        private void BtnSelectVideo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".mp4";
            dialog.Filter = "Video Files|*.mp4;*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (dialog.ShowDialog() == true)
            {
                FillTextBoxes(dialog);
            }
        }

        private void SelectedVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            selectedVideo.Pause();
            btnPlay.IsEnabled = btnPause.IsEnabled = true;
        }

        private void NewVideo_MediaOpened(object sender, RoutedEventArgs e)
        {
            newVideo.Pause();
            btnPlay2.IsEnabled = btnPause2.IsEnabled = true;
        }

        private void NewVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            btnReplay2.IsEnabled = true;
            btnPlay2.IsEnabled = false;
            isEnded2 = true;
        }

        private void NewVideo_Loaded(object sender, RoutedEventArgs e)
        {
            newVideo.Pause();
        }

        private void SelectedVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            btnReplay.IsEnabled = true;
            btnPlay.IsEnabled = false;
            isEnded = true;
        }

        private void FetchPixels() // Pixels are fetching.
        {
            VideoFileReader reader = new VideoFileReader();
            reader.Open(filePath);
            framecount = reader.FrameCount;
            frameRate = reader.FrameRate.Value;
            bitRate = reader.BitRate;
            codecName = (VideoCodec)Enum.Parse(typeof(VideoCodec), reader.CodecName.ToUpper());
            for (int i = 0; i < framecount; i++)
            {
                Bitmap eachFrame = reader.ReadVideoFrame();
                bitmaps.Add(eachFrame);
            }

            width = reader.Width; // Width value is defining.
            height = reader.Height; // Height value is defining.
            capacityHiding = (framecount * width * height * 3) / 8;
            capacityPerFrame = capacityHiding / framecount;
        }

        private void FillTextBoxes(Microsoft.Win32.OpenFileDialog dialog)
        {
            filePath = txtVideoPath.Text = dialog.FileName;
            changeVideoSource(selectedVideo, filePath);
            selectedVideo.LoadedBehavior = MediaState.Manual;
            txtName.Text = dialog.SafeFileNames[0];
            FetchPixels();
            txtWidth.Text = width.ToString();
            txtHeight.Text = height.ToString();
            txtType.Text = System.IO.Path.GetExtension(filePath);
            txtSize.Text = (new FileInfo(filePath).Length / 1024).ToString() + "KB";
            txtStatus.Text = $"You have chance that can hide {capacityHiding} chracters.";
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        private void createVideo(List<Bitmap> imageList)
        {
            using (var writer = new VideoFileWriter())          // Initialize a VideoFileWriter
            {
                string path = $@"{videoDirectory}\{Guid.NewGuid()}.mp4";
                writer.Open(path, imageList[0].Width, imageList[0].Height, (int)frameRate, VideoCodec.MPEG4, bitRate);              // Start output of video
                foreach (var bmp in imageList)
                {
                    writer.WriteVideoFrame(bmp);
                }
                writer.Close();                                 // Close VideoFileWriter
                changeVideoSource(newVideo, path);
            }
        }

        private void changeVideoSource(MediaElement mediaElement, string url)
        {
            mediaElement.Source = new Uri(url);
        }
    }
}
