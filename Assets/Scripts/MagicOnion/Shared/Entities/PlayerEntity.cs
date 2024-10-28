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
        public PlayerTypeEnum PlayerType { get; private set; }
        [Key(3)]
        public string RoomName { get; private set; }
        
        public PlayerEntity(string name, Guid id, PlayerTypeEnum type, string roomName)
        {
            Name = name;
            Id = id;
            PlayerType = type;
            RoomName = roomName;
        }
    }
}