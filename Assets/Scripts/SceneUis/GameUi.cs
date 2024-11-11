using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using THE.Player;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Button quitButton;
        
        private GamingHubReceiver gamingHubReceiver;

        private void Awake()
        {
            quitButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => QuitGame())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        private async UniTaskVoid QuitGame()
        {
            await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
        }

        public void Initialize()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
        }
    }
}