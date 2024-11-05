using System;
using System.Collections.Generic;
using MessagePack;

namespace THE.MagicOnion.Shared.Entities
{
    [MessagePackObject]
    public class PlayerEntity
    {
        [Key(0)]
        public string Name { get; private set; }
        
        [Key(1)]
        public Guid Id { get; private set; }
        
        [Key(2)]
        public PlayerRoleEnum PlayerRole { get; set; }
        
        [Key(3)]
        public Guid RoomId { get; set; }
        
        [Key(4)]
        public bool IsDealer { get; set; }
        
        [Key(5)]
        public CardEntity[] CardHand { get; set; }
        
        [Key(6)]
        public List<CardEntity> CardPool { get; set; }
        
        [Key(7)]
        public bool IsReady { get; set; }
        
        public PlayerEntity(string name, Guid id, PlayerRoleEnum role)
        {
            Name = name;
            Id = id;
            PlayerRole = role;
            CardHand = new CardEntity[2];
        }
    }
}