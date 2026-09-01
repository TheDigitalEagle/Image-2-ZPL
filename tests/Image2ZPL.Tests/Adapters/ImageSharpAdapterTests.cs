using Image2ZPL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace Image2ZPL.Tests.Adapters;

public class ImageSharpAdapterTests
{
    [Fact]
    public void ToZpl_MatchesCoreOutputForTheSamePixels()
    {
        const int width = 5;
        const int height = 3;

        using var image = new Image<Rgba32>(width, height);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                image[x, y] = ((x + y) % 2 == 0) ? new Rgba32(0, 0, 0, 255) : new Rgba32(255, 255, 255, 255);

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
}
