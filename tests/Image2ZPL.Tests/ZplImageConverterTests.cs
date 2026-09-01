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
        Assert.Equal(width <= 0 ? "width" : "height", ex.ParamName);
    }

    [Fact]
    public void ToZpl_RejectsNegativeHeight()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, -1, 2, SourcePixelFormat.Grayscale8));
        Assert.Equal("height", ex.ParamName);
    }

    [Fact]
    public void ToZpl_RejectsStrideNarrowerThanARow()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 1, SourcePixelFormat.Grayscale8));
        Assert.Contains("stride", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("stride", ex.ParamName);
    }

    [Fact]
    public void ToZpl_RejectsBufferShorterThanTheImage()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ZplImageConverter.ToZpl(new byte[3], 2, 2, 2, SourcePixelFormat.Grayscale8));
        Assert.Contains("4", ex.Message);
        Assert.Equal("pixels", ex.ParamName);
    }

    [Fact]
    public void ToZpl_RejectsNegativePosition()
    {
        var options = new ZplImageOptions { X = -1 };
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8, options));
        Assert.Equal("options", ex.ParamName);
    }

    [Fact]
    public void ToZpl_RejectsNegativeY()
    {
        var options = new ZplImageOptions { Y = -1 };
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8, options));
        Assert.Equal("options", ex.ParamName);
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

    [Fact]
    public void ToZpl_ChecksFormatBeforeStride()
    {
        // Stride and buffer are both invalid too. Only checking the format
        // first yields ArgumentOutOfRangeException rather than ArgumentException.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(new byte[1], 2, 2, 1, (SourcePixelFormat)99));
    }

    [Fact]
    public void ToZpl_AcceptsMono1MinimumStride()
    {
        // 8 pixels wide packs into exactly one byte per row.
        var bits = new byte[] { 0xFF };
        string zpl = ZplImageConverter.ToZpl(bits, 8, 1, 1, SourcePixelFormat.Mono1);
        Assert.StartsWith("^FO0,0^GFA,", zpl);
    }

    [Fact]
    public void ToZpl_RejectsUnknownDitherMode()
    {
        var options = new ZplImageOptions { Dither = (DitherMode)99 };
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => ZplImageConverter.ToZpl(TwoByTwoGray, 2, 2, 2, SourcePixelFormat.Grayscale8, options));
        Assert.Equal("options", ex.ParamName);
    }
}
