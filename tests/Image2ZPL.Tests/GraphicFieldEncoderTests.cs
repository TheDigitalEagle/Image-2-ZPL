using System.IO;
using Image2ZPL.Internal;
using Xunit;

namespace Image2ZPL.Tests;

/// <summary>
/// Tests for GraphicFieldEncoder.
/// </summary>
public class GraphicFieldEncoderTests
{
    private static string Encode(MonochromeBitmap bitmap, bool compress, int x = 0, int y = 0)
    {
        using var writer = new StringWriter();
        GraphicFieldEncoder.Write(writer, bitmap, x, y, compress);
        return writer.ToString();
    }

    /// <summary>
    /// Verifies that uncompressed output emits the correct header with byte counts and row width.
    /// </summary>
    [Fact]
    public void Uncompressed_EmitsHeaderWithByteCountsAndRowWidth()
    {
        // 17 px wide needs 3 bytes per row; 2 rows gives 6 bytes total.
        var bitmap = new MonochromeBitmap(17, 2);
        var zpl = Encode(bitmap, compress: false, x: 20, y: 30);
        Assert.StartsWith("^FO20,30^GFA,6,6,3,", zpl);
        Assert.EndsWith("^FS", zpl);
    }

    /// <summary>
    /// Verifies that uncompressed output emits two uppercase hex characters per byte.
    /// </summary>
    [Fact]
    public void Uncompressed_EmitsTwoUppercaseHexCharactersPerByte()
    {
        var bitmap = new MonochromeBitmap(8, 1);
        bitmap.SetBlack(0, 0);
        bitmap.SetBlack(1, 0);
        var zpl = Encode(bitmap, compress: false);
        Assert.Equal("^FO0,0^GFA,1,1,1,C0^FS", zpl);
    }

    /// <summary>
    /// Verifies that uncompressed output emits every row in order.
    /// </summary>
    [Fact]
    public void Uncompressed_EmitsEveryRowInOrder()
    {
        var bitmap = new MonochromeBitmap(8, 2);
        bitmap.SetBlack(0, 1);
        var zpl = Encode(bitmap, compress: false);
        Assert.Equal("^FO0,0^GFA,2,2,1,0080^FS", zpl);
    }
}
