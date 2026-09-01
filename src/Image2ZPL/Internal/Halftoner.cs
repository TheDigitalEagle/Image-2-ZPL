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
    }
}
