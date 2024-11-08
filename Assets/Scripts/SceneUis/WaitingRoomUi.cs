using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
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

        private void Awake()
        {
            userName.text = $"Name: {GamingHubReceiver.Instance.GetSelf().Name}";
            roomName.text = $"Room id:\n{GamingHubReceiver.Instance.GetSelf().RoomId}";
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
            
            GamingHubReceiver.Instance.UpdatePlayerCount = UpdatePlayerCount;
        }

        public async UniTask Initialize()
        {
            await GamingHubReceiver.Instance.GetPlayers(UpdatePlayerCount);
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
            await GamingHubReceiver.Instance.StartGame();
        }

        private async UniTaskVoid CancelAction()
        {
            startButton.interactable = true;
            cancelButton.interactable = false;
            await GamingHubReceiver.Instance.CancelStartGame();
        }
        
        private async UniTaskVoid LeaveRoom()
        {
            await GamingHubReceiver.Instance.LeaveRoom(() => SceneManager.LoadScene("StartScene"));
        }
    }
}