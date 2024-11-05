using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
using THE.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.MagicOnion.Client
{
    public class GamingHubReceiver : Singleton<GamingHubReceiver>, IGamingHubReceiver
    {
        private CancellationTokenSource shutdownCancellation;
        private GrpcChannelx channel;
        private IGamingHub client;
        private PlayerEntity[] players;
        private PlayerEntity self;

        public string UserName;
        public string RoomName;
        public bool IsHost;
        
        public Action OnConnectSuccess;
        public Action OnConnectFailed;
        public Action OnCancel;

        public Action<int> UpdatePlayerCount;
        public Action<PlayerEntity[]> OnGameStartAction;

        public async void CallCreateRoom(string userName)
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;
            
            self = await CallCreate(userName);
            UserName = self.Name;
            RoomName = self.RoomName;
            IsHost = true;
            OnConnectSuccess?.Invoke();
        }
        
        public async void CallJoinRoom(string userName)
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;
            
            self = await CallJoin(userName);
            UserName = self.Name;
            RoomName = self.RoomName;
            OnConnectSuccess?.Invoke();
        }

        public async void CallLeaveMethod(Action onFinish)
        {
            await CallLeave();
            Disconnect();
            onFinish?.Invoke();
        }

        public void CallGetPlayersMethod(Action<int> onFinish)
        {
            CallGetPlayers(onFinish);
        }

        public async void CallStartGameMethod(string roomName, Action onFinish)
        {
            await CallStartGame(roomName);
            onFinish?.Invoke();
        }

        public List<PlayerEntity> GetAllPlayers() => players.ToList();

        private void Disconnect()
        {
            client.DisposeAsync();
        }
        
        private async ValueTask<PlayerEntity> CallCreate(string userName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName);
        }
        
        private async ValueTask<PlayerEntity> CallJoin(string userName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName);
        }
        
        private async ValueTask CallLeave()
        {
            Debug.Log("Calling LeaveRoom");
            await client.LeaveRoomAsync(self.RoomName);
        }
        
        private async void CallSendMessage()
        {
            await client.SendMessageAsync("hello");
        }

        private async void CallGetPlayers(Action<int> onFinish)
        {
            Debug.Log("Calling GetAllPlayers");
            players = await client.GetAllPlayers(self.RoomName);
            onFinish?.Invoke(players.Length);
        }

        private async ValueTask CallStartGame(string roomName)
        {
            Debug.Log("Calling StartGame");
            await client.StartGame(roomName);
        }
        
        public void OnJoinRoom(PlayerEntity player, int playerCount)
        {
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomName}");
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void OnLeaveRoom(PlayerEntity player, int playerCount)
        {
            Debug.Log($"{player.Name}:{player.Id} left");
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void SendMessage(string message)
        {
            Debug.Log(message);
        }

        public void OnGetAllPlayers(PlayerEntity[] playerEntities)
        {
            Debug.Log($"Player count: {playerEntities.Length}");
        }

        public void OnUpdatePlayerRole(PlayerRoleEnum role)
        {
            Debug.Log($"Player role is {role}");
        }

        public void OnGameStart(PlayerEntity[] playerEntities)
        {
            Debug.Log("Game started");
            SceneManager.LoadSceneAsync("GameScene");
            players = playerEntities;
            //OnGameStartAction?.Invoke(playerEntities);
        }
        
        public async Task InitializeClientAsync()
        {
            // Initialize the Hub
            channel = GrpcChannelx.ForAddress("https://localhost:7007");

            shutdownCancellation?.Dispose();
            shutdownCancellation = new();
            while (!shutdownCancellation.IsCancellationRequested)
            {
                try
                {
                    Debug.Log("Connecting to the server...");
                    client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(channel, this, cancellationToken: shutdownCancellation.Token);
                    RegisterDisconnectEvent(client);
                    Debug.Log("Connection is established.");
                    break;
                }
                catch (Exception e)
                {
                    OnConnectFailed?.Invoke();
                    Debug.LogError(e);
                }

                Debug.Log("Failed to connect to the server. Retry after 5 seconds...");
                await Task.Delay(5 * 1000);
            }

            if (shutdownCancellation.IsCancellationRequested)
            {
                Debug.Log("Request cancelled");
                OnCancel?.Invoke();
            }
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

        public void SetCancellation()
        {
            shutdownCancellation.Cancel();
        }
    }
}