using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class SurveyModal : ModalScreen
    {
        protected override void Awake()
        {
            base.Awake();

            ButtonWiring.Replace(Find<Button>("SurveyBox/Panel/Ok"), OnYesButtonClick);
            ButtonWiring.Replace(Find<Button>("SurveyBox/Panel/Later"), () => Navigator.GetManager().Back());
        }

        void OnYesButtonClick()
        {
            Application.OpenURL(Const.SURVEY_URL);
            PlayerPrefs.SetInt(Const.PREF_SURVEY_V1_DONE, 1);
            PlayerPrefs.Save();
            Navigator.GetManager().Back();
        }
    }
}
