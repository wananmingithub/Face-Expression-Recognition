using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Cuda;
using System.Diagnostics;
using Emgu.CV.UI;
using System.Security.Permissions;

namespace FaceExpression
{
    public partial class Form1 : Form
    {
        CascadeClassifier haar;
        private Image myImage = null;
        Image<Bgr, Byte> frame = null;     //原始读入图像     
        Image<Gray, Byte> gray = null;     //灰度图像
        Image<Bgr, Byte> smallframe = null;     //裁剪后图像
        double scale = 1.5;

        private VideoCapture capture;
        private bool capture_flag = true;
        private System.Timers.Timer capture_tick;


        public Form1()
        {
            InitializeComponent();
            haar = new CascadeClassifier("haarcascade_frontalface_alt.xml");     //载入人脸级联
            capture_tick = new System.Timers.Timer();
            capture_tick.Interval = 100;
            capture_tick.Enabled = Enabled;
            capture_tick.Stop();
            capture_tick.Elapsed += new System.Timers.ElapsedEventHandler(CaptureProcess);

            textBox2.Visible = false;
           
        }
        //选择图片并调用后台进行分析
        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Visible = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "选择文件";
            ofd.Filter = "*.jpg,*.jpeg,*.bmp,*.gif,*.ico,*.png,*.tif,*.wmf|*.jpg;*.jpeg;*.bmp;*.gif;*.ico;*.png;*.tif;*.wmf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //textBox1.Text = ofd.FileName;
                myImage = System.Drawing.Image.FromFile(ofd.FileName);
                frame = new Image<Bgr, byte>(ofd.FileName);
                //pictureBox1.Image = myImage;
                smallframe = frame.Resize(1 / scale, Emgu.CV.CvEnum.Inter.Linear);//缩放摄像头拍到的大尺寸照片 
                
