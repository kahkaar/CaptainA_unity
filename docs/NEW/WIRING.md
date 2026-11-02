# UI Wiring

How screens connect to their UI, and how to add a new one. Wiring is done in code wherever
possible.

## Rules

| Need                   | Do this                                                              |
| ---------------------- | -------------------------------------------------------------------- |
| Child object or component | `Find<T>("Path/To/Child")` or `FindObject("Path/To/Child")` in `Awake` |
| Button click           | `ButtonWiring.Replace(button, Handler)` once, in `Awake`             |
| Localized text         | `new LocalizedString(table, key)`                                    |
| Another screen         | Ask `Navigator` by type. Never hold a reference to a screen.         |
| Screen layer           | Override `Layer`                                                     |
| Object outside the screen | `Navigator.FindInCanvas(this, "ObjectName")`, for a shared popup  |
| Popup with no logic    | `ModalScreen.Attach(popupGO, dismissible)` from the owning screen, after any `Find` inside it |
| Popup waiting on the server | `ModalScreen.Attach<LoadingModal>(popupGO, false)`, which spins its `LoadingIcon` |
| Reset on closing a popup | `modal.OnBack`, so every way out behaves the same                  |
| Drop-down error message | `ErrorBanner.Show` and `ErrorBanner.Hide`                           |
| Durations and constants | Constants in code                                                   |

`Find<T>` and `FindObject` are `AppScreen` methods built on `transform.Find`, which also
finds inactive children. Paths are relative to the screen root, and a modal's own `Find` works
after `Attach` has lifted it.

**Wire buttons in `Awake`, not `OnEnable`.** `OnEnable` runs every time a screen reopens,
and a repeated `AddListener` makes one tap run its handler several times.

`ButtonWiring.Replace` also switches off any Inspector `OnClick` entries while the app runs,
so a migrated button can't fire twice and its entries don't have to be cleared by hand.

## What stays in the Inspector

- Script components on screen roots and on the tab bar
- Prefab references, such as `ListItemPanel` and `ListDetailPanel`
- `ASATaskScreen.tasks`, because it holds sprites
- `AudioManager.recordNotif`, because it is an audio clip
- Layout and visual values on UI components

`NetworkManager` needs nothing assigned, because it contains no UI.

## Renaming and moving

| Change                                | Effect                               |
| ------------------------------------- | ------------------------------------ |
| Move a whole screen in the hierarchy  | Safe: its paths are relative         |
| Rename or move a child inside a screen | Breaks its path, so update the string |
| Rename a tab button                   | Breaks `TabBar`, which finds it by name, and its icon lookup |

A broken path logs an error naming the screen and the path. `Awake` only runs the first
time a screen opens, so the error appears when that screen is first opened.

## Hierarchy

Sibling order sets draw order, so this order keeps tabs below overlays and modals. Everything
is a direct child of the root Canvas, in this order:

```text
Canvas
├── Tab roots          MainPanel, CardDeckPanel, AdvancePanel, ProfilePanel
├── Pushed screens     TaskPanel, ASAPanel, FeedbackPanel, ASAProfilePanel,
│                      SuperMemoPanel, NumberGamePanel
├── NavigationBar      TabBar
├── NewFeatureBadgeGO
├── Overlays           DetailScorePanel, PhonemePanel, InfoPanel
└── Modals             PopUpPanel, SurveyPopUpPanel, SecretPopUpPanel
```

Scene objects keep their old names, because `TabBar`, `Find` paths and `FindInCanvas` look them
up by name. Only the classes on them were renamed.

`Navigator`, `ModalHost`, the shared dim, and each screen's nested Canvas are all created
from code, so they do not appear in the scene. Popups owned by a screen stay under it in the
scene, and `ModalScreen.Attach` lifts them to the top of the Canvas while the app runs.

## Naming

- Full-screen view classes end in `Screen`, and popups end in `Modal`. "Panel" is only for
  sections inside a screen.
- Abbreviations stay in capitals: `ASATaskScreen`, not `AsaTaskScreen`.
- Do not name a class `Screen`, because it would shadow `UnityEngine.Screen`.
- During the migration, new files go in `Assets/Scripts/NewScripts/`, and all use the
  `SaySuomi.Navigation` namespace. A single namespace matters while old classes exist: a
  file-level `using` loses to a global class of the same name, so the old `NetworkManager`
  would be picked silently.

## Adding a screen

1. Create the GameObject as a direct child of the Canvas, and leave it inactive. Place it before
   `NavigationBar` for a `Screen` layer entry, or after it for an overlay.
2. Add a class and attach it to the root:

   ```csharp
   using TMPro;
   using UnityEngine.UI;

   namespace SaySuomi.Navigation
   {
       public class ExampleScreen : AppScreen
       {
           // Omit this for the default Screen layer.
           public override ScreenLayer Layer => ScreenLayer.Overlay;

           TextMeshProUGUI titleText;

           protected override void Awake()
           {
               base.Awake();
               titleText = Find<TextMeshProUGUI>("Content/TitleText");
               ButtonWiring.Replace(Find<Button>("BackButton"), () => Navigator.GetManager().Back());
           }

           public void Show(string title)
           {
               titleText.text = title;
           }
       }
   }
   ```

3. Open it from the caller:

   ```csharp
   Navigator.GetManager().Push<ExampleScreen>().Show("Hello");
   ```

4. Add it to [Screens](SCREENS.md).

There is no Escape handling or registration to add. `Navigator` finds the screen and
handles back.

## Stopping work when a screen closes

Put cleanup in `OnDisable`, which runs on every way of leaving a screen, including tab
changes. Any screen that records audio must cancel the recording there. If a back press
should do something other than close the screen, override `HandleBack` and return true.
