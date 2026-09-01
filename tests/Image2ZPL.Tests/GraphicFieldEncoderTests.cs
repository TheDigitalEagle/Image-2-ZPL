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

    /// <summary>
    /// Returns just the data portion. The header holds five commas before
    /// the data begins: ^FO{x},{y}^GFA,{byteCount},{fieldCount},{bytesPerRow},
    /// </summary>
    private static string DataOf(string zpl)
    {
        int start = 0;
        for (int i = 0; i < 5; i++)
        {
            start = zpl.IndexOf(',', start) + 1;
        }
        return zpl.Substring(start, zpl.Length - start - "^FS".Length);
    }

    [Fact]
    public void Compressed_AllWhiteRowBecomesComma()
    {
        var bitmap = new MonochromeBitmap(16, 1);
        Assert.Equal(",", DataOf(Encode(bitmap, compress: true)));
    }

    [Fact]
    public void Compressed_AllBlackRowBecomesBang()
    {
        var bitmap = new MonochromeBitmap(16, 1);
        for (int x = 0; x < 16; x++) bitmap.SetBlack(x, 0);
        Assert.Equal("!", DataOf(Encode(bitmap, compress: true)));
    }

    [Fact]
    public void Compressed_RepeatedRowBecomesColon()
    {
        var bitmap = new MonochromeBitmap(16, 2);
        bitmap.SetBlack(0, 0);
        bitmap.SetBlack(0, 1);
        Assert.Equal("8,:", DataOf(Encode(bitmap, compress: true)));
    }

    [Theory]
    [InlineData(1, "G")]
    [InlineData(19, "Y")]
    [InlineData(20, "g")]
    [InlineData(400, "z")]
    [InlineData(419, "zY")]
    [InlineData(21, "gG")]
    public void RunLengthCode_MatchesZplTable(int count, string expected)
    {
        Assert.Equal(expected, GraphicFieldEncoder.RunLengthCode(count));
    }

    [Fact]
    public void Compressed_RunLongerThan419EmitsConsecutiveCodes()
    {
        // 240 bytes of 0xFF is 480 nibbles of F, which exceeds one count code.
        // A fully black row collapses to "!", so make the last byte differ.
        var bitmap = new MonochromeBitmap(240 * 8, 1);
        for (int x = 0; x < (240 * 8) - 4; x++) bitmap.SetBlack(x, 0);
        var data = DataOf(Encode(bitmap, compress: true));
        Assert.Contains("zY", data);
        Assert.DoesNotContain("^", data);
    }

    [Fact]
    public void Compressed_TrailingWhiteBecomesComma()
    {
        var bitmap = new MonochromeBitmap(32, 1);
        bitmap.SetBlack(0, 0);
        Assert.Equal("8,", DataOf(Encode(bitmap, compress: true)));
    }

    [Fact]
    public void Compressed_TrailingBlackBecomesBang()
    {
        var bitmap = new MonochromeBitmap(32, 1);
        for (int x = 4; x < 32; x++) bitmap.SetBlack(x, 0);
        Assert.Equal("0!", DataOf(Encode(bitmap, compress: true)));
    }
}
