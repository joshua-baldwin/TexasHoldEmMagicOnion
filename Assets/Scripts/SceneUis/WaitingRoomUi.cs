using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using THE.SceneControllers;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class WaitingRoomUi : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text roomName;
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button leaveButton;
        
        private int playerCount;
        private GamingHubReceiver gamingHubReceiver;

        private void Awake()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            userName.text = $"Name: {gamingHubReceiver.Self.Name}";
            roomName.text = $"Room id:\n{gamingHubReceiver.Self.RoomId}";
            startButton.interactable = playerCount > 1;
            cancelButton.interactable = false;
            startButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => StartAction())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelAction())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            leaveButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => LeaveRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            gamingHubReceiver.UpdatePlayerCount = UpdatePlayerCount;
        }

        public async UniTask Initialize()
        {
            await gamingHubReceiver.GetPlayers(UpdatePlayerCount);
        }

        private void UpdatePlayerCount(int count)
        {
            currentPlayerCount.text = $"{count}/10";
            playerCount = count;
            startButton.interactable = playerCount > 1;
        }

        private async UniTaskVoid StartAction()
        {
            startButton.interactable = false;
            cancelButton.interactable = true;
            await gamingHubReceiver.StartGame(() =>
            {
                gamingHubReceiver.UpdatePlayerCount = null;
                StartCoroutine(UtilityMethods.LoadAsyncScene("GameScene"));
            });
        }

        private async UniTaskVoid CancelAction()
        {
            startButton.interactable = true;
            cancelButton.interactable = false;
            await gamingHubReceiver.CancelStartGame();
        }
        
        private async UniTaskVoid LeaveRoom()
        {
            await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
        }
    }
}