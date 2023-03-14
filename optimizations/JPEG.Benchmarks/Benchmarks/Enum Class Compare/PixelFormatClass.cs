namespace JPEG.Benchmarks.Benchmarks.Enum_Class_Compare;
public class PixelFormatClass
{
    private string Format;

    private PixelFormatClass(string format)
    {
        Format = format;
    }

    public static PixelFormatClass RGB = new PixelFormatClass(nameof(RGB));
    public static PixelFormatClass YCbCr = new PixelFormatClass(nameof(YCbCr));

    protected bool Equals(PixelFormatClass other)
    {
        return string.Equals(Format, other.Format);
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PixelFormatClass)obj);
    }

    public override int GetHashCode()
    {
        return (Format != null ? Format.GetHashCode() : 0);
    }

    public static bool operator ==(PixelFormatClass a, PixelFormatClass b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(PixelFormatClass a, PixelFormatClass b)
    {
        return !a.Equals(b);
    }

    public override string ToString()
    {
        return Format;
    }

    ~PixelFormatClass()
    {
        Format = null;
    }
}