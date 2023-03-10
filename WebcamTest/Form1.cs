using Library;
using MathNet.Numerics;
//以下、OpenCvsharpの使用とMatの変換用として追加
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Library.Processor;

namespace HashimotoB2Autumun
{
	public partial class Form1 : Form
	{
		bool stop = false;
		public Form1()
		{
			InitializeComponent();

			var chartArea = chart1.ChartAreas["ChartArea1"];
			chartArea.AxisX.Minimum = 0;
			chartArea.AxisX.Maximum = 100;
			chartArea.AxisY.Minimum = 0;
			chartArea.AxisY.Maximum = 0.5;
			chartArea.AxisY.Interval = 0.05;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//PictureBoxのサイズに合わせて表示
			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
		}


		private void button1_Click(object sender, EventArgs e)
		{
			var capture = new VideoCapture();
			capture.Open(((int)numericUpDown1.Value));
			if (!capture.IsOpened())
			{
				return;
			}

			var port = TryStartSerial();
			var processor = new Processor();

			while (true)
			{
				var flame = new Mat();
				capture.Read(flame);
				if (flame.Empty())
				{
					Task.Delay(1);
					break;
				}

				if (flame.Size().Width > 0)
				{

					pictureBox1.Image = BitmapConverter.ToBitmap(flame);
					pictureBox1.Refresh();

					processor.ProcessAll(flame, func, func2);

					void func(Dictionary<string, double> list)
					{
						if (list["clover"] < 0.21)
						{
							label1.Text += "clover\n";
						}
						else if (list["alpha"]<0.21){
							label1.Text += "a\n";
						}
						label1.Text += "clover="+ list["clover"].Round(3) + ",a=" + list["alpha"].Round(3) + "\n";
						if (port == null)
						{
							port = TryStartSerial();
						}

						if (list["clover"] < 0.21)
						{
							Console.WriteLine("C!");
							if (port != null)
								port.Write("c");
						}
						else if (list["alpha"] < 0.21)
						{
							Console.WriteLine("A!");
							if (port != null)
								port.Write("o");
						}
					}

					void func2(Mat image)
					{
						pictureBox2.Image = BitmapConverter.ToBitmap(image);
						pictureBox2.Refresh();
					}

				}

				Cv2.WaitKey();

				if (this.IsDisposed || this.stop)
				{
					this.stop = false;
					pictureBox1.Image = null;
					pictureBox1.Refresh();
					pictureBox2.Image = null;
					pictureBox2.Refresh();
					break;
				}

			}
		}
		private void button2_Click(object sender, EventArgs e)
		{
			label1.Text = "";
		}

		private void button2_Click_1(object sender, EventArgs e)
		{
			this.stop = true;
		}
	}
}