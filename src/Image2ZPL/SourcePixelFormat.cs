namespace Image2ZPL
{
    /// <summary>
    /// Layout of the pixel data handed to the converter.
    /// </summary>
    public enum SourcePixelFormat
    {
        /// <summary>One bit per pixel, most significant bit first, a set bit meaning black.</summary>
        Mono1,

        /// <summary>Eight bits per pixel, 0 meaning black.</summary>
        Grayscale8,

        /// <summary>Three bytes per pixel in red, green, blue order.</summary>
        Rgb24,

        /// <summary>Three bytes per pixel in blue, green, red order.</summary>
        Bgr24,

        /// <summary>Four bytes per pixel in red, green, blue, alpha order.</summary>
        Rgba32,

        /// <summary>Four bytes per pixel in blue, green, red, alpha order.</summary>
        Bgra32,
    }
}
