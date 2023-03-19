namespace JPEG.Images;

public class Pixel
{
    private readonly PixelFormat format;
    private bool isRGB;

    public Pixel(double firstComponent, double secondComponent, double thirdComponent, PixelFormat pixelFormat)
    {
        format = pixelFormat;
        if (pixelFormat == PixelFormat.RGB)
        {
            r = firstComponent;
            g = secondComponent;
            b = thirdComponent;
            isRGB = true;
        }
        else if (pixelFormat == PixelFormat.YCbCr)
        {
            y = firstComponent;
            cb = secondComponent;
            cr = thirdComponent;
            isRGB = false;
        }
    }

    private readonly double r;
    private readonly double g;
    private readonly double b;

    private readonly double y;
    private readonly double cb;
    private readonly double cr;

    public double R => isRGB ? r : (298.082 * Y + 408.583 * Cr) / 256.0 - 222.921;

    public double G => isRGB? g: (298.082 * Y - 100.291 * Cb - 208.120 * Cr) / 256.0 + 135.576;

    public double B => isRGB ? b : (298.082 * Y + 516.412 * Cb) / 256.0 - 276.836;

    public double Y => isRGB ? 16.0 + (65.738 * R + 129.057 * G + 24.064 * B) / 256.0 : y;
    public double Cb => isRGB ? 128.0 + (-37.945 * R - 74.494 * G + 112.439 * B) / 256.0 : cb;
    public double Cr => isRGB ? 128.0 + (112.439 * R - 94.154 * G - 18.285 * B) / 256.0 : cr;
}