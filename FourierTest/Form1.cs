using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using System.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace FourierTest
{
	public partial class Form1 : Form
	{
		List<Complex> history = new List<Complex>();
		Graphics gra;
		Pen p = new Pen(Brushes.Red);
		bool drag = false;
		Complex[] fourie;
		Complex[] target_fourier;
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

		private Complex[] Normalize(Complex[] list,int points)
		{
			var first = list[0];
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = list[i] - first;
			}

			var length_sum = 0.0;
			for (int i = 1; i < list.Length; i++)
			{
				length_sum += (list[i - 1] - list[i]).Magnitude;
			}

			var length_per_points = length_sum / points;
			var array = new List<Complex>();
			array.Add(Complex.Zero);

			var remainig_length = length_per_points;
			for (int i = 1; i < list.Length;)
			{
				var temp = list[i] - array.Last();
				if (temp.Magnitude > remainig_length)
				{
					array.Add(array.Last() + (temp / temp.Magnitude * remainig_length));
					remainig_length = length_per_points;
				}
				else
				{
					remainig_length -= temp.Magnitude;
					i++;
				}
			}

			for (int i = array.Count - 1; i > 0; i--)
			{
				array.Add(array[i]);
			}

			for (int i = 0; i < array.Count; i++)
			{
				array[i] /= length_sum * 2;
			}

			return array.ToArray();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var normalized = Normalize(history.ToArray(), 30);
			Fourier.Forward(normalized, FourierOptions.Default);
			ShowChart(normalized);
			fourie = normalized;
			
			if(target_fourier != null)
			{
				var error = calc_error(target_fourier, fourie);
				label1.Text = error.ToString();
			}

		}

		private double calc_error(Complex[] a,Complex[] b)
		{
			var sum = 0.0;
			for (int i = 1; i < a.Length && i < b.Length; i++)
			{
				sum += ((a[i] - b[i]) * (a[i] - b[i])).Magnitude;
			}
			return sum;
		}

		private void button3_Click(object sender, EventArgs e)
		{
			target_fourier = fourie;
			Console.Write("private Complex[] shape1 = new Complex[]{");
			foreach (var item in target_fourier)
			{
				Console.Write("new Complex("+item.Real+"," + item.Imaginary + "),");
			}
			Console.WriteLine("};");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			history.Clear();
			gra.Clear(Color.Gray);
			pictureBox1.Refresh();
		}
	}
}
