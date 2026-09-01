using Image2ZPL;
using Image2ZPL.Internal;
using Xunit;

namespace Image2ZPL.Tests;

public class HalftonerTests
{
    private static byte[] Uniform(int width, int height, byte value)
    {
        var gray = new byte[width * height];
        for (int i = 0; i < gray.Length; i++) gray[i] = value;
        return gray;
    }

    [Fact]
    public void Threshold_DarkPixelsBecomeBlack()
    {
        byte[] gray = { 127, 128 };
        var bitmap = Halftoner.Apply(gray, 2, 1, DitherMode.Threshold, 128, invert: false);
        Assert.True(bitmap.IsBlack(0, 0));
        Assert.False(bitmap.IsBlack(1, 0));
    }

    [Fact]
    public void Threshold_RespectsCustomThreshold()
    {
        byte[] gray = { 200 };
        var bitmap = Halftoner.Apply(gray, 1, 1, DitherMode.Threshold, 220, invert: false);
        Assert.True(bitmap.IsBlack(0, 0));
    }

    [Fact]
    public void Invert_FlipsPolarity()
    {
        byte[] gray = { 0 };
        var bitmap = Halftoner.Apply(gray, 1, 1, DitherMode.Threshold, 128, invert: true);
        Assert.False(bitmap.IsBlack(0, 0));
    }

    /// <summary>
    /// Regression test for PR #3. A fully black image whose width is not a
    /// multiple of eight must not set the unused bits in the last byte of a
    /// row, or the label prints a black stripe down the right edge.
    /// </summary>
    [Theory]
    [InlineData(1, 0x80)]
    [InlineData(7, 0xFE)]
    [InlineData(9, 0x80)]
    [InlineData(17, 0x80)]
    public void PaddingBitsInLastByteAreAlwaysZero(int width, int expectedLastByte)
    {
        var bitmap = Halftoner.Apply(Uniform(width, 1, 0), width, 1, DitherMode.Threshold, 128, invert: false);
        Assert.Equal(expectedLastByte, bitmap.Bits[bitmap.BytesPerRow - 1]);
    }

    // Task 8 adds an InlineData row per dithering mode. Only Threshold
    // exists at this point, so only Threshold is listed here.
    [Theory]
    [InlineData(DitherMode.Threshold)]
    public void EveryMode_LeavesPaddingBitsZero(DitherMode mode)
    {
        const int width = 13;
        var bitmap = Halftoner.Apply(Uniform(width, 5, 0), width, 5, mode, 128, invert: false);
        for (int y = 0; y < 5; y++)
        {
            byte last = bitmap.Bits[(y * bitmap.BytesPerRow) + bitmap.BytesPerRow - 1];
            Assert.Equal(0, last & 0x07); // 13 px leaves 3 unused bits
        }
    }
}
