# gregMod.Potato

Potato mode for Data Center: disables perf-heavy effects and forces the lowest texture quality. Standalone-safe, GregCore-integrated (F1 hub + key HUD + toasts).

## What it does (all pref-gated, `MelonPreferences / gregMod.Potato`)

- **Textures:** `globalTextureMipmapLimit = 3` (lowest), texture streaming off, anisotropic filtering off.
- **Quality:** AA off, shadows off (distance 20 m, low resolution), 0 pixel lights, LOD bias 0.4, soft particles off, realtime reflection probes off, two-bone skinning.
- **Framing:** VSync off, target 60 FPS (uncapped with `-1`).
- **HDRP:** SSAO, contact shadows, global illumination, screen-space reflections, volumetric fog, and decals off; max 16 shadow requests (mirrors gregCore's verified `GregFrameRateLimiter` paths).
- **Scene:** post-process `Volume`s off; optional full particle kill (`DisableParticles`, default off — gameplay cues).

## Controls

- `F3` = potato on/off at runtime (persisted to the `Enabled` pref).
- Originals are snapshotted on first apply and restored on toggle-off / scene-safe re-apply / unload. Camera far plane is deliberately untouched (builder game needs distance view).

## GregCore integration

- `GregMenuRegistry` (no locks — potato needs no input capture), `GregMenuBinding.BindToggle`, `GregHudRegistry`, `GregModRegistry`, toasts via `greg.UI.ShowNotification` — all behind `GregHost.HasCore` (JIT-split in `src/Core/PotatoBridge.cs`).
- No new GregCore API needed: everything used is public engine API. Proposed upstream follow-up in `docs/GREGCORE_GAPS.md` (public static profile applier so mods share one implementation instead of duplicating `GregFrameRateLimiter` logic).

## Build

```bash
dotnet build gregMod.Potato.csproj -c Release
cp bin/Release/net6.0/gregMod.Potato.dll "$DATACENTER_HOME/Mods/"
```

Check `MelonLoader/Latest.log` for `[Potato] Loaded`.
