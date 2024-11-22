using TexasHoldEmShared.Enums;

namespace THE.Player
{
    public class ChipData
    {
        public enum ChipTypeEnum
        {
            White = 1,
            Red = 5,
            Blue = 10,
            Green = 25,
            Black = 100
        }
        
        public ChipTypeEnum ChipType { get; private set; }
        public int ChipCount { get; set; }

        public ChipData(ChipTypeEnum chipType, int chipCount)
        {
            ChipType = chipType;
            ChipCount = chipCount;
        }
    }
}