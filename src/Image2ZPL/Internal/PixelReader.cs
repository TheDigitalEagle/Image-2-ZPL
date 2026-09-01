using System;

namespace Image2ZPL.Internal
{
    /// <summary>
    /// Converts any supported source layout into a tightly packed eight bit
    /// grayscale buffer, one byte per pixel, 0 meaning black.
    /// </summary>
    internal static class PixelReader
    {
        public static int BytesPerPixel(SourcePixelFormat format)
        {
            switch (format)
            {
                case SourcePixelFormat.Mono1: return 0; // sub-byte, handled separately
                case SourcePixelFormat.Grayscale8: return 1;
                case SourcePixelFormat.Rgb24:
                case SourcePixelFormat.Bgr24: return 3;
                case SourcePixelFormat.Rgba32:
                case SourcePixelFormat.Bgra32: return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown pixel format.");
            }
        }

        public static int MinimumStride(int width, SourcePixelFormat format)
        {
            return format == SourcePixelFormat.Mono1
                ? (width + 7) / 8
                : width * BytesPerPixel(format);
        }

        public static byte[] ToGrayscale(ReadOnlySpan<byte> pixels, int width, int height, int stride, SourcePixelFormat format)
        {
            var gray = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                ReadOnlySpan<byte> row = pixels.Slice(y * stride, stride);
                int outputBase = y * width;

                for (int x = 0; x < width; x++)
                {
                    gray[outputBase + x] = ReadPixel(row, x, format);
                }
            }

            return gray;
        }

        private static byte ReadPixel(ReadOnlySpan<byte> row, int x, SourcePixelFormat format)
        {
            switch (format)
            {
                case SourcePixelFormat.Mono1:
                    return (row[x >> 3] & (0x80 >> (x & 7))) != 0 ? (byte)0 : (byte)255;

                case SourcePixelFormat.Grayscale8:
                    return row[x];

                case SourcePixelFormat.Rgb24:
                    return Luminance(row[x * 3], row[(x * 3) + 1], row[(x * 3) + 2]);

                case SourcePixelFormat.Bgr24:
                    return Luminance(row[(x * 3) + 2], row[(x * 3) + 1], row[x * 3]);

                case SourcePixelFormat.Rgba32:
                    return Composite(row[x * 4], row[(x * 4) + 1], row[(x * 4) + 2], row[(x * 4) + 3]);

                case SourcePixelFormat.Bgra32:
                    return Composite(row[(x * 4) + 2], row[(x * 4) + 1], row[x * 4], row[(x * 4) + 3]);

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown pixel format.");
            }
        }

        /// <summary>ITU-R BT.601 luma weights.</summary>
        private static byte Luminance(byte r, byte g, byte b)
        {
            return (byte)(((r * 299) + (g * 587) + (b * 114)) / 1000);
        }

        /// <summary>
        /// Composites over a white background, because label stock is white
        /// and a transparent pixel should print as nothing, not as a dot.
        /// </summary>
        private static byte Composite(byte r, byte g, byte b, byte a)
        {
            int value = Luminance(r, g, b);
            return (byte)(((value * a) + (255 * (255 - a))) / 255);
        }
    }
}
