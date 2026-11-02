using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class PhonemeScreen : AppScreen
    {
        public override ScreenLayer Layer => ScreenLayer.Overlay;

        TextMeshProUGUI explainText;
        Image frontMouth1;
        Image frontMouth2;
        Image sideMouth1;
        Image sideMouth2;
        TextMeshProUGUI creditText;
        Animation mouthAnimation;

        SOPhonemeHelper phonemeSO;

        protected override void Bind()
        {
            explainText = Find<TextMeshProUGUI>("ExplainPanel/ExplainText");
            frontMouth1 = Find<Image>("FrontPanel/FrontMouth");
            frontMouth2 = Find<Image>("FrontPanel/FrontMouth/FrontMouth2");
            sideMouth1 = Find<Image>("SidePanel/SideMouth");
            sideMouth2 = Find<Image>("SidePanel/SideMouth/SideMouth2");
            creditText = Find<TextMeshProUGUI>("CreditPanel/CreditText");
            mouthAnimation = GetComponent<Animation>();

            ButtonWiring.Replace(Find<Button>("backPanel/backButton"), () => Navigator.GetManager().Back());
            ButtonWiring.Replace(Find<Button>("VideoPanel/VideoButton"), OnVideoButtonClick);
        }

        public void Show(string phoneme)
        {
            phonemeSO = Resources.Load<SOPhonemeHelper>(Const.PHONE_HELP_PATH + phoneme.ToLower());

            // In case there is no entry for this phoneme
            if (phonemeSO == null)
            {
                phonemeSO = Resources.Load<SOPhonemeHelper>(Const.PHONE_HELP_PATH + "spacing");
            }

            Display();
        }

        void OnVideoButtonClick()
        {
            Application.OpenURL(phonemeSO.videoURL);
        }

        void OnEnable()
        {
            // Empty the first time, because the caller only gives the phoneme after Push.
            // Set when something on the Screen layer covered this and has now closed.
            if (phonemeSO != null)
            {
                Display();
            }

            if (!PlayerPrefs.HasKey(Const.PREF_INS_PHONE))
            {
                InstructionModal.Show(Const.PREF_INS_PHONE, Const.INSTRUCTION_PHONE);
            }
        }

        void Display()
        {
            string phonemeString = TextUtils.WrapPhonemeSO(phonemeSO, Const.GOOD_COLOR, isBold: false);

            explainText.text = phonemeString + " " + phonemeSO.instruction;
            frontMouth1.sprite = phonemeSO.front_1;
            frontMouth2.sprite = phonemeSO.front_2;
            sideMouth1.sprite = phonemeSO.side_1;
            sideMouth2.sprite = phonemeSO.side_2;
            mouthAnimation.clip = phonemeSO.animaClip;
            mouthAnimation.Play();

            creditText.text =
                "Top images: "
                + phonemeSO.front_credit
                + ".\nBottom images: "
                + phonemeSO.side_credit
                + ".";
        }

        void OnDisable()
        {
            mouthAnimation.clip = null;
        }

        void Update()
        {
            // Set focus to the panel
            // so that the back button won't go back to the URL
            EventSystem.current.SetSelectedGameObject(gameObject, null);
        }
    }
}
