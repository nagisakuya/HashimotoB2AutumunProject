using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using static Library.Processor;

namespace FourierTest
{
	public partial class Form1 : Form
	{
		private List<Complex> history = new List<Complex>();
		private Graphics gra;
		private Pen p = new Pen(Brushes.Red);
		private bool drag = false;
		private Complex[] fourie;
		private Complex[] target_fourier;
		public Form1()
		{
			InitializeComponent();

			var chartArea = chart1.ChartAreas["ChartArea1"];
			chartArea.AxisX.Minimum = 1;
			chartArea.AxisX.Maximum = 15;
			chartArea.AxisY.Minimum = -1;
			chartArea.AxisY.Maximum = 1;
			chartArea.AxisY.Interval = 0.2;
			chart1.Series.Clear();
			var re = chart1.Series.Add("Y");
			var im = chart1.Series.Add("X");
			re.ChartType = SeriesChartType.Line;
			im.ChartType = SeriesChartType.Line;

			pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
			gra = Graphics.FromImage(pictureBox1.Image);
			gra.Clear(Color.Gray);
		}
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			drag = true;
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			drag = false;
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (!drag) return;
			history.Add(new Complex(e.X, e.Y));
			pictureBox1.Refresh();

			if (history.Count <= 1) return;
			var last = history[history.Count - 2];
			gra.DrawLine(p, new Point((int)(last.Real), (int)(last.Imaginary)), new Point(e.X, e.Y));
		}
		private void ShowChart(Complex[] list)
		{
			chart1.Series["Y"].Points.Clear();
			chart1.Series["X"].Points.Clear();

			for (int i = 1; i < list.Length; i++)
			{
				chart1.Series["Y"].Points.AddXY(i, list[i].Real);
				chart1.Series["X"].Points.AddXY(i, list[i].Imaginary);
			}
		}



		private void button2_Click(object sender, EventArgs e)
		{
			var normalized = Normalize(history.ToArray(), 30);
			Fourier.Forward(normalized, FourierOptions.Default);
			ShowChart(normalized);
			fourie = normalized;

			if (target_fourier != null)
			{
				var error = Calc_error(target_fourier, fourie);
				label1.Text = error.ToString();
			}

		}

		private void button3_Click(object sender, EventArgs e)
		{
			target_fourier = fourie;
			BinaryFormatter bf = new BinaryFormatter();
			var file = File.Create("./shape.shape");
			bf.Serialize(file, fourie);
			file.Close();
			//Console.Write("private Complex[] shape1 = new Complex[]{");
			//foreach (var item in target_fourier)
			//{
			//	Console.Write("new Complex(" + item.Real + "," + item.Imaginary + "),");
			//}
			//Console.WriteLine("};");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			history.Clear();
			gra.Clear(Color.Gray);
			pictureBox1.Refresh();
		}
	}
}
