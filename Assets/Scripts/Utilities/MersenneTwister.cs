using System;
using System.Runtime.CompilerServices;

public class MersenneTwister
{
    private const int k_N = 624;
    private const int k_M = 397;
    private const uint k_MatrixA = 0x9908b0dfU;
    private const uint k_UMask = 0x80000000U;
    private const uint k_LMask = 0x7fffffffU;

    private static readonly uint[] s_Mag01 = { 0x0U, k_MatrixA };

    private readonly uint[] m_MT = new uint[k_N];
    private int m_Initf;
    private int m_Left = 1;
    private uint m_Next;

    public void InitGenRand(uint seed)
    {
        m_MT[0] = seed;
        for (var i = 1u; i < k_N; i++)
            m_MT[i] = 1812433253U * (m_MT[i - 1] ^ (m_MT[i - 1] >> 30)) + i;

        m_Left = 1;
        m_Initf = 1;
    }

    public void InitByArray(uint[] initKey, int keyLength)
    {
        InitGenRand(19650218U);
        uint i = 1, j = 0;
        var k = Math.Max(k_N, keyLength);

        for (; k != 0; k--)
        {
            m_MT[i] = (m_MT[i] ^ ((m_MT[i - 1] ^ (m_MT[i - 1] >> 30)) * 1664525U)) + initKey[j] + j;
            i++;
            j++;
            if (i >= k_N)
            {
                m_MT[0] = m_MT[k_N - 1];
                i = 1;
            }

            if (j >= keyLength) j = 0;
        }

        for (k = k_N - 1; k != 0; k--)
        {
            m_MT[i] = (m_MT[i] ^ ((m_MT[i - 1] ^ (m_MT[i - 1] >> 30)) * 1566083941U)) - i;
            i++;
            if (i >= k_N)
            {
                m_MT[0] = m_MT[k_N - 1];
                i = 1;
            }
        }

        m_MT[0] = 0x80000000U;
        m_Left = 1;
        m_Initf = 1;
    }

    public uint GenRandInt32()
    {
        if (--m_Left == 0) NextState();

        var y = m_MT[m_Next++];
        y ^= y >> 11;
        y ^= (y << 7) & 0x9d2c5680U;
        y ^= (y << 15) & 0xefc60000U;
        y ^= y >> 18;

        return y;
    }

    // generates a random number on [0,0x7fffffff]-interval
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GenRandInt31() => (int)(GenRandInt32() >> 1);

    // generates a random number on [0,1]-real-interval
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GenRandReal1() => GenRandInt32() * (1.0 / 4294967295.0);

    // generates a random number on [0,1)-real-interval
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GenRandReal2() => GenRandInt32() * (1.0 / 4294967296.0);

    // generates a random number on (0,1)-real-interval
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GenRandReal3() => (GenRandInt32() + 0.5) * (1.0 / 4294967296.0);

    // generates a random number on [0,1) with 53-bit resolution
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GenRandRes53()
    {
        uint a = GenRandInt32() >> 5, b = GenRandInt32() >> 6;
        return (((ulong)a << 26) | b) * (1.0 / 9007199254740992.0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint MixBits(uint u, uint v)
    {
        return (u & k_UMask) | (v & k_LMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Twist(uint u, uint v)
    {
        return (MixBits(u, v) >> 1) ^ s_Mag01[v & 1U];
    }

    private void NextState()
    {
        if (m_Initf == 0) InitGenRand(5489U);

        m_Left = k_N;
        m_Next = 0;

        uint p = 0;
        for (var j = k_N - k_M + 1; --j > 0; p++)
            m_MT[p] = m_MT[p + k_M] ^ Twist(m_MT[p], m_MT[p + 1]);

        for (var j = k_M; --j > 0; p++)
            m_MT[p] = m_MT[p + (k_M - k_N)] ^ Twist(m_MT[p], m_MT[p + 1]);

        m_MT[p] = m_MT[p + (k_M - k_N)] ^ Twist(m_MT[p], m_MT[0]);
    }
}