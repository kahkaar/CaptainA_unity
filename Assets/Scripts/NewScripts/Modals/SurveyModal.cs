using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class SurveyModal : ModalScreen
    {
        [SerializeField]
        private Button okButton;

        [SerializeField]
        private Button laterButton;

        protected override void Bind()
        {
            ButtonWiring.Replace(okButton, OnYesButtonClick);
            ButtonWiring.Replace(laterButton, () => Navigator.GetManager().Back());
        }

        private void OnYesButtonClick()
        {
            Application.OpenURL(Const.SURVEY_URL);
            PlayerPrefs.SetInt(Const.PREF_SURVEY_V1_DONE, 1);
            PlayerPrefs.Save();
            Navigator.GetManager().Back();
        }
    }
}
