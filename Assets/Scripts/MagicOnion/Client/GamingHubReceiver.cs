using System;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
using UnityEngine;
using UnityEngine.UI;

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

        public async void CallCreateRoom()
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallCreate();
            CallGetPlayers(() => OnConnectSuccess?.Invoke(players.Length));
        }
        
        public async void CallJoinRoom()
        {
            if (client == null)
                await InitializeClientAsync();
            self = await CallJoin();
            CallGetPlayers(() => OnConnectSuccess?.Invoke(players.Length));
        }

        public async void CallLeaveMethod(Action onFinish)
        {
            await CallLeave();
            onFinish?.Invoke();
        }

        public void Disconnect()
        {
            client.DisposeAsync();
        }
        
        private async ValueTask<PlayerEntity> CallCreate()
        {
            return await client.JoinRoomAsync("user1", true);
        }
        
        private async ValueTask<PlayerEntity> CallJoin()
        {
            return await client.JoinRoomAsync("user1", false);
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