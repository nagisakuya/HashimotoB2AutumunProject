using Library;
//以下、OpenCvsharpの使用とMatの変換用として追加
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using static Library.Processor;

namespace Program
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var capture = new VideoCapture();
			capture.Open(0);
			if (!capture.IsOpened())
			{
				throw new Exception("capture initialization failed");
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
					continue;
				}

				if (flame.Size().Width > 0)
				{

					processor.ProcessAll(flame, func);

					void func(Dictionary<string, double> list)
					{
						if(port == null)
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

				}

				Cv2.WaitKey();
			}
		}
	}
}
