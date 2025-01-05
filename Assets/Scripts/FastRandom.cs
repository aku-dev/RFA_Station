using System;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Multiplicative Congruence generator using a modulus of 2^31
/// </summary>
public sealed class FastRandom : IRandom
{
    public int Seed { get; private set; }

    private const ulong Modulus = 2147483647; //2^31
    private const ulong Multiplier = 1132489760;
    private const double ModulusReciprocal = 1.0 / Modulus;

    private ulong _next;

    public FastRandom()
        : this(RandomSeed.Crypto()) { }

    public FastRandom(int seed)
    {
        NewSeed(seed);
    }

    public void NewSeed()
    {
        NewSeed(RandomSeed.Crypto());
    }

    /// <inheritdoc />
    /// <remarks>If the seed value is zero, it is set to one.</remarks>
    public void NewSeed(int seed)
    {
        if (seed == 0)
            seed = 1;

        Seed = seed;
        _next = (ulong)seed % Modulus;
    }

    public float GetFloat()
    {
        return (float)InternalSample();
    }

    public int GetInt()
    {
        return Range(int.MinValue, int.MaxValue);
    }

    public float Range(float min, float max)
    {
        return (float)(InternalSample() * (max - min) + min);
    }

    public int Range(int min, int max)
    {
        return (int)(InternalSample() * (max - min) + min);
    }

    public Vector2 GetInsideCircle(float radius = 1)
    {
        var x = Range(-1f, 1f) * radius;
        var y = Range(-1f, 1f) * radius;
        return new Vector2(x, y);
    }

    public Vector3 GetInsideSphere(float radius = 1)
    {
        var x = Range(-1f, 1f) * radius;
        var y = Range(-1f, 1f) * radius;
        var z = Range(-1f, 1f) * radius;
        return new Vector3(x, y, z);
    }

    public Quaternion GetRotation()
    {
        return GetRotationOnSurface(GetInsideSphere());
    }

    public Quaternion GetRotationOnSurface(Vector3 surface)
    {
        return new Quaternion(surface.x, surface.y, surface.z, GetFloat());
    }

    private double InternalSample()
    {
        var ret = _next * ModulusReciprocal;
        _next = _next * Multiplier % Modulus;
        return ret;
    }
}

public static class RandomSeed
{
    private static readonly RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

    /// <summary>Create seed based on DateTime structure</summary>
    public static int Time()
    {
        return DateTime.UtcNow.GetHashCode();
    }

    /// <summary>Create seed based on <see cref="Environment.TickCount"/> and <see cref="System.Guid"/></summary>
    public static int Guid()
    {
        return Environment.TickCount ^ System.Guid.NewGuid().GetHashCode();
    }

    /// <summary>Create seed based on <see cref="System.Security.Cryptography.RandomNumberGenerator"/></summary>
    public static int Crypto()
    {
        var bytes = new byte[4];
        RandomNumberGenerator.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
}
public interface IRandom
{
    int Seed { get; }
    /// <summary>Create new random with new unique seed</summary>
    void NewSeed();

    /// <summary>Create new random sequence based on new seed</summary>
    void NewSeed(int seed);

    /// <returns>A random float number between 0.0f (inclusive) and 1.0f (inclusive)</returns>
    float GetFloat();

    /// <returns>A random int number between int.MinValue (inclusive) and int.MaxValue (inclusive)</returns>
    int GetInt();

    /// <returns>A random float number between min (inclusive) and max (inclusive) values</returns>
    float Range(float min, float max);

    /// <returns>A random int number between int.MinValue (inclusive) and int.MaxValue (inclusive) values</returns>
    int Range(int min, int max);

    /// <returns>A point inside the circle with given radius</returns>
    Vector2 GetInsideCircle(float radius);

    /// <returns>A point inside the sphere with given radius</returns>
    Vector3 GetInsideSphere(float radius);

    /// <returns>A random Quaternion struct where X,Y,Z,W has numbers in 0.0f .. 1.0f (inclusive)</returns>
    Quaternion GetRotation();

    /// <returns>A random Quaternion struct where W has numbers in 0.0f .. 1.0f (inclusive)</returns>
    Quaternion GetRotationOnSurface(Vector3 surface);
}