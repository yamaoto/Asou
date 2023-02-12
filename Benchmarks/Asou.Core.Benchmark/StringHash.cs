using System.Text;
using BenchmarkDotNet.Attributes;
using HashDepot;

namespace Asou.Core.Benchmark;

public class StringHash
{
    private string _testString1 = "";
    private string _testString2 = "";

    [GlobalSetup]
    public void Setup()
    {
        var st = new StringBuilder();
        for (var i = 0; i < 2; i++)
        {
            foreach (char j in Enumerable.Range('\x1', 127).ToArray())
            {
                st.Append(j);
            }
        }

        _testString1 = st.ToString();
        _testString2 = st.ToString();
    }

    [Benchmark(Baseline = true)]
    public int ConcatHashcode()
    {
        return (_testString1 + _testString2).GetHashCode();
    }

    [Benchmark]
    public int HashCode()
    {
        return _testString1.GetHashCode() * 17 + _testString2.GetHashCode() * 17;
    }

    [Benchmark]
    public uint XxHash32()
    {
        var buffer = new byte[_testString1.Length + _testString2.Length];
        Encoding.UTF8.GetBytes(_testString1, 0, _testString1.Length, buffer, 0);
        Encoding.UTF8.GetBytes(_testString2, 0, _testString2.Length, buffer, _testString1.Length);
        var result = XXHash.Hash32(buffer, 123);
        return result;
    }

    [Benchmark]
    public uint XxHash32Concat()
    {
        var buffer = Encoding.UTF8.GetBytes(_testString1 + _testString2);
        var result = XXHash.Hash32(buffer, 123);
        return result;
    }

    [Benchmark]
    public ulong XxHash64()
    {
        var buffer = new byte[_testString1.Length + _testString2.Length];
        Encoding.UTF8.GetBytes(_testString1, 0, _testString1.Length, buffer, 0);
        Encoding.UTF8.GetBytes(_testString2, 0, _testString2.Length, buffer, _testString1.Length);
        var result = XXHash.Hash64(buffer, 123);
        return result;
    }

    [Benchmark]
    public ulong XxHash64Concat()
    {
        var buffer = Encoding.UTF8.GetBytes(_testString1 + _testString2);
        var result = XXHash.Hash64(buffer, 123);
        return result;
    }
}
