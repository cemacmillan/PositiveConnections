# Multi-version builds (RimWorld 1.5 and 1.6)

Positive Connections ships **two assemblies** in one mod package:

| Path | Built with |
|------|------------|
| `1.5/Assemblies/PositiveConnections.dll` | `Source/PositiveConnections.1.5.csproj` → `Krafs.Rimworld.Ref` **1.5.\*** |
| `1.6/Assemblies/PositiveConnections.dll` | `Source/PositiveConnections.csproj` → `Krafs.Rimworld.Ref` **1.6.\*** |

RimWorld loads the folder matching the running game version. **Do not** copy the 1.6 DLL into `1.5/Assemblies` (or vice versa).

## Source layout

- **`PositiveConnections.sources.props`** — shared list of `.cs` files (single place to add/remove sources).
- **`PositiveConnections.1.5.csproj`** — 1.5 reference assembly, `OutDir` → `1.5/Assemblies`, optional `V15` define for API forks.
- **`PositiveConnections.csproj`** — 1.6 reference assembly, `OutDir` → `1.6/Assemblies`.

Mind Matters integration uses **reflection only** (no compile-time `MindMattersInterface` reference). Both game versions get the same behavior; see `docs/r161.md` §6–§7 for debug-menu / TypeLoad history.

## Commands (from `Source/`)

```bash
make build      # build15 + build16 (Release)
make build15    # 1.5 only
make build16    # 1.6 only
make check-dll  # both DLLs exist
make check-stale # fail if Source/*.cs newer than either DLL
make deploy     # build + check-stale + tar to local RimWorld Mods
```

For local testing **with** Mind Matters loaded, `make deploymm` copies `MindMattersInterface.dll` into both assembly folders. **`make deploy`** (Steam/release) removes those interface DLLs so the package does not ship them.

## Release checklist

1. Change C# under `Source/`.
2. Run **`make build`** from `Source/`.
3. Run **`make check-stale`** (or rely on **`make deploy`**, which runs it).
4. Smoke-test on **1.5** and **1.6** if the change touches runtime behavior.
5. Publish the **whole mod tree** (both `1.5/` and `1.6/` content + both DLLs).
6. Changelog: mention **1.5 assembly rebuilt** whenever C# changed, even if defs only changed under `1.6/`.

## When RimWorld adds 1.7

1. Add `PositiveConnections.1.7.csproj` (or retarget: keep previous csproj, add new current).
2. Add `1.7/` defs/languages as needed.
3. Extend `Makefile` `build` target.
4. Keep supporting **previous** game version until you explicitly drop it from `About/About.xml`.

Pattern reference: **MakingDo** (`MakingDo/Source/Makefile`, `MakingDo.1.5.csproj`).
