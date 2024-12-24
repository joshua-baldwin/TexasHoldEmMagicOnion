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
        public int OrderInQueue { get; set; }
        public Guid RoomId { get; set; }
        public bool IsDealer { get; set; }
        public List<CardData> HoleCards { get; set; }
        public int Chips { get; }
        public int CurrentBet { get; }
        public bool HasFolded { get; set; }
        public Enums.CommandTypeEnum LastCommand { get; }
        public int RaiseAmount { get; }
        public BestHandEntity CurrentBestHand { get; set; }
        public int AllInAmount { get; set; }
        public List<JokerData> JokerCards { get; set; }
        public List<CardData> TempHoleCards { get; set; }
        
        public PlayerData(PlayerEntity playerEntity)
        {
            Name = playerEntity.Name;
            Id = playerEntity.Id;
            PlayerRole = playerEntity.PlayerRole;
            OrderInQueue = playerEntity.OrderInQueue;
            RoomId = playerEntity.RoomId;
            IsDealer = playerEntity.IsDealer;
            HoleCards = playerEntity.HoleCards.Select(c => new CardData(c)).ToList();
            Chips = playerEntity.Chips;
            CurrentBet = playerEntity.CurrentBet;
            HasFolded = playerEntity.HasFolded;
            LastCommand = playerEntity.LastCommand;
            RaiseAmount = playerEntity.RaiseAmount;
            CurrentBestHand = playerEntity.CurrentBestHand;
            AllInAmount = playerEntity.AllInAmount;
            JokerCards = playerEntity.JokerCards.Select(j => new JokerData(j)).ToList();
            TempHoleCards = playerEntity.TempHoleCards.Select(t => new CardData(t)).ToList();
        }
    }
}