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

            byte[]? previousRow = null;
            byte[] currentRow = new byte[bitmap.BytesPerRow];
            for (int row = 0; row < bitmap.Height; row++)
            {
                if (compress)
                {
                    System.Array.Copy(bitmap.Bits, row * bitmap.BytesPerRow, currentRow, 0, bitmap.BytesPerRow);
                    WriteCompressedRow(writer, currentRow, previousRow);
                    previousRow = (byte[])currentRow.Clone();
                }
                else
                {
                    WriteUncompressedRow(writer, bitmap, row);
                }
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

        private const string LowCodes = " GHIJKLMNOPQRSTUVWXY";
        private const string HighCodes = " ghijklmnopqrstuvwxyz";

        /// <summary>
        /// Returns the ZPL repeat code for a count between 1 and 419.
        /// </summary>
        internal static string RunLengthCode(int count)
        {
            int high = count / 20;
            int low = count % 20;
            string result = string.Empty;
            if (high > 0)
            {
                result += HighCodes[high];
            }
            if (low > 0)
            {
                result += LowCodes[low];
            }
            return result;
        }

        private static void WriteCompressedRow(TextWriter writer, byte[] row, byte[]? previousRow)
        {
            if (AllBytesAre(row, 0x00))
            {
                writer.Write(',');
                return;
            }

            if (AllBytesAre(row, 0xFF))
            {
                writer.Write('!');
                return;
            }

            if (previousRow != null && SameBytes(row, previousRow))
            {
                writer.Write(':');
                return;
            }

            int nibbleCount = row.Length * 2;
            int i = 0;
            while (i < nibbleCount)
            {
                int nibble = NibbleAt(row, i);
                int run = 1;
                while (i + run < nibbleCount && NibbleAt(row, i + run) == nibble)
                {
                    run++;
                }

                // A run that reaches the end of the row can collapse to a
                // fill code, which is never longer than spelling it out.
                if (i + run == nibbleCount)
                {
                    if (nibble == 0x0)
                    {
                        writer.Write(',');
                        return;
                    }

                    if (nibble == 0xF)
                    {
                        writer.Write('!');
                        return;
                    }
                }

                WriteRun(writer, run, HexDigits[nibble]);
                i += run;
            }
        }

        private static void WriteRun(TextWriter writer, int count, char hexDigit)
        {
            // Two literal characters are no longer than a code plus a digit,
            // so only compress runs of three or more.
            if (count <= 2)
            {
                for (int i = 0; i < count; i++)
                {
                    writer.Write(hexDigit);
                }
                return;
            }

            while (count > 0)
            {
                int chunk = count > 419 ? 419 : count;
                if (chunk > 1)
                {
                    writer.Write(RunLengthCode(chunk));
                }
                writer.Write(hexDigit);
                count -= chunk;
            }
        }

        private static int NibbleAt(byte[] row, int index)
        {
            byte value = row[index >> 1];
            return (index & 1) == 0 ? value >> 4 : value & 0x0F;
        }

        private static bool AllBytesAre(byte[] row, byte value)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] != value)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool SameBytes(byte[] a, byte[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
