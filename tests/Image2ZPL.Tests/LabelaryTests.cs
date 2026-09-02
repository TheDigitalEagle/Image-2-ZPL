using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Image2ZPL.Tests;

public class LabelaryTests
{
    // Set IMAGE2ZPL_NETWORK_TESTS=1 to run. Kept out of the default suite
    // so CI does not depend on a third-party service being reachable.
    private const string EnvGate = "IMAGE2ZPL_NETWORK_TESTS";

    // What this test proves: Labelary parses our compression syntax and
    // accepts the header convention we send, and hands back a well-formed
    // PNG. What it does not prove: what was actually rendered. It cannot
    // catch inverted pixels, shifted rows, or wrong dimensions, because the
    // test project has no PNG decoder and the round-trip tests already
    // cover pixel correctness against our own decoder. This is a minimum
    // external anchor, not a substitute for that.
    [SkippableFact]
    public async Task Labelary_RendersOurOutputWithoutError()
    {
        Skip.If(System.Environment.GetEnvironmentVariable(EnvGate) != "1");

        var bitmap = Infrastructure.BitmapFactory.Random(37, 20, seed: 1, blackPercent: 40);
        using var writer = new System.IO.StringWriter();
        Image2ZPL.Internal.GraphicFieldEncoder.Write(writer, bitmap, 0, 0, compress: true);
        string zpl = "^XA" + writer.ToString() + "^XZ";

        // Labelary rejects the default text/plain content type StringContent
        // would otherwise send (HTTP 415), so match what a plain form post
        // sends instead.
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
        using var content = new StringContent(zpl, Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync(
            "https://api.labelary.com/v1/printers/8dpmm/labels/4x6/0/", content);

        byte[] body = await response.Content.ReadAsByteArrayAsync();
        Assert.True(response.IsSuccessStatusCode, Encoding.UTF8.GetString(body));
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.True(body.Length > 100, "Response body is too short to be a real rendered PNG.");
        Assert.Equal(0x89, body[0]);
        Assert.Equal(0x50, body[1]);
        Assert.Equal(0x4E, body[2]);
        Assert.Equal(0x47, body[3]);
    }
}
