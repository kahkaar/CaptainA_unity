using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class SecretModal : ModalScreen
    {
        [Tooltip("The code field's text object, which holds what the learner typed.")]
        [SerializeField]
        private TextMeshProUGUI secretTextTMP;

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
            string secretText = secretTextTMP.text;
            secretText = secretText.Replace("\r", "").Replace("\n", "").Trim();
            PlayerPrefs.SetString(Const.PREF_SECRET_TEXT, secretText);
            PlayerPrefs.Save();
            Navigator.GetManager().Back();
        }
    }
}
