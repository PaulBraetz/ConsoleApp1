#pragma warning disable IDE0251 // Make member 'readonly'
#pragma warning disable IDE0044 // Add readonly modifier
namespace ConsoleApp1;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a enumerator for enumerating chars from byte sources using the utf8 encoding.
/// </summary>
/// <typeparam name="TSource">The type of byte source to scan.</typeparam>
/// <typeparam name="TLength">The type of length used by the byte source.</typeparam>
/// <typeparam name="TOperators">The type providing operators to operate on length values.</typeparam>
public interface IUtf8CharEnumerable<TEnumerator, TSource, TLength, TOperators>
    where TEnumerator : IEnumerator<Char>, allows ref struct
    where TSource : IByteSource<TSource, TLength, TOperators>, allows ref struct
    where TOperators : IByteSourceLengthOperators<TLength>
    where TLength : unmanaged
{
    /// <summary>
    /// Gets a new enumerator enumerating over the chars represented the source.
    /// </summary>
    /// <returns>
    /// A new enumerator.
    /// </returns>
    TEnumerator GetEnumerator();
    /// <summary>
    /// Gets the byte source being enumerated as chars.
    /// </summary>
    /// <returns>
    /// The source being enumerated as chars.
    /// </returns>
    TSource GetSource();
}
