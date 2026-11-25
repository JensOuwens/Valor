using System.Text;
using Godot;

public static class SeededRng
{
    public static ulong HashStringToUInt64(string seed)
    {
        const ulong fnvOffsetBasis = 14695981039346656037UL;
        const ulong fnvPrime = 1099511628211UL;

        ulong hash = fnvOffsetBasis;
        byte[] bytes = Encoding.UTF8.GetBytes(seed ?? string.Empty);

        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= fnvPrime;
        }

        return hash;
    }

    public static RandomNumberGenerator CreateRng(string seed)
    {
        RandomNumberGenerator rng = new();
        rng.Seed = HashStringToUInt64(seed);
        return rng;
    }
}