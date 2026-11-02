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

        /// <summary>Makes a script-less popup openable, without needing any scene changes.</summary>
        public static ModalScreen Attach(GameObject popup, bool dismissible = true)
        {
            if (!popup.TryGetComponent(out ModalScreen modal))
            {
                modal = popup.AddComponent<ModalScreen>();
            }

            modal.Dismissible = dismissible;
            return modal;
        }
    }
}
