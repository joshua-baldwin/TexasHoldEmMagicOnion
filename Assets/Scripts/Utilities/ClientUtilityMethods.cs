using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TexasHoldEmShared.Enums;
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
            return $"W={chips.First(x => x.ChipType == Enums.ChipTypeEnum.White).ChipCount}, " +
                   $"R={chips.First(x => x.ChipType == Enums.ChipTypeEnum.Red).ChipCount}, " +
                   $"Blu={chips.First(x => x.ChipType == Enums.ChipTypeEnum.Blue).ChipCount}\n" +
                   $"G={chips.First(x => x.ChipType == Enums.ChipTypeEnum.Green).ChipCount}, " +
                   $"Bla={chips.First(x => x.ChipType == Enums.ChipTypeEnum.Black).ChipCount}";
        }
    }
}