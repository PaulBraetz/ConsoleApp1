#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;

/// <summary>
/// Defines operations required for byte source length types.
/// </summary>
/// <typeparam name="TLength">The type of length to operate on.</typeparam>
public interface IByteSourceLengthOperators<TLength>
{
    /// <summary>
    /// Increments the value at the provided location by another value.
    /// </summary>
    /// <param name="location">The location pointing to the variable to increment.</param>
    /// <param name="value">The value by which to increment.</param>
    public static abstract void Increment(ref TLength location, Int32 value);
    /// <summary>
    /// Subtracts a value from another.
    /// </summary>
    /// <param name="left">The left operand to subtract from.</param>
    /// <param name="right">The right operand to subtract from the left.</param>
    /// <returns>The result of subtracting the right from the left operand.</returns>
    public static abstract TLength Subtract(TLength left, TLength right);
    /// <summary>
    /// Subtracts a <see cref="Int32"/> value from a <typeparamref name="TLength"/> value.
    /// </summary>
    /// <param name="left">The left operand to subtract from.</param>
    /// <param name="right">The right operand to subtract from the left.</param>
    /// <returns>The result of subtracting the right from the left operand.</returns>
    public static abstract TLength Subtract(TLength left, Int32 right);
    /// <summary>
    /// Gets the smaller value of the two provided values.
    /// </summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>The smaller of the two values provided.</returns>
    public static abstract Int32 Min(TLength left, Int32 right);
    /// <summary>
    /// Gets a value indicating whether the left operand is less than the right operand.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static abstract Boolean LessThan(TLength left, TLength right);
    /// <summary>
    /// Gets the length value as an <see cref="Int32"/>.
    /// </summary>
    /// <param name="value">
    /// The value to convert.
    /// </param>
    /// <returns>
    /// The value represented as an <see cref="Int32"/>.
    /// </returns>
    public static abstract Int32 ToInt32(TLength value);
}