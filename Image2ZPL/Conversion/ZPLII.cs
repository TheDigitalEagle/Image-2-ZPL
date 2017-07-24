using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Image2ZPL.Conversion
{
    /// <summary>
    /// ZPLII image conversion.
    /// </summary>
    internal class ZPLII
    {
        /// <summary>
        /// Builds the string for the converted ZB64 to be used in tha ZPLII command string.
        /// </summary>
        /// <param name="srcbitmap">Provided Image</param>
        /// <param name="posx">Horizontal posistion</param>
        /// <param name="posy">Vertical posistion</param>
        /// <returns></returns>
        internal static string GetZPLIIImage(Bitmap srcbitmap, int posx, int posy)
        {

            Rectangle dim = new Rectangle(Point.Empty, srcbitmap.Size);
            int rowdata = ((dim.Width + 7) / 8);
            int bytes = rowdata * dim.Height;

            using (Bitmap bmpCompressed = srcbitmap.Clone(dim, PixelFormat.Format1bppIndexed))
            {
                StringBuilder result = new StringBuilder();

                result.AppendFormat("^FO{0},{1}^GFA,{2},{2},{3},", posx, posy, rowdata * dim.Height, rowdata);
                byte[][] imageData = ConvertImageBinary(dim, rowdata, bmpCompressed);

                byte[] previousRow = null;
                foreach (byte[] row in imageData)
                {
                    AppendLine(row, previousRow, result);
                    previousRow = row;
                }
                result.Append(@"^FS");

                return result.ToString();
            }
        }

        /// <summary>
        /// Converts the image into a byte array (pointer) and converts the image byte-by-byte while inverting the color of the image color for printing.
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="stride"></param>
        /// <param name="bmpimage"></param>
        /// <returns></returns>
        private static byte[][] ConvertImageBinary(Rectangle dim, int stride, Bitmap bmpimage)
        {
            byte[][] imagebytes;
            var data = bmpimage.LockBits(dim, ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);
            try
            {
                // This is required to perform operations with pointers. This is only working with a locked bitmap in memory so it is "safe".
                unsafe
                {
                    byte* pixelData = (byte*)data.Scan0.ToPointer();
                    byte mask = (byte)(0xff << (data.Stride * 8 - dim.Width));
                    imagebytes = new byte[dim.Height][];

                    for (int x = 0; x < dim.Height; x++)
                    {
                        byte* rowStart = pixelData + x * data.Stride;
                        imagebytes[x] = new byte[stride];

                        for (int y = 0; y < stride; y++)
                        {
                            byte invert = (byte)(0xff ^ rowStart[y]);
                            invert = (y == stride - 1) ? (byte)(invert & mask) : invert;
                            imagebytes[x][y] = invert;
                        }
                    }
                }
            }
            finally
            {
                bmpimage.UnlockBits(data);
            }
            return imagebytes;
        }

        /// <summary>
        /// Converts byte to ZB64 and appends to current string.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="previousRow"></param>
        /// <param name="zb64stream"></param>
        private static void AppendLine(byte[] row, byte[] previousRow, StringBuilder zb64stream)
        {
            if (row.All(r => r == 0))
            {
                zb64stream.Append(",");
                return;
            }

            if (row.All(r => r == 0xff))
            {
                zb64stream.Append("!");
                return;
            }

            if (previousRow != null && MatchByteArray(row, previousRow))
            {
                zb64stream.Append(":");
                return;
            }

            byte[] nibbles = new byte[row.Length * 2];
            for (int i = 0; i < row.Length; i++)
            {
                nibbles[i * 2] = (byte)(row[i] >> 4);
                nibbles[i * 2 + 1] = (byte)(row[i] & 0x0f);
            }

            for (int i = 0; i < nibbles.Length; i++)
            {
                byte pixel = nibbles[i];

                int repcount = 0;
                for (int j = i; j < nibbles.Length && repcount <= 400; j++)
                {
                    if (pixel == nibbles[j])
                    {
                        repcount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (repcount > 2)
                {
                    if (repcount == nibbles.Length - i
                        && (pixel == 0 || pixel == 0xf))
                    {
                        if (pixel == 0)
                        {
                            if (i % 2 == 1)
                            {
                                zb64stream.Append("0");
                            }
                            zb64stream.Append(",");
                            return;
                        }
                        else if (pixel == 0xf)
                        {
                            if (i % 2 == 1)
                            {
                                zb64stream.Append("F");
                            }
                            zb64stream.Append("!");
                            return;
                        }
                    }
                    else
                    {
                        zb64stream.Append(ConvertZB64(repcount));
                        i += repcount - 1;
                    }
                }
                zb64stream.Append(pixel.ToString("X"));
            }
        }

        /// <summary>
        /// Converts to ZB64 format specified in the ZPLII Programming document.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static string ConvertZB64(int count)
        {
            if (count > 419)
                throw new ArgumentOutOfRangeException();

            int high = count / 20;
            int low = count % 20;

            const string lowString = " GHIJKLMNOPQRSTUVWXY";
            const string highString = " ghijklmnopqrstuvwxyz";

            string repeatSequence = "";
            if (high > 0)
            {
                repeatSequence += highString[high];
            }
            if (low > 0)
            {
                repeatSequence += lowString[low];
            }

            return repeatSequence;
        }

        private static bool MatchByteArray(byte[] row, byte[] lastrow)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] != lastrow[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
