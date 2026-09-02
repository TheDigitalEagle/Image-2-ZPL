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

You can fix a single call site by qualifying it, either as
`System.Convert.ToBase64String(...)` or `Image2ZPL.Convert.BitmapToZPLII(...)`,
whichever you use less often. Dropping `using Image2ZPL;` does not work as a
fix: `ToZpl` is an extension method, and extension methods are only found
through the `using` for their namespace, so removing it breaks
`bitmap.ToZpl(...)` outright, not just the ambiguous call.

The fix that scales to a whole file is a using alias directive, which wins
over a using-namespace directive:

```csharp
using System;
using Image2ZPL;
using Convert = System.Convert;   // resolves the ambiguity

// Convert.ToBase64String(...) now means System.Convert
// bitmap.ToZpl(...) still works
// Image2ZPL.Convert.BitmapToZPLII(...) still works when fully qualified
```

`Convert` alone now always means `System.Convert` in that file, `ToZpl` is
still found because `using Image2ZPL;` is still in scope, and the old API is
still reachable through its full name.

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

## FileLoadException for System.Memory on .NET Framework

If your application targets .NET Framework (for example `net472`) and you
see this the first time you call into `Image2ZPL` or
`Image2ZPL.SystemDrawing`:

```
System.IO.FileLoadException: Could not load file or assembly 'System.Memory,
Version=4.0.5.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of
its dependencies. The located assembly's manifest definition does not match
the assembly reference.
```

The cause: on `netstandard2.0` (the build your .NET Framework project
consumes) the core library references the `System.Memory` package for
`ReadOnlySpan<byte>` support, which is not part of .NET Framework itself.
.NET Core and .NET 5+ unify differing versions of the same assembly
automatically at load time; .NET Framework does not. If any other component
in your process already loaded a different version of `System.Memory` (or
`System.Buffers`, `System.Numerics.Vectors`, or
`System.Runtime.CompilerServices.Unsafe`, which `System.Memory` itself
depends on), .NET Framework refuses to load the second, differently
versioned copy and throws `FileLoadException` instead of silently picking
one.

The fix is a binding redirect, telling .NET Framework "any version of this
assembly is fine, use the one you find." Most modern .NET Framework project
templates (anything using `PackageReference` rather than `packages.config`)
already set this by default. If yours does not, add it to your project
file:

```xml
<PropertyGroup>
  <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
</PropertyGroup>
```

If your project is an older `packages.config`-style project that does not
support `AutoGenerateBindingRedirects`, or the automatic generation does not
cover the specific version your process ends up loading, add the redirects
to `app.config` (or `web.config`) by hand instead:

```xml
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Memory" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-99.9.9.9" newVersion="4.0.2.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Buffers" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-99.9.9.9" newVersion="4.0.4.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Numerics.Vectors" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-99.9.9.9" newVersion="4.1.5.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-99.9.9.9" newVersion="6.0.1.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

The exact `newVersion` values above match the `System.Memory 4.6.0` package
that `Image2ZPL` currently references; if that reference is ever bumped, the
versions above will need to move with it.

Note that this is not something `Image2ZPL.SystemDrawing` (or
`Image2ZPL` itself) can fix on your behalf by shipping its own binding
redirects. A library's binding redirect config file, even if it had one,
is not consulted for your process; only the top-level executable's own
`app.config`/`web.config` (or its `AutoGenerateBindingRedirects` output) is.
This has to live in your application.

## A note on the output format

The output was described in 1.x documentation as "ZB64". That was
incorrect and has been corrected in the README: the library has always
emitted `^GFA` graphic fields in ASCII hex with ZPL run-length compression,
not ZB64 (base64 plus LZ77, written `:Z64:`). Nothing about the bytes this
library produces has changed; only the description of them has.
