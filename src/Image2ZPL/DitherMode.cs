namespace Image2ZPL
{
    /// <summary>
    /// How a grayscale image is reduced to one bit per pixel.
    /// </summary>
    public enum DitherMode
    {
        /// <summary>Every pixel is compared against a fixed threshold. Best for line art and text.</summary>
        Threshold,

        /// <summary>Floyd and Steinberg error diffusion. Best general choice for photographs.</summary>
        FloydSteinberg,

        /// <summary>Atkinson error diffusion. Higher contrast than Floyd-Steinberg, with cleaner whites.</summary>
        Atkinson,

        /// <summary>Ordered dithering with a four by four Bayer matrix. Fast, with a visible regular pattern.</summary>
        Ordered4x4,
    }
}
