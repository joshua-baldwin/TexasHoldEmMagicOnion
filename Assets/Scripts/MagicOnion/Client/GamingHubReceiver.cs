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
using THE.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.MagicOnion.Client
{
    public class GamingHubReceiver : IGamingHubReceiver
    {
        private const int MaxRetry = 5;
        private CancellationTokenSource shutdownCancellation;
        private GrpcChannel channel;
        private IGamingHub client;
        private PlayerData[] players;
        
        public AsyncReactiveProperty<string> UserName { get; } = new("");
        public AsyncReactiveProperty<int> BetAmount { get; } = new(0);
        public PlayerData Self { get; private set; }
        public PlayerData CurrentPlayer { get; private set; }
        public Enums.GameStateEnum GameState { get; private set; }
        
        public Action OnRoomConnectSuccess;
        public Action OnRoomConnectFailed;
        public Action OnCancelRoomConnect;
        public Action<int> UpdatePlayerCount;
        public Action<bool, PlayerData, PlayerData, int, List<CardData>> UpdateGameUi;
        public Action<string> ShowMessage;
        public Action OnGameOverAction;
        
        public bool IsMyTurn => CurrentPlayer.Id == Self.Id;
        
        public async UniTask JoinRoom()
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;

            try
            {
                await CallJoinRoom(UserName.Value);
            }
            catch (Exception)
            {
                OnRoomConnectFailed?.Invoke();
            }
        }

        public async UniTask LeaveRoom(Action onFinish, Action onDisconnect)
        {
            try
            {
                await CallLeaveRoom();
                onFinish?.Invoke();
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTask GetPlayers(Action<int> onFinish, Action onDisconnect)
        {
            try
            {
                await CallGetPlayers(onFinish);
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTask StartGame(Action onFinish, Action onDisconnect)
        {
            try
            {
                await CallStartGame(onFinish);
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTask CancelStartGame(Action onDisconnect)
        {
            try
            {
                await CallCancelGame();
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTaskVoid QuitGame()
        {
            await CallQuitGame();
        }

        public async UniTask DoAction(Enums.CommandTypeEnum commandType, Guid targetPlayerId, Action onDisconnect)
        {
            try
            {
                await CallDoAction(commandType, BetAmount.Value, targetPlayerId);
                BetAmount.Value = 0;
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTask ChooseHand(Guid playerId, List<CardData> showdownCards, Action onDisconnect)
        {
            try
            {
                var cardEntities = showdownCards.Select(card => new CardEntity(card.Suit, card.Rank)).ToArray();
                await CallChooseHand(playerId, cardEntities);
            }
            catch (ObjectDisposedException)
            {
                Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public List<PlayerData> GetPlayerList() => players.ToList();
        
        public bool CanPlaceBet() => BetAmount.Value <= Self.Chips;

        private void Disconnect()
        {
            client.DisposeAsync();
            client = null;
        }

        #region RPC calls
        
        private async UniTask CallJoinRoom(string userName)
        {
            Debug.Log("Calling JoinRoom");
            await client.JoinRoomAsync(userName);
        }
        
        private async UniTask CallLeaveRoom()
        {
            Debug.Log("Calling LeaveRoom");
            await client.LeaveRoomAsync();
        }

        private async UniTask CallGetPlayers(Action<int> onFinish)
        {
            Debug.Log("Calling GetAllPlayers");
            var playerEntities = await client.GetAllPlayers();
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            onFinish?.Invoke(players.Length);
        }

        private async UniTask CallStartGame(Action onFinish)
        {
            Debug.Log("Calling StartGame");
            var canStart = await client.StartGame(Self.Id);
            if (canStart)
                onFinish?.Invoke();
        }

        private async UniTask CallCancelGame()
        {
            Debug.Log("Calling CancelGame");
            await client.CancelStart(Self.Id);
        }

        private async UniTask CallQuitGame()
        {
            Debug.Log("Calling QuitGame");
            await client.QuitGame(Self.Id);
        }

        private async UniTask CallDoAction(Enums.CommandTypeEnum commandType, int chipsBet, Guid targetPlayerId)
        {
            Debug.Log("Calling DoAction");
            await client.DoAction(commandType, chipsBet, targetPlayerId);
        }

        private async UniTask CallChooseHand(Guid playerId, CardEntity[] showdownCards)
        {
            Debug.Log("Calling ChooseHand");
            await client.ChooseHand(playerId, showdownCards);
        }
        
        #endregion
        
        #region RPC callbacks
        
        public void OnJoinRoom(PlayerEntity player, int playerCount)
        {
            Self = new PlayerData(player);
            UserName.Value = "";
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomId}");
            OnRoomConnectSuccess?.Invoke();
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

        public void OnGameStart(PlayerEntity[] playerEntities, PlayerEntity currentPlayer, Enums.GameStateEnum gameState)
        {
            Debug.Log("Game started");
            UpdatePlayerCount = null;
            //do this in scene manager
            SceneManager.LoadSceneAsync("GameScene");
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            Self = new PlayerData(playerEntities.First(x => x.Id == Self.Id));
            CurrentPlayer = new PlayerData(currentPlayer);
            GameState = gameState;
        }

        public void OnCancelGameStart()
        {
            Debug.Log("Cancelled");
        }

        public void OnQuitGame()
        {
            Debug.Log("Game quit");
        }

        public void OnDoAction(Enums.CommandTypeEnum commandType, PlayerEntity[] playerEntities, Guid previousPlayerId, Guid currentPlayerId, Guid targetPlayerId, int currentPot, CardEntity[] communityCards, Enums.GameStateEnum gameState, bool isError, string actionMessage)
        {
            Debug.Log($"Doing action {commandType}");
            GameState = gameState;
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            if (isError)
                ShowMessage?.Invoke(actionMessage);
            else
            {
                var cards = new List<CardData>();
                foreach (var card in communityCards)
                {
                    if (card != null)
                        cards.Add(new CardData(card));
                }
                
                UpdateGameUi?.Invoke(currentPlayerId == Self.Id, players.First(x => x.Id == previousPlayerId), players.First(x => x.Id == currentPlayerId), currentPot, cards);
            }
        }

        public void OnChooseHand(Guid winnerId, PlayerEntity[] playerEntities)
        {
            Debug.Log("Hand chosen");
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            var player = playerEntities.First(x => x.Id == winnerId);
            ShowMessage?.Invoke($"{player.Name} is the winner!");
            OnGameOverAction?.Invoke();
        }

        public void OnGameOver(Guid winnerId, PlayerEntity[] playerEntities)
        {
            Debug.Log("Game over");
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            var player = playerEntities.First(x => x.Id == winnerId);
            ShowMessage?.Invoke($"{player.Name} is the winner!");
            OnGameOverAction?.Invoke();
        }

        #endregion
        
        private async UniTask InitializeClientAsync()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var url = "http://localhost:5137";
#else
            var url = "http://54.178.31.18:5137";
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