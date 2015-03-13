using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using System.IO; // Add Kinect Konect 的Function. 

namespace Kinect_Image_Basic_2
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor sensor;
        private short[] colorPixels;
        private WriteableBitmap depBitmap;
  
        private DepthImagePixel[] depthPixels; // 定義影像深度, 記憶體的長度


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Message_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //檢查連接狀況
            foreach (var potentisalSensor in KinectSensor.KinectSensors)
            {
                if (potentisalSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentisalSensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                

                this.colorPixels = new short[this.sensor.DepthStream.FramePixelDataLength];
                // 把深度的資料用影像顯示出來,轉給人看

                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                // Save User 編號跟距離 = 實際數字, 轉給電腦看

                this.depBitmap = new WriteableBitmap(
                                                        this.sensor.DepthStream.FrameWidth,
                                                        this.sensor.DepthStream.FrameHeight,
                                                        96.0, 96.0,
                                                        PixelFormats.Gray8,null
                                                    );

                Image.Source = this.depBitmap; // 扔到WPF 的Image 

                this.sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);

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
            else
            {
                Message.Content = "Kinect Don't Connect. ";
            }

        }

        // 灰階處理
        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            //hrow new NotImplementedException();

            using(DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {

                if (depthFrame != null)
                {
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    for (int i = 0; i < this.depthPixels.Length; i++)
                    {
                        this.colorPixels[i] = this.depthPixels[i].Depth;
                    }

                    this.depBitmap.WritePixels(
                                                new Int32Rect(
                                                                0, 0,
                                                                this.depBitmap.PixelWidth,
                                                                this.depBitmap.PixelHeight
                                                             ),
                                                this.colorPixels,
                                                this.depBitmap.PixelWidth * sizeof(short),
                                                0
                                                );
                }

            }
        }

   

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        // 算位址
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Image);

            // 四捨五入
            int x = (int)(p.X + 0.5);
            int y = (int)(p.Y + 0.5);

            int idx = y * this.depBitmap.PixelWidth + x;

            short d = this.depthPixels[idx].Depth;
            
            Address.Content = "(" + x.ToString() + "," + y.ToString() + ")" + d.ToString();
        }
    }
}
