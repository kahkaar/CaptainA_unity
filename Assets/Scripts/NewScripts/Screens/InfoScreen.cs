using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    public class InfoScreen : AppScreen
    {
        public override ScreenLayer Layer => ScreenLayer.Overlay;

        protected override void Bind()
        {
            ButtonWiring.Replace(Find<Button>("AboutTextBG/BackButton"), () => Navigator.GetManager().Back());
            ButtonWiring.Replace(
                Find<Button>("AboutTextBG/SecretButton"),
                () => Navigator.GetManager().Open<SecretModal>()
            );
        }
    }
}
