using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// One-off tool that copies MainScene's UI values into TESTING_SCENE. Delete it once that is
    /// done. See docs/NEW/MIGRATION.md.
    /// </summary>
    public sealed class MainSceneLookWindow : EditorWindow
    {
        private const string TargetSceneName = "TESTING_SCENE";
        private const string UndoName = "Match MainScene Look";

        private const string Modals = "Canvas/Modals/";
        private const string Screens = "Canvas/Screens/";
        private const string ProfileContent = Screens + "ASAProfileScreen/Viewport/Content/";
        private const string ResultsContent =
            Screens + "ASAResultsScreen/ScrollView/Viewport/Content/";
        private const string ProfileFeedback = Modals + "ASAProfileFeedbackModal/";
        private const string ProfileSettings = Modals + "ASAProfileSettingsModal/";
        private const string ResultsFeedback = Modals + "ASAResultsFeedbackModal/";
        private const string ListFeedback = Modals + "ASATaskListFeedbackModal/";
        private const string ListLoading = Modals + "ASATaskListProfileLoadingModal/";
        private const string Sending = Modals + "ASATaskSendingModal/Content/";
        private const string Background = Modals + "AdvancedBackgroundModal/Viewport/Content/";
        private const string Consent = Modals + "AdvancedConsentModal/Content";
        private const string Placeholder = "/Text Area/Placeholder";

        private const string AdvancedTitle = "Canvas/Tabs/AdvancedScreen/TopPanelNew/AdvancedText";
        private const string ListTitle = Screens + "ASATaskListScreen/TopPanelNew/AdvancedText";
        private const string ProfileTitle = Screens + "ASAProfileScreen/TopPanelNew/AdvancedText";
        private const string ResultsTitle = Screens + "ASAResultsScreen/TopPanelNew/FeedbackText";

        private static readonly Color DimColor = new Color32(74, 80, 95, 50);

        private readonly List<string> missing = new List<string>();

        private bool applyText = true;
        private bool applyScreens = true;
        private bool applyModalContents = true;
        private bool applyModalLayout;
        private bool applyDimAndDot;
        private string resultText = "";
        private MessageType resultType;

        [MenuItem("Tools/Match MainScene Look")]
        private static void Open()
        {
            GetWindow<MainSceneLookWindow>("Match MainScene");
        }

        private void OnGUI()
        {
            bool canApply =
                SceneManager.GetActiveScene().name == TargetSceneName
                && !EditorApplication.isPlaying;
            if (!canApply)
            {
                EditorGUILayout.HelpBox(
                    $"Open {TargetSceneName} outside Play mode to use this tool.",
                    MessageType.Warning
                );
            }

            applyText = EditorGUILayout.ToggleLeft("Text", applyText);
            applyScreens = EditorGUILayout.ToggleLeft("Screens", applyScreens);
            applyModalContents = EditorGUILayout.ToggleLeft("Modal contents", applyModalContents);
            applyModalLayout = EditorGUILayout.ToggleLeft(
                "Modal layout (reverts the modal changes in 666ce4f)",
                applyModalLayout
            );
            applyDimAndDot = EditorGUILayout.ToggleLeft("Dim and dot", applyDimAndDot);

            using (new EditorGUI.DisabledScope(!canApply))
            {
                if (GUILayout.Button("Apply"))
                {
                    Apply();
                }
            }

            if (resultText.Length > 0)
            {
                EditorGUILayout.HelpBox(resultText, resultType);
            }
        }

        private void Apply()
        {
            missing.Clear();
            int editCount = 0;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            int undoGroup = Undo.GetCurrentGroup();

            void Run(bool isOn, Func<int> group)
            {
                if (isOn)
                {
                    editCount += group();
                }
            }

            Run(applyText, ApplyText);
            Run(applyScreens, ApplyScreens);
            Run(applyModalContents, ApplyModalContents);
            Run(applyModalLayout, ApplyModalLayout);
            Run(applyDimAndDot, ApplyDimAndDot);

            if (missing.Count > 0)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                resultText = "Nothing changed. Not found:\n" + string.Join("\n", missing);
                resultType = MessageType.Error;
                return;
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            resultText = $"Applied {editCount} edits. Check the scene, then save it.";
            resultType = MessageType.Info;
        }

        private int ApplyText()
        {
            int count = 0;

            foreach (
                string path in new[]
                {
                    ProfileFeedback + "ComparisonFeedbackTextField" + Placeholder,
                    ResultsFeedback + "AccuracyFeedbackTextField" + Placeholder,
                    ResultsFeedback + "UnderstandFeedbackTextField" + Placeholder,
                    ListFeedback + "OverallFeedbackTextField" + Placeholder,
                    Sending + "FeedbackTextField" + Placeholder,
                    Background + "MotherTongueOptions/InputField" + Placeholder,
                    Background + "OtherLanguagesOptions/InputField" + Placeholder,
                }
            )
            {
                count += SetFontSize(path, 24);
            }

            foreach (
                string path in new[]
                {
                    ProfileSettings + "PopupText",
                    Consent + "/PopupText",
                    Consent + "/ConsentToggle/Label",
                    Consent + "/LinkButton/LinkText",
                    ProfileContent + "DescriptionText",
                }
            )
            {
                count += SetFontSize(path, 28);
            }

            foreach (
                string path in new[]
                {
                    ProfileFeedback + "AccuracyFeedbackTitle",
                    ResultsFeedback + "AccuracyFeedbackTitle",
                    ResultsFeedback + "UnderstandFeedbackTitle",
                    ListFeedback + "OverallFeedbackTitle",
                    ListLoading + "ErrorText",
                    ListLoading + "PopupTitle",
                    Sending + "ErrorText",
                    Sending + "Title",
                }
            )
            {
                count += SetFontSize(path, 30);
            }

            count += SetFontSize(Consent + "/PopupTitle", 36);
            count += SetFontSize(ProfileContent + "CurrentStatus", 36);

            foreach (string path in new[] { AdvancedTitle, ListTitle, ProfileTitle })
            {
                count += SetTitle(path, VerticalAlignmentOptions.Middle);
            }
            count += SetTitle(ResultsTitle, VerticalAlignmentOptions.Top);

            count += Place(AdvancedTitle, 100, -20, 470, 110);
            count += Place(ListTitle, 80, -20, 450, 110);
            count += Place(ProfileTitle, 80, -20, 450, 110);
            count += Place(ResultsTitle, 80, -20, 450, 110);

            count += Edit<TMP_Text>(
                ProfileContent + "InsufficientDataNotice/InsufficientDataText",
                text => text.fontSharedMaterial = text.font.material
            );
            return count;
        }

        private int ApplyScreens()
        {
            string notice = ProfileContent + "InsufficientDataNotice";
            string list = Screens + "ASATaskListScreen/";
            string task = Screens + "ASATaskScreen/";

            return Place(ProfileContent + "CurrentStatus", 0, -56, 482.5031f, 60)
                + Place(ProfileContent + "DescriptionText", 0, -340, 305, 50)
                + Move(ProfileContent + "LevelBar", 0, -290)
                + Move(ProfileContent + "LevelBarBackground", 0, -290)
                + Move(ProfileContent + "LevelText", 0, -245)
                + Resize(ProfileContent + "FeedbackButton", 335, 80)
                + Place(ProfileContent + "GlobalRank/GlobeIcon", 58, 5, 65, 65)
                + Place(ProfileContent + "GlobalRank/RankText", 309.3547f, 0, 397.0906f, 90)
                + Place(ProfileContent + "Performance/StarIcon", 58, 5, 65, 65)
                + Place(ProfileContent + "Performance/PerformanceText", 376, 0, 535, 90)
                + Place(notice, 0, -732.02655f, -40, 164.0531f)
                + Place(notice + "/InsufficientDataText", 0, -0.00078583f, -49.999992f, 164.05f)
                + Resize(ResultsContent + "FeedbackButton", 335, 80)
                + Resize(ResultsContent + "NewTaskButton", 335, 80)
                + Place(
                    ResultsContent + "TitleGroup/ProficiencyText",
                    -32.79844f,
                    -64,
                    465.8032f,
                    56
                )
                + Place(list + "InfoText", 0.084512f, -405, 683.17f, 80)
                + Move(list + "Task1", 0, -526)
                + Move(list + "Task2", 0, -664)
                + Move(list + "Task3", 0, -802)
                + Move(list + "Task4", 0, -940)
                + Move(list + "Task5", 0, -1078)
                + Place(task + "TaskImage", 0, -502, 609.6952f, 618.4701f)
                + Move(task + "TaskText", 0, -175);
        }

        private int ApplyModalContents()
        {
            string options = Background + "SelfAssessmentOptions";

            return Place(ProfileFeedback + "AccuracyFeedbackTitle", 0, -205, 600, 150)
                + Move(ProfileFeedback + "ComparisonFeedbackTextField", 0, -112.3f)
                + Move(ProfileFeedback + "ComparisonRatingGroup", 20, -195)
                + Move(ProfileFeedback + "FeedbackBackButton", -267.5f, 720.3f)
                + Move(ProfileFeedback + "FeedbackSendButton", 0, 98.2f)
                + Move(ProfileSettings + "GuidText", 0, -362.8f)
                + Move(ProfileSettings + "PopupText", 0, -232)
                + Move(ProfileSettings + "PopupTitle", 0, -145)
                + Place(ResultsFeedback + "AccuracyFeedbackTitle", 0, -177, 600, 100)
                + Place(ResultsFeedback + "UnderstandFeedbackTitle", 0, -616, 600, 100)
                + Place(ListFeedback + "OverallFeedbackTitle", 0, -205, 600, 150)
                + Place(ListLoading + "PopupTitle", 0.12884f, -84, 600, 60)
                + Move(Sending + "ErrorPopup", -0.12780863f, 845.1384f)
                + Move(Sending + "ErrorText", 0, 60)
                + Resize(Sending + "FeedbackTitle", 600, 90)
                + Move(Sending + "LoadingIcon", 0, 170)
                + Move(Sending + "ResultsButton", 0, 170)
                + Move(Sending + "Title", 0, -100)
                + Resize(options, 600, 396.066f)
                + Resize(options + "/Title", 620, 94.7354f);
        }

        // The old parents of some modals were offset by 75 or 80, and Modals is not, so those
        // positions add that offset to the values in MainScene.
        private int ApplyModalLayout()
        {
            return Place(Modals + "ASAProfileDeleteConfirmModal", 0, 76, 655, 706.3125f)
                + Place(Modals + "ASAProfileSettingsModal", 0, 76, 655, 706.3125f)
                + Place(Modals + "ASAProfileFeedbackModal", 0, 76, 655, 785)
                + Place(Modals + "ASAResultsFeedbackModal", 0, 80, 655, 1184)
                + Place(Modals + "ASAResultsInfoModal", 0, 105.6098f, 654.74f, 1168.7803f)
                + Place(Modals + "ASAResultsScoreInfoModal", 0, 78.5336f, 654.7444f, 657.67f)
                + Place(Modals + "ASATaskListFeedbackModal", 0, 75, 655, 785)
                + Place(Modals + "ASATaskListProfileLoadingModal", 0, 130.77f, 655, 688.46f)
                + Place(Modals + "ASATaskSendingModal/Content", 0.12775f, 61.715f, 655, 871.57f)
                + Place(Modals + "AdvancedBackgroundModal", 0, -42.702263f, 686.3264f, 1126.2021f)
                + ApplyConsentLayout()
                + ApplyStretchedContent(Modals + "InstructionModal/Content", -80, -570, -35)
                + ApplyStretchedContent(Modals + "SurveyModal/Content", -100, -1000, 0)
                + ApplyStretchedContent(Modals + "SecretModal/Content", -100, -1000, 0);
        }

        private int ApplyConsentLayout()
        {
            return Edit<RectTransform>(
                    Consent,
                    rect =>
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.right;
                        rect.pivot = new Vector2(0.5f, 0);
                        rect.anchoredPosition = new Vector2(0, 150);
                        rect.sizeDelta = new Vector2(0, 1128.4032f);
                    }
                )
                + Edit<VerticalLayoutGroup>(Consent, group => group.childControlWidth = false)
                + Edit<LayoutElement>(
                    Consent + "/LinkButton",
                    element =>
                    {
                        element.minWidth = -1;
                        element.preferredWidth = -1;
                    }
                )
                + Resize(Consent + "/AcceptButton", 643.3365f, 110)
                + Resize(Consent + "/ConsentToggle", 643.3365f, 110)
                + Resize(Consent + "/RefuseButton", 643.3365f, 100)
                + Resize(Consent + "/LinkButton", 648, 110)
                + Resize(Consent + "/LinkButton/LinkText", 500, 113.1562f)
                + Resize(Consent + "/PopupText", 685.86f, 256.4032f)
                + Resize(Consent + "/PopupTitle", 700.0364f, 90);
        }

        private int ApplyStretchedContent(string path, float width, float height, float y)
        {
            return Edit<RectTransform>(
                    path,
                    rect =>
                    {
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.anchoredPosition = new Vector2(0, y);
                        rect.sizeDelta = new Vector2(width, height);
                    }
                ) + Edit<Image>(path, image => image.sprite = null);
        }

        private int ApplyDimAndDot()
        {
            return Edit<Image>(Modals + "ModalDim", image => image.color = DimColor)
                + Move("Canvas/TabBar/Group/Advanced/NewFeatureDot", 40.5f, 25);
        }

        private int SetFontSize(string path, float size)
        {
            return Edit<TMP_Text>(path, text => text.fontSize = size);
        }

        private int SetTitle(string path, VerticalAlignmentOptions vertical)
        {
            return SetFontSize(path, 66)
                + Edit<TMP_Text>(
                    path,
                    text =>
                    {
                        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
                        text.verticalAlignment = vertical;
                    }
                );
        }

        private int Move(string path, float x, float y)
        {
            return Edit<RectTransform>(path, rect => rect.anchoredPosition = new Vector2(x, y));
        }

        private int Resize(string path, float width, float height)
        {
            return Edit<RectTransform>(path, rect => rect.sizeDelta = new Vector2(width, height));
        }

        private int Place(string path, float x, float y, float width, float height)
        {
            return Move(path, x, y) + Resize(path, width, height);
        }

        private int Edit<T>(string path, Action<T> change)
            where T : Component
        {
            T component = FindInScene(path)?.GetComponent<T>();
            if (component == null)
            {
                missing.Add($"{path} ({typeof(T).Name})");
                return 0;
            }

            Undo.RecordObject(component, UndoName);
            change(component);
            return 1;
        }

        private static Transform FindInScene(string path)
        {
            int rootEnd = path.IndexOf('/');
            string rootName = path.Substring(0, rootEnd);
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    return root.transform.Find(path.Substring(rootEnd + 1));
                }
            }
            return null;
        }
    }
}
