using System.Collections.Generic;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class PlayerClass : MonoBehaviour
    {
        [SerializeField] private Text role;
        [SerializeField] private Text dealer;
        [SerializeField] private List<CardClass> cardList;
        [SerializeField] private GameObject cover;
        
        private GamingHubReceiver gamingHubReceiver;

        public void Initialize(PlayerEntity player)
        {
            cover.gameObject.SetActive(player.Name != gamingHubReceiver.GetSelf().Name);
            dealer.gameObject.SetActive(player.IsDealer);
            if (player.PlayerRole != PlayerRoleEnum.None)
            {
                role.text = player.PlayerRole == PlayerRoleEnum.SmallBlind
                    ? "SB"
                    : "BB";
            }
            
            cardList[0].Initialize(player.CardHand[0].Suit, player.CardHand[0].Rank);
            cardList[1].Initialize(player.CardHand[1].Suit, player.CardHand[1].Rank);
            
        }
    }
}
