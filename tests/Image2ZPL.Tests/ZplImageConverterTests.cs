using System;
using System.IO;
using Image2ZPL;
using Xunit;

namespace Image2ZPL.Tests;

public class ZplImageConverterTests
{
    private static readonly byte[] TwoByTwoGray = { 0, 255, 255, 0 };

    [Fact]
    public void ToZpl_ProducesACompleteGraphicField()
    {
        string zpl = ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8);
        Assert.StartsWith("^FO0,0^GFA,", zpl);
        Assert.EndsWith("^FS", zpl);
    }

    [Fact]
    public void ToZpl_HonoursPosition()
    {
        var options = new ZplImageOptions { X = 40, Y = 60 };
        string zpl = ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8, options);
        Assert.StartsWith("^FO40,60^GFA,", zpl);
    }

    [Fact]
    public void WriteZpl_MatchesToZpl()
    {
        using var writer = new StringWriter();
        ZplImageConverter.WriteZpl(writer, TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8);
        Assert.Equal(ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8), writer.ToString());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    public void ToZpl_RejectsNonPositiveDimensions(int width, int height)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, width, height, 2, SourcePixelFormat.Grayscale8));
        Assert.False(string.IsNullOrEmpty(ex.Message));
    }

    [Fact]
    public void ToZpl_RejectsStrideNarrowerThanARow()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 1, SourcePixelFormat.Grayscale8));
        Assert.Contains("stride", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToZpl_RejectsBufferShorterThanTheImage()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ZplImageConverter.ToZpl(new byte[3], 2, 2, 2, SourcePixelFormat.Grayscale8));
        Assert.Contains("4", ex.Message);
    }

    [Fact]
    public void ToZpl_RejectsNegativePosition()
    {
        var options = new ZplImageOptions { X = -1 };
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8, options));
    }

    [Fact]
    public void WriteZpl_RejectsNullWriter()
    {
        Assert.Throws<ArgumentNullException>(
            () => ZplImageConverter.WriteZpl(null!, TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8));
    }

    [Fact]
    public void ToZpl_RejectsUnknownPixelFormat()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, (SourcePixelFormat)99));
    }
}
