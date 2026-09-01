using Image2ZPL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Image2ZPL.Tests.Adapters;

public class ImageSharpAdapterTests
{
    // RGB(255, 100, 0) sits on the far side of the default luma threshold
    // (128) from its own red/blue swap: read correctly its luma is 134
    // (white, no dot); read with red and blue swapped it is 87 (black, a
    // dot). Pure black and pure white cannot catch a channel swap because R,
    // G and B are equal in both, so this colour is included specifically to
    // make the parity test sensitive to RGBA/BGRA channel-order bugs.
    private static readonly Rgba32 Orange = new Rgba32(255, 100, 0, 255);

    [Fact]
    public void ToZpl_MatchesCoreOutputForTheSamePixels()
    {
        const int width = 5;
        const int height = 3;

        using var image = new Image<Rgba32>(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                image[x, y] = PixelAt(x, y);

        var pixels = new byte[width * height * 4];
        image.CopyPixelDataTo(pixels);

        string viaCore = ZplImageConverter.ToZpl(pixels, width, height, width * 4, SourcePixelFormat.Rgba32);
        Assert.Equal(viaCore, image.ToZpl());
    }

    [Fact]
    public void ToZpl_PassesOptionsThrough()
    {
        using var image = new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 255));
        Assert.StartsWith("^FO7,9^GFA,", image.ToZpl(new ZplImageOptions { X = 7, Y = 9 }));
    }

    private static Rgba32 PixelAt(int x, int y)
    {
        return ((x + y) % 3) switch
        {
            0 => new Rgba32(0, 0, 0, 255),
            1 => new Rgba32(255, 255, 255, 255),
            _ => Orange,
        };
    }
}
