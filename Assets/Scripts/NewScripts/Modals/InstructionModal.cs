using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>
    /// The first-time instruction and what's-new popup. Added to PopUpPanel by Navigator, so it
    /// needs no scene changes. Only OK marks it as seen: leaving any other way shows it again
    /// on the next visit.
    /// </summary>
    public sealed class InstructionModal : ModalScreen
    {
        TextMeshProUGUI messageText;
        string prefsKey;
        int saveInt;

        protected override void Bind()
        {
            messageText = Find<TextMeshProUGUI>("TextPanel/MessageText");
            ButtonWiring.Replace(Find<Button>("TextPanel/OkButton"), OnOkClicked);
        }

        public static void Show(string prefsKey, string message, int saveInt = 1)
        {
            // A screen active in the scene enables before Navigator exists. Navigator enables
            // the home tab again once it is ready, which asks again.
            Navigator navigator = Navigator.GetManager();
            if (navigator == null)
            {
                return;
            }

            InstructionModal modal = navigator.Open<InstructionModal>();
            modal.prefsKey = prefsKey;
            modal.saveInt = saveInt;
            modal.messageText.text = message;
        }

        void OnOkClicked()
        {
            PlayerPrefs.SetInt(prefsKey, saveInt);
            PlayerPrefs.Save();
            Navigator.GetManager().Back();
        }
    }
}
