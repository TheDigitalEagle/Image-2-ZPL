using System;
using Image2ZPL;
using Image2ZPL.Internal;
using Xunit;

namespace Image2ZPL.Tests;

public class PixelReaderTests
{
    [Fact]
    public void Grayscale8_PassesValuesThrough()
    {
        byte[] pixels = { 0, 128, 255 };
        var gray = PixelReader.ToGrayscale(pixels, 3, 1, 3, SourcePixelFormat.Grayscale8);
        Assert.Equal(new byte[] { 0, 128, 255 }, gray);
    }

    [Fact]
    public void Grayscale8_SkipsStridePadding()
    {
        byte[] pixels = { 10, 20, 99, 99, 30, 40, 99, 99 };
        var gray = PixelReader.ToGrayscale(pixels, 2, 2, 4, SourcePixelFormat.Grayscale8);
        Assert.Equal(new byte[] { 10, 20, 30, 40 }, gray);
    }

    [Fact]
    public void Rgb24_UsesLuminanceWeights()
    {
        // Pure green is the brightest channel by the ITU-R BT.601 weights.
        byte[] pixels = { 255, 0, 0, 0, 255, 0, 0, 0, 255 };
        var gray = PixelReader.ToGrayscale(pixels, 3, 1, 9, SourcePixelFormat.Rgb24);
        Assert.Equal(76, gray[0]);
        Assert.Equal(149, gray[1]);
        Assert.Equal(29, gray[2]);
    }

    [Fact]
    public void Bgr24_ReadsChannelsInReverseOrder()
    {
        byte[] pixels = { 0, 0, 255 };
        var gray = PixelReader.ToGrayscale(pixels, 1, 1, 3, SourcePixelFormat.Bgr24);
        Assert.Equal(76, gray[0]);
    }

    [Fact]
    public void Rgba32_CompositesTransparencyOverWhite()
    {
        // Fully transparent black must read as white, not black.
        byte[] pixels = { 0, 0, 0, 0 };
        var gray = PixelReader.ToGrayscale(pixels, 1, 1, 4, SourcePixelFormat.Rgba32);
        Assert.Equal(255, gray[0]);
    }

    [Fact]
    public void Rgba32_OpaqueBlackStaysBlack()
    {
        byte[] pixels = { 0, 0, 0, 255 };
        var gray = PixelReader.ToGrayscale(pixels, 1, 1, 4, SourcePixelFormat.Rgba32);
        Assert.Equal(0, gray[0]);
    }

    [Fact]
    public void Mono1_TreatsSetBitsAsBlack()
    {
        byte[] pixels = { 0x80 };
        var gray = PixelReader.ToGrayscale(pixels, 2, 1, 1, SourcePixelFormat.Mono1);
        Assert.Equal(0, gray[0]);
        Assert.Equal(255, gray[1]);
    }

    [Fact]
    public void Bgra32_OpaquePureRed()
    {
        // Opaque pure red in BGRA order: {0, 0, 255, 255} must read as 76.
        byte[] pixels = { 0, 0, 255, 255 };
        var gray = PixelReader.ToGrayscale(pixels, 1, 1, 4, SourcePixelFormat.Bgra32);
        Assert.Equal(76, gray[0]);
    }

    [Fact]
    public void Bgra32_CompositesTransparencyOverWhite()
    {
        // Fully transparent black in BGRA order: {0, 0, 0, 0} must read as white.
        byte[] pixels = { 0, 0, 0, 0 };
        var gray = PixelReader.ToGrayscale(pixels, 1, 1, 4, SourcePixelFormat.Bgra32);
        Assert.Equal(255, gray[0]);
    }

    [Theory]
    [InlineData(SourcePixelFormat.Mono1, 8, 1)]
    [InlineData(SourcePixelFormat.Mono1, 9, 2)]
    [InlineData(SourcePixelFormat.Mono1, 10, 2)]
    [InlineData(SourcePixelFormat.Mono1, 16, 2)]
    [InlineData(SourcePixelFormat.Mono1, 17, 3)]
    [InlineData(SourcePixelFormat.Grayscale8, 10, 10)]
    [InlineData(SourcePixelFormat.Rgb24, 10, 30)]
    [InlineData(SourcePixelFormat.Bgr24, 10, 30)]
    [InlineData(SourcePixelFormat.Rgba32, 10, 40)]
    [InlineData(SourcePixelFormat.Bgra32, 10, 40)]
    public void MinimumStride_CalculatesCorrectly(SourcePixelFormat format, int width, int expectedStride)
    {
        var stride = PixelReader.MinimumStride(width, format);
        Assert.Equal(expectedStride, stride);
    }

    [Fact]
    public void BytesPerPixel_Mono1_Throws()
    {
        var ex = Assert.Throws<NotSupportedException>(() => PixelReader.BytesPerPixel(SourcePixelFormat.Mono1));
        Assert.Contains("Mono1 is one bit per pixel", ex.Message);
    }

    [Fact]
    public void BytesPerPixel_UnknownFormat_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => PixelReader.BytesPerPixel((SourcePixelFormat)999));
        Assert.Equal("format", ex.ParamName);
    }

    [Fact]
    public void MinimumStride_Mono1_WorksWithoutCallingBytesPerPixel()
    {
        // This must work even though BytesPerPixel(Mono1) throws, because MinimumStride
        // takes the ceiling path and never calls BytesPerPixel.
        var stride = PixelReader.MinimumStride(10, SourcePixelFormat.Mono1);
        Assert.Equal(2, stride);
    }
}
