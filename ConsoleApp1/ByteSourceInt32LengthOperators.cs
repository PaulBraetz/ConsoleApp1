#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides byte source operators for <see cref="Int32"/>.
/// </summary>
public class ByteSourceInt32LengthOperators : IByteSourceLengthOperators<Int32>
{
    private ByteSourceInt32LengthOperators() { }
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment(ref Int32 location, Int32 value) => location += value;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 Subtract(Int32 left, Int32 right) => left - right;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 Min(Int32 left, Int32 right) => Math.Min(left, right);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Boolean LessThan(Int32 left, Int32 right) => left < right;
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int32 ToInt32(Int32 value) => value;
}
