using System;
using System.Linq;

namespace JPEG.Utilities;

public static class MathEx
{
	public static double Sum(int from, int to, Func<int, double> function)
	{
		double s = 0;
		for (int i = from; i < to; i++)
		{
			s += function(i);
		}

		return s;
	}

	public static double SumByTwoVariables(int from1, int to1, int from2, int to2, Func<int, int, double> function)
	{
		double s = 0;
		for (int i = from1; i < to1; i++)
		{
			for (int j = from2; j < to2; j++)
			{
				s += function(i, j);
			}
		}
		return s;
	}

	public static void LoopByTwoVariables(int from1, int to1, int from2, int to2, Action<int, int> function)
	{
		for (int i = from1; i < to1; i++)
		{
			for (int j = from2; j < to2; j++)
			{
				function(i, j);
			}
		}
	}
}