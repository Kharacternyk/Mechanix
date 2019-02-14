using System;
using Mechanix;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ParallelismBenchmark>();
        }
    }
}
