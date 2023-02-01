using Library;
//以下、OpenCvsharpの使用とMatの変換用として追加
using OpenCvSharp;
using System;
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

					void func(string str)
					{
						if (port != null)
						{
							if (str == "reset")
							{
								port.Write("c");
							}

							if (str == "door")
							{
								port.Write("o");
							}
						}
						else
						{
							port = TryStartSerial();
						}
					}

				}

				Cv2.WaitKey();
			}
		}
	}
}
