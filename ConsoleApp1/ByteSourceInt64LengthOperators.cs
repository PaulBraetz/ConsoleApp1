#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides byte source operators for <see cref="Int32"/>.
/// </summary>
public class ByteSourceInt64LengthOperators : IByteSourceLengthOperators<Int64>
{
    private ByteSourceInt64LengthOperators() { }
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment(ref Int64 location, Int32 value) => location += value;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int64 Subtract(Int64 left, Int64 right) => left - right;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 Min(Int64 left, Int32 right) => (Int32)Math.Min(left, right);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean LessThan(Int64 left, Int64 right) => left < right;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 ToInt32(Int64 value) => checked((Int32)value);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int64 Subtract(Int64 left, Int32 right) => left - right;
}
