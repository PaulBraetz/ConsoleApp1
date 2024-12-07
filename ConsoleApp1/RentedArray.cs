namespace ConsoleApp1;

using System;
using System.Buffers;

/// <summary>
/// Provides factory methods for creating <see cref="RentedArray{T}"/> instances.
/// </summary>
public static class RentedArray
{
    /// <summary>
    /// Creates a new <see cref="RentedArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element provided by the array.</typeparam>
    /// <param name="length">The length of the array created.</param>
    /// <param name="owner">The owner to rent the array from.</param>
    /// <returns>A new rented array.</returns>
    public static RentedArray<T> Create<T>(Int32 length, ArrayPool<T> owner) => 
        new(owner.Rent(length), length, owner);
    /// <summary>
    /// Creates a new <see cref="RentedArray{T}"/> using <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    /// <typeparam name="T">The type of element provided by the array.</typeparam>
    /// <param name="minimumLength">The minimum length of the array created.</param>
    /// <returns>A new rented array using the shared array pool.</returns>
    public static RentedArray<T> Create<T>(Int32 minimumLength) => Create(minimumLength, ArrayPool<T>.Shared);
}

/// <summary>
/// Represents a rented array that is to be returned to its pool upon disposal.
/// </summary>
/// <typeparam name="T">
/// The type of value provided by the array.
/// </typeparam>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="values">
/// The array to return to the owner upon disposal.
/// </param>
/// <param name="length">
/// The length of the rented array to make available to consumers.
/// </param>
/// <param name="owner">
/// The owner to return the array to upon disposal.
/// </param>
public readonly struct RentedArray<T>(T[] values, Int32 length, ArrayPool<T> owner) : IDisposable
{
    /// <summary>
    /// The rented array.
    /// </summary>    
    public readonly Span<T> Values => values.AsSpan(0, length);
    /// <summary>
    /// The owner of the rented array.
    /// </summary>
    public readonly ArrayPool<T> Owner = owner;
    /// <inheritdoc/>
    public void Dispose() => Owner.Return(values);
}