                if (smallframe.NumberOfChannels == 3)
                {
                    gray = smallframe.Convert<Gray, Byte>(); //Convert it to Grayscale
                    
                }
                gray._EqualizeHist();//均衡化 
                
                
                Rectangle[] rects = haar.DetectMultiScale(gray, 1.3, 3, new Size(30, 30), Size.Empty);
                Image<Bgr, Byte> temp = null;
                foreach (Rectangle r in rects)
                {
                    //This will focus in on the face from the haar results its not perfect but it will remove a majoriy  
                    //of the background noise  
                    Rectangle facesDetected = r;
                    facesDetected.X += (int)(facesDetected.Height * 0.2);
                    facesDetected.Y += (int)(facesDetected.Width * 0.4);
                    facesDetected.Height += (int)(facesDetected.Height * 0.5);
                    facesDetected.Width += (int)(facesDetected.Width * 0.3);
                    temp = frame.GetSubRect(facesDetected);

                    frame.Draw(facesDetected, new Bgr(Color.Red), 3);//绘制检测框  
                }
                imageBox1.Image = temp;
                string fileName = "img" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                temp.Save("./image/" + fileName);

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "classification/classification.exe";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.Arguments = "classification/rgb.prototxt classification/rgb_mean.caffemodel classification/rgb_mean.binaryproto classification/rgb.txt "+ "./image/"+ fileName;
                p.Start();
                p.WaitForExit();
            }

          }
        //分析结果从txt文件中读取输出在界面中
        private void button2_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader("test.txt");
            string nextline = sr.ReadLine();
            //string temp = nextline.Substring(10, 8);
            textBox2.Visible = true;
            if (nextline.Contains("angry"))
            {
                textBox2.Text = "ANGRY";
                //pictureBox1.Image = Image.FromFile("emotion/0.jpg");
            }
            else if (nextline.Contains("disgust"))
            {
                textBox2.Text = "DISGUST";
                //pictureBox1.Image = Image.FromFile("emotion/1.jpg");
            }
            else if (nextline.Contains("fear"))
            {
                textBox2.Text = "NEUTRAL";
                //pictureBox1.Image = Image.FromFile("emotion/2.jpg");
            }
            else if (nextline.Contains("happy"))
            {
                textBox2.Text = "HAPPY";
                //pictureBox1.Image = Image.FromFile("emotion/3.jpg");
            }
            else if (nextline.Contains("neutral"))
            {
                textBox2.Text = "NEUTRAL";
                //pictureBox1.Image = Image.FromFile("emotion/4.jpg");
            }
            else if (nextline.Contains("sad"))
            {
                textBox2.Text = "SAD";
                //pictureBox1.Image = Image.FromFile("emotion/5.jpg");
            }
            else if (nextline.Contains("surprise"))
            {
                textBox2.Text = "SURPRISE";
            }
            else
            {
            }
            //textBox2.Text = temp;
            nextline = sr.ReadLine();
            textBox3.Text = nextline;
            nextline = sr.ReadLine();
            textBox4.Text = nextline;
            nextline = sr.ReadLine();
            textBox5.Text = nextline;
            nextline = sr.ReadLine();
            textBox6.Text = nextline;
            sr.Close();
         }
        private void CaptureProcess(object sender, EventArgs arg)
        {
            Mat temframe = capture.QueryFrame();
            frame = temframe.ToImage<Bgr, Byte>();
            smallframe = frame.Resize(1 / scale, Emgu.CV.CvEnum.Inter.Linear);//缩放摄像头拍到的大尺寸照片  
            if (smallframe.NumberOfChannels == 3)
            {
                gray = smallframe.Convert<Gray, Byte>(); //Convert it to Grayscale  
            }
            gray._EqualizeHist();//均衡化  
            Rectangle[] rects = haar.DetectMultiScale(gray, 1.3, 3, new Size(30, 30), Size.Empty);
            foreach (Rectangle r in rects)
            {
                //This will focus in on the face from the haar results its not perfect but it will remove a majoriy  
                //of the background noise  
                Rectangle facesDetected = r;
                facesDetected.X += (int)(facesDetected.Height * 0.4);
                facesDetected.Y += (int)(facesDetected.Width * 0.4);
                facesDetected.Height += (int)(facesDetected.Height * 0.5);
                facesDetected.Width += (int)(facesDetected.Width * 0.3);

                frame.Draw(facesDetected, new Bgr(Color.Red), 3);//绘制检测框  
            }
            imageBox1.Image = frame;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (capture == null)
            {
                try
                {
                    capture = new VideoCapture();
                }
                catch (NullReferenceException except)
                {
                    MessageBox.Show(except.Message);
                }
            }

            if (capture != null)
            {
                if (capture_flag)
                {
                    //Application.Idle += new EventHandler(CaptureProcess);  
                    capture_tick.Start();
                    button3.Text = "Stop";
                }
                else
                {
                    //Application.Idle -= new EventHandler(CaptureProcess);  
                    capture_tick.Stop();
                    button3.Text = "Start";
                }
                capture_flag = !capture_flag;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Mat temframe = capture.QueryFrame();
            frame = temframe.ToImage<Bgr, Byte>();
            smallframe = frame.Resize(1 / scale, Emgu.CV.CvEnum.Inter.Linear);//缩放摄像头拍到的大尺寸照片  
            if (smallframe.NumberOfChannels == 3)
            {
                gray = smallframe.Convert<Gray, Byte>(); //Convert it to Grayscale  
            }
            gray._EqualizeHist();//均衡化  
            Rectangle[] rects = haar.DetectMultiScale(gray, 1.3, 3, new Size(30, 30), Size.Empty);
            foreach (Rectangle r in rects)
            {
                //This will focus in on the face from the haar results its not perfect but it will remove a majoriy  
                //of the background noise  
                Rectangle facesDetected = r;
                facesDetected.X += (int)(facesDetected.Height * 0.6);
                facesDetected.Y += (int)(facesDetected.Width * 0.4);
                facesDetected.Height += (int)(facesDetected.Height * 0.5);
                facesDetected.Width += (int)(facesDetected.Width * 0.3);

                frame.Draw(facesDetected, new Bgr(Color.Red), 3);//绘制检测框  
            }
            imageBox1.Image = frame;
            //实时拍照照片保存
            string fileName = "img" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            frame.Save("./image/"+ fileName);

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "classification/classification.exe";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = "classification/rgb.prototxt classification/rgb_mean.caffemodel classification/rgb_mean.binaryproto classification/rgb.txt " +"./image/"+fileName;
            p.Start();
            p.WaitForExit();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
