using System.Collections;
using TexasHoldEmShared.Enums;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class CardClass : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
        [SerializeField] private Text cardRank;
        [SerializeField] private GameObject cover;
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Image highlightImage;
        
        public CardData CardData { get; private set; }

        public void Initialize(CardData data, bool isOwnCardOrCommunity)
        {
            CardData = data;
            StartCoroutine(LoadFromResourcesFolder(CardData.Suit));
            cardRank.text = CardData.Rank.GetDescription();
            cover.SetActive(!isOwnCardOrCommunity);
        }

        public void UpdateCard(CardData newCardData)
        {
            CardData = newCardData;
            StartCoroutine(LoadFromResourcesFolder(CardData.Suit));
            cardRank.text = CardData.Rank.GetDescription();
        }
        
        public void HighlightCard()
        {
            highlightObject.SetActive(true);
        }

        public void ShowCard()
        {
            cover.SetActive(false);
        }

        public void Clear()
        {
            CardData = null;
            cardRank.text = "";
            cover.SetActive(false);
            cardImage.sprite = null;
        }
        
        private IEnumerator LoadFromResourcesFolder(Enums.CardSuitEnum suit)
        {
            var loadAsync = Resources.LoadAsync($"Suits/{suit}", typeof(Sprite));
            
            while (!loadAsync.isDone)
            {
                yield return null;
            }

            if (loadAsync.asset != null)
            {
                cardImage.sprite = loadAsync.asset as Sprite;
            }
        }
    }
}
