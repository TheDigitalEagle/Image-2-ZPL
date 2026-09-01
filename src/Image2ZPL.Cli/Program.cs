using System;
using System.IO;
using Image2ZPL;
using SkiaSharp;

internal static class Program
{
    private const string Usage = @"image2zpl, converts an image into a ZPL II graphic field.

Usage:
  image2zpl <input> [options]

Options:
  -o, --output <path>    Write to a file instead of standard output.
  -x <dots>              Horizontal position on the label. Default 0.
  -y <dots>              Vertical position on the label. Default 0.
  -d, --dither <mode>    threshold, floyd, atkinson, ordered. Default threshold.
  -t, --threshold <n>    Decision point from 0 to 255. Default 128.
      --invert           Swap black and white.
      --no-compress      Emit plain ASCII hex with no run-length codes.
      --wrap             Wrap the field in ^XA and ^XZ so it is a whole label.
  -h, --help             Show this help.";

    private static int Main(string[] args)
    {
        if (args.Length == 0 || Array.IndexOf(args, "-h") >= 0 || Array.IndexOf(args, "--help") >= 0)
        {
            Console.WriteLine(Usage);
            return args.Length == 0 ? 1 : 0;
        }

        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("error: " + ex.Message);
            return 1;
        }
    }

    private static int Run(string[] args)
    {
        string? input = null;
        string? output = null;
        bool wrap = false;
        var options = new ZplImageOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--output":
                    output = NextValue(args, ref i);
                    break;
                case "-x":
                    options.X = int.Parse(NextValue(args, ref i));
                    break;
                case "-y":
                    options.Y = int.Parse(NextValue(args, ref i));
                    break;
                case "-d":
                case "--dither":
                    options.Dither = ParseDither(NextValue(args, ref i));
                    break;
                case "-t":
                case "--threshold":
                    options.Threshold = byte.Parse(NextValue(args, ref i));
                    break;
                case "--invert":
                    options.Invert = true;
                    break;
                case "--no-compress":
                    options.Compress = false;
                    break;
                case "--wrap":
                    wrap = true;
                    break;
                default:
                    if (args[i].StartsWith("-", StringComparison.Ordinal))
                    {
                        throw new ArgumentException($"Unknown option '{args[i]}'. Run with --help for usage.");
                    }
                    input = args[i];
                    break;
            }
        }

        if (input == null)
        {
            throw new ArgumentException("No input file given. Run with --help for usage.");
        }

        if (!File.Exists(input))
        {
            throw new FileNotFoundException($"Input file not found: {input}");
        }

        using SKBitmap? bitmap = SKBitmap.Decode(input);
        if (bitmap == null)
        {
            throw new InvalidOperationException($"Could not decode '{input}' as an image.");
        }

        string zpl = bitmap.ToZpl(options);
        if (wrap)
        {
            zpl = "^XA" + zpl + "^XZ";
        }

        if (output == null)
        {
            Console.Out.Write(zpl);
            Console.Out.Write(Environment.NewLine);
        }
        else
        {
            File.WriteAllText(output, zpl);
        }

        return 0;
    }

    private static string NextValue(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"Option '{args[i]}' needs a value.");
        }
        return args[++i];
    }

    private static DitherMode ParseDither(string value)
    {
        switch (value.ToLowerInvariant())
        {
            case "threshold": return DitherMode.Threshold;
            case "floyd": return DitherMode.FloydSteinberg;
            case "atkinson": return DitherMode.Atkinson;
            case "ordered": return DitherMode.Ordered4x4;
            default:
                throw new ArgumentException($"Unknown dither mode '{value}'. Use threshold, floyd, atkinson or ordered.");
        }
    }
}
