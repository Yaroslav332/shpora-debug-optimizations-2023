using System;
using JPEG.Processor;

namespace JPEG;

public class DCT
{
	private static readonly double[,] BasisValues;

	static DCT()
	{
		var count = JpegProcessor.DCTSize;
		double[] cos = new double[count];
		double[] sin = new double[count];
		double bSin = Math.Sin(Math.PI / count);
		double bCos = Math.Cos(Math.PI / count);
		cos[0] = Math.Cos(Math.PI / (2 * count));
		sin[0] = Math.Sin(Math.PI / (2 * count));
		for (int i = 1; i < count; i++)
		{
			(cos[i], sin[i]) =
			(
				cos[i - 1] * bCos - sin[i - 1] * bSin,
				cos[i - 1] * bSin + sin[i - 1] * bCos
			);
		}

		double[,] valuesCos = new double[count, count];
		double[,] valuesSin = new double[count, count];
		for (int i = 0; i < count; i++)
		{
			valuesCos[i, 0] = 1;
			valuesSin[i, 0] = 0;
		}
		for (int i = 0; i < count; i++)
		{
			for (int j = 1; j < count; j++)
			{
				(valuesCos[i, j], valuesSin[i, j]) =
					(valuesCos[i, j - 1] * cos[i] - valuesSin[i, j - 1] * sin[i],
						valuesCos[i, j - 1] * sin[i] + valuesSin[i, j - 1] * cos[i]);
			}
		}

		BasisValues = valuesCos;
	}

	public static double[,] DCT2D(double[,] input)
	{
		
		var height = input.GetLength(0);
		var width = input.GetLength(1);
		var coeffs = new double[width, height];
		var wValues = BasisValues;
		var hValues = BasisValues;
		
		var beta = Beta(height, width);
		
		double sum = 0;
		for (int i3 = 0; i3 < width; i3++)
		{
			for (int i4 = 0; i4 < height; i4++)
			{
				sum += hValues[i4, 0] * wValues[i3, 0] * input[i3, i4];
			}
		}

		coeffs[0, 0] = sum * beta / 2;
		
		for (int i2 = 1; i2 < height; i2++)
		{
			sum = 0;
			for (int i3 = 0; i3 < width; i3++)
			{
				for (int i4 = 0; i4 < height; i4++)
				{
					sum += hValues[i4, i2] * wValues[i3, 0] * input[i3, i4];
				}
			}

			coeffs[0, i2] = sum * beta * _basicAlpha;
		}

		for (int i1 = 1; i1 < width; i1++)
		{
			sum = 0;
			for (int i3 = 0; i3 < width; i3++)
			{
				for (int i4 = 0; i4 < height; i4++)
				{
					sum += hValues[i4, 0] * wValues[i3, i1] * input[i3, i4];
				}
			}

			coeffs[i1, 0] = sum * beta * _basicAlpha;
			for (int i2 = 1; i2 < height; i2++)
			{
				sum = 0;
				for (int i3 = 0; i3 < width; i3++)
				{
					for (int i4 = 0; i4 < height; i4++)
					{
						sum += hValues[i4, i2] * wValues[i3, i1] * input[i3, i4];
					}
				}

				coeffs[i1, i2] = sum * beta;
			}
		}
		return coeffs;
	}

	public static void IDCT2D(double[,] coeffs, double[,] output)
	{
		var l0 = coeffs.GetLength(0);
		var l1 = coeffs.GetLength(1);
		var wValues = BasisValues;
		var hValues = BasisValues;
		var beta = Beta(l0, l1);
		
		for (var i1 = 0; i1 < l1; i1++)
		{
			for (var i2 = 0; i2 < l0; i2++)
			{
				double sum = 0;
				sum += coeffs[0, 0] * wValues[i1, 0] * hValues[i2, 0] / 2;
				for (int i4 = 1; i4 < l0; i4++)
				{
					sum += coeffs[0, i4] * wValues[i1, 0] * hValues[i2, i4] * _basicAlpha;
				}

				for (int i3 = 1; i3 < l1; i3++)
				{
					sum += coeffs[i3, 0] * wValues[i1, i3] * hValues[i2, 0] * _basicAlpha;
					for (int i4 = 1; i4 < l0; i4++)
					{
						sum += coeffs[i3, i4] * wValues[i1, i3] * hValues[i2, i4];
					}
				}

				output[i1, i2] = sum * beta;
			}
		}
	}
	
	private static readonly double _basicAlpha = 1 / Math.Sqrt(2);

	private static double Beta(int height, int width)
	{
		return 1d / width + 1d / height;
	}
}