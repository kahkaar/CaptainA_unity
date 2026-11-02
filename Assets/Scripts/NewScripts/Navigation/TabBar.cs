using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// The tab buttons, their icons and the new-feature dot. Navigator owns which tab is
    /// current, and this follows TabChanged, so back selecting a tab updates the icons too.
    ///
    /// To add a tab, add a child button and a line in Start. A button's name also selects its
    /// icons: Resources/app_icons/ic_[name]_active and ic_[name]_inactive.
    /// </summary>
    public class TabBar : MonoBehaviour
    {
        const string NewFeatureBadgeName = "NewFeatureBadgeGO";

        struct Tab
        {
            public AppScreen Root;
            public Image Icon;
        }

        Tab[] tabs;
        GameObject newFeatureBadgeGO;

        // Start rather than Awake: Navigator is created after every Awake in the scene.
        void Start()
        {
            Navigator navigator = Navigator.GetManager();

            tabs = new[]
            {
                WireTab("Freestyle", navigator.Get<FreestyleScreen>(), navigator),
                WireTab("Flashcard", navigator.Get<FlashcardsScreen>(), navigator),
                WireTab("Advanced", navigator.Get<AdvancedScreen>(), navigator),
                WireTab("Progress", navigator.Get<ProgressScreen>(), navigator),
            };

            newFeatureBadgeGO = Navigator.FindInCanvas(this, NewFeatureBadgeName);

            navigator.TabChanged += OnTabChanged;
            OnTabChanged(navigator.CurrentTab);
        }

        void OnDestroy()
        {
            Navigator navigator = Navigator.GetManager();
            if (navigator != null)
            {
                navigator.TabChanged -= OnTabChanged;
            }
        }

        Tab WireTab(string buttonName, AppScreen root, Navigator navigator)
        {
            Transform button = transform.Find(buttonName);
            ButtonWiring.Replace(button.GetComponent<Button>(), () => navigator.SelectTab(root));

            return new Tab { Root = root, Icon = button.GetComponent<Image>() };
        }

        void OnTabChanged(AppScreen current)
        {
            foreach (Tab tab in tabs)
            {
                SetIcon(tab.Icon, tab.Root == current);
            }

            // After the screen is up: it records itself as seen in its own OnEnable, which has
            // already run, so the dot goes in the same frame it was earned.
            if (newFeatureBadgeGO != null)
            {
                newFeatureBadgeGO.SetActive(
                    NewFeatureBadge.ShouldShow(Const.PREF_SEEN_ADVANCED, Const.VER_MAX_SHOW_ADVANCED)
                );
            }
        }

        static void SetIcon(Image icon, bool isActive)
        {
            string spriteName = "ic_" + icon.name + "_" + (isActive ? "active" : "inactive");
            Sprite sprite = Resources.Load<Sprite>("app_icons/" + spriteName);
            if (sprite != null)
            {
                icon.sprite = sprite;
            }
            else
            {
                Debug.LogError("Source image not found: " + spriteName);
            }
        }
    }
}
