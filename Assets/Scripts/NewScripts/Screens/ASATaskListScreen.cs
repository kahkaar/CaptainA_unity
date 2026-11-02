using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class ASATaskListScreen : AppScreen
    {
        const string OverallPopUp = "OverallFeedbackPopUp/";

        GameObject errorPopupGO;
        GameObject loadingIconGO;
        GameObject profileLoadingBackButtonGO;
        GameObject profileLoadingErrorTextGO;
        TMP_InputField overallFeedbackTextGO;
        ToggleGroup overallRatingOptions;

        LoadingModal profileLoadingModal;
        ModalScreen overallFeedbackModal;

        LocalizedTextHelper profileErrorHelper;

        protected override void Awake()
        {
            base.Awake();

            errorPopupGO = FindObject("ErrorPopup");
            loadingIconGO = FindObject("LoadingPopUpProfile/LoadingIcon");
            profileLoadingBackButtonGO = FindObject("LoadingPopUpProfile/BackButton");
            profileLoadingErrorTextGO = FindObject("LoadingPopUpProfile/ErrorText");
            overallFeedbackTextGO = Find<TMP_InputField>(OverallPopUp + "OverallFeedbackTextField");
            overallRatingOptions = Find<ToggleGroup>(OverallPopUp + "OverallRatingGroup");

            Navigator navigator = Navigator.GetManager();

            ButtonWiring.Replace(Find<Button>("BackButton"), () => navigator.Back());
            ButtonWiring.Replace(Find<Button>("Task1"), () => OpenTask(0));
            ButtonWiring.Replace(Find<Button>("Task2"), () => OpenTask(1));
            ButtonWiring.Replace(Find<Button>("Task3"), () => OpenTask(2));
            ButtonWiring.Replace(Find<Button>("Task4"), () => OpenTask(3));
            ButtonWiring.Replace(Find<Button>("Task5"), () => OpenTask(4));
            ButtonWiring.Replace(Find<Button>("ProfileButton"), LoadProfile);
            ButtonWiring.Replace(Find<Button>("LoadingPopUpProfile/BackButton"), () => navigator.Back());
            ButtonWiring.Replace(Find<Button>(OverallPopUp + "FeedbackSendButton"), SendOverallFeedback);
            ButtonWiring.Replace(Find<Button>(OverallPopUp + "FeedbackBackButton"), () => navigator.Back());

            profileLoadingModal = ModalScreen.Attach<LoadingModal>(
                FindObject("LoadingPopUpProfile"),
                dismissible: false
            );

            overallFeedbackModal = ModalScreen.Attach(FindObject("OverallFeedbackPopUp"));

            // Declining restarts the count, exactly as answering does. It used to only
            // hide the window and write nothing, so every return to the task list
            // reopened it and the only ways out were sending feedback or completing
            // another task.
            overallFeedbackModal.OnBack = () =>
            {
                MarkOverallFeedbackAsked();
                overallRatingOptions.SetAllTogglesOff();
                overallFeedbackTextGO.text = "";
                ErrorBanner.Hide(errorPopupGO);
                return false;
            };
        }

        void OnEnable()
        {
            // Stops animated dropdown error message from appearing
            ErrorBanner.Hide(errorPopupGO);

            Debug.Log(PlayerPrefs.GetInt("TasksSent"));
            // Ask general feedback if number of completed tasks so far is a multiple of 5
            OverallFeedback();
        }

        void SendOverallFeedback()
        {
            var overallRating = overallRatingOptions.ActiveToggles().FirstOrDefault();
            string comment_overall = overallFeedbackTextGO.text;

            // Show drop-down error animation if attempting to send feedback without selecting an emoji
            if (overallRating == null)
            {
                ErrorBanner.Show(errorPopupGO);
                return;
            }

            // Send feedback to server
            StartCoroutine(
                NetworkManager
                    .GetManager()
                    .ServerPost_feedback(
                        POSTType.USER_ASA_FEEDBACK,
                        "overall_experience",
                        overallRating.name,
                        comment_overall
                    )
            );

            // OnBack resets the form and records that the learner was asked.
            Navigator.GetManager().Back();
        }

        /// <summary>
        /// Restarts the countdown to the next overall-feedback ask.
        ///
        /// Called for both Send and Cancel: what is recorded is that the learner was asked at
        /// this point, not what they said. That is enough to pace the question, and it means
        /// declining is respected for a full interval rather than until the next task.
        ///
        /// Saved immediately - a decision lost to the app being killed is a learner who gets
        /// asked again anyway, which is the behaviour this exists to stop.
        /// </summary>
        void MarkOverallFeedbackAsked()
        {
            PlayerPrefs.SetInt(
                Const.PREF_OVERALL_FEEDBACK_ASKED_AT,
                PlayerPrefs.GetInt("TasksSent", 0)
            );
            PlayerPrefs.Save();
        }

        // The five task buttons differ only by index.
        void OpenTask(int taskIndex)
        {
            Navigator.GetManager().Push<ASATaskScreen>().DisplayTask(taskIndex);
        }

        /// <summary>
        /// Fetches the profile behind the loading popup, then opens it. Also used by
        /// ASAProfileScreen to reload after a level change, when it is already open.
        ///
        /// Runs on NetworkManager, because this screen is hidden during that reload and a
        /// disabled object cannot run a coroutine. The modal stops the learner leaving while
        /// it waits, so nothing can be closed underneath the response.
        /// </summary>
        public void LoadProfile()
        {
            ShowProfileLoading();

            NetworkManager network = NetworkManager.GetManager();

            network.StartCoroutine(
                network.ServerPost_profile(
                    POSTType.ASA_PROFILE,
                    json =>
                    {
                        if (json == null)
                        {
                            ShowProfileError();
                            return;
                        }

                        Navigator navigator = Navigator.GetManager();

                        profileLoadingModal.Dismissible = true;
                        navigator.Back();

                        ASAProfileScreen profile = navigator.Get<ASAProfileScreen>();
                        if (!navigator.IsOpen(profile))
                        {
                            navigator.Push<ASAProfileScreen>();
                        }

                        profile.ShowStats(json);
                    }
                )
            );
        }

        public void ShowProfileLoading()
        {
            profileLoadingModal.Dismissible = false;
            loadingIconGO.SetActive(true);
            profileLoadingErrorTextGO.SetActive(false);
            profileLoadingBackButtonGO.SetActive(false);
            Navigator.GetManager().Open(profileLoadingModal);
        }

        /// <summary>Shows the last failed request on the loading popup, with its back button.</summary>
        public void ShowProfileError()
        {
            profileLoadingModal.Dismissible = true;
            loadingIconGO.SetActive(false);
            profileLoadingErrorTextGO.SetActive(true);
            profileLoadingBackButtonGO.SetActive(true);

            profileErrorHelper ??= new LocalizedTextHelper(
                profileLoadingErrorTextGO.GetComponent<TextMeshProUGUI>()
            );
            ServerNotices.Show(profileErrorHelper, NetworkManager.GetManager().lastServerError);
        }

        void OnDestroy()
        {
            profileErrorHelper?.ClearText();
        }

        void OverallFeedback()
        {
            // Ask for overall feedback once Const.ASA_OVERALL_FEEDBACK_EVERY tasks have been
            // completed since the last time we asked.
            //
            // Deliberately >= rather than landing on a multiple. The check only runs when the
            // task list opens, so an exact test silently skipped anyone who finished their
            // tenth task and went straight on to an eleventh - the one moment they were owed
            // the question was the one moment nobody was looking. A backlog cannot be missed:
            // it just waits until they next open the list.
            int tasksSent = PlayerPrefs.GetInt("TasksSent", 0);
            int askedAt = PlayerPrefs.GetInt(Const.PREF_OVERALL_FEEDBACK_ASKED_AT, 0);

            if (tasksSent - askedAt >= Const.ASA_OVERALL_FEEDBACK_EVERY)
            {
                Navigator.GetManager().Open(overallFeedbackModal);
            }
        }
    }
}
