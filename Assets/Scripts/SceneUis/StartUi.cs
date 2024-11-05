using System;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class StartUi : MonoBehaviour
    {
        [SerializeField] private InputField userName;
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button cancelJoinRoom;
        
        private Guid myId;
        private PlayerEntity[] players;

        private void Awake()
        {
            joinRoom.onClick.AddListener(CreateRoom);
            cancelJoinRoom.onClick.AddListener(CancelCreateRoom);
            SetCancelButtons(false);
            GamingHubReceiver.Instance.OnRoomConnectSuccess = () => SceneManager.LoadSceneAsync("WaitingRoomScene");
            GamingHubReceiver.Instance.OnRoomConnectFailed = () => SetRoomButtons(true);
            GamingHubReceiver.Instance.OnCancelRoomConnect = () => SetRoomButtons(true);
        }

        public void Initialize()
        {
            
        }

        private void CreateRoom()
        {
            SetRoomButtons(false);
            SetCancelButtons(true);
            GamingHubReceiver.Instance.CreateRoom(userName.text);
        }

        private void SetRoomButtons(bool isActive)
        {
            joinRoom.interactable = isActive;
        }

        private void SetCancelButtons(bool isActive)
        {
            cancelJoinRoom.interactable = isActive;
        }
        
        private void CancelCreateRoom()
        {
            SetCancelButtons(false);
            SetRoomButtons(true);
            GamingHubReceiver.Instance.SetCancellation();
        }
    }
}