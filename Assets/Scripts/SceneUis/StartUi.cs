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
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button cancelJoinRoom;

        private void Awake()
        {
            joinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CreateRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelJoinRoom.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelCreateRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            SetCancelButton(false);
        }

        public void Initialize()
        {
            
        }

        private async UniTaskVoid CreateRoom()
        {
            SetRoomButton(false);
            SetCancelButton(true);
            await GamingHubReceiver.Instance.CreateRoom(userName.text);
        }

        public void SetRoomButton(bool isActive)
        {
            joinRoom.interactable = isActive;
        }

        private void SetCancelButton(bool isActive)
        {
            cancelJoinRoom.interactable = isActive;
        }
        
        private void CancelCreateRoom()
        {
            SetCancelButton(false);
            SetRoomButton(true);
            GamingHubReceiver.Instance.SetCancellation();
        }
    }
}