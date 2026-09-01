using System;
using SkiaSharp;

namespace Image2ZPL
{
    /// <summary>
    /// Converts SkiaSharp bitmaps into ZPL II graphic fields.
    /// </summary>
    public static class ZplSkiaSharpExtensions
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

            // The already-Bgra8888 case is the common one (it is what
            // SKBitmap.Decode produces on most platforms), and the core
            // takes a ReadOnlySpan<byte> specifically so this path needs no
            // copy at all: read the caller's pixels directly and leave the
            // bitmap alone. Copying here, only so a using block would have
            // something safe to dispose, cost a full native copy plus a
            // second managed copy through .Bytes on every call.
            if (bitmap.ColorType == SKColorType.Bgra8888)
            {
                return ZplImageConverter.ToZpl(
                    bitmap.GetPixelSpan(), bitmap.Width, bitmap.Height,
                    bitmap.RowBytes, SourcePixelFormat.Bgra32, options);
            }

            // Any other colour type still needs converting to a known
            // layout, which does require an owned, disposable bitmap.
            using (SKBitmap converted = ConvertToBgra8888(bitmap))
            {
                return ZplImageConverter.ToZpl(
                    converted.Bytes, converted.Width, converted.Height, converted.RowBytes,
                    SourcePixelFormat.Bgra32, options);
            }
        }

        private static SKBitmap ConvertToBgra8888(SKBitmap bitmap)
        {
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
