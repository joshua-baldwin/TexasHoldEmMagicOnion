using System;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
using THE.SceneControllers;
using UnityEngine;

namespace THE.MagicOnion.Client
{
    public class GamingHubReceiver : IGamingHubReceiver
    {
        private CancellationTokenSource shutdownCancellation = new();
        private GrpcChannelx channel;
        private IGamingHub client;
        private PlayerEntity[] players;
        private PlayerEntity self;

        public Action OnConnectSuccess;
        public Action OnConnectFailed;
        public Action OnCancel;

        public Action<int> UpdatePlayerCount;

        public async void CallCreateRoom(string userName)
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallCreate(userName);
            StreamingHubManager.UserName = self.Name;
            StreamingHubManager.RoomName = self.RoomName;
            OnConnectSuccess?.Invoke();
            CallGetPlayers();
        }
        
        public async void CallJoinRoom(string userName, string roomName)
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallJoin(userName, roomName);
            StreamingHubManager.UserName = self.Name;
            StreamingHubManager.RoomName = self.RoomName;
            OnConnectSuccess?.Invoke();
            CallGetPlayers();
        }

        public async void CallLeaveMethod(Action onFinish)
        {
            await CallLeave();
            Disconnect();
            onFinish?.Invoke();
        }

        private void Disconnect()
        {
            client.DisposeAsync();
        }
        
        private async ValueTask<PlayerEntity> CallCreate(string userName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName, "");
        }
        
        private async ValueTask<PlayerEntity> CallJoin(string userName, string roomName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName, roomName);
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

        private async void CallGetPlayers()
        {
            Debug.Log("Calling GetAllPlayers");
            players = await client.GetAllPlayers(self.RoomName);
        }
        
        public void OnJoinRoom(PlayerEntity player)
        {
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomName}");
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
            UpdatePlayerCount?.Invoke(playerEntities.Length);
        }
        
        public async Task InitializeClientAsync()
        {
            // Initialize the Hub
            channel = GrpcChannelx.ForAddress("https://localhost:7007");

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
            
            OnCancel?.Invoke();
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