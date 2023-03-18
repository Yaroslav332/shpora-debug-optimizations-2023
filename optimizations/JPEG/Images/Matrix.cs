using System.Drawing;

namespace JPEG.Images;

class Matrix
{
    public readonly Pixel[,] Pixels;
    public readonly int Height;
    public readonly int Width;

    public Matrix(int height, int width)
    {
        Height = height;
        Width = width;

        Pixels = new Pixel[height, width];
        for (var i = 0; i < height; ++i)
        for (var j = 0; j < width; ++j)
            Pixels[i, j] = new Pixel(0, 0, 0, PixelFormat.RGB);
    }

    public static explicit operator Matrix(Bitmap bmp)
    {
        var height = bmp.Height - bmp.Height % 8;
        var width = bmp.Width - bmp.Width % 8;
        var matrix = new Matrix(height, width);

        unsafe
        {
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            byte* row = (byte*) bmpData.Scan0;
            var stride = bmpData.Stride;
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    var t = i * 3;
                    matrix.Pixels[j, i] = new Pixel(row[t + 2], row[t + 1], row[t], PixelFormat.RGB);
                }

                row += stride;
            }
            bmp.UnlockBits(bmpData);
        }

        return matrix;
    }

    public static explicit operator Bitmap(Matrix matrix)
    {
        var bmp = new Bitmap(matrix.Width, matrix.Height);
        unsafe
        {
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            byte* row = (byte*) bmpData.Scan0;
            var stride = bmpData.Stride;
            for (var j = 0; j < bmp.Height; j++)
            {
                for (var i = 0; i < bmp.Width; i++)
                {
                    var pixel = matrix.Pixels[j, i];
                    var t = i * 4;
                    row[t] = (byte) pixel.B;
                    row[t+1] = (byte) pixel.G;
                    row[t+2] = (byte) pixel.R;
                    row[t + 3] = 255;
                }
                row += stride;
            }
            bmp.UnlockBits(bmpData);
        }

        return bmp;
    }

    public static int ToByte(double d)
    {
        var val = (int)d;
        if (val > byte.MaxValue)
            return byte.MaxValue;
        if (val < byte.MinValue)
            return byte.MinValue;
        return val;
    }
}