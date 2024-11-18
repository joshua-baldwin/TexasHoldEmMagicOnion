using System;
using System.Collections.Generic;
using System.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Shared.Entities;

namespace THE.Player
{
    public class PlayerData
    {
        public string Name { get; private set; }
        public Guid Id { get; private set; }
        public Enums.PlayerRoleEnum PlayerRole { get; set; }
        public Guid RoomId { get; set; }
        public bool IsDealer { get; set; }
        public List<CardData> HoleCards { get; set; }
        public List<ChipEntity> Chips { get; set; }
        public int CurrentBet { get; set; }
        public bool CanSelectCard { get; set; }
        
        public PlayerData(PlayerEntity playerEntity)
        {
            Name = playerEntity.Name;
            Id = playerEntity.Id;
            PlayerRole = playerEntity.PlayerRole;
            RoomId = playerEntity.RoomId;
            IsDealer = playerEntity.IsDealer;
            if (playerEntity.HoleCards[0] != null && playerEntity.HoleCards[1] != null)
                HoleCards = playerEntity.HoleCards.Select(c => new CardData(c)).ToList();
            Chips = playerEntity.Chips;
            CurrentBet = playerEntity.CurrentBet;
        }
    }
}