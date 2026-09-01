using Image2ZPL.Tests.Infrastructure;
using Xunit;

namespace Image2ZPL.Tests;

public class GfaDecoderTests
{
    [Fact]
    public void Decode_ReadsHeaderGeometry()
    {
        var field = GfaDecoder.Decode("^FO0,0^GFA,6,6,3,C00000C00000^FS");
        Assert.Equal(3, field.BytesPerRow);
        Assert.Equal(2, field.Height);
    }

    [Fact]
    public void Decode_ReadsPlainHex()
    {
        var field = GfaDecoder.Decode("^FO0,0^GFA,1,1,1,C0^FS");
        Assert.Equal(0xC0, field.Rows[0][0]);
    }

    [Fact]
    public void Decode_CommaFillsRestOfRowWithZeros()
    {
        var field = GfaDecoder.Decode("^FO0,0^GFA,2,2,2,8,^FS");
        Assert.Equal(0x80, field.Rows[0][0]);
        Assert.Equal(0x00, field.Rows[0][1]);
    }

    [Fact]
    public void Decode_BangFillsRestOfRowWithOnes()
    {
        var field = GfaDecoder.Decode("^FO0,0^GFA,2,2,2,0!^FS");
        Assert.Equal(0x0F, field.Rows[0][0]);
        Assert.Equal(0xFF, field.Rows[0][1]);
    }

    [Fact]
    public void Decode_ColonRepeatsPreviousRow()
    {
        var field = GfaDecoder.Decode("^FO0,0^GFA,2,2,1,8,:^FS");
        Assert.Equal(field.Rows[0][0], field.Rows[1][0]);
    }

    [Theory]
    [InlineData("G8,", 1)]
    [InlineData("Y8,", 19)]
    [InlineData("g8,", 20)]
    [InlineData("gG8,", 21)]
    public void Decode_ExpandsRepeatCodes(string data, int expectedNibbles)
    {
        var field = GfaDecoder.Decode($"^FO0,0^GFA,220,220,220,{data}^FS");
        int count = 0;
        for (int i = 0; i < field.BytesPerRow * 2; i++)
        {
            int nibble = (i & 1) == 0 ? field.Rows[0][i >> 1] >> 4 : field.Rows[0][i >> 1] & 0x0F;
            if (nibble == 0x8) count++;
        }
        Assert.Equal(expectedNibbles, count);
    }
}
