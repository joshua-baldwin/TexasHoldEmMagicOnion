using TexasHoldEmShared.Enums;
using THE.MagicOnion.Shared.Entities;

namespace THE.Player
{
    public class CardData
    {
        public Enums.CardSuitEnum Suit { get; private set; }
        public Enums.CardRankEnum Rank { get; private set; }

        public CardData(CardEntity cardEntity)
        {
            Suit = cardEntity.Suit;
            Rank = cardEntity.Rank;
        }
    }
}