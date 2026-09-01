using System;
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

    [Theory]
    [InlineData(DitherMode.Threshold)]
    [InlineData(DitherMode.FloydSteinberg)]
    [InlineData(DitherMode.Atkinson)]
    [InlineData(DitherMode.Ordered4x4)]
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

    private static int CountBlack(MonochromeBitmap bitmap)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                if (bitmap.IsBlack(x, y)) count++;
        return count;
    }

    private static string[] Render(MonochromeBitmap bitmap)
    {
        var rows = new string[bitmap.Height];
        for (int y = 0; y < bitmap.Height; y++)
        {
            var row = new char[bitmap.Width];
            for (int x = 0; x < bitmap.Width; x++)
                row[x] = bitmap.IsBlack(x, y) ? '#' : '.';
            rows[y] = new string(row);
        }
        return rows;
    }

    [Theory]
    [InlineData(DitherMode.FloydSteinberg)]
    [InlineData(DitherMode.Atkinson)]
    [InlineData(DitherMode.Ordered4x4)]
    public void Dithering_TurnsFlatMidGrayIntoAMixOfDots(DitherMode mode)
    {
        // Plain thresholding makes flat 50% gray entirely black or entirely
        // white. Dithering must produce both, which is the whole point.
        var bitmap = Halftoner.Apply(Uniform(32, 32, 128), 32, 32, mode, 128, invert: false);
        int black = CountBlack(bitmap);
        Assert.InRange(black, 1, (32 * 32) - 1);
    }

    [Theory]
    [InlineData(DitherMode.FloydSteinberg)]
    [InlineData(DitherMode.Atkinson)]
    [InlineData(DitherMode.Ordered4x4)]
    public void Dithering_KeepsSolidBlackSolid(DitherMode mode)
    {
        var bitmap = Halftoner.Apply(Uniform(16, 16, 0), 16, 16, mode, 128, invert: false);
        Assert.Equal(16 * 16, CountBlack(bitmap));
    }

    [Theory]
    [InlineData(DitherMode.FloydSteinberg)]
    [InlineData(DitherMode.Atkinson)]
    [InlineData(DitherMode.Ordered4x4)]
    public void Dithering_KeepsSolidWhiteSolid(DitherMode mode)
    {
        var bitmap = Halftoner.Apply(Uniform(16, 16, 255), 16, 16, mode, 128, invert: false);
        Assert.Equal(0, CountBlack(bitmap));
    }

    [Fact]
    public void Ordered4x4_RepeatsEveryFourPixels()
    {
        var bitmap = Halftoner.Apply(Uniform(8, 8, 128), 8, 8, DitherMode.Ordered4x4, 128, invert: false);
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                Assert.Equal(bitmap.IsBlack(x, y), bitmap.IsBlack(x + 4, y + 4));
    }

    [Fact]
    public void Ordered4x4_OnFlatMidGrayIsExactlyHalfBlack()
    {
        // bias = (Bayer - 8) * 8, so at gray 128 against threshold 128 the
        // decision reduces to Bayer < 8: 8 of every 16 cells, 64 tiles, 512 dots.
        var bitmap = Halftoner.Apply(Uniform(32, 32, 128), 32, 32, DitherMode.Ordered4x4, 128, invert: false);
        Assert.Equal(512, CountBlack(bitmap));
    }

    [Fact]
    public void FloydSteinberg_OnFlatMidGrayMatchesGoldenPattern()
    {
        // Golden regression test pinning the exact Floyd-Steinberg kernel
        // output on a 4x4 flat 50% gray field. This value was measured, not
        // hand derived: it was captured from a run of the implementation and
        // then hand checked for plausibility (error propagates rightward and
        // downward, coverage is close to half, no all black or all white
        // rows). Changing this expected pattern requires deliberately
        // re-deriving the kernel, not adjusting the assertion to match
        // whatever the code currently produces.
        var bitmap = Halftoner.Apply(Uniform(4, 4, 128), 4, 4, DitherMode.FloydSteinberg, 128, invert: false);
        string[] expected =
        {
            ".#.#",
            "#.#.",
            ".#.#",
            "#.#.",
        };
        Assert.Equal(expected, Render(bitmap));
    }

    [Fact]
    public void FloydSteinbergAndAtkinson_MeasuredGoldenBlackCountsOnFlatMidGray()
    {
        // Measured regression goldens on a 32x32 flat 50% gray field, unlike
        // the derived Ordered4x4 count above: these two numbers were
        // captured from a run of the implementation, not computed by hand
        // from the kernel weights. Both kernels happen to land on exactly
        // half black by symmetry of a flat 50% input against a threshold of
        // 128, so the counts alone do not distinguish the two kernels from
        // each other; see FloydSteinbergAndAtkinsonProduceDifferentResults
        // below for that.
        var gray = Uniform(32, 32, 128);
        var floyd = Halftoner.Apply(gray, 32, 32, DitherMode.FloydSteinberg, 128, invert: false);
        var atkinson = Halftoner.Apply(gray, 32, 32, DitherMode.Atkinson, 128, invert: false);
        Assert.Equal(512, CountBlack(floyd));
        Assert.Equal(512, CountBlack(atkinson));
    }

    [Fact]
    public void FloydSteinbergAndAtkinsonProduceDifferentResults()
    {
        var gray = Uniform(32, 32, 128);
        var floyd = Halftoner.Apply(gray, 32, 32, DitherMode.FloydSteinberg, 128, invert: false);
        var atkinson = Halftoner.Apply(gray, 32, 32, DitherMode.Atkinson, 128, invert: false);
        Assert.NotEqual(floyd.Bits, atkinson.Bits);
    }

    [Fact]
    public void Apply_RejectsUnknownDitherMode()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Halftoner.Apply(Uniform(4, 4, 128), 4, 4, (DitherMode)99, 128, invert: false));
    }
}
