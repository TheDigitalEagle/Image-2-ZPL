using Image2ZPL;
using SkiaSharp;
using Xunit;

namespace Image2ZPL.Tests.Adapters;

public class SkiaSharpAdapterTests
{
    [Fact]
    public void ToZpl_MatchesCoreOutputForTheSamePixels()
    {
        const int width = 5;
        const int height = 3;

        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                bitmap.SetPixel(x, y, ((x + y) % 2 == 0) ? SKColors.Black : SKColors.White);

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
}
