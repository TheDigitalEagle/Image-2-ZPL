using System;
using System.Drawing;

namespace Image2ZPL
{
    /// <summary>
    /// The version 1.x entry point, kept so existing code compiles after
    /// adding a reference to this package.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Converts a bitmap into a ZPL II graphic field.
        /// </summary>
        [Obsolete("Use bitmap.ToZpl(new ZplImageOptions { X = posX, Y = posY }) instead. See MIGRATION.md.")]
        public static string BitmapToZPLII(Bitmap imageToConvert, int posX, int posY)
        {
            return imageToConvert.ToZpl(new ZplImageOptions { X = posX, Y = posY });
        }
    }
}
