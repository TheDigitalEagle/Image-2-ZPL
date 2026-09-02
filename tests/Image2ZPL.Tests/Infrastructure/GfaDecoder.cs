using System;
using System.Globalization;

namespace Image2ZPL.Tests.Infrastructure;

public sealed class DecodedField
{
    public DecodedField(int bytesPerRow, byte[][] rows)
    {
        BytesPerRow = bytesPerRow;
        Rows = rows;
    }

    public int BytesPerRow { get; }

    public byte[][] Rows { get; }

    public int Height => Rows.Length;
}

/// <summary>
/// Decodes a ZPL ^GFA graphic field back into packed rows. Test
/// infrastructure only, so that encoder output can be verified by
/// round trip rather than by hand-written expected strings.
/// </summary>
public static class GfaDecoder
{
    public static DecodedField Decode(string zpl)
    {
        int gfa = zpl.IndexOf("^GFA,", StringComparison.Ordinal);
        if (gfa < 0)
        {
            throw new FormatException("No ^GFA command found.");
        }

        int cursor = gfa + "^GFA,".Length;
        int totalBytes = ReadInt(zpl, ref cursor);
        int fieldCount = ReadInt(zpl, ref cursor);
        int bytesPerRow = ReadInt(zpl, ref cursor);

        if (fieldCount != totalBytes)
        {
            throw new FormatException(
                $"Graphic field count {fieldCount} does not match byte count {totalBytes}.");
        }

        if (totalBytes % bytesPerRow != 0)
        {
            throw new FormatException(
                $"Byte count {totalBytes} is not an exact multiple of bytes per row {bytesPerRow}.");
        }

        int end = zpl.IndexOf("^FS", cursor, StringComparison.Ordinal);
        string data = zpl.Substring(cursor, end - cursor);

        int height = totalBytes / bytesPerRow;
        var rows = new byte[height][];
        for (int i = 0; i < height; i++)
        {
            rows[i] = new byte[bytesPerRow];
        }

        int nibblesPerRow = bytesPerRow * 2;
        int row = 0;
        int nibble = 0;
        int count = 0;

        foreach (char c in data)
        {
            if (c >= 'g' && c <= 'z')
            {
                count += (c - 'f') * 20;
            }
            else if (c >= 'G' && c <= 'Y')
            {
                count += c - 'F';
            }
            else if (c == ',' || c == '!')
            {
                int fill = c == '!' ? 0xF : 0x0;
                while (nibble < nibblesPerRow)
                {
                    SetNibble(rows[row], nibble++, fill);
                }
                row++;
                nibble = 0;
                count = 0;
            }
            else if (c == ':')
            {
                Array.Copy(rows[row - 1], rows[row], bytesPerRow);
                row++;
                nibble = 0;
                count = 0;
            }
            else if (c >= 'a' && c <= 'f')
            {
                // ZPL hex data is uppercase, and our encoder only ever emits
                // uppercase. A decoder more permissive than a printer would
                // mask the same class of bug the completeness check below
                // exists to catch, so reject it rather than silently
                // accepting it as int.Parse's HexNumber style otherwise would.
                throw new FormatException($"Lowercase hex digit '{c}' is not valid ZPL compression data.");
            }
            else
            {
                int value = int.Parse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int repeat = count == 0 ? 1 : count;
                for (int i = 0; i < repeat; i++)
                {
                    SetNibble(rows[row], nibble++, value);
                    if (nibble == nibblesPerRow)
                    {
                        row++;
                        nibble = 0;
                    }
                }
                count = 0;
            }
        }

        if (row != height || nibble != 0)
        {
            throw new FormatException(
                $"Data described {row} complete rows plus {nibble} nibbles, expected {height} rows.");
        }

        return new DecodedField(bytesPerRow, rows);
    }

    private static void SetNibble(byte[] row, int index, int value)
    {
        if ((index & 1) == 0)
        {
            row[index >> 1] = (byte)((row[index >> 1] & 0x0F) | (value << 4));
        }
        else
        {
            row[index >> 1] = (byte)((row[index >> 1] & 0xF0) | value);
        }
    }

    private static int ReadInt(string s, ref int cursor)
    {
        int start = cursor;
        while (s[cursor] != ',')
        {
            cursor++;
        }
        int value = int.Parse(s.Substring(start, cursor - start), CultureInfo.InvariantCulture);
        cursor++;
        return value;
    }
}
