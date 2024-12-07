namespace Tests;

using System.Buffers;
using System.Diagnostics;
using System.Text;

using ConsoleApp1;

public class ByteSequenceCharEnumeratorTests
{
    private static ReadOnlySequence<Byte> GetSequence(String s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        var rng = new Random(bytes.Length);
        ByteSegment? first = null;
        var last = first;
        for(var consumed = 0; consumed < bytes.Length;)
        {
            var remaining = bytes.Length - consumed;
            var segmentLength = rng.Next(1, remaining);
            var segmentArr = new Byte[segmentLength];
            var sliceToConsume = bytes.AsSpan(consumed, segmentLength);
            sliceToConsume.CopyTo(segmentArr);

            var newLast = new ByteSegment(segmentArr);

            if(first is null)
            {
                first = newLast;
                last = newLast;
            } else
            {
                Debug.Assert(last != null);
                newLast.Previous = last;
                last = newLast;
            }

            consumed += segmentLength;
        }

        Debug.Assert(first != null);
        Debug.Assert(last != null);

        var result = new ReadOnlySequence<Byte>(first, 0, last, last.Memory.Length);
        var resultArr = result.ToArray();
        var decoded = Encoding.UTF8.GetString(resultArr);
        Debug.Assert(s == decoded, "encoded member data incorrectly");

        return result;
    }
    public static Object[][] ToCharEnumerableData => CharEnumeratorTestData.Strings.Select(GetSequence).Select(o => ( new Object[] { o } )).ToArray();

    #region ToCharEnumerable
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ReturnsCorrectChars(ReadOnlySequence<Byte> input)
    {
        // Arrange & Act
        using var chars = input.ToCharEnumerable().GetEnumerator();

        // Assert
        AssertCharEnumerator(input, chars);
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForZeroBufferSize(ReadOnlySequence<Byte> input)
    {
        // Arrange & Act & Assert
        try
        {
            _ = input.ToCharEnumerable(0);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForNegativeBufferSize(ReadOnlySequence<Byte> input)
    {
        // Arrange & Act & Assert
        try
        {
            _ = input.ToCharEnumerable(-1);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForLowBufferSize(ReadOnlySequence<Byte> input)
    {
        // Arrange & Act & Assert
        try
        {
            _ = input.ToCharEnumerable(31);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_CustomPool_ReturnsCorrectChars(ReadOnlySequence<Byte> input)
    {
        // Arrange
        var pool = ArrayPool<Char>.Create();

        // Act
        using var chars = input.ToCharEnumerable(pool).GetEnumerator();

        // Assert
        AssertCharEnumerator(input, chars);
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_Span_ReturnsCorrectChars(ReadOnlySequence<Byte> input)
    {
        // Arrange & Act
        using var chars = input.ToCharEnumerable(stackalloc Char[Utf8CharEnumerable.RecommendedBufferLength]);

        // Assert
        AssertCharEnumerator(input, chars);
    }

    private static void AssertCharEnumerator<T>(ReadOnlySequence<Byte> input, T chars)
        where T : IEnumerator<Char>, allows ref struct
    {
        var inputStr = Encoding.UTF8.GetString(input);

        for(var i = 0; i < inputStr.Length; i++)
        {
            Assert.True(chars.MoveNext(), "Enumerator ended prematurely.");
            Assert.Equal(inputStr[i], chars.Current);
        }

        // Assert that the enumerator has no more items
        Assert.False(chars.MoveNext(), "Enumerator has extra items.");
    }
    #endregion
}
