using System;
using System.Collections.Generic;
using System.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.SceneControllers;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class PlayerClass : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text chipsText;
        [SerializeField] private Image nameColor;
        [SerializeField] private Text role;
        [SerializeField] private Text dealer;
        [SerializeField] private GameObject cardListRoot;
        [SerializeField] private List<CardClass> cardList;
        [SerializeField] private GameObject foldCover;
        
        private GamingHubReceiver gamingHubReceiver;
        public PlayerData PlayerData { get; private set; }
        private bool cardsInitialized;

        public void Initialize(PlayerData player)
        {
            cardsInitialized = false;
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            if (player.Id == gamingHubReceiver.Self.Id)
                nameColor.color = Color.green;
            PlayerData = player;
            nameText.text = player.Name;
            chipsText.text = $"Chips: {player.Chips}";
            //chipsText.text = ClientUtilityMethods.GetChipText(player.Chips);
            dealer.gameObject.SetActive(player.IsDealer);
            role.gameObject.SetActive(player.PlayerRole != Enums.PlayerRoleEnum.None);
            foldCover.SetActive(false);
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
            var playerEntity = MySceneManager.Instance.HubReceiver.GetPlayerList().First(x => x.Id == PlayerData.Id);
            var isSelf = playerEntity.Id == gamingHubReceiver.Self.Id;
            cardList[0].Initialize(playerEntity.HoleCards[0], isSelf);
            cardList[1].Initialize(playerEntity.HoleCards[1], isSelf);
            cardsInitialized = true;
        }

        public void UpdatePlayerUi(PlayerData playerData)
        {
            chipsText.text = $"Chips: {playerData.Chips}";
            if (playerData.HasFolded)
                foldCover.SetActive(true);
            //chipsText.text = ClientUtilityMethods.GetChipText(playerData.Chips);
        }

        public void ChangeCardVisibility(bool visible)
        {
            cardListRoot.SetActive(visible);
        }
        
        public void HighlightCards()
        {
            cardList.ForEach(card => card.HighlightCard());
        }

        public void ShowCards()
        {
            cardList.ForEach(card => card.ShowCard());
        }
    }
}
