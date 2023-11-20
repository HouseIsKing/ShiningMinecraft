using System.Drawing;

namespace MinecraftLibrary.Engine;

public sealed class Random
{
    private const long Multiplier = 0x5DEECE66DL;
    private const long Addend = 0xBL;
    private const long Mask = (1L << 48) - 1;
    private long Seed { get; set; }
    private static long _seedUniquifier = 8682522807148012L;

    public Random() : this(++_seedUniquifier + EngineDefaults.NanoTime())
    {
    }

    public Random(long seed)
    {
        SetSeed(seed);
    }

    public void SetSeed(long seed)
    {
        seed = (seed ^ Multiplier) & Mask;
        Seed = seed;
    }

    public long GetSeed()
    {
        return Seed ^ Multiplier;
    }

    private int Next(int bits)
    {
        long oldSeed;
        do
        {
            oldSeed = Seed;
            Seed = (oldSeed * Multiplier + Addend) & Mask;
        } while (Seed == oldSeed);

        return (int)(Seed >>> (48 - bits));
    }

    public int NextInt()
    {
        return Next(32);
    }

    public void NextBytes(byte[] bytes)
    {
        for (int i = 0, len = bytes.Length; i < len; i++)
        for (int rnd = NextInt(), n = Math.Min(len - i, 4); n-- > 0; rnd >>= 8)
            bytes[i++] = (byte)rnd;
    }

    public int NextInt(int n)
    {
        if (n <= 0)
            throw new Exception("n must be positive");

        if ((n & -n) == n) // i.e., n is a power of 2
            return (int)((n * (long)Next(31)) >> 31);

        int bits, val;
        do
        {
            bits = Next(31);
            val = bits % n;
        } while (bits - val + (n - 1) < 0);

        return val;
    }

    public long NextLong()
    {
        // it's okay that the bottom word remains signed.
        return ((long)(Next(32)) << 32) + Next(32);
    }

    public bool NextBoolean()
    {
        return Next(1) != 0;
    }

    public float NextFloat()
    {
        return Next(24) / (float)(1 << 24);
    }

    public double NextDouble()
    {
        return (((long)(Next(26)) << 27) + Next(27))
               / (double)(1L << 53);
    }
}