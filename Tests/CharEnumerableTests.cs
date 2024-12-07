namespace Tests;

using System.Buffers;
using System.Text;

using ConsoleApp1;

public class CharEnumerableTests
{
    public static Object[][] ToArrayData => CharEnumeratorTestData.Strings.Select(input => new Object[] { input, input.ToCharArray() }).ToArray();
    #region ToArray
    [Theory]
    [MemberData(nameof(ToArrayData))]
    public void ToArray_Stackalloc_ReturnsInputChars(String input, Char[] expected)
    {
        // Arrange
        var enumerator = ( (ReadOnlySpan<Byte>)Encoding.UTF8.GetBytes(input) ).ToCharEnumerable(stackalloc Char[Utf8CharEnumerable.RecommendedBufferLength]);

        // Act
        var result = enumerator.ToArray();

        // Assert
        Assert.Equal(expected, result);
    }
    [Theory]
    [MemberData(nameof(ToArrayData))]
    public void ToArray_Buffered_ReturnsInputChars(String input, Char[] expected)
    {
        // Arrange
        var enumerator = ( (ReadOnlySpan<Byte>)Encoding.UTF8.GetBytes(input) ).ToCharEnumerable();

        // Act
        var result = enumerator.ToArray();

        // Assert
        Assert.Equal(expected, result);
    }
    #endregion
}