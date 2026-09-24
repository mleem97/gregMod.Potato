# Potato spec (v1)

## Goal

One mod that disables all perf-heavy effects and forces the lowest texture quality ("potato mode"), with per-lever prefs and full restore.

## Levers (default = maximum potato)

| Area | Setting | Default |
|---|---|---|
| Textures | `globalTextureMipmapLimit` | 3 (lowest) |
| Textures | `streamingMipmapsActive` | off |
| Textures | `anisotropicFiltering` | off |
| Quality | `antiAliasing` | 0 |
| Quality | `shadows` | Disable |
| Quality | `shadowDistance` / `shadowResolution` | 20 m / Low |
| Quality | `pixelLightCount` | 0 |
| Quality | `lodBias` | 0.4 |
| Quality | `softParticles` | off |
| Quality | `realtimeReflectionProbes` | off |
| Quality | `skinWeights` | TwoBones |
| Pacing | `vSyncCount` / `targetFrameRate` | 0 / 60 |
| HDRP volume | SSAO / contact shadows / GI / SSR / volumetric fog | all off |
| HDRP asset | `maxShadowRequests` / decals | 16 / off |
| Scene | post `Volume`s | off |
| Scene | `ParticleSystem`s | kept (opt-in kill) |

Deliberately omitted: `SetQualityLevel` (stomps game tiers), camera far plane (gameplay).

## Lifecycle

- `OnInitializeMelon`: prefs → apply if `Enabled` (default true).
- `F3`: toggle at runtime, persists `Enabled`, toast + hub report (gregCore).
- `OnSceneWasLoaded`: re-kill scene volumes/particles (game respawns them).
- `OnDeinitializeMelon`: full restore.
- Snapshot taken once on first apply; every setter guarded — one failure never blocks the rest.

## File map

- `src/PotatoMod.cs`, `src/Core/{GregHost,PotatoBridge,PotatoSettings,PotatoApplier}.cs`

## Manual tests (in game)

1. Launch with mod → log `[Potato] Potato mode ON`; textures visibly coarse, no shadows.
2. `F3` → OFF → originals back (shadows, AA, volumes); `F3` → ON again.
3. Change scene → potato still applied (volumes stay dead).
4. Quit → no errors; relaunch without mod → vanilla look.
5. With gregCore: F1 hub lists Potato, key HUD shows `F3 Potato`, toast on toggle.
6. Without gregCore.dll: same behavior minus hub/HUD/toast.
