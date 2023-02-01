using System;
using System.Collections.Generic;
using System.Linq;

using System.Numerics;

namespace ClassLibrary1
{
	public class Processor
	{
        static double Moved_length(Complex[] list)
        {
            var length_sum = 0.0;
            for (int i = 1; i < list.Length; i++)
            {
                length_sum += (list[i - 1] - list[i]).Magnitude;
            }
            return length_sum;
        }
        static double Calc_error(Complex[] a, Complex[] b)
        {
            var sum = 0.0;
            for (int i = 1; i < a.Length && i < b.Length; i++)
            {
                sum += ((a[i] - b[i]) * (a[i] - b[i])).Magnitude;
            }
            return sum;
        }
        static Complex[] Normalize(Complex[] list, int points)
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
    }
}
