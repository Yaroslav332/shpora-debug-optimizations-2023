using BenchmarkDotNet.Attributes;
using JPEG.Benchmarks.Benchmarks.Enum_Class_Compare;
using JPEG.Images;
using JPEG.Processor;

namespace JPEG.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class BenchmarkEnum
{
    

    [Benchmark]
    public void EnumCompare()
    {
        PixelFormat a = PixelFormat.RGB;
        PixelFormat b = PixelFormat.RGB;
        PixelFormat c = PixelFormat.YCbCr;
        for (int i = 0; i < 100000; i++)
        {
            bool k = a == b;
            bool l = a == c;
        }
    }

    [Benchmark]
    public void ClassCompare()
    {
        PixelFormatClass a = PixelFormatClass.RGB;
        PixelFormatClass b = PixelFormatClass.RGB;
        PixelFormatClass c = PixelFormatClass.YCbCr;
        for (int i = 0; i < 100000; i++)
        {
            bool k = a == b;
            bool l = a == c;
        }
    }
}