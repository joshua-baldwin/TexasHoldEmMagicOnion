using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using THE.SceneControllers;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class StartUi : MonoBehaviour
    {
        [SerializeField] private InputField userName;
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button cancelJoinRoom;
        
        private GamingHubReceiver gamingHubReceiver;
        private PopupUi popupUi;

        private void Awake()
        {
            userName.OnValueChangedAsAsyncEnumerable()
                .Where(x => gamingHubReceiver.UserName.Value != x)
                .Subscribe(x => gamingHubReceiver.UserName.Value = x)
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            joinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => JoinRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelJoinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelCreateRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            SetCancelButton(false);
        }

        public void Initialize()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            gamingHubReceiver.ShowMessage = ShowMessage;
        }
        
        private async UniTaskVoid JoinRoom()
        {
            SetRoomButton(false);
            SetCancelButton(true);
            await gamingHubReceiver.JoinRoom();
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
        
        private void ShowMessage(string message)
        {
            popupUi = FindFirstObjectByType<PopupUi>();
            popupUi.ShowMessage(message);
        }
    }
}