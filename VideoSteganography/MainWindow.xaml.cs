using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
        private int counfOfChracter;
        private int unimportant = 0;
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
            btnHide.IsEnabled = true;
        }

        private void BtnHide_Click(object sender, RoutedEventArgs e)
        {
            InfoHiding = new TextRange(txtHide.Document.ContentStart, txtHide.Document.ContentEnd).Text;
            if (btnDelSpaces.IsChecked == true)
            {
                InfoHiding = Regex.Replace(InfoHiding, @"\s+", "");
            }
            //InfoHiding = File.ReadAllText(@"C:\Users\deniz\OneDrive\Masaüstü\5milyonlukmetin.txt");
            string firstPart = String.Empty, elapsedPart = String.Empty;
            countFrame = 0;
            bitmapsEmbedded = Extensions.Clone(bitmaps);
            try
            {
                do
                {
                    bool isBigger = InfoHiding.Length > capacityPerFrame;
                    firstPart = InfoHiding.Substring(0, isBigger ? (int)capacityPerFrame : InfoHiding.Length);
                    InfoHiding = isBigger ? InfoHiding.Substring((int)capacityPerFrame, InfoHiding.Length - (int)capacityPerFrame) : String.Empty;
                    Steganography.embedText(firstPart, bitmapsEmbedded[countFrame]);
                    countFrame++;
                } while (!String.IsNullOrEmpty(InfoHiding));
                txtStatus.Text = $"Status : Congrulations! You concealed your information successfully and quality values of new video was calculated.";
                createVideo(bitmapsEmbedded);
                btnSolve.IsEnabled = true;
                fillValues();
            }
            catch (Exception)
            {

                txtStatus.Text = "Status : Something is wrong! You didn't conceal your information successfully!";
            }
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            InfoSolved = String.Empty;
            foreach (var bitmap in bitmapsEmbedded)
            {
                InfoSolved += Steganography.extractText(bitmap);
            }
            txtStatus.Text = $"Status : Your concealed information is solved successfully!";
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
        

        private void BtnDelSpaces_Click(object sender, RoutedEventArgs e)
        {
            setCountOfChracter();
            txtCountChracter.Text = $"Chracter count of your information : {counfOfChracter}";
        }

        private void TxtHide_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (unimportant > 3)
            {
                RichTextBox txtInfo = (sender as RichTextBox);
                setCountOfChracter();
                txtCountChracter.Text = $"Chracter count of your information : {counfOfChracter}";
            }
            unimportant++;
        }

        private void setCountOfChracter()
        {
            if (btnDelSpaces.IsChecked == true)
            {
                counfOfChracter = Regex.Replace(new TextRange(txtHide.Document.ContentStart, txtHide.Document.ContentEnd).Text, @"\s+", "").Count();
            }
            else
            {
                counfOfChracter = new TextRange(txtHide.Document.ContentStart, txtHide.Document.ContentEnd).Text.Trim().Count();
            }
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
            txtStatus.Text = $"Status : You have chance that can hide {capacityHiding} chracters.";
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

        private void fillValues()
        {
            txtMSE.Text = Steganography.calcMSE(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList()).ToString("0.0000000");
            lblMSE.Visibility = Visibility.Visible;
            txtPSNR.Text = Steganography.calcPSNR(Steganography.calcMSE(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList())).ToString("0.0000000");
            lblPSNR.Visibility = Visibility.Visible;
            txtAD.Text = Steganography.calcAD(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList()).ToString("0.0000000");
            lblAD.Visibility = Visibility.Visible;
            txtNC.Text = Steganography.calcNC(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList()).ToString("0.0000000");
            lblNC.Visibility = Visibility.Visible;
            txtNAE.Text = Steganography.calcNAE(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList()).ToString("0.0000000");
            lblNAE.Visibility = Visibility.Visible;
            txtSC.Text = Steganography.calcSC(bitmaps.Take(countFrame).ToList(), bitmapsEmbedded.Take(countFrame).ToList()).ToString("0.0000000");
            lblSC.Visibility = Visibility.Visible;
        }
    }

    static class Extensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}
