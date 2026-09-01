using System;
using Image2ZPL.Internal;
using Xunit;

namespace Image2ZPL.Tests;

/// <summary>
/// Tests for MonochromeBitmap.
/// </summary>
public class MonochromeBitmapTests
{
    /// <summary>
    /// Verifies that BytesPerRow correctly rounds up to whole bytes.
    /// </summary>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(8, 1)]
    [InlineData(9, 2)]
    [InlineData(16, 2)]
    [InlineData(17, 3)]
    public void BytesPerRow_RoundsUpToWholeBytes(int width, int expected)
    {
        var bitmap = new MonochromeBitmap(width, 1);
        Assert.Equal(expected, bitmap.BytesPerRow);
    }

    /// <summary>
    /// Verifies that a new bitmap is initialized as all white (all bits zero).
    /// </summary>
    [Fact]
    public void NewBitmap_IsAllWhite()
    {
        var bitmap = new MonochromeBitmap(17, 3);
        Assert.All(bitmap.Bits, b => Assert.Equal(0, b));
    }

    /// <summary>
    /// Verifies that SetBlack sets the most significant bit first.
    /// </summary>
    [Fact]
    public void SetBlack_SetsTheMostSignificantBitFirst()
    {
        var bitmap = new MonochromeBitmap(8, 1);
        bitmap.SetBlack(0, 0);
        Assert.Equal(0x80, bitmap.Bits[0]);
    }

    /// <summary>
    /// Verifies that SetBlack is readable by IsBlack.
    /// </summary>
    [Fact]
    public void SetBlack_IsReadableByIsBlack()
    {
        var bitmap = new MonochromeBitmap(17, 3);
        bitmap.SetBlack(16, 2);
        Assert.True(bitmap.IsBlack(16, 2));
        Assert.False(bitmap.IsBlack(15, 2));
    }

    /// <summary>
    /// Verifies that Row returns only that row's bytes.
    /// </summary>
    [Fact]
    public void Row_ReturnsOnlyThatRowsBytes()
    {
        var bitmap = new MonochromeBitmap(17, 3);
        bitmap.SetBlack(0, 1);
        Assert.Equal(3, bitmap.Row(1).Length);
        Assert.Equal(0x80, bitmap.Row(1)[0]);
        Assert.Equal(0x00, bitmap.Row(0)[0]);
    }
}
