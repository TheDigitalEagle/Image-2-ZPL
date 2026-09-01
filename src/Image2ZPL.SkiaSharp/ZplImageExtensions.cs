using System;
using SkiaSharp;

namespace Image2ZPL
{
    /// <summary>
    /// Converts SkiaSharp bitmaps into ZPL II graphic fields.
    /// </summary>
    public static class ZplImageExtensions
    {
        /// <summary>
        /// Converts an <see cref="SKBitmap"/> into a ZPL II graphic field.
        /// </summary>
        public static string ToZpl(this SKBitmap bitmap, ZplImageOptions? options = null)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            // Normalise to a known layout, because callers may hand us any
            // colour type Skia supports.
            using (SKBitmap source = EnsureBgra8888(bitmap))
            {
                return ZplImageConverter.ToZpl(
                    source.Bytes, source.Width, source.Height, source.RowBytes,
                    SourcePixelFormat.Bgra32, options);
            }
        }

        private static SKBitmap EnsureBgra8888(SKBitmap bitmap)
        {
            if (bitmap.ColorType == SKColorType.Bgra8888)
            {
                // Copy so the caller's bitmap is never disposed by us.
                SKBitmap? copy = bitmap.Copy();
                if (copy == null)
                {
                    throw new NotSupportedException("Could not copy the source SKBitmap.");
                }
                return copy;
            }

            var converted = new SKBitmap(new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
            if (!bitmap.CopyTo(converted, SKColorType.Bgra8888))
            {
                converted.Dispose();
                throw new NotSupportedException($"Could not convert an SKBitmap of colour type {bitmap.ColorType} to Bgra8888.");
            }
            return converted;
        }
    }
}
