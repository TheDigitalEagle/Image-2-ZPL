using System;
using System.Globalization;
using System.IO;
using System.Text;
using Image2ZPL.Internal;

namespace Image2ZPL
{
    /// <summary>
    /// Converts raw pixel data into a ZPL II graphic field that can be sent
    /// to a Zebra printer without uploading an image to the printer first.
    /// </summary>
    public static class ZplImageConverter
    {
        /// <summary>
        /// Converts pixel data into a ZPL graphic field string.
        /// </summary>
        /// <param name="pixels">Source pixel data, at least stride multiplied by height bytes.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="stride">Bytes per source row, including any padding.</param>
        /// <param name="format">Layout of the source pixel data.</param>
        /// <param name="options">Conversion settings, or null for the defaults.</param>
        public static string ToZpl(
            ReadOnlySpan<byte> pixels, int width, int height, int stride,
            SourcePixelFormat format, ZplImageOptions? options = null)
        {
            // Output is at minimum two characters per source byte, so start
            // well above the default capacity of 16 for anything but a tiny
            // image. A bad width or height here still yields a small,
            // positive fallback capacity, the real value is validated below.
            long estimatedCapacity = 64L + ((long)width * height / 4);
            int capacity = estimatedCapacity > 0 && estimatedCapacity <= int.MaxValue
                ? (int)estimatedCapacity
                : 64;

            var builder = new StringBuilder(capacity);
            using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
            {
                WriteZpl(writer, pixels, width, height, stride, format, options);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Writes a ZPL graphic field directly to a writer. Prefer this over
        /// <see cref="ToZpl"/> for large images, which produce large strings.
        /// </summary>
        /// <param name="writer">Destination for the ZPL text.</param>
        /// <param name="pixels">Source pixel data, at least stride multiplied by height bytes.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="stride">Bytes per source row, including any padding.</param>
        /// <param name="format">Layout of the source pixel data.</param>
        /// <param name="options">Conversion settings, or null for the defaults.</param>
        public static void WriteZpl(
            TextWriter writer, ReadOnlySpan<byte> pixels, int width, int height, int stride,
            SourcePixelFormat format, ZplImageOptions? options = null)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            options = options ?? new ZplImageOptions();
            Validate(pixels, width, height, stride, format, options);

            byte[] gray = PixelReader.ToGrayscale(pixels, width, height, stride, format);
            MonochromeBitmap bitmap = Halftoner.Apply(gray, width, height, options.Dither, options.Threshold, options.Invert);
            GraphicFieldEncoder.Write(writer, bitmap, options.X, options.Y, options.Compress);
        }

        private static void Validate(
            ReadOnlySpan<byte> pixels, int width, int height, int stride,
            SourcePixelFormat format, ZplImageOptions options)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be greater than zero.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be greater than zero.");
            }

            if (options.X < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options.X, "X must not be negative. ZPL has no negative field origin.");
            }

            if (options.Y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options.Y, "Y must not be negative. ZPL has no negative field origin.");
            }

            // Validated here, on the public boundary, rather than left to
            // Halftoner.Apply. That defers the throw until after a full
            // width * height grayscale pass, and reports the internal
            // parameter name "mode" instead of "options". An explicit
            // switch avoids the boxing Enum.IsDefined would cost on
            // netstandard2.0.
            switch (options.Dither)
            {
                case DitherMode.Threshold:
                case DitherMode.FloydSteinberg:
                case DitherMode.Atkinson:
                case DitherMode.Ordered4x4:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(options),
                        options.Dither,
                        string.Format(CultureInfo.InvariantCulture, "Unknown dither mode {0}.", options.Dither));
            }

            // Throws for an unknown format, so call it before using stride.
            int minimumStride = PixelReader.MinimumStride(width, format);

            if (stride < minimumStride)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Stride {0} is too small for a {1} pixel row in format {2}, which needs at least {3} bytes.",
                        stride, width, format, minimumStride),
                    nameof(stride));
            }

            long required = (long)stride * height;
            if (pixels.Length < required)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Pixel buffer holds {0} bytes but {1} are required for {2} rows of stride {3}.",
                        pixels.Length, required, height, stride),
                    nameof(pixels));
            }
        }
    }
}
