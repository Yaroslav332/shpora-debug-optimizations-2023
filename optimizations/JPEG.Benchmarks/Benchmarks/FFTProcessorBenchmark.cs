using BenchmarkDotNet.Attributes;
using JPEG.Processor;

namespace JPEG.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class FFTProcessorBenchmark
{
    private IJpegProcessor jpegProcessor;
    private static readonly string imagePath = @"sample.bmp";
    private static readonly string compressedImagePath = imagePath + ".compressed.";

    private static readonly string uncompressedImagePath =
        imagePath + ".uncompressed." + ".bmp";

    [GlobalSetup]
    public void SetUp()
    {
        jpegProcessor = FastFourierProcessor.Init;
    }

    [Benchmark]
    public void Compress()
    {
        jpegProcessor.Compress(imagePath, compressedImagePath);
    }

    [Benchmark]
    public void Uncompress()
    {
        jpegProcessor.Uncompress(compressedImagePath, uncompressedImagePath);
    }
}