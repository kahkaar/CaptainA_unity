using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class InfoScreen : AppScreen
    {
        [SerializeField]
        private Button backButton;

        [Tooltip("Opens the secret code modal.")]
        [SerializeField]
        private Button secretButton;

        public override ScreenLayer Layer => ScreenLayer.Overlay;

        protected override void Bind()
        {
            ButtonWiring.Replace(backButton, () => Navigator.GetManager().Back());
            ButtonWiring.Replace(secretButton, () => Navigator.GetManager().Open<SecretModal>());
        }
    }
}
