using System.Collections;
using System.Collections.Generic;
using THE.MagicOnion.Shared.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class PlayerClass : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text role;
        [SerializeField] private Text dealer;
        [SerializeField] private List<CardClass> cardList;

        public void Initialize(PlayerEntity player)
        {
            userName.text = player.Name;
            role.gameObject.SetActive(player.IsDealer);
            if (player.PlayerRole != PlayerRoleEnum.None)
            {
                role.text = player.PlayerRole == PlayerRoleEnum.SmallBlind
                    ? "SB"
                    : "BB";
            }
            
        }
    }
}
