using System.Runtime.CompilerServices;

namespace MersenneTwister.MT
{
    public sealed class mt19937ar_cok_opt_t
    {
        /* Period parameters */
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0dfU; /* constant vector a */
        private const uint UMASK = 0x80000000U; /* most significant w-r bits */
        private const uint LMASK = 0x7fffffffU; /* least significant r bits */

        private static readonly uint[] mag01 = {0x0U, MATRIX_A};

        private readonly uint[] mt = new uint[N]; /* the array for the state vector  */
        private int initf;
        private int left = 1;
        private uint next;

        /* initializes mt[N] with a seed */
        public void init_genrand(uint s)
        {
            var mt = this.mt;
            uint j;
            mt[0] = s;
            for (j = 1; j < N; j++) mt[j] = 1812433253U * (mt[j - 1] ^ (mt[j - 1] >> 30)) + j;
            /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
            /* In the previous versions, MSBs of the seed affect   */
            /* only MSBs of the array mt[].                        */
            /* 2002/01/09 modified by Makoto Matsumoto             */
            left = 1;
            initf = 1;
        }

        /* initialize by an array with array-length */
        /* init_key is the array for initializing keys */
        /* key_length is its length */
        /* slight change for C++, 2004/2/26 */
        public void init_by_array(uint[] init_key, int key_length)
        {
            var mt = this.mt;
            init_genrand(19650218U);
            uint i = 1;
            uint j = 0;
            var k = N > key_length ? N : key_length;
            for (; k != 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j; /* non linear */
                i++;
                j++;
                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }

                if (j >= key_length) j = 0;
            }

            for (k = N - 1; k != 0; k--)
            {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1566083941U)) - i; /* non linear */
                i++;
                if (i >= N)
                {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }

            mt[0] = 0x80000000U; /* MSB is 1; assuring non-zero initial array */
            left = 1;
            initf = 1;
        }

        /* generates a random number on [0,0xffffffff]-interval */
        public uint genrand_int32()
        {
            uint y;
            if (--left == 0) next_state();

            y = mt[next++];

            /* Tempering */
            y ^= y >> 11;
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= y >> 18;

            return y;
        }

        /* generates a random number on [0,0x7fffffff]-interval */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int genrand_int31()
        {
            return (int) (genrand_int32() >> 1);
        }

        /* generates a random number on [0,1)-real-interval */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double genrand_real2()
        {
            return genrand_int32() * (1.0 / 4294967296.0);
            /* divided by 2^32 */
        }
        /* These real versions are due to Isaku Wada, 2002/01/09 added */

        /* generates a random number on [0,1) with 53-bit resolution*/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double genrand_res53()
        {
            uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            //return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
            return (((ulong) a << 26) | b) * (1.0 / 9007199254740992.0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MIXBITS(uint u, uint v)
        {
            return (u & UMASK) | (v & LMASK);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint TWIST(uint u, uint v)
        {
            return (MIXBITS(u, v) >> 1) ^ mag01[v & 1U];
        }

        private void next_state()
        {
            var mt = this.mt;

            /* if init_genrand() has not been called, */
            /* a default initial seed is used         */
            if (initf == 0) init_genrand(5489U);

            left = N;
            next = 0;

            uint p = 0;
            for (var j = N - M + 1; --j > 0; p++) mt[p] = mt[p + M] ^ TWIST(mt[p], mt[p + 1]);

            for (var j = M; --j > 0; p++) mt[p] = mt[p + (M - N)] ^ TWIST(mt[p], mt[p + 1]);

            mt[p] = mt[p + (M - N)] ^ TWIST(mt[p], mt[0]);
        }

        /* generates a random number on [0,1]-real-interval */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double genrand_real1()
        {
            return genrand_int32() * (1.0 / 4294967295.0);
            /* divided by 2^32-1 */
        }

        /* generates a random number on (0,1)-real-interval */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double genrand_real3()
        {
            return (genrand_int32() + 0.5) * (1.0 / 4294967296.0);
            /* divided by 2^32 */
        }
    }
}