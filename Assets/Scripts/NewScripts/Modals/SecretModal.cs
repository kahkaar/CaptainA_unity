using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class SecretModal : ModalScreen
    {
        TextMeshProUGUI secretTextTMP;

        protected override void Bind()
        {
            secretTextTMP = Find<TextMeshProUGUI>("SurveyBox/Text (TMP)/secretInput/Text Area/Text");
            ButtonWiring.Replace(Find<Button>("SurveyBox/Panel/Ok"), OnYesButtonClick);
            ButtonWiring.Replace(Find<Button>("SurveyBox/Panel/Later"), () => Navigator.GetManager().Back());
        }

        void OnYesButtonClick()
        {
            string secretText = secretTextTMP.text;
            secretText = secretText.Replace("\r", "").Replace("\n", "").Trim();
            PlayerPrefs.SetString(Const.PREF_SECRET_TEXT, secretText);
            PlayerPrefs.Save();
            Navigator.GetManager().Back();
        }
    }
}
