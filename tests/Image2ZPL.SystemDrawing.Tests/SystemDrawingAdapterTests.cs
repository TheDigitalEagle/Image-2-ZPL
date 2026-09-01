using System.Drawing;
using Image2ZPL;
using Xunit;

namespace Image2ZPL.SystemDrawing.Tests;

public class SystemDrawingAdapterTests
{
    private static Bitmap Checkerboard(int width, int height)
    {
        var bitmap = new Bitmap(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bitmap.SetPixel(x, y, ((x + y) % 2 == 0) ? Color.Black : Color.White);
        return bitmap;
    }

    private static Bitmap FromColors(int width, int height, Color[] colors)
    {
        var bitmap = new Bitmap(width, height);
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bitmap.SetPixel(x, y, colors[i++]);
        return bitmap;
    }

    [Fact]
    public void ToZpl_ProducesAGraphicField()
    {
        using var bitmap = Checkerboard(5, 3);
        string zpl = bitmap.ToZpl();
        Assert.StartsWith("^FO0,0^GFA,", zpl);
        Assert.EndsWith("^FS", zpl);
    }

    [Fact]
    public void ToZpl_PassesOptionsThrough()
    {
        using var bitmap = Checkerboard(2, 2);
        Assert.StartsWith("^FO7,9^GFA,", bitmap.ToZpl(new ZplImageOptions { X = 7, Y = 9 }));
    }

    [Fact]
    public void ToZpl_MatchesAHandBuiltBgraBuffer()
    {
        // GDI+ locks Format32bppArgb as, per pixel and little endian, the
        // byte order B, G, R, A. This array is written independently of
        // LockBits, using that known layout, so the comparison below
        // actually exercises the adapter's copy, stride, and row origin
        // rather than testing the adapter against itself.
        Color[] colors =
        {
            Color.FromArgb(10, 20, 30),
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(255, 255, 255),
            Color.FromArgb(200, 100, 50),
            Color.FromArgb(50, 200, 100),
            Color.FromArgb(100, 50, 200),
        };
        using var bitmap = FromColors(3, 2, colors);

        byte[] handBuilt =
        {
            // row 0: (0,0) (1,0) (2,0), each as B, G, R, A
            30, 20, 10, 255,
            0, 0, 0, 255,
            255, 255, 255, 255,
            // row 1: (0,1) (1,1) (2,1)
            50, 100, 200, 255,
            100, 200, 50, 255,
            200, 50, 100, 255,
        };

        var options = new ZplImageOptions();
        string expected = ZplImageConverter.ToZpl(handBuilt, 3, 2, 12, SourcePixelFormat.Bgra32, options);
        Assert.Equal(expected, bitmap.ToZpl(options));
    }

    [Fact]
    public void ToZpl_DoesNotSwapRedAndBlueChannels()
    {
        // Pure red has luminance about 76 and pure blue has luminance about
        // 29, on 0.299R + 0.587G + 0.114B. A threshold of 50 puts them on
        // opposite sides of the dot cutoff, so a red/blue channel swap
        // (reading the locked bytes as Rgba32 instead of Bgra32) would flip
        // both dots and change the output. The default threshold of 128
        // cannot see this: both colors fall below it either way.
        using var bitmap = FromColors(2, 1, new[] { Color.Red, Color.Blue });

        byte[] handBuilt =
        {
            0, 0, 255, 255,   // (0,0) pure red: B=0 G=0 R=255 A=255
            255, 0, 0, 255,   // (1,0) pure blue: B=255 G=0 R=0 A=255
        };

        var options = new ZplImageOptions { Threshold = 50 };
        string expected = ZplImageConverter.ToZpl(handBuilt, 2, 1, 8, SourcePixelFormat.Bgra32, options);
        Assert.Equal(expected, bitmap.ToZpl(options));
    }

    [Fact]
    public void LegacyConvert_MatchesTheNewExtensionMethod()
    {
        using var bitmap = Checkerboard(9, 4);
#pragma warning disable CS0618 // testing the obsolete shim on purpose
        string legacy = global::Image2ZPL.Convert.BitmapToZPLII(bitmap, 20, 35);
#pragma warning restore CS0618
        Assert.Equal(bitmap.ToZpl(new ZplImageOptions { X = 20, Y = 35 }), legacy);
    }
}
