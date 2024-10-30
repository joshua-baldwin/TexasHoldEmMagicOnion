using System;
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