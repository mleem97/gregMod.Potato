// PotatoSettings — pref-backed config snapshot (plain data, no Unity refs).
// Every lever is individually gated so users can re-enable single effects.
using System;

namespace GregMod.Potato.Core
{
    internal sealed class PotatoSettings
    {
        internal bool Enabled = true;
        internal string ToggleKeyText = "F3";

        // QualitySettings levers
        internal int TextureMipmapLimit = 3;      // 0=full .. 3=lowest
        internal int AntiAliasing = 0;            // 0/2/4/8
        internal bool ShadowsOff = true;
        internal float ShadowDistance = 20f;
        internal int PixelLightCount = 0;
        internal float LodBias = 0.4f;
        internal bool SoftParticlesOff = true;
        internal bool RealtimeProbesOff = true;
        internal bool AnisoOff = true;
        internal bool StreamingMipmapsOff = true;
        internal bool TwoBoneSkinning = true;
        internal bool LowShadowResolution = true;

        // Frame pacing
        internal bool VSyncOff = true;
        internal int TargetFps = 60;              // -1 = uncapped

        // HDRP volume levers (game's SettingsSingleton volumeProfile)
        internal bool KillSSAO = true;
        internal bool KillContactShadows = true;
        internal bool KillGI = true;
        internal bool KillSSR = true;
        internal bool KillVolumetricFog = true;

        // HDRP pipeline asset levers
        internal int MaxShadowRequests = 16;
        internal bool DecalsOff = true;

        // Scene levers
        internal bool DisablePostVolumes = true;  // generic Volume components
        internal bool DisableParticles = false;   // gameplay cues — opt-in kill
    }
}
