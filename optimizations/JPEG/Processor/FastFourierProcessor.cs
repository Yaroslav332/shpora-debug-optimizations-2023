using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JPEG.Images;
using PixelFormat = JPEG.Images.PixelFormat;

namespace JPEG.Processor;

public class FastFourierProcessor : IJpegProcessor
{
    public static readonly FastFourierProcessor Init = new();
    public static readonly float Y_Threshhold = 0.8f;
    public static readonly float C_Threshhold = 0.99f;
    public void Compress(string imagePath, string compressedImagePath)
    {
        using var fileStream = File.OpenRead(imagePath);
        using var bmp = (Bitmap)Image.FromStream(fileStream, false, false);
        var height = bmp.Height;
        var width = bmp.Width;
        Complex[,] y = new Complex[width, height];
        Complex[,] cb = new Complex[width, height];
        Complex[,] cr = new Complex[width, height];
        for (int i = 0; i < width; i++)
        {
            
            for (int j = 0; j < height; j++)
            {
                var c = bmp.GetPixel(i, j);
                var p = new Pixel(c.R, c.G, c.B, PixelFormat.RGB);
                y[i, j] = p.Y;
                cb[i, j] = p.Cb;
                cr[i, j] = p.Cr;
            }
        }
        Fourier(y);
        Fourier(cb);
        Fourier(cr);

        Threshold(y, Y_Threshhold);
        Threshold(cb, C_Threshhold);
        Threshold(cr, C_Threshhold);
        
        var compressionResult = Compress(y, cb, cr);
        compressionResult.Save(compressedImagePath);
    }

    public void Uncompress(string compressedImagePath, string uncompressedImagePath)
    {
        var uncompressionResult = CompressedFourierImage.Load(compressedImagePath);
        var bmp = new Bitmap(uncompressionResult.Width, uncompressionResult.Height);
        Complex[,] y = new Complex[uncompressionResult.Width, uncompressionResult.Height];
        Complex[,] cb = new Complex[uncompressionResult.Width, uncompressionResult.Height];
        Complex[,] cr = new Complex[uncompressionResult.Width, uncompressionResult.Height];
        (y, cb, cr) = Uncompress(uncompressionResult);
        
        ReverseFourier(y);
        ReverseFourier(cb);
        ReverseFourier(cr);

        for (int i = 0; i < uncompressionResult.Width; i++)
        {

            for (int j = 0; j < uncompressionResult.Height; j++)
            {
                var p = new Pixel(y[i, j].Magnitude, cb[i, j].Magnitude, cr[i, j].Magnitude, PixelFormat.YCbCr);
                bmp.SetPixel(i, j,
                    Color.FromArgb((int) Math.Clamp(p.R, 0, 255), (int) Math.Clamp(p.G, 0, 255),
                        (int) Math.Clamp(p.B, 0, 255)));
            }
        }

        bmp.Save(compressedImagePath+".fourierTest.bmp", ImageFormat.Bmp);
    }
    
    private static void Threshold(Complex[,] arr, double coefficient){
        int width = arr.GetLength(0);
        int height = arr.GetLength(1);
        
        List<double> values = new List<double>();
        for (int i = 0; i < width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                values.Add(arr[i, j].Magnitude);
            }
        }

        values.Sort();
        double threshold = values[(int)(values.Count * coefficient)];
        
