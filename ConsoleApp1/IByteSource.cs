#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;

/// <summary>
/// Represents a generic source of bytes.
/// </summary>
/// <typeparam name="TSelf">The type itself (CRTP).</typeparam>
/// <typeparam name="TLength">The type used for determining the sources length, indices etc.</typeparam>
public interface IByteSource<TSelf, TLength, TOperators>
    where TOperators : IByteSourceLengthOperators<TLength>
    where TSelf : IByteSource<TSelf, TLength, TOperators>, allows ref struct
    where TLength : unmanaged
{
    /// <summary>
    /// Attempts to copy the source to a span, starting at the specified source
    /// index. If more bytes are available than would fit into <paramref
    /// name="buffer"/>, a new span no larger than <paramref name="maxLength"/>
    /// is allocated and used to replace <paramref name="buffer"/>. If less
    /// bytes are available than would fit into <paramref name="buffer"/>, they
    /// will be written to <paramref name="buffer"/> and it will be resized to
    /// fit the written bytes exactly.
    /// </summary>
    /// <param name="source">
    /// The byte source to attempt to copy bytes from.
    /// </param>
    /// <param name="start">
    /// The index from which to start copying.
    /// </param>
    /// <param name="buffer">
    /// The buffer to copy the source bytes to.
    /// </param>
    /// <returns>
    /// A minimum amount of elements left to be copied; that is, <see
    /// langword="default"/> if all bytes after <paramref name="start"/> were
    /// copied; otherwise, a value equivalent to <c> >=1 </c>.
    /// </returns>
    public static abstract TLength TryCopyTo(TSelf source, ref Span<Byte> buffer, TLength start = default, Int32 maxLength = Int32.MaxValue);
    /// <summary>
    /// Attempts to copy the source to a span, starting at the specified source
    /// index. If more bytes are available than would fit into <paramref
    /// name="buffer"/>, it is written to fully. If less
    /// bytes are available than would fit into <paramref name="buffer"/>, they
    /// will be written to <paramref name="buffer"/> and it will be resized to
    /// fit the written bytes exactly.
    /// </summary>
    /// <param name="source">
    /// The byte source to attempt to copy bytes from.
    /// </param>
    /// <param name="start">
    /// The index from which to start copying.
    /// </param>
    /// <param name="buffer">
    /// The buffer to copy the source bytes to.
    /// </param>
    public static abstract void CopyTo(TSelf source, ref Span<Byte> buffer, TLength start = default);
}

