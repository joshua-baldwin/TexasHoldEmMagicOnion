using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using THE.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.Utilities
{
    public static class ClientUtilityMethods
    {
        public static IEnumerator LoadAsyncScene(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }

        public static string GetChipText(List<ChipData> chips)
        {
            return $"W={chips.First(x => x.ChipType == ChipData.ChipTypeEnum.White).ChipCount}, " +
                   $"R={chips.First(x => x.ChipType == ChipData.ChipTypeEnum.Red).ChipCount}, " +
                   $"Blu={chips.First(x => x.ChipType == ChipData.ChipTypeEnum.Blue).ChipCount}\n" +
                   $"G={chips.First(x => x.ChipType == ChipData.ChipTypeEnum.Green).ChipCount}, " +
                   $"Bla={chips.First(x => x.ChipType == ChipData.ChipTypeEnum.Black).ChipCount}";
        }
        
        public static ChipData.ChipTypeEnum GetNextChipType(ChipData.ChipTypeEnum chipType)
        {
            switch (chipType)
            {
                case ChipData.ChipTypeEnum.White:
                    return ChipData.ChipTypeEnum.Red;
                case ChipData.ChipTypeEnum.Red:
                    return ChipData.ChipTypeEnum.Blue;
                case ChipData.ChipTypeEnum.Blue:
                    return ChipData.ChipTypeEnum.Green;
                case ChipData.ChipTypeEnum.Green:
                    return ChipData.ChipTypeEnum.Black;
                case ChipData.ChipTypeEnum.Black:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chipType), chipType, null);
            }
        }

        public static ChipData.ChipTypeEnum GetPreviousChipType(ChipData.ChipTypeEnum chipType)
        {
            switch (chipType)
            {
                case ChipData.ChipTypeEnum.White:
                    return 0;
                case ChipData.ChipTypeEnum.Red:
                    return ChipData.ChipTypeEnum.White;
                case ChipData.ChipTypeEnum.Blue:
                    return ChipData.ChipTypeEnum.Red;
                case ChipData.ChipTypeEnum.Green:
                    return ChipData.ChipTypeEnum.Blue;
                case ChipData.ChipTypeEnum.Black:
                    return ChipData.ChipTypeEnum.Green;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chipType), chipType, null);
            }
        }
    }
}