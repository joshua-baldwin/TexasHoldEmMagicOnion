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
        public int Chips { get; }
        public int CurrentBet { get; }
        public Enums.CommandTypeEnum LastCommand { get; }
        public int RaiseAmount { get; }
        public bool HasFolded { get; set; }
        
        public PlayerData(PlayerEntity playerEntity)
        {
            Name = playerEntity.Name;
            Id = playerEntity.Id;
            PlayerRole = playerEntity.PlayerRole;
            RoomId = playerEntity.RoomId;
            IsDealer = playerEntity.IsDealer;
            if (playerEntity.HoleCards != null)
                HoleCards = playerEntity.HoleCards.Select(c => new CardData(c)).ToList();
            Chips = playerEntity.Chips;
            CurrentBet = playerEntity.CurrentBet;
            LastCommand = playerEntity.LastCommand;
            RaiseAmount = playerEntity.RaiseAmount;
            HasFolded = playerEntity.HasFolded;
        }
    }
}