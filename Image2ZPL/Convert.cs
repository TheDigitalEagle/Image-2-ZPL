using System.Drawing;

namespace Image2ZPL
{
    public static class Convert
    {
        /// <summary>
        /// Converts BMP, PNG, JPG into a ZB64 string that can be used for a ZPLII printer.
        /// </summary>
        /// <param name="imageToConvert">BMP, PNG, JPG to be converted.</param>
        /// <param name="posX">The horizontal posistion to be printed.</param>
        /// <param name="posY">The veritical posisition to be printed.</param>
        /// <returns></returns>
        public static string BitmapToZPLII(Bitmap imageToConvert, int posX, int posY)
        {
            string InsertImageZPL = Conversion.ZPLII.GetZPLIIImage(imageToConvert, posX, posY);
            return InsertImageZPL;
        }
    }
}
