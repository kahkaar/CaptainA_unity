using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>What a screen does to the one below it. See docs/NEW/NAVIGATION.md.</summary>
    public enum ScreenLayer
    {
        /// <summary>Hides the screen below, which refreshes through OnEnable on return.</summary>
        Screen,

        /// <summary>Covers the screen below, which stays active so its state stays in place.</summary>
        Overlay,
    }

    /// <summary>
    /// Base class for everything Navigator opens. Screens never switch each other on or off.
    /// </summary>
    public abstract class AppScreen : MonoBehaviour
    {
        bool prepared;
        Vector2 homePosition;

        public virtual ScreenLayer Layer => ScreenLayer.Screen;

        public virtual bool IsTabRoot => false;

        /// <summary>Return true to consume a back press rather than closing this screen.</summary>
        public virtual bool HandleBack() => false;

        internal CanvasGroup Group { get; private set; }

        internal RectTransform Rect { get; private set; }

        internal Vector2 HomePosition => homePosition;

        protected virtual void Awake()
        {
            Prepare();
        }

        /// <summary>
        /// Adds what transitions need. Safe to repeat, and Navigator calls it before animating
        /// because Awake does not run until the screen's whole parent chain is active.
        ///
        /// The nested Canvas stops an animating screen from rebuilding the others. It copies
        /// the parent's shader channels because TextMeshPro needs them and a new Canvas has none.
        /// </summary>
        internal void Prepare()
        {
            if (prepared)
            {
                return;
            }

            prepared = true;
            Rect = (RectTransform)transform;
            homePosition = Rect.anchoredPosition;

            if (!TryGetComponent(out Canvas canvas))
            {
                Canvas parent =
                    transform.parent != null ? transform.parent.GetComponentInParent<Canvas>(true) : null;
                canvas = gameObject.AddComponent<Canvas>();
                if (parent != null)
                {
                    canvas.additionalShaderChannels = parent.additionalShaderChannels;
                    canvas.vertexColorAlwaysGammaSpace = parent.vertexColorAlwaysGammaSpace;
                }
            }

            if (!TryGetComponent<GraphicRaycaster>(out _))
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            if (!TryGetComponent(out CanvasGroup group))
            {
                group = gameObject.AddComponent<CanvasGroup>();
            }
            Group = group;
        }

        /// <summary>Restores what a transition changes. Every transition ends here.</summary>
        internal void ResetVisuals()
        {
            Prepare();
            Group.alpha = 1f;
            Group.blocksRaycasts = true;
            Rect.anchoredPosition = homePosition;
            Rect.localScale = Vector3.one;
        }

        /// <summary>Finds a child by a path relative to this screen, including inactive ones.</summary>
        protected GameObject FindObject(string path)
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                Debug.LogError($"{GetType().Name}: no child at '{path}'.", this);
                return null;
            }

            return child.gameObject;
        }

        /// <summary>Finds a component on a child by a path relative to this screen.</summary>
        protected T Find<T>(string path)
            where T : Component
        {
            GameObject child = FindObject(path);
            if (child == null)
            {
                return null;
            }

            if (!child.TryGetComponent(out T component))
            {
                Debug.LogError($"{GetType().Name}: no {typeof(T).Name} on '{path}'.", this);
            }

            return component;
        }
    }
}
