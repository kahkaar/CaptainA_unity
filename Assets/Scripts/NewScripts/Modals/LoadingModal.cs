using UnityEngine;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// A popup waiting on the server, with a spinning icon. The spin lives here because the
    /// screen that opened it may be hidden while it waits.
    /// </summary>
    public sealed class LoadingModal : ModalScreen
    {
        const float DegreesPerFrame = -6f;

        Transform icon;

        protected override void Bind()
        {
            icon = FindObject("LoadingIcon").transform;
        }

        void Update()
        {
            if (icon.gameObject.activeSelf)
            {
                icon.Rotate(0, 0, DegreesPerFrame, Space.Self);
            }
        }
    }
}
