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
        [SerializeField] private Image cardImage;
        [SerializeField] private Text cardRank;
        [SerializeField] private GameObject cover;

        public void Initialize(CardSuitEnum suit, CardRankEnum rank, bool isOwnCard)
        {
            StartCoroutine(LoadFromResourcesFolder(suit));
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
