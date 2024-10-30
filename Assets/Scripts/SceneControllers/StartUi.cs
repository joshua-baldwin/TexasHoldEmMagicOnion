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
        [SerializeField] private Button createRoom;
        [SerializeField] private Button joinRoom;
        [SerializeField] private InputField roomName;
        [SerializeField] private Button cancelCreateRoom;
        [SerializeField] private Button cancelJoinRoom;
        
        private Guid myId;
        private PlayerEntity[] players;

        private void Awake()
        {
            createRoom.onClick.AddListener(CreateRoom);
            joinRoom.onClick.AddListener(JoinRoom);
            cancelCreateRoom.onClick.AddListener(CancelCreateRoom);
            cancelJoinRoom.onClick.AddListener(CancelJoinRoom);
            SetCancelButtons(false);
            StreamingHubManager.Receiver = new GamingHubReceiver();
            StreamingHubManager.Receiver.OnConnectSuccess = () => SceneManager.LoadSceneAsync("WaitingRoomScene");
            StreamingHubManager.Receiver.OnConnectFailed = () => SetRoomButtons(true);
            StreamingHubManager.Receiver.OnCancel = () => SetRoomButtons(true);
        }

        private void CreateRoom()
        {
            SetRoomButtons(false);
            SetCancelButtons(true);
            StreamingHubManager.Receiver.CallCreateRoom(userName.text);
        }

        private void JoinRoom()
        {
            SetRoomButtons(false);
            SetCancelButtons(true);
            StreamingHubManager.Receiver.CallJoinRoom(userName.text, roomName.text);
        }

        private void SetRoomButtons(bool isActive)
        {
            createRoom.interactable = isActive;
            joinRoom.interactable = isActive;
        }

        private void SetCancelButtons(bool isActive)
        {
            cancelCreateRoom.interactable = isActive;
            cancelJoinRoom.interactable = isActive;
        }
        
        private void CancelCreateRoom()
        {
            SetCancelButtons(false);
            SetRoomButtons(true);
            StreamingHubManager.Receiver.SetCancellation();
        }
        
        private void CancelJoinRoom()
        {
            SetCancelButtons(false);
            SetRoomButtons(true);
            StreamingHubManager.Receiver.SetCancellation();
        }
    }
}