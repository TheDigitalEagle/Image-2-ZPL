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

    // The next three tests exercise ZplImageOptions.Invert, .Compress and
    // .Threshold through the public ToZpl entry point. Every internal layer
    // (Halftoner, GraphicFieldEncoder) already has direct unit coverage, but
    // nothing previously verified that ZplImageConverter actually threads
    // these three options through to those layers: a refactor that dropped
    // options.Invert from the WriteZpl call, for example, would have left
    // every other test green. Expected strings below are derived by hand
    // from Halftoner.ApplyThreshold and GraphicFieldEncoder.Write, not
    // copied from a test run.

    [Fact]
    public void ToZpl_InvertFlipsAnAllBlackImageToAllWhite()
    {
        // A 2x2 grayscale image, every pixel value 0 (pure black).
        byte[] allBlack = { 0, 0, 0, 0 };

        // Invert = false, default threshold 128: 0 < 128 so every pixel is a
        // dot. BytesPerRow = (2 + 7) / 8 = 1, so each row packs to 0xC0 (the
        // top two bits, one per column). Row 1 is not all 0x00 or all 0xFF
        // and has no previous row, so it is spelled out nibble by nibble:
        // nibble 0xC (run of 1, "C") then nibble 0x0, whose run reaches the
        // row end and so collapses to the ',' fill code. Row 2 repeats row 1
        // exactly, so it collapses to the ':' same-as-previous-row code.
        string notInverted = ZplImageConverter.ToZpl(allBlack, 2, 2, 2, SourcePixelFormat.Grayscale8);
        Assert.Equal("^FO0,0^GFA,2,2,1,C,:^FS", notInverted);

        // Invert = true flips every dot to blank: both rows become 0x00 and
        // each collapses to the ',' all-zero fill code.
        var inverted = new ZplImageOptions { Invert = true };
        string invertedResult = ZplImageConverter.ToZpl(allBlack, 2, 2, 2, SourcePixelFormat.Grayscale8, inverted);
        Assert.Equal("^FO0,0^GFA,2,2,1,,,^FS", invertedResult);

        Assert.NotEqual(notInverted, invertedResult);
    }

    [Fact]
    public void ToZpl_CompressProducesShorterOutputThanUncompressedForACompressibleImage()
    {
        // 16x1 grayscale, every pixel value 0 (pure black): a single 2-byte
        // row of 0xFF, 0xFF.
        byte[] allBlackWide = new byte[16];

        // Compress = true: the row is all 0xFF, which collapses to the '!'
        // fill code, one character for the whole row.
        var compressed = new ZplImageOptions { Compress = true };
        string compressedResult = ZplImageConverter.ToZpl(allBlackWide, 16, 1, 16, SourcePixelFormat.Grayscale8, compressed);
        Assert.Equal("^FO0,0^GFA,2,2,2,!^FS", compressedResult);

        // Compress = false: plain ASCII hex, two characters per byte, no
        // run-length codes at all.
        var uncompressed = new ZplImageOptions { Compress = false };
        string uncompressedResult = ZplImageConverter.ToZpl(allBlackWide, 16, 1, 16, SourcePixelFormat.Grayscale8, uncompressed);
        Assert.Equal("^FO0,0^GFA,2,2,2,FFFF^FS", uncompressedResult);

        Assert.True(compressedResult.Length < uncompressedResult.Length);
    }

    [Fact]
    public void ToZpl_ThresholdChangesWhetherAPixelBecomesADot()
    {
        // A single pixel with grayscale value 100, strictly between the two
        // thresholds tried below.
        byte[] midGray = { 100 };

        // Threshold = 110: 100 < 110, so the pixel is a dot. The single bit
        // packs to 0x80, which is not all-zero or all-0xFF and has no
        // previous row, so it spells out as nibble 0x8 (run of 1, "8") then
        // nibble 0x0, whose run reaches the row end and collapses to ','.
        var dotOptions = new ZplImageOptions { Threshold = 110 };
        string dot = ZplImageConverter.ToZpl(midGray, 1, 1, 1, SourcePixelFormat.Grayscale8, dotOptions);
        Assert.Equal("^FO0,0^GFA,1,1,1,8,^FS", dot);

        // Threshold = 90: 100 is not < 90, so the pixel is left blank. The
        // single bit is 0x00, which collapses to the ',' all-zero fill code.
        var noDotOptions = new ZplImageOptions { Threshold = 90 };
        string noDot = ZplImageConverter.ToZpl(midGray, 1, 1, 1, SourcePixelFormat.Grayscale8, noDotOptions);
        Assert.Equal("^FO0,0^GFA,1,1,1,,^FS", noDot);

        Assert.NotEqual(dot, noDot);
    }
}
