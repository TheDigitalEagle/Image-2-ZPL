using System;
using System.Collections.Generic;
using System.IO;
using Image2ZPL.Internal;
using Image2ZPL.Tests.Infrastructure;
using Xunit;

namespace Image2ZPL.Tests;

// MonochromeBitmap is internal, so it cannot appear in a public [Theory]
// method's parameter list (that is a CS0051 accessibility error, not just
// an analyzer warning). Theory data below yields plain, xUnit-serializable
// parameters instead (a shape label, width, height, seed, black percent),
// and the bitmap is built inside the test body via BitmapFactory. That
// keeps the data serializable and gives readable per-case test names,
// while covering exactly the same shapes the brief specified.
public enum Shape
{
    WhiteSolid,
    BlackSolid,
    AlternatingRows,
    LongRun,
    Random,
}

public class RoundTripTests
{
    public static IEnumerable<object[]> Shapes()
    {
        foreach (int width in BitmapFactory.InterestingWidths())
        {
            yield return new object[] { Shape.WhiteSolid, width, 3, 0, 0 };
            yield return new object[] { Shape.BlackSolid, width, 3, 0, 0 };
            yield return new object[] { Shape.AlternatingRows, width, 4, 0, 0 };
            yield return new object[] { Shape.LongRun, width, 2, 0, 0 };
            foreach (int density in new[] { 5, 50, 95 })
            {
                yield return new object[] { Shape.Random, width, 5, width * density, density };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void Compressed_RoundTripsToIdenticalBits(Shape shape, int width, int height, int seed, int blackPercent)
    {
        AssertRoundTrip(Build(shape, width, height, seed, blackPercent), compress: true);
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void Uncompressed_RoundTripsToIdenticalBits(Shape shape, int width, int height, int seed, int blackPercent)
    {
        AssertRoundTrip(Build(shape, width, height, seed, blackPercent), compress: false);
    }

    private static MonochromeBitmap Build(Shape shape, int width, int height, int seed, int blackPercent)
    {
        return shape switch
        {
            Shape.WhiteSolid => BitmapFactory.Solid(width, height, black: false),
            Shape.BlackSolid => BitmapFactory.Solid(width, height, black: true),
            Shape.AlternatingRows => BitmapFactory.AlternatingRows(width, height),
            Shape.LongRun => BitmapFactory.LongRun(width, height),
            Shape.Random => BitmapFactory.Random(width, height, seed, blackPercent),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unknown shape."),
        };
    }

    private static void AssertRoundTrip(MonochromeBitmap original, bool compress)
    {
        using var writer = new StringWriter();
        GraphicFieldEncoder.Write(writer, original, 0, 0, compress);
        var decoded = GfaDecoder.Decode(writer.ToString());

        Assert.Equal(original.BytesPerRow, decoded.BytesPerRow);
        Assert.Equal(original.Height, decoded.Height);
        for (int y = 0; y < original.Height; y++)
        {
            Assert.Equal(original.Row(y).ToArray(), decoded.Rows[y]);
        }
    }
}
