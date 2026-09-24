// PotatoBridge — the ONLY file that touches gregCore types.
// RULE (Greg-Vertrag): call only when GregHost.HasCore is true (JIT split).
using System;

namespace GregMod.Potato.Core
{
    internal static class PotatoBridge
    {
        private const string MenuId = "potato";
        private const string ModId = "gregMod.Potato";

        internal static void RegisterMenu()
        {
            gregCore.UI.GregMenuRegistry.RegisterMenu(MenuId,
                new gregCore.UI.GregMenuOptions
                {
                    LockCamera = false,
                    LockMovement = false,
                    LockInteract = false,
                    ShowCursor = false,
                });
        }

        internal static void RegisterExtras(string toggleKeyText, Action toggle, Func<bool> isOn)
        {
            gregCore.Core.Mods.GregModRegistry.Register(
                ModId, "Potato", "1.0.0", new string[] { "potato" });
            gregCore.UI.GregHudRegistry.Register(MenuId, toggleKeyText ?? "F3", "Potato");
            gregCore.UI.GregMenuRegistry.RegisterOpener(MenuId,
                () => { try { toggle(); } catch { } });
            gregCore.UI.GregMenuRegistry.RegisterCloser(MenuId,
                () => { try { if (isOn != null && isOn()) toggle(); } catch { } });
        }

        internal static void Report(bool open)
        {
            try { gregCore.UI.GregMenuRegistry.SetOpen(MenuId, open); } catch { }
        }

        internal static void Notify(string msg)
        {
            try { gregCore.PublicApi.greg.UI.ShowNotification(msg); }
            catch { /* best-effort: log path covers it */ }
        }
    }
}
