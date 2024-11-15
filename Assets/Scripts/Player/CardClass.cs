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

        public void Initialize(Enums.CardSuitEnum suit, Enums.CardRankEnum rank, bool isOwnCardOrCommunity)
        {
            StartCoroutine(LoadFromResourcesFolder(suit));
            cardRank.text = rank.GetDescription();
            cover.SetActive(!isOwnCardOrCommunity);
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
