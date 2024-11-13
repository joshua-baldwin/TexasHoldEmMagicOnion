using System;
using System.Collections.Generic;
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
        [SerializeField] private Text role;
        [SerializeField] private Text dealer;
        [SerializeField] private GameObject cardListObject;
        [SerializeField] private List<CardClass> cardList;
        [SerializeField] private Text currentBetText;
        
        private GamingHubReceiver gamingHubReceiver;
        public Guid PlayerId;

        public void Initialize(PlayerEntity player)
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            PlayerId = player.Id;
            dealer.gameObject.SetActive(player.IsDealer);
            if (player.PlayerRole != Enums.PlayerRoleEnum.None)
            {
                role.text = player.PlayerRole == Enums.PlayerRoleEnum.SmallBlind
                    ? "SB"
                    : "BB";
            }

            var isSelf = player.Id == gamingHubReceiver.Self.Id;
            cardList[0].Initialize(player.CardHand[0].Suit, player.CardHand[0].Rank, isSelf);
            cardList[1].Initialize(player.CardHand[1].Suit, player.CardHand[1].Rank, isSelf);
            UpdateBet(player.CurrentBet);
        }

        public void UpdateBet(int betAmount)
        {
            currentBetText.text = $"Current bet: {betAmount}";
        }

        public void ChangeCardVisibility(bool visible)
        {
            cardListObject.SetActive(visible);
        }
    }
}
