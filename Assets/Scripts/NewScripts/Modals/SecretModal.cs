using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class SecretModal : ModalScreen
    {
        [SerializeField]
        TextMeshProUGUI secretTextTMP;

        [SerializeField]
        Button okButton;

        [SerializeField]
        Button laterButton;

        protected override void Bind()
        {
            ButtonWiring.Replace(okButton, OnYesButtonClick);
            ButtonWiring.Replace(laterButton, () => Navigator.GetManager().Back());
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
