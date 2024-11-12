using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using GrpcWebSocketBridge.Client;
using MagicOnion.Client;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.MagicOnion.Client
{
    public class GamingHubReceiver : IGamingHubReceiver
    {
        public AsyncReactiveProperty<string> UserName { get; } = new("");
        
        private const int MaxRetry = 5;
        private CancellationTokenSource shutdownCancellation;
        private GrpcChannel channel;
        private IGamingHub client;
        private PlayerEntity[] players;
        private PlayerEntity self;
        
        public Action OnRoomConnectSuccess;
        public Action OnRoomConnectFailed;
        public Action OnCancelRoomConnect;

        public Action<int> UpdatePlayerCount;
        public Action<bool, string> UpdateGameUi;

        public PlayerEntity GetSelf() => self;
        public PlayerEntity CurrentPlayer;
        public bool IsMyTurn => CurrentPlayer == self;
        
        public async UniTask CreateRoom()
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;
            
            self = await CallCreateRoom(UserName.Value);
            UserName.Value = "";
            Debug.Log("room joined");
            OnRoomConnectSuccess?.Invoke();
        }

        public async UniTask LeaveRoom(Action onFinish)
        {
            await CallLeaveRoom();
            Disconnect();
            onFinish?.Invoke();
        }

        public async UniTask GetPlayers(Action<int> onFinish)
        {
            await CallGetPlayers(onFinish);
        }

        public async UniTask StartGame(Action onFinish)
        {
            await CallStartGame(onFinish);
        }

        public async UniTask CancelStartGame()
        {
            await CallCancelGame();
        }

        public async UniTaskVoid QuitGame()
        {
            await CallQuitGame();
        }

        public async UniTask DoAction(Enums.CommandTypeEnum commandType)
        {
            await CallDoAction(commandType);
        }

        public List<PlayerEntity> GetPlayerList() => players.ToList();

        private void Disconnect()
        {
            client.DisposeAsync();
            client = null;
        }

        #region RPC calls
        
        private async UniTask<PlayerEntity> CallCreateRoom(string userName)
        {
            Debug.Log("Calling JoinRoom");
            return await client.JoinRoomAsync(userName);
        }
        
        private async UniTask CallLeaveRoom()
        {
            Debug.Log("Calling LeaveRoom");
            await client.LeaveRoomAsync();
        }

        private async UniTask CallGetPlayers(Action<int> onFinish)
        {
            Debug.Log("Calling GetAllPlayers");
            players = await client.GetAllPlayers();
            onFinish?.Invoke(players.Length);
        }

        private async UniTask CallStartGame(Action onFinish)
        {
            Debug.Log("Calling StartGame");
            var canStart = await client.StartGame(self.Id);
            if (canStart)
                onFinish?.Invoke();
        }

        private async UniTask CallCancelGame()
        {
            Debug.Log("Calling CancelGame");
            await client.CancelStart(self.Id);
        }

        private async UniTask CallQuitGame()
        {
            Debug.Log("Calling QuitGame");
            await client.QuitGame(self.Id);
        }

        private async UniTask CallDoAction(Enums.CommandTypeEnum commandType)
        {
            Debug.Log("Calling DoAction");
            await client.DoAction(commandType);
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

        public void OnGameStart(PlayerEntity[] playerEntities, PlayerEntity currentPlayer)
        {
            Debug.Log("Game started");
            SceneManager.LoadSceneAsync("GameScene");
            players = playerEntities;
            CurrentPlayer = currentPlayer;
        }

        public void OnCancelGameStart()
        {
            Debug.Log("Cancelled");
        }

        public void OnQuitGame()
        {
            Debug.Log("Game quit");
        }

        public void OnDoAction(Enums.CommandTypeEnum commandType, PlayerEntity currentPlayer)
        {
            Debug.Log($"Doing action {commandType}");
            UpdateGameUi?.Invoke(currentPlayer.Id == self.Id, currentPlayer.Name);
        }
        
        #endregion
        
        private async UniTask InitializeClientAsync()
        {
            string url = "";
#if UNITY_EDITOR
            url = "http://localhost:5137";
#else
            url = "http://54.178.31.18:5137";
#endif
            // Initialize the Hub
            channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebSocketBridgeHandler()
            });

            shutdownCancellation?.Dispose();
            shutdownCancellation = new();
            var retryCount = 0;
            while (!shutdownCancellation.IsCancellationRequested && retryCount < MaxRetry)
            {
                try
                {
                    Debug.Log("Connecting to the server...");
                    client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(channel, this, cancellationToken: shutdownCancellation.Token);
                    //await RegisterDisconnectEvent(client);
                    WaitForDisconnected().Forget();
            
                    async UniTaskVoid WaitForDisconnected()
                    {
                        await RegisterDisconnectEvent(client);
                    }
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
        
        private async UniTask RegisterDisconnectEvent(IGamingHub streamingClient)
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