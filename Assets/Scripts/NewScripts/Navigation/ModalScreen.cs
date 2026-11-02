using System;
using UnityEngine;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// A popup on the navigation stack. ModalHost shows it above the shared dim. Subclass it for
    /// a popup with logic, or use Attach for one with no script of its own.
    /// </summary>
    public class ModalScreen : AppScreen
    {
        public override ScreenLayer Layer => ScreenLayer.Overlay;

        /// <summary>False ignores back, such as for a loading popup waiting on the server.</summary>
        public bool Dismissible { get; set; } = true;

        /// <summary>
        /// Runs on every way of closing through back, including the popup's own back button if
        /// it calls Navigator.Back. Return true to keep the popup from closing.
        /// </summary>
        public Func<bool> OnBack { get; set; }

        public override bool HandleBack()
        {
            return OnBack != null && OnBack();
        }

        /// <summary>Makes a script-less popup openable, without needing any scene changes.</summary>
        public static ModalScreen Attach(GameObject popup, bool dismissible = true)
        {
            return Attach<ModalScreen>(popup, dismissible);
        }

        /// <summary>
        /// Adds T to a popup and lifts it out of the screen that owns it, to the top of the root
        /// Canvas. Only there can it and the shared dim draw above the tab bar. It keeps its
        /// position on screen. Find anything the owner needs by path before calling this.
        /// </summary>
        public static T Attach<T>(GameObject popup, bool dismissible = true)
            where T : ModalScreen
        {
            if (!popup.TryGetComponent(out T modal))
            {
                modal = popup.AddComponent<T>();
            }

            popup.transform.SetParent(popup.transform.root, true);
            modal.Dismissible = dismissible;
            return modal;
        }
    }
}
