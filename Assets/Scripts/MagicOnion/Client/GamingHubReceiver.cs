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
        private const int MaxRetry = 5;
        private CancellationTokenSource shutdownCancellation;
        private GrpcChannelx channel;
        private IGamingHub client;
        private PlayerEntity[] players;
        private PlayerEntity self;
        
        public Action OnRoomConnectSuccess;
        public Action OnRoomConnectFailed;
        public Action OnCancelRoomConnect;

        public Action<int> UpdatePlayerCount;

        public PlayerEntity GetSelf() => self;
        
        public async void CreateRoom(string userName)
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;
            
            self = await CallCreateRoom(userName);
            OnRoomConnectSuccess?.Invoke();
        }

        public async void LeaveRoom(Action onFinish)
        {
            await CallLeaveRoom();
            Disconnect();
            onFinish?.Invoke();
        }

        public void GetPlayers(Action<int> onFinish)
        {
            CallGetPlayers(onFinish);
        }

        public async void StartGame()
        {
            await CallStartGame();
        }

        public async void CancelStartGame()
        {
            await CallCancelGame();
        }

        public async void QuitGame()
        {
            await CallQuitGame();
        }

        public List<PlayerEntity> GetPlayerList() => players.ToList();

        private void Disconnect()
        {
            client.DisposeAsync();
            client = null;
        }

        #region RPC calls
        
        private async ValueTask<PlayerEntity> CallCreateRoom(string userName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName);
        }
        
        private async ValueTask CallLeaveRoom()
        {
            Debug.Log("Calling LeaveRoom");
            await client.LeaveRoomAsync();
        }

        private async void CallGetPlayers(Action<int> onFinish)
        {
            Debug.Log("Calling GetAllPlayers");
            players = await client.GetAllPlayers();
            onFinish?.Invoke(players.Length);
        }

        private async ValueTask CallStartGame()
        {
            Debug.Log("Calling StartGame");
            await client.StartGame(self.Id);
        }

        private async ValueTask CallCancelGame()
        {
            Debug.Log("Calling CancelGame");
            await client.CancelStart(self.Id);
        }

        private async ValueTask CallQuitGame()
        {
            Debug.Log("Calling QuitGame");
            await client.QuitGame(self.Id);
        }
        
        #endregion
        
        #region RPC callbacks
        
        public void OnJoinRoom(PlayerEntity player, int playerCount)
        {
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomId}");
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void OnLeaveRoom(PlayerEntity player, int playerCount)
        {
            Debug.Log($"{player.Name}:{player.Id} left");
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void OnGetAllPlayers(PlayerEntity[] playerEntities)
        {
            Debug.Log($"Player count: {playerEntities.Length}");
        }

        public void OnGameStart(PlayerEntity[] playerEntities)
        {
            Debug.Log("Game started");
            SceneManager.LoadSceneAsync("GameScene");
            players = playerEntities;
            //OnGameStartAction?.Invoke(playerEntities);
        }

        public void OnCancelGameStart()
        {
            Debug.Log("Cancelled");
        }

        public void OnQuitGame()
        {
            Debug.Log("Game quit");
        }
        
        #endregion
        
        public async Task InitializeClientAsync()
        {
            // Initialize the Hub
            channel = GrpcChannelx.ForAddress("https://localhost:7007");

            shutdownCancellation?.Dispose();
            shutdownCancellation = new();
            var retryCount = 0;
            while (!shutdownCancellation.IsCancellationRequested && retryCount < MaxRetry)
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
                    Debug.LogError(e);
                }

                Debug.Log("Failed to connect to the server. Retry after 5 seconds...");
                retryCount++;
                await Task.Delay(5 * 1000);
            }

            if (shutdownCancellation.IsCancellationRequested)
            {
                Debug.Log("Request cancelled");
                OnCancelRoomConnect?.Invoke();
            }

            if (retryCount >= MaxRetry)
            {
                Debug.LogError("Failed to connect to the server. Retry after 5 seconds...");
                OnRoomConnectFailed?.Invoke();
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