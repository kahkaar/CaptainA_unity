using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace SaySuomi.Navigation
{
    public class ASATaskListScreen : MonoBehaviour
    {
        [SerializeField]
        GameObject advancePanelGO;

        [SerializeField]
        GameObject taskPanelGO;

        [SerializeField]
        GameObject ASAPanelGO;

        [SerializeField]
        GameObject backButtonGO;

        [SerializeField]
        GameObject task1ButtonGO;

        [SerializeField]
        GameObject task2ButtonGO;

        [SerializeField]
        GameObject task3ButtonGO;

        [SerializeField]
        GameObject task4ButtonGO;

        [SerializeField]
        GameObject task5ButtonGO;

        [SerializeField]
        [FormerlySerializedAs("ASAPanel")]
        ASATaskScreen ASATaskScreen;

        [SerializeField]
        GameObject dimPanelGO;

        [SerializeField]
        GameObject profileButtonGO;

        [SerializeField]
        GameObject profilePanelGO;

        [SerializeField]
        GameObject loadingIconGO;

        [SerializeField]
        GameObject profileLoadingBackButtonGO;

        [SerializeField]
        GameObject profileLoadingErrorTextGO;

        [SerializeField]
        GameObject loadingPopUpGO;

        [SerializeField]
        GameObject errorPopupGO;

        [SerializeField]
        GameObject overallFeedbackPopUpGO;

        [SerializeField]
        GameObject feedbackSendButtonGO;

        [SerializeField]
        public TMP_InputField overallFeedbackTextGO;

        [SerializeField]
        GameObject feedbackBackButtonGO;

        public bool isLoading = false;

        private LocalizedTextHelper profileErrorHelper;

        public ToggleGroup overallRatingOptions;

        public void StopErrorAnimation()
        {
            Animator anim = errorPopupGO.GetComponent<Animator>();
            anim.enabled = false;
            errorPopupGO.SetActive(false);
        }

        void OnEnable()
        {
            // New Task leaves the finished task running behind this list, where it would take
            // the next back press for itself with nothing visible happening.
            ASAPanelGO.SetActive(false);

            // Stops animated dropdown error message from appearing
            StopErrorAnimation();

            isLoading = false;
            Debug.Log(PlayerPrefs.GetInt("TasksSent"));
            // Ask general feedback if number of completed tasks so far is a multiple of 5
            OverallFeedback();

            // Set proper game objects active and establish button behavior.
            //
            // Every button goes through ButtonUtils.Rewire, which clears the previous
            // listener before adding this one. OnEnable runs on every reopen, so plain
            // AddListener stacked: three of these used to be cleared by hand at the top of
            // this method and the rest were not, which held only because the rest happen to be
            // idempotent. Rewire makes that hold by construction instead of by luck.
            ButtonUtils.Rewire(backButtonGO, LeaveToAdvanced);

            ButtonUtils.Rewire(task1ButtonGO, () => OpenTask(0));
            ButtonUtils.Rewire(task2ButtonGO, () => OpenTask(1));
            ButtonUtils.Rewire(task3ButtonGO, () => OpenTask(2));
            ButtonUtils.Rewire(task4ButtonGO, () => OpenTask(3));
            ButtonUtils.Rewire(task5ButtonGO, () => OpenTask(4));

            ButtonUtils.Rewire(profileButtonGO, LoadProfile);

            ButtonUtils.Rewire(profileLoadingBackButtonGO, () => OnProfileLoadingBackButtonClicked());

            ButtonUtils.Rewire(
                feedbackSendButtonGO,
                () =>
                {
                    var overallRating = overallRatingOptions.ActiveToggles().FirstOrDefault();
                    string comment_overall = overallFeedbackTextGO.text;
                    // Show drop-down error animation if attempting to send feedback without selecting an emoji
                    if (overallRating == null)
                    {
                        Animator anim = errorPopupGO.GetComponent<Animator>();

                        errorPopupGO.SetActive(true);

                        anim.enabled = true;

                        anim.Play("Error Popup Animation");

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
                    // Reset feedback form when done
                    MarkOverallFeedbackAsked();
                    overallRatingOptions.SetAllTogglesOff();
                    overallFeedbackTextGO.text = "";
                    overallFeedbackPopUpGO.SetActive(false);
                    dimPanelGO.SetActive(false);
                    errorPopupGO.SetActive(false);
                }
            );

            ButtonUtils.Rewire(feedbackBackButtonGO, DeclineOverallFeedback);
        }

        private void LeaveToAdvanced()
        {
            advancePanelGO.SetActive(true);
            taskPanelGO.SetActive(false);
        }

        private void DeclineOverallFeedback()
        {
            // Declining restarts the count, exactly as answering does. It used to only
            // hide the window and write nothing, so every return to the task list
            // reopened it and the only ways out were sending feedback or completing
            // another task.
            MarkOverallFeedbackAsked();

            overallRatingOptions.SetAllTogglesOff();
            overallFeedbackTextGO.text = "";
            overallFeedbackPopUpGO.SetActive(false);
            dimPanelGO.SetActive(false);
            errorPopupGO.SetActive(false);
        }

        /// <summary>
        /// Back does what the visible back control does, and nothing when there is none: the
        /// loading popup only offers Back once its request has failed. Temporary: goes in
        /// Stage 5, when these join the stack.
        /// </summary>
        public void OnBackPressed()
        {
            if (loadingPopUpGO.activeSelf)
            {
                if (profileLoadingBackButtonGO.activeSelf)
                {
                    OnProfileLoadingBackButtonClicked();
                }

                return;
            }

            if (overallFeedbackPopUpGO.activeSelf)
            {
                DeclineOverallFeedback();
                return;
            }

            LeaveToAdvanced();
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
        private void MarkOverallFeedbackAsked()
        {
            PlayerPrefs.SetInt(
                Const.PREF_OVERALL_FEEDBACK_ASKED_AT,
                PlayerPrefs.GetInt("TasksSent", 0)
            );
            PlayerPrefs.Save();
        }

        // The five task buttons differ only by index.
        private void OpenTask(int taskIndex)
        {
            ASATaskScreen.DisplayTask(taskIndex);
            ASAPanelGO.SetActive(true);
            taskPanelGO.SetActive(false);
        }

        public void OnProfileLoadingBackButtonClicked()
        {
            loadingPopUpGO.SetActive(false);
            dimPanelGO.SetActive(false);
        }

        /// <summary>
        /// Fetches the profile behind the loading popup, then opens it. Also used by
        /// ASAProfileScreen to reload after a level change.
        /// </summary>
        public void LoadProfile()
        {
            ShowProfileLoading();

            StartCoroutine(
                NetworkManager
                    .GetManager()
                    .ServerPost_profile(
                        POSTType.ASA_PROFILE,
                        json =>
                        {
                            if (json == null)
                            {
                                ShowProfileError();
                                return;
                            }

                            loadingPopUpGO.SetActive(false);
                            profilePanelGO.SetActive(true);
                            dimPanelGO.SetActive(false);
                            isLoading = false;

                            profilePanelGO.GetComponent<ASAProfileScreen>().ShowStats(json);
                        }
                    )
            );
        }

        public void ShowProfileLoading()
        {
            loadingPopUpGO.SetActive(true);
            loadingIconGO.SetActive(true);
            profileLoadingErrorTextGO.SetActive(false);
            profileLoadingBackButtonGO.SetActive(false);
            dimPanelGO.SetActive(true);
            isLoading = true;
        }

        /// <summary>Shows the last failed request on the loading popup, with its back button.</summary>
        public void ShowProfileError()
        {
            isLoading = false;
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

        public void OverallFeedback()
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
                overallFeedbackPopUpGO.SetActive(true);
                dimPanelGO.SetActive(true);
            }
        }

        public void ReturnToTaskPanel()
        {
            isLoading = false;
            loadingPopUpGO.SetActive(true);
        }

        void Update()
        {
            // For updating different animations depending on state
            if (isLoading)
            {
                AnimateLoadingProfile();
            }
        }

        public void AnimateLoadingProfile()
        {
            // Change value -6.0f to change rotation speed
            loadingIconGO.transform.Rotate(0, 0, -6.0f, Space.Self);
        }
    }
}
