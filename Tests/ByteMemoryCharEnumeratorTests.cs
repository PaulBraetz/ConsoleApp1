namespace Tests;

using System.Buffers;
using System.Text;

using ConsoleApp1;

public class ByteMemoryCharEnumeratorTests
{
    public static Object[][] ToCharEnumerableData => CharEnumeratorTestData.ToCharEnumerableData;

    #region ToCharEnumerable
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ReturnsCorrectChars(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);

        // Act
        using var chars = bytes.ToCharEnumerable().GetEnumerator();

        // Assert
        AssertCharEnumerator(input, chars);
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForZeroBufferSize(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);

        // Act & Assert
        try
        {
            _ = bytes.ToCharEnumerable(0);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForNegativeBufferSize(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);

        // Act & Assert
        try
        {
            _ = bytes.ToCharEnumerable(-1);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_SharedPool_ThrowsArgExceptionForLowBufferSize(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);

        // Act & Assert
        try
        {
            _ = bytes.ToCharEnumerable(31);
        } catch(ArgumentOutOfRangeException ex)
        {
            Assert.Equal("bufferLength", ex.ParamName);
            return;
        }

        Assert.Fail("expected ArgumentOutOfRangeException");
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_CustomPool_ReturnsCorrectChars(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);
        var pool = ArrayPool<Char>.Create();

        // Act
        using var chars = bytes.ToCharEnumerable(pool).GetEnumerator();

        // Assert
        AssertCharEnumerator(input, chars);
    }
    [Theory]
    [MemberData(nameof(ToCharEnumerableData))]
    public void ToCharEnumerable_ReturnsCorrectChars(String input)
    {
        // Arrange
        ReadOnlyMemory<Byte> bytes = Encoding.UTF8.GetBytes(input);

        // Act
        using var chars = bytes.ToCharEnumerable(stackalloc Char[Utf8CharEnumerable.RecommendedBufferLength]);

        // Assert
        AssertCharEnumerator(input, chars);
    }

    private static void AssertCharEnumerator<T>(String input, T chars)
        where T : IEnumerator<Char>, allows ref struct
    {
        for(var i = 0; i < input.Length; i++)
        {
            Assert.True(chars.MoveNext(), "Enumerator ended prematurely.");
            Assert.Equal(input[i], chars.Current);
        }

        // Assert that the enumerator has no more items
        Assert.False(chars.MoveNext(), "Enumerator has extra items.");
    }
    #endregion
}
