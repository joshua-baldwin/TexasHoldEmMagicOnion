using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using THE.MagicOnion.Shared.Entities;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class CardClass : MonoBehaviour
    {
        [SerializeField] private Text cardSuit;
        [SerializeField] private Image cardImage;
        [SerializeField] private Text cardRank;
        [SerializeField] private GameObject cover;

        public void Initialize(CardSuitEnum suit, CardRankEnum rank, bool isOwnCard)
        {
            StartCoroutine(LoadFromResourcesFolder(suit));
            cardSuit.text = suit switch
            {
                CardSuitEnum.Heart => "\u2764\ufe0f",
                CardSuitEnum.Spade => "\u2660\ufe0f",
                CardSuitEnum.Diamond => "\u2666\ufe0f",
                CardSuitEnum.Club => "\u2663\ufe0f",
                _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null)
            };
            
            cardRank.text = rank.GetDescription();
            cover.SetActive(!isOwnCard);
        }
        
        private IEnumerator LoadFromResourcesFolder(CardSuitEnum suit)
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
