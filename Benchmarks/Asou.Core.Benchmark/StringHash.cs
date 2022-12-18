using System.Text;
using BenchmarkDotNet.Attributes;
using HashDepot;

namespace Asou.Core.Benchmark;

public class StringHash
{
    private string testString1;
    private string testString2;

    [GlobalSetup]
    public void Setup()
    {
        var st = new StringBuilder();
        for (var i = 0; i < 2; i++)
            foreach (char j in Enumerable.Range('\x1', 127).ToArray())
                st.Append(j);
        testString1 = st.ToString();
        testString2 = st.ToString();
    }

    [Benchmark(Baseline = true)]
    public int ConcatHashcode()
    {
        return (testString1 + testString2).GetHashCode();
    }

    [Benchmark]
    public int HashCode()
    {
        return testString1.GetHashCode() * 17 + testString2.GetHashCode() * 17;
    }

    [Benchmark]
    public uint XxHash32()
    {
        var buffer = new byte[testString1.Length + testString2.Length];
        Encoding.UTF8.GetBytes(testString1, 0, testString1.Length, buffer, 0);
        Encoding.UTF8.GetBytes(testString2, 0, testString2.Length, buffer, testString1.Length);
        var result = XXHash.Hash32(buffer, 123);
        return result;
    }

    [Benchmark]
    public uint XxHash32Concat()
    {
        var buffer = Encoding.UTF8.GetBytes(testString1 + testString2);
        var result = XXHash.Hash32(buffer, 123);
        return result;
    }

    [Benchmark]
    public ulong XxHash64()
    {
        var buffer = new byte[testString1.Length + testString2.Length];
        Encoding.UTF8.GetBytes(testString1, 0, testString1.Length, buffer, 0);
        Encoding.UTF8.GetBytes(testString2, 0, testString2.Length, buffer, testString1.Length);
        var result = XXHash.Hash64(buffer, 123);
        return result;
    }

    [Benchmark]
    public ulong XxHash64Concat()
    {
        var buffer = Encoding.UTF8.GetBytes(testString1 + testString2);
        var result = XXHash.Hash64(buffer, 123);
        return result;
    }
}