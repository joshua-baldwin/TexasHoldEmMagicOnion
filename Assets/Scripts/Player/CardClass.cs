using System;
using System.Collections;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.SceneControllers;
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
        private GamingHubReceiver gamingHubReceiver;
        
        public CardData CardData { get; private set; }
        public Action<bool, CardData> CardSelectedAction;

        private void Awake()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            selectButton.onClick.AddListener(() =>
            {
                if (!gamingHubReceiver.Self.CanSelectCard && !isSelected)
                    return;
                
                if (isSelected)
                    DeselectCard();
                else
                    SelectCard();
            });
            selectButton.interactable = false;
        }

        public void Initialize(CardData data, bool isOwnCardOrCommunity, Action<bool, CardData> onCardSelected)
        {
            CardData = data;
            CardSelectedAction = onCardSelected;
            StartCoroutine(LoadFromResourcesFolder(CardData.Suit));
            cardRank.text = CardData.Rank.GetDescription();
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
            CardSelectedAction?.Invoke(true, CardData);
        }

        public void DeselectCard()
        {
            highlightImage.color = Color.yellow;
            isSelected = false;
            CardSelectedAction?.Invoke(false, CardData);
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