        for (int i = 0; i < width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (arr[i, j].Magnitude < threshold) arr[i, j] = 0;
            }
        }
    }

    private static void ReverseFourier(Complex[,] arr)
    {
        int width = arr.GetLength(0);
        int height = arr.GetLength(1);
        
            
        for (int i = height-1; i >=0; i--)
        {
            List<Complex> row = new List<Complex>(width);
            for (int j = width-1; j >=0; j--)
            {
                row.Add(arr[j, i]);
            }
            var nRow = ReverseSumFourier(row);
            for (int j = width-1; j >=0; j--)
            {
                arr[j, i] = nRow[j];
            }
        }


        for (int i = width-1; i >=0; i--)
        {
            List<Complex> column = new List<Complex>(height);
            for (int j = height-1; j >=0; j--)
            {
                column.Add(arr[i, j]);
            }

            var nColumn = ReverseSumFourier(column);
            for (int j = height-1; j >=0; j--)
            {
                arr[i, j] = nColumn[j];
            }
        }

        
        
    }
    private static void Fourier(Complex[,] arr)
    {
        int width = arr.GetLength(0);
        int height = arr.GetLength(1);
        
        
        for (int i = 0; i < width; i++)
        {
            List<Complex> column = new List<Complex>(height);
            for (int j = 0; j < height; j++)
            {
                column.Add(arr[i, j]);
            }
            var nColumn = SumFourier(column);
            for (int j = 0; j < height; j++)
            {
                arr[i, j] = nColumn[j];
            }
        }
        
        
        for (int i = 0; i < height; i++)
        {
            List<Complex> row = new List<Complex>(width);
            for (int j = 0; j < width; j++)
            {
                row.Add(arr[j, i]);
            }
            var nRow = SumFourier(row);
            for (int j = 0; j < width; j++)
            {
                arr[j, i] = nRow[j];
            }
        }
    }

    private static List<Complex> ReverseSumFourier(List<Complex> array)
    {
        int count = array.Count;
        if (count == 1)
        {
            return array;
        }

        List<Complex> even = new List<Complex>();
        List<Complex> odd = new List<Complex>();
        for (int i = 0; i < count; i += 2) even.Add(array[i]);
        for (int i = 1; i < count; i += 2) odd.Add(array[i]);
        var e = SumFourier(even);
        var o = SumFourier(odd);
        List<Complex> result = new List<Complex>(count);
        for (int i = 0; i < count; i++) result.Add(Complex.Zero);
        Complex comp = 1;
        var mult = Complex.Exp(-Complex.ImaginaryOne * 2 * Math.PI / count);
        for (int i = 0; i < count / 2; i++)
        {
            result[i] = e[i] + comp*o[i];
            result[i] /= count;
            result[i + count / 2] = e[i] - comp*o[i];
            result[i + count / 2] /= count;
            comp *= mult;
        }

        return result;
    }
    private static List<Complex> SumFourier(List<Complex> array)
    {
        int count = array.Count;
        if (count == 1)
        {
            return array;
        }

        List<Complex> even = new List<Complex>();
        List<Complex> odd = new List<Complex>();
        for (int i = 0; i < count; i += 2) even.Add(array[i]);
        for (int i = 1; i < count; i += 2) odd.Add(array[i]);
        var e = SumFourier(even);
        var o = SumFourier(odd);
        List<Complex> result = new List<Complex>(count);
        for (int i = 0; i < count; i++) result.Add(Complex.Zero);
        Complex comp = 1;
        var mult = Complex.Exp(-Complex.ImaginaryOne * 2 * Math.PI / count);
        for (int i = 0; i < count / 2; i++)
        {
            result[i] = e[i] + comp * o[i];
            result[i + count / 2] = e[i] - comp * o[i];
            comp *= mult;
        }

        return result;
    }

    private static CompressedFourierImage Compress(Complex[,] y, Complex[,] cb, Complex[,] cr)
    {
        List<Tuple<int, int, Complex>> yl = new List<Tuple<int, int, Complex>>();
        List<Tuple<int, int, Complex>> cbl = new List<Tuple<int, int, Complex>>();
        List<Tuple<int, int, Complex>> crl = new List<Tuple<int, int, Complex>>();
        
        int width = y.GetLength(0);
        int height = y.GetLength(1);
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (y[i, j] != 0) yl.Add(new Tuple<int, int, Complex>(i, j, y[i, j]));
                if (cb[i, j] != 0) cbl.Add(new Tuple<int, int, Complex>(i, j, cb[i, j]));
                if (cr[i, j] != 0) crl.Add(new Tuple<int, int, Complex>(i, j, cr[i, j]));
            }
        }

        var allBytes = new List<byte>();
        
        allBytes.AddRange(BitConverter.GetBytes(yl.Count));
        for (int i = 0; i < yl.Count; i++)
        {
            allBytes.AddRange(BitConverter.GetBytes(yl[i].Item1));
            allBytes.AddRange(BitConverter.GetBytes(yl[i].Item2));
            allBytes.AddRange(BitConverter.GetBytes((int)yl[i].Item3.Real));
            allBytes.AddRange(BitConverter.GetBytes((int) yl[i].Item3.Imaginary));
        }
            
        allBytes.AddRange(BitConverter.GetBytes(cbl.Count));
        for (int i = 0; i < cbl.Count; i++)
        {
            allBytes.AddRange(BitConverter.GetBytes(cbl[i].Item1));
            allBytes.AddRange(BitConverter.GetBytes(cbl[i].Item2));
            allBytes.AddRange(BitConverter.GetBytes( (int)cbl[i].Item3.Real));
            allBytes.AddRange(BitConverter.GetBytes( (int)cbl[i].Item3.Imaginary));
        }
            
        allBytes.AddRange(BitConverter.GetBytes(crl.Count));
        for (int i = 0; i < crl.Count; i++)
        {
            allBytes.AddRange(BitConverter.GetBytes(crl[i].Item1));
            allBytes.AddRange(BitConverter.GetBytes(crl[i].Item2));
            allBytes.AddRange(BitConverter.GetBytes( (int)crl[i].Item3.Real));
            allBytes.AddRange(BitConverter.GetBytes( (int)crl[i].Item3.Imaginary));
        }
        
        long bitsCount;
        Dictionary<BitsWithLength, byte> decodeTable;
        var compressedBytes = HuffmanCodec.Encode(allBytes, out decodeTable, out bitsCount);

        return new CompressedFourierImage()
        {
            CompressedBytes = compressedBytes, BitsCount = bitsCount, DecodeTable = decodeTable,
            Height = height, Width = width
        };
    }

    private static (Complex[,], Complex[,], Complex[,]) Uncompress(CompressedFourierImage image)
    {
        var y = new Complex[image.Width, image.Height];
        var cb = new Complex[image.Width, image.Height];
        var cr = new Complex[image.Width, image.Height];
        
        using (var allBytes =
               new MemoryStream(HuffmanCodec.Decode(image.CompressedBytes, image.DecodeTable, image.BitsCount)))
        {
            byte[] arr = new byte[4];
            allBytes.ReadAsync(arr, 0, 4).Wait();
            int count = BitConverter.ToInt32(arr);
            for (int i = 0; i < count; i++)
            {
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int a = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int b = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int real = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int imaginary = BitConverter.ToInt32(arr);
                y[a, b] = new Complex(real, imaginary);
            }
            allBytes.ReadAsync(arr, 0, 4).Wait();
            count = BitConverter.ToInt32(arr);
            for (int i = 0; i < count; i++)
            {
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int a = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int b = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int real = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int imaginary = BitConverter.ToInt32(arr);
                cb[a, b] = new Complex(real, imaginary);
            }
            allBytes.ReadAsync(arr, 0, 4).Wait();
            count = BitConverter.ToInt32(arr);
            for (int i = 0; i < count; i++)
            {
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int a = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int b = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int real = BitConverter.ToInt32(arr);
                allBytes.ReadAsync(arr, 0, 4).Wait();
                int imaginary = BitConverter.ToInt32(arr);
                cr[a, b] = new Complex(real, imaginary);
            }
        }

        return (y, cb, cr);
    }
}