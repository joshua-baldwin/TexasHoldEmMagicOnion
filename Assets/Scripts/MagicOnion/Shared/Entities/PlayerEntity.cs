using MessagePack;

namespace THE.MagicOnion.Shared.Entities
{
    [MessagePackObject]
    public class PlayerEntity
    {
        [Key(0)]
        public string Name { get; private set; }
        
        [Key(1)]
        public long Id { get; private set; }
        
        [Key(2)]
        public PlayerTypeEnum PlayerType { get; private set; }
        
        public PlayerEntity(string name, long id, PlayerTypeEnum type)
        {
            Name = name;
            Id = id;
            PlayerType = type;
        }
    }
}