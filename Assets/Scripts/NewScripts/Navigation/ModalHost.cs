using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// Presents modals: the one shared dim, and the open and close animations. Navigator owns
    /// the stack and decides which modal is on top. Added to Navigator's object from code.
    /// </summary>
    internal sealed class ModalHost : MonoBehaviour
    {
        const float DimAlpha = 0.5f;

        Image dim;
        TransitionRunner transition;

        void Awake()
        {
            transition = new TransitionRunner(this);
        }

        public void Snap()
        {
            transition.Snap();
        }

        public void Show(ModalScreen modal)
        {
            bool dimAlreadyShowing = dim != null && dim.gameObject.activeSelf;
            PlaceDimBelow(modal);

            System.Action<float> animate = ScreenTransitions.Modal(modal, opening: true);
            modal.Group.blocksRaycasts = false;

            transition.Play(
                ScreenTransitions.ModalSeconds,
                t =>
                {
                    animate(t);
                    if (!dimAlreadyShowing)
                    {
                        SetDimAlpha(DimAlpha * t);
                    }
                },
                () =>
                {
                    modal.ResetVisuals();
                    SetDimAlpha(DimAlpha);
                }
            );
        }

        /// <param name="next">The modal now on top, which keeps the dim, or null for none.</param>
        public void Hide(ModalScreen modal, ModalScreen next)
        {
            if (next != null)
            {
                PlaceDimBelow(next);
            }

            System.Action<float> animate = ScreenTransitions.Modal(modal, opening: false);
            modal.Group.blocksRaycasts = false;

            transition.Play(
                ScreenTransitions.ModalSeconds,
                t =>
                {
                    animate(t);
                    if (next == null)
                    {
                        SetDimAlpha(DimAlpha * (1f - t));
                    }
                },
                () =>
                {
                    modal.ResetVisuals();
                    modal.gameObject.SetActive(false);
                    if (next == null)
                    {
                        HideDim();
                    }
                }
            );
        }

        public void HideDim()
        {
            if (dim != null)
            {
                dim.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Puts the dim directly below the modal, in the modal's own parent, because sibling
        /// order is what sets draw order.
        /// </summary>
        void PlaceDimBelow(ModalScreen modal)
        {
            if (dim == null)
            {
                dim = CreateDim();
            }

            dim.transform.SetParent(modal.transform.parent, false);
            dim.transform.SetAsLastSibling();
            dim.transform.SetSiblingIndex(modal.transform.GetSiblingIndex());
            dim.gameObject.SetActive(true);
        }

        void SetDimAlpha(float alpha)
        {
            if (dim != null)
            {
                dim.color = new Color(0f, 0f, 0f, alpha);
            }
        }

        static Image CreateDim()
        {
            var dimObject = new GameObject("ModalDim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            var rect = (RectTransform)dimObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // A raycast target, so it blocks taps on everything below, including the tab bar.
            Image image = dimObject.GetComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(0f, 0f, 0f, 0f);
            return image;
        }
    }
}
