using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Image2ZPL
{
    /// <summary>
    /// Converts ImageSharp images into ZPL II graphic fields.
    /// </summary>
    public static class ZplImageSharpExtensions
    {
        /// <summary>
        /// Converts an ImageSharp image into a ZPL II graphic field.
        /// </summary>
        public static string ToZpl(this Image<Rgba32> image, ZplImageOptions? options = null)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            return ZplImageConverter.ToZpl(
                pixels, image.Width, image.Height, image.Width * 4,
                SourcePixelFormat.Rgba32, options);
        }
    }
}
