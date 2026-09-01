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
    public void LegacyConvert_MatchesTheNewExtensionMethod()
    {
        using var bitmap = Checkerboard(9, 4);
#pragma warning disable CS0618 // testing the obsolete shim on purpose
        string legacy = global::Image2ZPL.Convert.BitmapToZPLII(bitmap, 20, 20);
#pragma warning restore CS0618
        Assert.Equal(bitmap.ToZpl(new ZplImageOptions { X = 20, Y = 20 }), legacy);
    }
}
