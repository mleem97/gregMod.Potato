// PotatoApplier — snapshots original render settings on first apply and
// forces the potato levers; restores everything on disable/unload.
// Granular QualitySettings only (never SetQualityLevel — that would stomp
// game-specific tiers). HDRP paths mirror gregCore's verified
// GregFrameRateLimiter (same game, same volumeProfile). All best-effort:
// one failing lever never blocks the rest.
using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace GregMod.Potato.Core
{
    internal static class PotatoApplier
    {
        private sealed class Snapshot
        {
            internal int AntiAliasing;
            internal ShadowQuality Shadows;
            internal float ShadowDistance;
            internal ShadowResolution ShadowRes;
            internal int PixelLights;
            internal float LodBias;
            internal int MipLimit;
            internal bool SoftParticles;
            internal bool RealtimeProbes;
            internal AnisotropicFiltering Aniso;
            internal bool StreamingMipmaps;
            internal float StreamingBudget;
            internal SkinWeights SkinWeights;
            internal int VSync;
            internal int TargetFps;
            // HDRP pipeline asset
            internal bool HasHdrpAsset;
            internal int MaxShadowRequests;
            internal bool SupportDecals;
            internal bool SupportSSAO;
            internal bool SupportSSR;
            // HDRP volume components (active flags + fog values)
            internal readonly Dictionary<string, bool> VolumeActive = new Dictionary<string, bool>();
            internal bool FogOverrideState;
            internal bool FogValue;
        }

        private static Snapshot _original;
        private static bool _applied;
        private static readonly List<Volume> _killedVolumes = new List<Volume>();
        private static readonly List<UnityEngine.ParticleSystem> _killedParticles = new List<UnityEngine.ParticleSystem>();
        private static readonly List<bool> _killedParticlesWasEmitting = new List<bool>();

        internal static bool IsApplied { get { try { return _applied; } catch { return false; } } }

        internal static void Apply(PotatoSettings s)
        {
            try
            {
                if (_applied) { ReapplySceneLevers(s); return; }
                _original = Capture();
                ApplyQuality(s);
                ApplyHdrp(s);
                ReapplySceneLevers(s);
                _applied = true;
                MelonLogger.Msg("[Potato] Potato mode ON (mip×" + s.TextureMipmapLimit + ", AA=" + s.AntiAliasing + ", shadows " + (s.ShadowsOff ? "off" : "kept") + ").");
            }
            catch (Exception ex)
            {
                try { MelonLogger.Warning("[Potato] Apply failed: " + ex.GetBaseException().Message); } catch { }
            }
        }

        internal static void Restore()
        {
            try
            {
                if (!_applied || _original == null) return;
                var o = _original;
                try { QualitySettings.antiAliasing = o.AntiAliasing; } catch { }
                try { QualitySettings.shadows = o.Shadows; } catch { }
                try { QualitySettings.shadowDistance = o.ShadowDistance; } catch { }
                try { QualitySettings.shadowResolution = o.ShadowRes; } catch { }
                try { QualitySettings.pixelLightCount = o.PixelLights; } catch { }
                try { QualitySettings.lodBias = o.LodBias; } catch { }
                try { QualitySettings.globalTextureMipmapLimit = o.MipLimit; } catch { }
                try { QualitySettings.softParticles = o.SoftParticles; } catch { }
                try { QualitySettings.realtimeReflectionProbes = o.RealtimeProbes; } catch { }
                try { QualitySettings.anisotropicFiltering = o.Aniso; } catch { }
                try { QualitySettings.streamingMipmapsActive = o.StreamingMipmaps; } catch { }
                try { if (o.StreamingMipmaps) QualitySettings.streamingMipmapsMemoryBudget = o.StreamingBudget; } catch { }
                try { QualitySettings.skinWeights = o.SkinWeights; } catch { }
                try { QualitySettings.vSyncCount = o.VSync; } catch { }
                try { Application.targetFrameRate = o.TargetFps; } catch { }
                RestoreHdrp(o);
                RestoreSceneObjects();
                _applied = false;
                _original = null;
                MelonLogger.Msg("[Potato] Original settings restored.");
            }
            catch (Exception ex)
            {
                try { MelonLogger.Warning("[Potato] Restore failed: " + ex.GetBaseException().Message); } catch { }
            }
        }

        /// <summary>Re-kill scene objects (volumes/particles) after scene loads.</summary>
        internal static void ReapplySceneLevers(PotatoSettings s)
        {
            try
            {
                if (s.DisablePostVolumes) KillVolumes();
                if (s.DisableParticles) KillParticles();
            }
            catch { }
        }

        // ── Capture ──────────────────────────────────────────────────────────

        private static Snapshot Capture()
        {
            var o = new Snapshot();
            try { o.AntiAliasing = QualitySettings.antiAliasing; } catch { }
            try { o.Shadows = QualitySettings.shadows; } catch { }
            try { o.ShadowDistance = QualitySettings.shadowDistance; } catch { }
            try { o.ShadowRes = QualitySettings.shadowResolution; } catch { }
            try { o.PixelLights = QualitySettings.pixelLightCount; } catch { }
            try { o.LodBias = QualitySettings.lodBias; } catch { }
            try { o.MipLimit = QualitySettings.globalTextureMipmapLimit; } catch { }
            try { o.SoftParticles = QualitySettings.softParticles; } catch { }
            try { o.RealtimeProbes = QualitySettings.realtimeReflectionProbes; } catch { }
            try { o.Aniso = QualitySettings.anisotropicFiltering; } catch { }
            try { o.StreamingMipmaps = QualitySettings.streamingMipmapsActive; } catch { }
            try { o.StreamingBudget = QualitySettings.streamingMipmapsMemoryBudget; } catch { }
            try { o.SkinWeights = QualitySettings.skinWeights; } catch { }
            try { o.VSync = QualitySettings.vSyncCount; } catch { }
            try { o.TargetFps = Application.targetFrameRate; } catch { }
            CaptureHdrp(o);
            return o;
        }

        // ── Quality ──────────────────────────────────────────────────────────

        private static void ApplyQuality(PotatoSettings s)
        {
            try { QualitySettings.globalTextureMipmapLimit = Math.Max(0, Math.Min(3, s.TextureMipmapLimit)); } catch { }
            try { QualitySettings.antiAliasing = (s.AntiAliasing == 2 || s.AntiAliasing == 4 || s.AntiAliasing == 8) ? s.AntiAliasing : 0; } catch { }
            try { if (s.ShadowsOff) QualitySettings.shadows = ShadowQuality.Disable; } catch { }
            try { QualitySettings.shadowDistance = Math.Max(5f, s.ShadowDistance); } catch { }
            try { if (s.LowShadowResolution) QualitySettings.shadowResolution = ShadowResolution.Low; } catch { }
            try { QualitySettings.pixelLightCount = Math.Max(0, s.PixelLightCount); } catch { }
            try { QualitySettings.lodBias = Math.Max(0.1f, s.LodBias); } catch { }
            try { if (s.SoftParticlesOff) QualitySettings.softParticles = false; } catch { }
            try { if (s.RealtimeProbesOff) QualitySettings.realtimeReflectionProbes = false; } catch { }
            try { if (s.AnisoOff) QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable; } catch { }
            try { if (s.StreamingMipmapsOff) QualitySettings.streamingMipmapsActive = false; } catch { }
            try { if (s.TwoBoneSkinning) QualitySettings.skinWeights = SkinWeights.TwoBones; } catch { }
            try { if (s.VSyncOff) QualitySettings.vSyncCount = 0; } catch { }
            try { if (s.VSyncOff || s.TargetFps > 0) Application.targetFrameRate = s.TargetFps; } catch { }
        }

        // ── HDRP (mirrors gregCore GregFrameRateLimiter, same game) ──────────

        private static void CaptureHdrp(Snapshot o)
        {
            try
            {
                var hdrpAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
                if (hdrpAsset == null) return;
                o.HasHdrpAsset = true;
                var st = hdrpAsset.currentPlatformRenderPipelineSettings;
                try { o.MaxShadowRequests = st.hdShadowInitParams.maxShadowRequests; } catch { }
                try { o.SupportDecals = st.supportDecals; } catch { }
                try { o.SupportSSAO = st.supportSSAO; } catch { }
                try { o.SupportSSR = st.supportSSR; } catch { }
            }
            catch { }
            try
            {
                var volProfile = SettingsSingleton.instance?.settingsGraphics?.volumeProfile;
                if (volProfile == null) return;
                RememberVolume(volProfile, o, "ssao");
                RememberVolume(volProfile, o, "contact");
                RememberVolume(volProfile, o, "gi");
                RememberVolume(volProfile, o, "ssr");
                if (volProfile.TryGet<Fog>(out var fog) && fog != null)
                {
                    try { o.FogOverrideState = fog.enableVolumetricFog.overrideState; } catch { }
                    try { o.FogValue = fog.enableVolumetricFog.value; } catch { }
                }
            }
            catch { }
        }

        private static void RememberVolume(object volProfile, Snapshot o, string key)
        {
            try
            {
                var vp = volProfile as VolumeProfile;
                if (vp == null) return;
                if (key == "ssao" && vp.TryGet<ScreenSpaceAmbientOcclusion>(out var a) && a != null) o.VolumeActive[key] = a.active;
                else if (key == "contact" && vp.TryGet<ContactShadows>(out var b) && b != null) o.VolumeActive[key] = b.active;
                else if (key == "gi" && vp.TryGet<GlobalIllumination>(out var c) && c != null) o.VolumeActive[key] = c.active;
                else if (key == "ssr" && vp.TryGet<ScreenSpaceReflection>(out var d) && d != null) o.VolumeActive[key] = d.active;
            }
            catch { }
        }

        private static void ApplyHdrp(PotatoSettings s)
        {
            try
            {
                var volProfile = SettingsSingleton.instance?.settingsGraphics?.volumeProfile;
                if (volProfile != null)
                {
                    int killed = 0;
                    if (s.KillSSAO && volProfile.TryGet<ScreenSpaceAmbientOcclusion>(out var ssao) && ssao != null) { try { ssao.active = false; killed++; } catch { } }
                    if (s.KillContactShadows && volProfile.TryGet<ContactShadows>(out var cs) && cs != null) { try { cs.active = false; killed++; } catch { } }
                    if (s.KillGI && volProfile.TryGet<GlobalIllumination>(out var gi) && gi != null) { try { gi.active = false; killed++; } catch { } }
                    if (s.KillSSR && volProfile.TryGet<ScreenSpaceReflection>(out var ssr) && ssr != null) { try { ssr.active = false; killed++; } catch { } }
                    if (s.KillVolumetricFog && volProfile.TryGet<Fog>(out var fog) && fog != null)
                    {
                        try { fog.enableVolumetricFog.overrideState = true; fog.enableVolumetricFog.value = false; killed++; } catch { }
                    }
                    if (killed > 0) MelonLogger.Msg("[Potato] Disabled " + killed + " HDRP volume effect(s).");
                }
            }
            catch (Exception ex)
            {
                try { MelonLogger.Warning("[Potato] HDRP volume kill failed: " + ex.GetBaseException().Message); } catch { }
            }
            try
            {
                var hdrpAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
                if (hdrpAsset == null) return;
                var st = hdrpAsset.currentPlatformRenderPipelineSettings;
                try { st.hdShadowInitParams.maxShadowRequests = Math.Max(0, s.MaxShadowRequests); } catch { }
                try { if (s.DecalsOff) st.supportDecals = false; } catch { }
                try { if (s.KillSSAO) st.supportSSAO = false; } catch { }
                try { if (s.KillSSR) st.supportSSR = false; } catch { }
                try { hdrpAsset.currentPlatformRenderPipelineSettings = st; } catch { }
            }
            catch (Exception ex)
            {
                try { MelonLogger.Warning("[Potato] HDRP pipeline kill failed: " + ex.GetBaseException().Message); } catch { }
            }
        }

        private static void RestoreHdrp(Snapshot o)
        {
            try
            {
                var volProfile = SettingsSingleton.instance?.settingsGraphics?.volumeProfile;
                if (volProfile != null)
                {
                    RestoreVolume(volProfile, o, "ssao");
                    RestoreVolume(volProfile, o, "contact");
                    RestoreVolume(volProfile, o, "gi");
                    RestoreVolume(volProfile, o, "ssr");
                    if (volProfile.TryGet<Fog>(out var fog) && fog != null)
                    {
                        try { fog.enableVolumetricFog.overrideState = o.FogOverrideState; } catch { }
                        try { fog.enableVolumetricFog.value = o.FogValue; } catch { }
                    }
                }
            }
            catch { }
            try
            {
                if (!o.HasHdrpAsset) return;
                var hdrpAsset = GraphicsSettings.currentRenderPipeline as HDRenderPipelineAsset;
                if (hdrpAsset == null) return;
                var st = hdrpAsset.currentPlatformRenderPipelineSettings;
                try { st.hdShadowInitParams.maxShadowRequests = o.MaxShadowRequests; } catch { }
                try { st.supportDecals = o.SupportDecals; } catch { }
                try { st.supportSSAO = o.SupportSSAO; } catch { }
                try { st.supportSSR = o.SupportSSR; } catch { }
                try { hdrpAsset.currentPlatformRenderPipelineSettings = st; } catch { }
            }
            catch { }
        }

        private static void RestoreVolume(object volProfile, Snapshot o, string key)
        {
            try
            {
                if (!o.VolumeActive.TryGetValue(key, out var wasActive)) return;
                var vp = volProfile as VolumeProfile;
                if (vp == null) return;
                if (key == "ssao" && vp.TryGet<ScreenSpaceAmbientOcclusion>(out var a) && a != null) a.active = wasActive;
                else if (key == "contact" && vp.TryGet<ContactShadows>(out var b) && b != null) b.active = wasActive;
                else if (key == "gi" && vp.TryGet<GlobalIllumination>(out var c) && c != null) c.active = wasActive;
                else if (key == "ssr" && vp.TryGet<ScreenSpaceReflection>(out var d) && d != null) d.active = wasActive;
            }
            catch { }
        }

        // ── Scene objects ────────────────────────────────────────────────────

        private static void KillVolumes()
        {
            try
            {
                _killedVolumes.RemoveAll(v => v == null);
                var all = Resources.FindObjectsOfTypeAll<Volume>();
                if (all == null) return;
                foreach (var v in all)
                {
                    try
                    {
                        if (v == null || !v.enabled) continue;
                        var go = v.gameObject;
                        if (go == null) continue;
                        try { if (!go.scene.IsValid() || !go.scene.isLoaded) continue; } catch { continue; }
                        v.enabled = false;
                        if (!_killedVolumes.Contains(v)) _killedVolumes.Add(v);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void KillParticles()
        {
            try
            {
                PruneParticleLists();
                var all = Resources.FindObjectsOfTypeAll<UnityEngine.ParticleSystem>();
                if (all == null) return;
                foreach (var p in all)
                {
                    try
                    {
                        if (p == null) continue;
                        if (_killedParticles.Contains(p)) continue;
                        var go = p.gameObject;
                        if (go == null) continue;
                        try { if (!go.scene.IsValid() || !go.scene.isLoaded) continue; } catch { continue; }
                        bool wasEmitting = false;
                        try { wasEmitting = p.isPlaying; } catch { continue; }
                        if (!wasEmitting) continue;
                        try { p.Stop(); } catch { continue; }
                        try { p.Clear(); } catch { }
                        _killedParticles.Add(p);
                        _killedParticlesWasEmitting.Add(true);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void PruneParticleLists()
        {
            try
            {
                for (int i = _killedParticles.Count - 1; i >= 0; i--)
                {
                    try { if (_killedParticles[i] == null) { _killedParticles.RemoveAt(i); _killedParticlesWasEmitting.RemoveAt(i); } }
                    catch { _killedParticles.RemoveAt(i); _killedParticlesWasEmitting.RemoveAt(i); }
                }
            }
            catch { }
        }

        private static void RestoreSceneObjects()
        {
            try
            {
                foreach (var v in _killedVolumes)
                {
                    try { if (v != null) v.enabled = true; } catch { }
                }
                _killedVolumes.Clear();
                for (int i = 0; i < _killedParticles.Count; i++)
                {
                    try
                    {
                        var p = _killedParticles[i];
                        if (p != null && _killedParticlesWasEmitting[i])
                        {
                            try { p.Play(); } catch { }
                        }
                    }
                    catch { }
                }
                _killedParticles.Clear();
                _killedParticlesWasEmitting.Clear();
            }
            catch { }
        }
    }
}
