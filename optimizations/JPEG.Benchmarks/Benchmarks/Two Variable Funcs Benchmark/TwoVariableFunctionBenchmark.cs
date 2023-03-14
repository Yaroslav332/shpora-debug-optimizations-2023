using BenchmarkDotNet.Attributes;

namespace JPEG.Benchmarks.Benchmarks.Two_Variable_Funcs_Benchmark;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class TwoVariableFunctionBenchmark
{
    private int from1 = 0;
    private int from2 = 100;
    private int to1 = 0;
    private int to2 = 100;
    private Func<int, int, double> func = (i, i1) => i + i1;

    public static double Sum(int from, int to, Func<int, double> function)
        => Enumerable.Range(from, to - from).Sum(function);
    
    public static double SumByTwoVariables(int from1, int to1, int from2, int to2, Func<int, int, double> function)
        => Sum(from1, to1, x => Sum(from2, to2, y => function(x, y)));

    public static double LoopByTwoVariables(int from1, int to1, int from2, int to2, Action<int, int> function)
        => Sum(from1, to1, x => Sum(from2, to2, y =>
        {
            function(x, y);
            return 0;
        }));
    
    [Benchmark]
    public void LambdaSum()
    {
        double s = SumByTwoVariables(from1, to1, from2, to2, func);
    }

    [Benchmark]
    public void ForSum()
    {
        double s = 0;
        for (int i = from1; i <= to1; i++)
        {
            for (int j = from2; j < to2; j++)
            {
                s += func(i, j);
            }
        }
        
    }
    [Benchmark]
    public void LambdaAction()
    {
        double s = 0;
        LoopByTwoVariables(from1, to1, from2, to2, (i, i1) => { s += i + i1;});
    }

    [Benchmark]
    public void ForAction()
    {
        double s = 0;
        for (int i = from1; i <= to1; i++)
        {
            for (int j = from2; j < to2; j++)
            {
                s += i + j;
            }
        }
        
    }
    
}