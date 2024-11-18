using System;
using System.Collections;
using TexasHoldEmShared.Enums;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class CardClass : MonoBehaviour
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private Image cardImage;
        [SerializeField] private Text cardRank;
        [SerializeField] private GameObject cover;
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Image highlightImage;

        private bool isSelected;

        private void Awake()
        {
            selectButton.onClick.AddListener(() =>
            {
                if (isSelected)
                    DeselectCard();
                else
                    SelectCard();
            });
            selectButton.interactable = false;
        }

        public void Initialize(Enums.CardSuitEnum suit, Enums.CardRankEnum rank, bool isOwnCardOrCommunity)
        {
            StartCoroutine(LoadFromResourcesFolder(suit));
            cardRank.text = rank.GetDescription();
            cover.SetActive(!isOwnCardOrCommunity);
        }
        
        public void HighlightCard()
        {
            highlightObject.SetActive(true);
            selectButton.interactable = true;
        }

        public void SelectCard()
        {
            highlightImage.color = Color.green;
            isSelected = true;
        }

        public void DeselectCard()
        {
            highlightImage.color = Color.yellow;
            isSelected = false;
        }

        public void ShowCard()
        {
            cover.SetActive(false);
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
