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
            joinRoom.onClick.AddListener(CreateRoom);
            cancelJoinRoom.onClick.AddListener(CancelCreateRoom);
            SetCancelButton(false);
        }

        public void Initialize()
        {
            
        }

        private void CreateRoom()
        {
            SetRoomButton(false);
            SetCancelButton(true);
            GamingHubReceiver.Instance.CreateRoom(userName.text);
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