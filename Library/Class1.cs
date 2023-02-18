using MathNet.Numerics.IntegralTransforms;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Library
{
	public class Processor
	{
		public class Shape
		{
			public string name { get; private set; }
			public Complex[] shape { get; private set; }
			public static Shape LoadFile(string name, string path)
			{
				BinaryFormatter bf = new BinaryFormatter();
				var shape = bf.Deserialize(File.OpenRead(path)) as Complex[];
				return new Shape()
				{
					name = name,
					shape = shape,
				};
			}
		}
		private static List<Shape> LoadAllShape(string directry_path)
		{
			var shapes = new List<Shape>();
			var files = Directory.GetFiles(directry_path);
			foreach (var file in files)
			{
				if (file.EndsWith(".shape"))
				{
					shapes.Add(Shape.LoadFile(Path.GetFileName(file).Replace(".shape", ""), file));
				}
			}
			return shapes;
		}

		private List<Shape> shapes = LoadAllShape("./shapes");

		public static double Moved_length(Complex[] list)
		{
			var length_sum = 0.0;
			for (int i = 1; i < list.Length; i++)
			{
				length_sum += (list[i - 1] - list[i]).Magnitude;
			}
			return length_sum;
		}
		public static double Calc_error(Complex[] a, Complex[] b)
		{
			var sum = 0.0;
			for (int i = 1; i < a.Length && i < b.Length; i++)
			{
				sum += ((a[i] - b[i]) * (a[i] - b[i])).Magnitude;
			}
			return sum;
		}
		public static Complex[] Normalize(Complex[] list, int points)
		{
			var first = list[0];
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = list[i] - first;
			}

			var length_sum = Moved_length(list);

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
		public static SerialPort TryStartSerial()
		{
			SerialPort port = new SerialPort();
			string[] PortList = SerialPort.GetPortNames();
			//Console.WriteLine("PortList=" + String.Join(",", PortList));
			if (PortList.Length > 0)
			{
				port.PortName = PortList[0];
				port.Open();
			}
			else
			{
				port = null;
			}
			return port;
		}

		private int framecount = 0;
		private List<Complex> history = new List<Complex>();
		public int FPS { set; get; } = 60;
		private double err_minimum = double.MaxValue;
		private Shape target_shape = null;
		private Dictionary<string,double> shapes_minimam_error = new Dictionary<string, double>();
		Point center = new Point();

		public void ProcessAll(Mat _flame, Action<Dictionary<string, double>> func = null, Action<Mat> func2 = null)
		{
			var image = _flame.CvtColor(ColorConversionCodes.BGR2GRAY);

			var binalized_image = image.Threshold(70, 255, ThresholdTypes.Binary);

			var moments = binalized_image.Moments();
			if (moments.M00 != 0)
			{
				center = new Point(moments.M10 / moments.M00, moments.M01 / moments.M00);
			}
			history.Add(new Complex(_flame.Width - center.X, center.Y));

			binalized_image = binalized_image.CvtColor(ColorConversionCodes.GRAY2RGB);
			binalized_image.DrawMarker(center, new Scalar(0, 0, 255),markerSize:100);

			if(func2 != null)
			func2(binalized_image);

			if (framecount++ % 3 == 0 && history.Count > FPS)
			{
				var moved_length_recently = Moved_length(history.GetRange(history.Count - (FPS / 3) - 1, FPS / 3).ToArray());

				if (moved_length_recently > 20)
				{
					var sample = Normalize(history.ToArray(), 30);
					Fourier.Forward(sample, FourierOptions.Default);
					foreach (var shape in shapes)
					{
						var error = Calc_error(sample, shape.shape);
						if(!shapes_minimam_error.ContainsKey(shape.name) || shapes_minimam_error[shape.name] > error)
						{
							shapes_minimam_error[shape.name] = error;
						}
						if (error < err_minimum)
						{
							err_minimum = error;
							target_shape = shape;
						}
					}
				}
				else
				{
					var total_moved_length = Moved_length(history.ToArray());
					if (total_moved_length > 300 && err_minimum < 0.35)
					{
						if (target_shape != null && func != null) func(shapes_minimam_error);

					}

					target_shape = null;
					err_minimum = double.MaxValue;
					shapes_minimam_error.Clear();
					history.Clear();
				}
			}
		}
		//public List<(double, string)> check_shapes(Complex[] sample){
		//	var list = new List<(double, string)>();
		//	foreach (var shape in shapes)
		//	{
		//		var error = Calc_error(sample, shape.shape);
		//		list.Add((error,shape.name));
		//	}
		//	return list;
		//}
	}
}
