using MessagePack;

namespace THE.MagicOnion.Shared.Entities
{
    [MessagePackObject]
    public class CardEntity
    {
        [Key(0)]
        public CardSuitEnum Suit { get; private set; }
        
        [Key(1)]
        public CardRankEnum Rank { get; private set; }

        public CardEntity(CardSuitEnum suit, CardRankEnum rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}