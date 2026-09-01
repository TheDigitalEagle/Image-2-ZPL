using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Image2ZPL
{
    /// <summary>
    /// Converts System.Drawing bitmaps into ZPL II graphic fields.
    /// Windows only, because System.Drawing is a GDI+ wrapper.
    /// </summary>
    public static class ZplSystemDrawingExtensions
    {
        /// <summary>
        /// Converts a <see cref="Bitmap"/> into a ZPL II graphic field.
        /// </summary>
        public static string ToZpl(this Bitmap bitmap, ZplImageOptions? options = null)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            var area = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                if (data.Stride < 0)
                {
                    throw new NotSupportedException(
                        "A bottom up bitmap (negative stride) is not supported. Clone the bitmap into a top down copy first.");
                }

                var pixels = new byte[data.Stride * data.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                return ZplImageConverter.ToZpl(
                    pixels, bitmap.Width, bitmap.Height, data.Stride,
                    SourcePixelFormat.Bgra32, options);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }
}
