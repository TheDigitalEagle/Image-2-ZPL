namespace Image2ZPL
{
    /// <summary>
    /// Settings for a single image conversion. The defaults are a fixed
    /// threshold of 128 at the label origin, with compression on.
    /// </summary>
    public sealed class ZplImageOptions
    {
        /// <summary>Horizontal position of the field on the label, in dots.</summary>
        public int X { get; set; }

        /// <summary>Vertical position of the field on the label, in dots.</summary>
        public int Y { get; set; }

        /// <summary>How the image is reduced to one bit per pixel.</summary>
        public DitherMode Dither { get; set; } = DitherMode.Threshold;

        /// <summary>Decision point for every mode. A pixel whose value, after any dithering adjustment, falls below this prints as a dot.</summary>
        public byte Threshold { get; set; } = 128;

        /// <summary>Swaps black and white in the output.</summary>
        public bool Invert { get; set; }

        /// <summary>When false, emits plain ASCII hex with no run-length codes. Useful for debugging.</summary>
        public bool Compress { get; set; } = true;
    }
}
