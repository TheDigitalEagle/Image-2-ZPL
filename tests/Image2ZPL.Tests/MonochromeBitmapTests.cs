using System;
using Image2ZPL.Internal;
using Xunit;

namespace Image2ZPL.Tests;

public class MonochromeBitmapTests
{
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

    [Fact]
    public void NewBitmap_IsAllWhite()
    {
        var bitmap = new MonochromeBitmap(17, 3);
        Assert.All(bitmap.Bits, b => Assert.Equal(0, b));
    }

    [Fact]
    public void SetBlack_SetsTheMostSignificantBitFirst()
    {
        var bitmap = new MonochromeBitmap(8, 1);
        bitmap.SetBlack(0, 0);
        Assert.Equal(0x80, bitmap.Bits[0]);
    }

    [Fact]
    public void SetBlack_IsReadableByIsBlack()
    {
        var bitmap = new MonochromeBitmap(17, 3);
        bitmap.SetBlack(16, 2);
        Assert.True(bitmap.IsBlack(16, 2));
        Assert.False(bitmap.IsBlack(15, 2));
    }

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
