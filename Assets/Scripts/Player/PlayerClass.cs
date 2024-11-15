using System;
using System.Collections.Generic;
using System.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.SceneControllers;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class PlayerClass : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Image nameColor;
        [SerializeField] private Text role;
        [SerializeField] private Text dealer;
        [SerializeField] private GameObject cardListRoot;
        [SerializeField] private List<CardClass> cardList;
        [SerializeField] private Text currentBetText;
        
        private GamingHubReceiver gamingHubReceiver;
        public Guid PlayerId;
        private bool cardsInitialized;

        public void Initialize(PlayerEntity player)
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            if (player.Id == gamingHubReceiver.Self.Id)
                nameColor.color = Color.green;
            PlayerId = player.Id;
            nameText.text = player.Name;
            dealer.gameObject.SetActive(player.IsDealer);
            if (player.PlayerRole != Enums.PlayerRoleEnum.None)
            {
                role.text = player.PlayerRole == Enums.PlayerRoleEnum.SmallBlind
                    ? "SB"
                    : "BB";
            }
        }

        public void InitializeCards()
        {
            if (cardsInitialized)
                return;
            
            ChangeCardVisibility(true);
            var playerEntity = MySceneManager.Instance.HubReceiver.GetPlayerList().First(x => x.Id == PlayerId);
            var isSelf = playerEntity.Id == gamingHubReceiver.Self.Id;
            cardList[0].Initialize(playerEntity.HoleCards[0].Suit, playerEntity.HoleCards[0].Rank, isSelf);
            cardList[1].Initialize(playerEntity.HoleCards[1].Suit, playerEntity.HoleCards[1].Rank, isSelf);
            cardsInitialized = true;
        }

        public void UpdateBet(int betAmount)
        {
            currentBetText.text = $"Current bet: {betAmount}";
        }

        public void ChangeCardVisibility(bool visible)
        {
            cardListRoot.SetActive(visible);
        }
    }
}
