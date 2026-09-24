# GregCore gaps for Potato — proposal (not implemented)

No new GregCore API was needed for this mod: everything used is public engine
API (`QualitySettings`, HDRP volume/pipeline, `Volume`, `ParticleSystem`), and
GregCore integration (hub, key HUD, toggle binding, toasts) uses existing APIs
behind the soft probe.

## Proposal (upstream, optional)

`GregPerformanceModule` / `GregFrameRateLimiter` already implement profile
application, but only for in-core `GregMod` subclasses (`GregApiContext` +
governor required) — standalone MelonMods cannot reach them. A shared,
public, static applier would remove duplicated logic across perf mods:

```csharp
namespace gregCore.Performance {
    public static class GregPerfProfiles {
        // Applies a PerformanceProfile without a governor (snapshot optional).
        public static void ApplyLowEnd();
        public static void Apply(PerformanceProfile profile);
    }
}
```

Deliberately not implemented here: it would duplicate `GregFrameRateLimiter`
internals, and the gregCore tree is under active concurrent development.
`PotatoApplier` mirrors the verified HDRP paths instead; migrate when/if the
upstream API lands.
