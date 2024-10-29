using System;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
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

        public Action<int> OnConnectSuccess;
        public Action OnConnectFailed;
        public Action OnCancel;

        public async void CallCreateRoom(string userName)
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallCreate(userName);
            CallGetPlayers(() => OnConnectSuccess?.Invoke(players.Length));
        }
        
        public async void CallJoinRoom(string userName, string roomName)
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallJoin(userName, roomName);
            CallGetPlayers(() => OnConnectSuccess?.Invoke(players.Length));
        }

        public async void CallLeaveMethod(Action onFinish)
        {
            await CallLeave();
            Disconnect();
            onFinish?.Invoke();
        }

        public void Disconnect()
        {
            client.DisposeAsync();
        }
        
        private async ValueTask<PlayerEntity> CallCreate(string userName)
        {
            return await client.JoinRoomAsync(userName, "");
        }
        
        private async ValueTask<PlayerEntity> CallJoin(string userName, string roomName)
        {
            return await client.JoinRoomAsync(userName, roomName);
        }
        
        private async ValueTask CallLeave()
        {
            await client.LeaveRoomAsync(self.RoomName);
        }
        
        private async void CallSendMessage()
        {
            await client.SendMessageAsync("hello");
        }

        private async void CallGetPlayers(Action callback)
        {
            players = await client.GetAllPlayers(self.RoomName);
            callback?.Invoke();
        }
        
        public void OnJoinRoom(PlayerEntity player)
        {
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomName}");
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