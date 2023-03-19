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
		double[,] priorSum1 = new double[height, width];
		double[,] priorSum2 = new double[width, height];
		
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				priorSum1[i, j] = 0;
				for (int k = 0; k < width; k++)
				{
					priorSum1[i, j] += hValues[k, i] * input[j, k];
				}
			}
		}

		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				priorSum2[i, j] = 0;
				for (int k = 0; k < width; k++)
				{
					priorSum2[i, j] += priorSum1[j, k] * wValues[k, i];
				}
			}
		}
		
		coeffs[0, 0] = priorSum2[0, 0] * beta / 2;

		var betaBaseAlpha = beta * _basicAlpha;
		
		for (int i2 = 1; i2 < height; i2++)
		{
			coeffs[0, i2] = priorSum2[0, i2] * betaBaseAlpha;
		}

		for (int i1 = 1; i1 < width; i1++)
		{
			coeffs[i1, 0] = priorSum2[i1,0] * betaBaseAlpha;
			
			for (int i2 = 1; i2 < height; i2++)
			{
				coeffs[i1, i2] = priorSum2[i1, i2] * beta;
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

		double[] priorSum1Values = new double[l1];
		for (int i = 0; i < l1; i++)
			priorSum1Values[i] = coeffs[0, 0] * wValues[i, 0] / 2;
		double[] priorSum2Values = new double[l0];
		for (int i = 0; i < l0; i++)
		{
			priorSum2Values[i] = 0;
			for (int j = 1; j < l0; j++)
			{
				priorSum2Values[i] += coeffs[0, j] * hValues[i, j];
			}
		}

		double[,] priorSum3Values = new double[l0, l1];
		for (int i = 0; i < l0; i++)
		{
			for (int j = 0; j < l1; j++)
			{
				priorSum3Values[i, j] = 0;
				for (int k = 1; k < l0; k++)
				{
					priorSum3Values[i,j] += coeffs[j, k] * hValues[i, k];
				}
			}
		}
		for (var i1 = 0; i1 < l1; i1++)
		{
			for (var i2 = 0; i2 < l0; i2++)
			{
				double sum1 = priorSum1Values[i1] * hValues[i2, 0];
				double sum2 = priorSum2Values[i2] * wValues[i1, 0];
				double sum22 = 0;
				for (int i3 = 1; i3 < l1; i3++)
				{
					sum22 += coeffs[i3, 0] * wValues[i1, i3];
					sum1 += priorSum3Values[i2, i3] * wValues[i1, i3];
				}
				sum22 *= hValues[i2, 0];
				output[i1, i2] = (sum1 + (sum2 + sum22) * _basicAlpha) * beta;
			}
		}
	}
	
	private static readonly double _basicAlpha = 1 / Math.Sqrt(2);

	private static double Beta(int height, int width)
	{
		return 1d / width + 1d / height;
	}
}