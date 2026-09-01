using System.Globalization;
using System.IO;

namespace Image2ZPL.Internal
{
    /// <summary>
    /// Writes a ZPL graphic field:
    /// ^FO{x},{y}^GFA,{byteCount},{fieldCount},{bytesPerRow},{data}^FS
    /// The data is ASCII hex, optionally using ZPL run-length compression.
    /// </summary>
    internal static class GraphicFieldEncoder
    {
        private const string HexDigits = "0123456789ABCDEF";

        public static void Write(TextWriter writer, MonochromeBitmap bitmap, int x, int y, bool compress)
        {
            int totalBytes = bitmap.BytesPerRow * bitmap.Height;

            writer.Write("^FO");
            writer.Write(x.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(y.ToString(CultureInfo.InvariantCulture));
            writer.Write("^GFA,");
            writer.Write(totalBytes.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(totalBytes.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');
            writer.Write(bitmap.BytesPerRow.ToString(CultureInfo.InvariantCulture));
            writer.Write(',');

            for (int row = 0; row < bitmap.Height; row++)
            {
                WriteUncompressedRow(writer, bitmap, row);
            }

            writer.Write("^FS");
        }

        private static void WriteUncompressedRow(TextWriter writer, MonochromeBitmap bitmap, int row)
        {
            int start = row * bitmap.BytesPerRow;
            for (int i = 0; i < bitmap.BytesPerRow; i++)
            {
                byte value = bitmap.Bits[start + i];
                writer.Write(HexDigits[value >> 4]);
                writer.Write(HexDigits[value & 0x0F]);
            }
        }
    }
}
