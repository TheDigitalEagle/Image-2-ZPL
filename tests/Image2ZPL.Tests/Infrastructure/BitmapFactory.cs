using System;
using System.Collections.Generic;
using Image2ZPL.Internal;

namespace Image2ZPL.Tests.Infrastructure;

// Internal, not public: MonochromeBitmap is itself internal, so a public
// factory returning it would be an accessibility error (CS0050).
internal static class BitmapFactory
{
    public static MonochromeBitmap Random(int width, int height, int seed, int blackPercent)
    {
        var random = new Random(seed);
        var bitmap = new MonochromeBitmap(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (random.Next(100) < blackPercent)
                {
                    bitmap.SetBlack(x, y);
                }
            }
        }
        return bitmap;
    }

    public static MonochromeBitmap Solid(int width, int height, bool black)
    {
        var bitmap = new MonochromeBitmap(width, height);
        if (black)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bitmap.SetBlack(x, y);
                }
            }
        }
        return bitmap;
    }

    public static MonochromeBitmap AlternatingRows(int width, int height)
    {
        var bitmap = new MonochromeBitmap(width, height);
        for (int y = 0; y < height; y += 2)
        {
            for (int x = 0; x < width; x++)
            {
                bitmap.SetBlack(x, y);
            }
        }
        return bitmap;
    }

    public static MonochromeBitmap LongRun(int width, int height)
    {
        // A long black run that stops short of the right edge, so the row
        // cannot collapse to the all-black shortcut.
        var bitmap = new MonochromeBitmap(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width - 4; x++)
            {
                bitmap.SetBlack(x, y);
            }
        }
        return bitmap;
    }

    /// <summary>
    /// Widths chosen to exercise byte boundaries and the padding bits that
    /// caused the right-edge clipping bug fixed in PR #3.
    /// </summary>
    public static IEnumerable<int> InterestingWidths()
    {
        for (int w = 1; w <= 17; w++)
        {
            yield return w;
        }
        yield return 63;
        yield return 64;
        yield return 65;
        yield return 240 * 8;
    }
}
