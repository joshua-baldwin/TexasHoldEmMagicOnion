using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class StartUi : MonoBehaviour
    {
        [SerializeField] private InputField userName;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button cancelJoinRoom;
        
        private GamingHubReceiver gamingHubReceiver;

        private void Awake()
        {
            userName.OnValueChangedAsAsyncEnumerable()
                .Where(x => gamingHubReceiver.UserName.Value != x)
                .Subscribe(x => gamingHubReceiver.UserName.Value = x)
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            connectButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => Connect())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            joinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CreateRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelJoinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelCreateRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            userName.gameObject.SetActive(false);
            connectButton.gameObject.SetActive(true);
            SetRoomButton(false);
            SetCancelButton(false);
        }

        public void Initialize()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
        }

        private async UniTaskVoid Connect()
        {
            await gamingHubReceiver.Connect();
            userName.gameObject.SetActive(true);
            connectButton.interactable = false;
            SetRoomButton(true);
        }
        
        private async UniTaskVoid CreateRoom()
        {
            SetRoomButton(false);
            SetCancelButton(true);
            await gamingHubReceiver.CreateRoom();
        }
        
        private void CancelCreateRoom()
        {
            SetCancelButton(false);
            SetRoomButton(true);
            gamingHubReceiver.SetCancellation();
        }

        public void SetRoomButton(bool isActive)
        {
            joinRoom.interactable = isActive;
        }

        private void SetCancelButton(bool isActive)
        {
            cancelJoinRoom.interactable = isActive;
        }
    }
}