using BenchmarkDotNet.Running;

namespace Bsii.Dotnet.Utils.Benchmarks
{
    class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
