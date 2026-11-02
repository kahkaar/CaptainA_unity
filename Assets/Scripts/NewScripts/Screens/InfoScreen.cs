using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class InfoScreen : AppScreen
    {
        public override ScreenLayer Layer => ScreenLayer.Overlay;

        [SerializeField]
        Button backButton;

        [SerializeField]
        Button secretButton;

        protected override void Bind()
        {
            ButtonWiring.Replace(backButton, () => Navigator.GetManager().Back());
            ButtonWiring.Replace(
                secretButton,
                () => Navigator.GetManager().Open<SecretModal>()
            );
        }
    }
}
