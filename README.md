# Image-2-ZPL

Converts images into ZPL II graphic fields for Zebra printers, without
uploading an image to the printer first. Useful for customized barcodes,
logos, and anything else you want to print alongside your label data.

[![CI](https://github.com/TheDigitalEagle/Image-2-ZPL/actions/workflows/ci.yml/badge.svg)](https://github.com/TheDigitalEagle/Image-2-ZPL/actions/workflows/ci.yml)

## Packages

| Package | Use it when |
|---|---|
| `Image2ZPL` | You already have raw pixel data. No third-party dependencies. |
| `Image2ZPL.SkiaSharp` | You want to load PNG, JPG, or BMP files. Cross-platform, MIT. On Linux, also add `SkiaSharp.NativeAssets.Linux`. |
| `Image2ZPL.ImageSharp` | You already use ImageSharp. See the licence note below. |
| `Image2ZPL.SystemDrawing` | You are on Windows and upgrading from version 1.x. |

No third-party dependencies. On `netstandard2.0` it references `System.Memory`,
Microsoft's MIT licensed `Span<T>` polyfill, which most .NET Framework
projects already have. On `net8.0` and `net10.0` it has none at all.

## Quick start

```bash
dotnet add package Image2ZPL.SkiaSharp
```

On Linux, SkiaSharp needs its native library supplied separately:

```bash
dotnet add package SkiaSharp.NativeAssets.Linux
```

Without it you get a native load failure at runtime rather than a build
error. This is a SkiaSharp packaging detail, not something Image2ZPL can
bundle for you.

```csharp
using Image2ZPL;
using SkiaSharp;

using var bitmap = SKBitmap.Decode("logo.png");
string zpl = bitmap.ToZpl(new ZplImageOptions { X = 20, Y = 20 });

Console.WriteLine("^XA" + zpl + "^XZ");
```

## Command line

```bash
dotnet tool install -g Image2ZPL.Cli
image2zpl logo.png -x 20 -y 20 --dither floyd --wrap
```

The tool bundles its own SkiaSharp Linux native assets, so no extra package
is needed to run it there. That requirement applies only when you reference
`Image2ZPL.SkiaSharp` from your own project, as in the quick start above.

```
image2zpl, converts an image into a ZPL II graphic field.

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
  -h, --help             Show this help.
```

A file name that begins with a dash must be written with a `./` prefix, for
example `./-logo.png`, so it is not mistaken for an option.

## Options

| Option | Default | Meaning |
|---|---|---|
| `X`, `Y` | 0 | Field position on the label, in dots. |
| `Dither` | `Threshold` | `Threshold`, `FloydSteinberg`, `Atkinson`, or `Ordered4x4`. |
| `Threshold` | 128 | Pixels darker than this print as a dot. |
| `Invert` | false | Swap black and white. |
| `Compress` | true | Emit ZPL run-length codes. Turn off to read the hex yourself. |

Use `Threshold` for line art, logos, and text. Use `FloydSteinberg` for
photographs. Version 1.x had no equivalent setting, because it let GDI+
decide.

## Using the core directly

Most callers want an adapter, which decodes a file and hands the pixels to
the core for you. If you already have raw pixel data, for example from your
own decoder or a hardware capture, the `Image2ZPL` package converts it
directly with no adapter and no third-party dependency:

```csharp
using System;
using Image2ZPL;

// A 2x2 grayscale image: top-left and bottom-right pixels black, the rest
// white. One byte per pixel, 0 is black and 255 is white.
byte[] pixels = { 0, 255, 255, 0 };
string zpl = ZplImageConverter.ToZpl(pixels, width: 2, height: 2, stride: 2, SourcePixelFormat.Grayscale8);

Console.WriteLine("^XA" + zpl + "^XZ");
```

`stride` is the number of bytes per row, including any padding; for a tightly
packed buffer it equals `width` times the bytes per pixel of `format`. This
is the low-level path the adapters themselves are built on. See
[Options](#options) below for `ZplImageOptions`.

## Testing without a printer

Paste the output into http://labelary.com/viewer.html.

## Output format

This library emits `^GFA` graphic fields in ASCII hex with ZPL run-length
compression. Documentation before version 2.0.0 described the output as
"ZB64", which was incorrect: ZB64 is base64 plus LZ77 and is written
`:Z64:`. The output format itself has not changed, only the description
of it.

## Upgrading from 1.x

See [MIGRATION.md](MIGRATION.md).

## Framework support

The core, the SkiaSharp adapter, and the System.Drawing adapter all reach
back to .NET Framework (`netstandard2.0`, or `net472` for the
System.Drawing adapter specifically). The ImageSharp adapter targets
`net8.0` and `net10.0` only, because ImageSharp 3.x ships no
`netstandard2.0` build. If you are on .NET Framework, use the SkiaSharp or
System.Drawing adapter.

## Licence

MIT. Note that `Image2ZPL.ImageSharp` depends on SixLabors.ImageSharp,
which is distributed under the Six Labors Split License rather than MIT.
The core library and every other adapter are dependency-free or MIT.
