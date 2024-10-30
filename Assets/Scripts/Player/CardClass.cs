using THE.MagicOnion.Shared.Entities;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class CardClass : MonoBehaviour
    {
        [SerializeField] private Text cardSuit;
        [SerializeField] private Text cardRank;

        public void Initialize(CardSuitEnum suit, CardRankEnum rank)
        {
            cardSuit.text = suit.ToString();
            cardRank.text = rank.GetDescription();
        }
    }
}
