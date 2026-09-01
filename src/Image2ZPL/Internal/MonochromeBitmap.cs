using System;

namespace Image2ZPL.Internal
{
    /// <summary>
    /// A packed one-bit-per-pixel bitmap. A set bit means a black dot, which
    /// matches the ZPL convention directly, so the encoder never inverts.
    /// Bits beyond <see cref="Width"/> in the final byte of a row are always
    /// zero, because nothing ever sets them.
    /// </summary>
    internal sealed class MonochromeBitmap
    {
        public MonochromeBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            BytesPerRow = (width + 7) / 8;
            Bits = new byte[BytesPerRow * height];
        }

        public int Width { get; }

        public int Height { get; }

        public int BytesPerRow { get; }

        public byte[] Bits { get; }

        public void SetBlack(int x, int y)
        {
            Bits[(y * BytesPerRow) + (x >> 3)] |= (byte)(0x80 >> (x & 7));
        }

        public bool IsBlack(int x, int y)
        {
            return (Bits[(y * BytesPerRow) + (x >> 3)] & (0x80 >> (x & 7))) != 0;
        }

        public ReadOnlySpan<byte> Row(int y)
        {
            return new ReadOnlySpan<byte>(Bits, y * BytesPerRow, BytesPerRow);
        }
    }
}
