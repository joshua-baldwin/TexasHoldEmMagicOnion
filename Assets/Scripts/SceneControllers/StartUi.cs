using System;
using System.Threading;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class StartUi : MonoBehaviour, IGamingHubReceiver
    {
        [SerializeField] private Button createRoom;
        [SerializeField] private Button joinRoom;
        [SerializeField] private Button cancelCreateRoom;
        [SerializeField] private Button cancelJoinRoom;
        
        private CancellationTokenSource shutdownCancellation = new();
        private Guid myId;
        private PlayerEntity[] players;

        private void Awake()
        {
            createRoom.onClick.AddListener(CreateRoom);
            joinRoom.onClick.AddListener(JoinRoom);
            cancelCreateRoom.onClick.AddListener(CancelCreateRoom);
            cancelJoinRoom.onClick.AddListener(CancelJoinRoom);
            MySceneManager.Receiver = new GamingHubReceiver();
            MySceneManager.Receiver.OnConnectSuccess = (playerCount) =>
            {
                MySceneManager.PlayerCount = playerCount;
                SceneManager.LoadSceneAsync("GameScene");
            };
            MySceneManager.Receiver.OnConnectFailed = () => SetRoomButtons(true);
            MySceneManager.Receiver.OnCancel = () => SetRoomButtons(true);
        }

        private async void CreateRoom()
        {
            SetRoomButtons(false);
            MySceneManager.Receiver.CallCreateRoom();
        }

        private async void JoinRoom()
        {
            SetRoomButtons(false);
            MySceneManager.Receiver.CallJoinRoom();
        }

        private void SetRoomButtons(bool isActive)
        {
            createRoom.interactable = isActive;
            joinRoom.interactable = isActive;
        }

        private void SetCancelButtons(bool isActive)
        {
            cancelCreateRoom.interactable = isActive;
        }
        
        private void CancelCreateRoom()
        {
            shutdownCancellation.Cancel();
            SetRoomButtons(true);
        }
        
        private void CancelJoinRoom()
        {
            shutdownCancellation.Cancel();
            SetRoomButtons(true);
        }
        
        public void OnJoinRoom(PlayerEntity player)
        {
            Debug.Log($"{player.Name}:{player.Id} joined");
        }

        public void OnLeaveRoom(PlayerEntity player)
        {
            Debug.Log($"{player.Name}:{player.Id} left");
        }

        public void SendMessage(string message)
        {
            Debug.Log(message);
        }

        public void OnGetAllPlayers(PlayerEntity[] playerEntities)
        {
            foreach (var player in playerEntities)
                Debug.Log(player.Name);   
        }
        
        private async void RegisterDisconnectEvent(IGamingHub streamingClient)
        {
            try
            {
                // you can wait disconnected event
                await streamingClient.WaitForDisconnect();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                // try-to-reconnect? logging event? close? etc...
                Debug.Log($"disconnected from the server.");

                // if (this.isSelfDisConnected)
                // {
                //     // there is no particular meaning
                //     await Task.Delay(2000);
                //
                //     // reconnect
                //     await this.ReconnectServerAsync();
                // }
            }
        }
    }
}