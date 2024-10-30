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
        public string RoomName { get; private set; }
        
        [Key(4)]
        public bool IsHost { get; private set; }
        
        [Key(5)]
        public bool IsDealer { get; set; }
        
        [Key(6)]
        public CardEntity[] CardHand { get; set; }
        
        [Key(7)]
        public List<CardEntity> CardPool { get; set; }
        
        public PlayerEntity(string name, Guid id, PlayerRoleEnum role, string roomName, bool isHost)
        {
            Name = name;
            Id = id;
            PlayerRole = role;
            RoomName = roomName;
            IsHost = isHost;
        }
    }
}