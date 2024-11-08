using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.Utilities
{
    public class UtilityMethods
    {
        public static IEnumerator LoadAsyncScene(string sceneName)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOperation.isDone)
            {
                yield return null;
            }
        }
    }
}