using TexasHoldEmShared.Enums;

namespace THE.Player
{
    public class ChipData
    {
        public Enums.ChipTypeEnum ChipType { get; private set; }
        public int ChipCount { get; set; }

        public ChipData(Enums.ChipTypeEnum chipType, int chipCount)
        {
            ChipType = chipType;
            ChipCount = chipCount;
        }
    }
}