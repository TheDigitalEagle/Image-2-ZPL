using System;

namespace Image2ZPL.Internal
{
    /// <summary>
    /// Reduces a grayscale buffer to a packed one bit per pixel bitmap.
    /// Nothing here ever addresses a pixel at or beyond the image width, so
    /// the unused bits in the final byte of each row stay zero. That is the
    /// structural fix for the right-edge clipping bug reported in PR #3.
    /// </summary>
    internal static class Halftoner
    {
        public static MonochromeBitmap Apply(byte[] gray, int width, int height, DitherMode mode, byte threshold, bool invert)
        {
            switch (mode)
            {
                case DitherMode.Threshold:
                    return ApplyThreshold(gray, width, height, threshold, invert);
                case DitherMode.FloydSteinberg:
                    return ApplyErrorDiffusion(gray, width, height, threshold, invert, FloydSteinbergKernel, 16);
                case DitherMode.Atkinson:
                    return ApplyErrorDiffusion(gray, width, height, threshold, invert, AtkinsonKernel, 8);
                case DitherMode.Ordered4x4:
                    return ApplyOrdered(gray, width, height, threshold, invert);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown dither mode.");
            }
        }

        private static MonochromeBitmap ApplyThreshold(byte[] gray, int width, int height, byte threshold, bool invert)
        {
            var bitmap = new MonochromeBitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                int rowBase = y * width;
                for (int x = 0; x < width; x++)
                {
                    if (IsDot(gray[rowBase + x] < threshold, invert))
                    {
                        bitmap.SetBlack(x, y);
                    }
                }
            }
            return bitmap;
        }

        private static bool IsDot(bool dark, bool invert)
        {
            return invert ? !dark : dark;
        }

        // Each entry is an offset from the current pixel and its share of the
        // quantisation error: dx, dy, weight.
        private static readonly int[][] FloydSteinbergKernel =
        {
            new[] { 1, 0, 7 },
            new[] { -1, 1, 3 },
            new[] { 0, 1, 5 },
            new[] { 1, 1, 1 },
        };

        private static readonly int[][] AtkinsonKernel =
        {
            new[] { 1, 0, 1 },
            new[] { 2, 0, 1 },
            new[] { -1, 1, 1 },
            new[] { 0, 1, 1 },
            new[] { 1, 1, 1 },
            new[] { 0, 2, 1 },
        };

        // Bayer matrix, values 0 to 15 in the standard recursive ordering.
        private static readonly int[] Bayer4x4 =
        {
            0, 8, 2, 10,
            12, 4, 14, 6,
            3, 11, 1, 9,
            15, 7, 13, 5,
        };

        private static MonochromeBitmap ApplyErrorDiffusion(
            byte[] gray, int width, int height, byte threshold, bool invert, int[][] kernel, int divisor)
        {
            var bitmap = new MonochromeBitmap(width, height);

            // Work in a wider buffer so diffused error can exceed byte range.
            var buffer = new int[gray.Length];
            for (int i = 0; i < gray.Length; i++)
            {
                buffer[i] = gray[i];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    int value = buffer[index];
                    bool dark = value < threshold;
                    if (IsDot(dark, invert))
                    {
                        bitmap.SetBlack(x, y);
                    }

                    int error = value - (dark ? 0 : 255);
                    for (int k = 0; k < kernel.Length; k++)
                    {
                        int nx = x + kernel[k][0];
                        int ny = y + kernel[k][1];
                        if (nx < 0 || nx >= width || ny >= height)
                        {
                            continue;
                        }
                        buffer[(ny * width) + nx] += error * kernel[k][2] / divisor;
                    }
                }
            }

            return bitmap;
        }

        private static MonochromeBitmap ApplyOrdered(byte[] gray, int width, int height, byte threshold, bool invert)
        {
            var bitmap = new MonochromeBitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Spread the decision point around the threshold by up to
                    // plus or minus 60 levels, ordered by the Bayer matrix.
                    int bias = (Bayer4x4[((y & 3) * 4) + (x & 3)] - 8) * 8;
                    int value = gray[(y * width) + x] + bias;
                    if (IsDot(value < threshold, invert))
                    {
                        bitmap.SetBlack(x, y);
                    }
                }
            }
            return bitmap;
        }
    }
}
