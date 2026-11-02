using UnityEngine;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// A popup waiting on the server, with a spinning icon. The spin lives here because the
    /// screen that opened it may be hidden while it waits.
    /// </summary>
    public class LoadingModal : ModalScreen
    {
        const float DegreesPerFrame = -6f;

        [SerializeField]
        Transform icon;

        void Update()
        {
            if (icon.gameObject.activeSelf)
            {
                icon.Rotate(0, 0, DegreesPerFrame, Space.Self);
            }
        }
    }
}
