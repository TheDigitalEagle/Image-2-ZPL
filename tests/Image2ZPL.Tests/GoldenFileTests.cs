using System.IO;
using Image2ZPL;
using Xunit;

namespace Image2ZPL.Tests;

public class GoldenFileTests
{
    // Set IMAGE2ZPL_UPDATE_GOLDEN=1 to rewrite the expected files after an
    // intentional behaviour change. Review the diff before committing.
    private static readonly bool Update =
        System.Environment.GetEnvironmentVariable("IMAGE2ZPL_UPDATE_GOLDEN") == "1";

    private static byte[] Gradient(int width, int height)
    {
        var gray = new byte[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                gray[(y * width) + x] = (byte)((x * 255) / (width - 1));
        return gray;
    }

    [Theory]
    [InlineData(DitherMode.Threshold, "gradient-threshold.zpl")]
    [InlineData(DitherMode.FloydSteinberg, "gradient-floyd.zpl")]
    [InlineData(DitherMode.Atkinson, "gradient-atkinson.zpl")]
    [InlineData(DitherMode.Ordered4x4, "gradient-ordered.zpl")]
    public void Gradient_MatchesGoldenOutput(DitherMode mode, string fileName)
    {
        const int width = 37;
        const int height = 12;
        string actual = ZplImageConverter.ToZpl(
            Gradient(width, height), width, height, width,
            SourcePixelFormat.Grayscale8,
            new ZplImageOptions { Dither = mode });

        string path = Path.Combine("Golden", fileName);
        if (Update)
        {
            Directory.CreateDirectory("Golden");
            File.WriteAllText(path, actual);
        }

        Assert.True(
            File.Exists(path),
            $"Golden file {path} is missing. Set IMAGE2ZPL_UPDATE_GOLDEN=1 to create it.");

        Assert.Equal(File.ReadAllText(path), actual);
    }
}
