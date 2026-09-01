# Upgrading from 1.x to 2.0.0

## The short version

```bash
dotnet add package Image2ZPL.SystemDrawing
```

Your existing code keeps compiling. `Image2ZPL.Convert.BitmapToZPLII` still
exists, now marked obsolete, and forwards to the new API.

## The recommended version

Move to a cross-platform adapter, because `System.Drawing` throws
`PlatformNotSupportedException` off Windows on .NET 7 and later.

Before:

```csharp
string zpl = Image2ZPL.Convert.BitmapToZPLII(bitmap, 20, 20);
```

After:

```csharp
using var bitmap = SKBitmap.Decode("logo.png");
string zpl = bitmap.ToZpl(new ZplImageOptions { X = 20, Y = 20 });
```

## The `Convert` name collision

`Image2ZPL.Convert` is a static class, matching the same name as
`System.Convert`. If your code has both `using System;` and
`using Image2ZPL;` in scope, an unqualified call to `Convert.ToBase64String(...)`
(or any other member of `System.Convert`) now fails to compile with
CS0104, "ambiguous reference between `System.Convert` and
`Image2ZPL.Convert`". This is not new in 2.0.0: it was already true of
version 1.x, which also defined `Image2ZPL.Convert`. It is called out
here because it is easy to be surprised by on a fresh upgrade.

The fix is to qualify the call, either as `System.Convert.ToBase64String(...)`
or `Image2ZPL.Convert.BitmapToZPLII(...)`, whichever you use less often. If
you have moved to the new API, the cleanest option is to drop
`using Image2ZPL;` in favour of qualifying `Image2ZPL.ZplImageOptions` and
the `ToZpl` extension method's namespace, which removes the collision
entirely.

## Output differences

Version 2.0.0 does its own halftoning. Version 1.x handed that job to GDI+
via `Clone(..., Format1bppIndexed)`, whose thresholding is undocumented and
varies by platform. Exact parity with 1.x is not achievable, and that is
by design:

- Images that are already pure black and white convert identically.
- Greyscale and colour images may differ by a pixel here and there. The
  default `DitherMode.Threshold` with `Threshold = 128` is the closest
  match to the old behaviour.
- If your images are photographs, `DitherMode.FloydSteinberg` will look
  considerably better than anything 1.x could produce.

This is why the release is 2.0.0 rather than 1.1.

## Renamed and moved

| 1.x | 2.0.0 |
|---|---|
| `Image2ZPL.Convert.BitmapToZPLII(bitmap, x, y)` | `bitmap.ToZpl(new ZplImageOptions { X = x, Y = y })` |
| `Image2ZPL` package, Windows only | `Image2ZPL` core plus an adapter package |
| No options | `ZplImageOptions` |

## A note on the output format

The output was described in 1.x documentation as "ZB64". That was
incorrect and has been corrected in the README: the library has always
emitted `^GFA` graphic fields in ASCII hex with ZPL run-length compression,
not ZB64 (base64 plus LZ77, written `:Z64:`). Nothing about the bytes this
library produces has changed; only the description of them has.
