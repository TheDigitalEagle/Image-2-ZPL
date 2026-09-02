using Image2ZPL;
using SkiaSharp;
using Xunit;

namespace Image2ZPL.Tests.Adapters;

public class SkiaSharpAdapterTests
{
    // RGB(255, 100, 0) sits on the far side of the default luma threshold
    // (128) from its own red/blue swap: read correctly its luma is 134
    // (white, no dot); read with red and blue swapped it is 87 (black, a
    // dot). Pure black and pure white cannot catch a channel swap because R,
    // G and B are equal in both, so this colour is included specifically to
    // make the parity test sensitive to BGRA/RGBA channel-order bugs.
    private static readonly SKColor Orange = new SKColor(255, 100, 0);

    [Fact]
    public void ToZpl_MatchesCoreOutputForTheSamePixels()
    {
        const int width = 5;
        const int height = 3;

        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bitmap.SetPixel(x, y, PixelAt(x, y));

        string viaAdapter = bitmap.ToZpl();

        byte[] expectedPixels = bitmap.Bytes;
        string viaCore = ZplImageConverter.ToZpl(
            expectedPixels, width, height, bitmap.RowBytes, SourcePixelFormat.Bgra32);

        Assert.Equal(viaCore, viaAdapter);
    }

    [Fact]
    public void ToZpl_PassesOptionsThrough()
    {
        using var bitmap = new SKBitmap(2, 2, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        bitmap.Erase(SKColors.Black);
        Assert.StartsWith("^FO7,9^GFA,", bitmap.ToZpl(new ZplImageOptions { X = 7, Y = 9 }));
    }

    [Fact]
    public void ToZpl_ConvertsNonBgra8888ColorTypesCorrectly()
    {
        // EnsureBgra8888 has a conversion branch for bitmaps that are not
        // already Bgra8888. Every other test uses a Bgra8888 bitmap, which
        // never exercises that branch. This builds the same pixels twice,
        // once in each colour type, and computes the expected value by
        // calling the core directly against the Bgra8888 reference bytes
        // (bypassing the adapter entirely for that side), then compares it
        // to the adapter's output for the Rgba8888 source. Comparing two
        // adapter calls to each other would not do: both would be wrong in
        // the same way under a channel-order bug in the adapter itself, and
        // the assertion would still pass. Routing the expected value through
        // the core directly keeps this sensitive to that bug, not just to
        // SkiaSharp's own conversion.
        const int width = 5;
        const int height = 3;

        using var bgraReference = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var rgbaBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                SKColor colour = PixelAt(x, y);
                bgraReference.SetPixel(x, y, colour);
                rgbaBitmap.SetPixel(x, y, colour);
            }

        string viaCore = ZplImageConverter.ToZpl(
            bgraReference.Bytes, width, height, bgraReference.RowBytes, SourcePixelFormat.Bgra32);

        Assert.Equal(viaCore, rgbaBitmap.ToZpl());
    }

    private static SKColor PixelAt(int x, int y)
    {
        return ((x + y) % 3) switch
        {
            0 => SKColors.Black,
            1 => SKColors.White,
            _ => Orange,
        };
    }
}
