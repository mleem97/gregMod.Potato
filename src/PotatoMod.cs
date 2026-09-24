// gregMod.Potato — disables perf-heavy effects and forces lowest texture
// quality. Standalone-safe (own applier, no hard gregCore dependency):
// GregHost probe + JIT-split PotatoBridge for F1-hub, key HUD and toasts.
// Originals are snapshotted on first apply and restored on toggle-off,
// scene changes re-apply, unload restores.
using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using GregMod.Potato.Core;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ModCoverage.Tests")]

[assembly: MelonInfo(typeof(GregMod.Potato.PotatoMod), "gregMod.Potato", "1.0.0", "TeamGreg Modding")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace GregMod.Potato
{
    public sealed class PotatoMod : MelonMod
    {
        internal static PotatoMod Instance { get; private set; }

        private static readonly PotatoSettings Settings = new PotatoSettings();
        private static Key _toggleKey = Key.F3;
        private static MelonPreferences_Entry<bool> _enabledEntry;

        public override void OnInitializeMelon()
        {
            try
            {
                Instance = this;
                var cat = MelonPreferences.CreateCategory("gregMod.Potato", "Potato");

                _enabledEntry = cat.CreateEntry("Enabled", true, "Potato mode",
                    "Master switch: lowest textures, no AA/shadows, HDRP effects off.");
                var keyEntry = cat.CreateEntry("ToggleKey", "F3", "Toggle key",
                    "Runtime on/off toggle (e.g. F3).");
                cat.CreateEntry("TextureMipmapLimit", 3, "Texture quality drop",
                    "0=full .. 3=lowest (globalTextureMipmapLimit).");
                cat.CreateEntry("AntiAliasing", 0, "Anti-aliasing",
                    "0/2/4/8 samples (0 = off).");
                cat.CreateEntry("ShadowsOff", true, "Disable shadows",
                    "ShadowQuality.Disable while potato is on.");
                cat.CreateEntry("ShadowDistance", 20f, "Shadow distance",
                    "Meters (lower = faster).");
                cat.CreateEntry("PixelLightCount", 0, "Pixel lights",
                    "Forward per-pixel light count.");
                cat.CreateEntry("LodBias", 0.4f, "LOD bias",
                    "Lower = coarser models sooner.");
                cat.CreateEntry("SoftParticlesOff", true, "Soft particles off", "");
                cat.CreateEntry("RealtimeProbesOff", true, "Realtime reflection probes off", "");
                cat.CreateEntry("AnisoOff", true, "Anisotropic filtering off", "");
                cat.CreateEntry("StreamingMipmapsOff", true, "Texture streaming off",
                    "With lowest mipmaps, full loads stay small.");
                cat.CreateEntry("TwoBoneSkinning", true, "Two-bone skinning",
                    "Cheaper character skinning.");
                cat.CreateEntry("LowShadowResolution", true, "Low shadow resolution", "");
                cat.CreateEntry("VSyncOff", true, "VSync off", "");
                cat.CreateEntry("TargetFps", 60, "Target FPS",
                    "Frame cap while potato is on (-1 = uncapped).");
                cat.CreateEntry("KillSSAO", true, "Kill SSAO (HDRP)", "");
                cat.CreateEntry("KillContactShadows", true, "Kill contact shadows (HDRP)", "");
                cat.CreateEntry("KillGI", true, "Kill global illumination (HDRP)", "");
                cat.CreateEntry("KillSSR", true, "Kill screen-space reflections (HDRP)", "");
                cat.CreateEntry("KillVolumetricFog", true, "Kill volumetric fog (HDRP)", "");
                cat.CreateEntry("MaxShadowRequests", 16, "Max HDRP shadow requests", "");
                cat.CreateEntry("DecalsOff", true, "HDRP decals off", "");
                cat.CreateEntry("DisablePostVolumes", true, "Disable post-process volumes",
                    "Generic scene Volume components off.");
                cat.CreateEntry("DisableParticles", false, "Disable ALL particles",
                    "Opt-in: kills gameplay cues (smoke/fire) too.");
                cat.SaveToFile(false);

                ReadPrefs(cat, keyEntry);
                if (GregHost.HasCore)
                {
                    try { RegisterCoreExtras(); } catch { }
                }

                if (Settings.Enabled) PotatoApplier.Apply(Settings);
                LoggerInstance.Msg("[Potato] Loaded. Potato mode " + (Settings.Enabled ? "ON" : "OFF") + " (" + _toggleKey + " = toggle).");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error("[Potato] OnInitializeMelon failed: " + ex.GetBaseException().Message);
            }
        }

        private static void ReadPrefs(MelonPreferences_Category cat, MelonPreferences_Entry<string> keyEntry)
        {
            try
            {
                Settings.Enabled = _enabledEntry.Value;
                if (Enum.TryParse<Key>(keyEntry.Value, true, out var k) && k != Key.None)
                {
                    _toggleKey = k;
                    Settings.ToggleKeyText = k.ToString();
                }
                else
                    MelonLogger.Warning("[Potato] Unknown ToggleKey '" + keyEntry.Value + "', defaulting to F3.");
                Settings.TextureMipmapLimit = GetInt(cat, "TextureMipmapLimit", 3);
                Settings.AntiAliasing = GetInt(cat, "AntiAliasing", 0);
                Settings.ShadowsOff = GetBool(cat, "ShadowsOff", true);
                Settings.ShadowDistance = GetFloat(cat, "ShadowDistance", 20f);
                Settings.PixelLightCount = GetInt(cat, "PixelLightCount", 0);
                Settings.LodBias = GetFloat(cat, "LodBias", 0.4f);
                Settings.SoftParticlesOff = GetBool(cat, "SoftParticlesOff", true);
                Settings.RealtimeProbesOff = GetBool(cat, "RealtimeProbesOff", true);
                Settings.AnisoOff = GetBool(cat, "AnisoOff", true);
                Settings.StreamingMipmapsOff = GetBool(cat, "StreamingMipmapsOff", true);
                Settings.TwoBoneSkinning = GetBool(cat, "TwoBoneSkinning", true);
                Settings.LowShadowResolution = GetBool(cat, "LowShadowResolution", true);
                Settings.VSyncOff = GetBool(cat, "VSyncOff", true);
                Settings.TargetFps = GetInt(cat, "TargetFps", 60);
                Settings.KillSSAO = GetBool(cat, "KillSSAO", true);
                Settings.KillContactShadows = GetBool(cat, "KillContactShadows", true);
                Settings.KillGI = GetBool(cat, "KillGI", true);
                Settings.KillSSR = GetBool(cat, "KillSSR", true);
                Settings.KillVolumetricFog = GetBool(cat, "KillVolumetricFog", true);
                Settings.MaxShadowRequests = GetInt(cat, "MaxShadowRequests", 16);
                Settings.DecalsOff = GetBool(cat, "DecalsOff", true);
                Settings.DisablePostVolumes = GetBool(cat, "DisablePostVolumes", true);
                Settings.DisableParticles = GetBool(cat, "DisableParticles", false);
            }
            catch { /* defaults stand */ }
        }

        private static int GetInt(MelonPreferences_Category cat, string key, int fallback)
        {
            try { return cat.GetEntry<int>(key).Value; } catch { return fallback; }
        }

        private static bool GetBool(MelonPreferences_Category cat, string key, bool fallback)
        {
            try { return cat.GetEntry<bool>(key).Value; } catch { return fallback; }
        }

        private static float GetFloat(MelonPreferences_Category cat, string key, float fallback)
        {
            try { return cat.GetEntry<float>(key).Value; } catch { return fallback; }
        }

        // ONLY with gregCore (JIT split).
        private void RegisterCoreExtras()
        {
            try
            {
                PotatoBridge.RegisterMenu();
                PotatoBridge.RegisterExtras(_toggleKey.ToString(), Toggle, () => PotatoApplier.IsApplied);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[Potato] Hub registration failed: " + ex.GetBaseException().Message);
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            try
            {
                if (Settings.Enabled && PotatoApplier.IsApplied)
                    PotatoApplier.ReapplySceneLevers(Settings);
            }
            catch { }
        }

        public override void OnUpdate()
        {
            try
            {
                var kb = Keyboard.current;
                if (kb == null) return;
                var key = kb[_toggleKey];
                if (key != null && key.wasPressedThisFrame && !IsPauseMenuActive())
                    Toggle();
            }
            catch { }
        }

        internal static void Toggle()
        {
            try
            {
                Settings.Enabled = !Settings.Enabled;
                try
                {
                    if (_enabledEntry != null)
                    {
                        _enabledEntry.Value = Settings.Enabled;
                        MelonPreferences.Save();
                    }
                }
                catch { }
                if (Settings.Enabled) PotatoApplier.Apply(Settings);
                else PotatoApplier.Restore();
                string msg = "[Potato] Potato mode " + (Settings.Enabled ? "ON" : "OFF") + ".";
                MelonLogger.Msg(msg);
                try
                {
                    if (GregHost.HasCore)
                    {
                        PotatoBridge.Report(Settings.Enabled);
                        PotatoBridge.Notify("Potato mode " + (Settings.Enabled ? "ON" : "OFF"));
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Potato] Toggle failed: " + ex.GetBaseException().Message);
            }
        }

        public override void OnDeinitializeMelon()
        {
            try { PotatoApplier.Restore(); } catch { }
        }

        internal static bool IsPauseMenuActive()
        {
            try
            {
                var all = Resources.FindObjectsOfTypeAll<Canvas>();
                if (all == null) return false;
                foreach (var c in all)
                {
                    if (c == null || !c.isActiveAndEnabled) continue;
                    var go = c.gameObject;
                    if (go == null) continue;
                    try { if (!go.scene.IsValid() || !go.scene.isLoaded) continue; } catch { continue; }
                    if (c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                    string n = go.name ?? "";
                    if (n.IndexOf("Pause", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("EscapeMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("InGameMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("SystemMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("OptionsMenu", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.IndexOf("SettingsMenu", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}
