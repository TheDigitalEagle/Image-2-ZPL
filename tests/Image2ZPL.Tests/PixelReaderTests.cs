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
}
