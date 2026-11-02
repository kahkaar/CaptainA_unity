using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaySuomi.Navigation
{
    /// <summary>Marks an object on FlashcardsScreen as a deck, and says which one.</summary>
    public sealed class FlashcardDeck : MonoBehaviour
    {
        [SerializeField]
        string deckName;

        [SerializeField]
        bool isSecret;

        [SerializeField]
        Image progressBar;

        [SerializeField]
        TextMeshProUGUI progressText;

        [SerializeField]
        TextMeshProUGUI cardInfoText;

        /// <summary>The name SaveData loads and saves this deck under.</summary>
        public string DeckName => deckName;

        /// <summary>True for a deck only shown once the secret text has been entered.</summary>
        public bool IsSecret => isSecret;

        public Image ProgressBar => progressBar;

        public TextMeshProUGUI ProgressText => progressText;

        public TextMeshProUGUI CardInfoText => cardInfoText;
    }
}
