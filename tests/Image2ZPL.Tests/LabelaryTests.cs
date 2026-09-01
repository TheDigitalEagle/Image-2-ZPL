using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Image2ZPL.Tests;

public class LabelaryTests
{
    // Set IMAGE2ZPL_NETWORK_TESTS=1 to run. Kept out of the default suite
    // so CI does not depend on a third-party service being reachable.
    private const string EnvGate = "IMAGE2ZPL_NETWORK_TESTS";

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
        using var client = new HttpClient();
        using var content = new StringContent(zpl, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await client.PostAsync(
            "http://api.labelary.com/v1/printers/8dpmm/labels/4x6/0/", content);

        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
    }
}
