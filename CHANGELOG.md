# Changelog

## 2.0.0

### Changed

- Relicensed from GPL-3.0 to MIT.
- Split into a dependency-free core plus adapter packages for SkiaSharp,
  ImageSharp, and System.Drawing.
- Targets netstandard2.0, net8.0, and net10.0. The library now runs on
  Linux and macOS, which was impossible in 1.x because `System.Drawing`
  throws `PlatformNotSupportedException` off Windows on .NET 7 and later.
- Halftoning is done in managed code rather than by GDI+, so output for
  greyscale and colour images may differ slightly from 1.x. See
  MIGRATION.md.
- Corrected the documentation, which described the output as "ZB64". The
  library emits ASCII hex `^GFA` with ZPL run-length compression. The
  output format is unchanged.

### Added

- `DitherMode` with threshold, Floyd-Steinberg, Atkinson, and ordered
  4x4 dithering. 1.x offered no control over this at all.
- `ZplImageOptions` for position, threshold, inversion, and compression.
- `ZplImageConverter.WriteZpl`, which streams to a `TextWriter` instead of
  building a large string.
- The `image2zpl` command line tool.
- A test suite, including a `^GFA` decoder used to verify that encoded
  output round-trips back to identical pixels.
- Continuous integration on Linux, Windows, and macOS.

### Fixed

- Runs longer than 419 repeats now emit consecutive repeat codes instead
  of throwing an exception.
- Right-edge clipping in the 1bpp bitmap masking, so images whose width is
  not a multiple of eight no longer print a black stripe down the right
  edge. Thanks to @neilwarland (#3). That fix was merged in February 2026
  but never published, so 2.0.0 is the first release to carry it. The
  halftoner in 2.0.0 makes this class of bug structurally impossible: it
  never addresses a pixel at or beyond the image width, so the padding
  bits in the final byte of each row are never set.

### Removed

- The `TestZPL` WinForms demo, replaced by the command line tool and the
  test suite.

## 1.0.0

- Initial release.
