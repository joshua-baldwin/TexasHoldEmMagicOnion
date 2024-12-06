using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using THE.Utilities;
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
        public int CurrentRound { get; private set; }
        
        public Action OnRoomConnectSuccess;
        public Action OnRoomConnectFailed;
        public Action OnCancelRoomConnect;
        public Action<int> UpdatePlayerCount;
        public Action<Enums.CommandTypeEnum, bool, Guid, Guid, List<(Guid, int)>, List<CardData>> UpdateGameUi;
        public Action<string> ShowMessage;
        public Action OnGameOverAction;
        public Action OnFinishStart;
        
        public bool IsMyTurn => CurrentPlayer.Id == Self.Id;
        
        public async UniTask JoinRoom()
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;

            Enums.JoinRoomResponseTypeEnum response;
            try
            {
                response = await CallJoinRoom(UserName.Value);
            }
            catch (Exception)
            {
                response = Enums.JoinRoomResponseTypeEnum.Failed;
                ShowMessage?.Invoke($"Failed to join room.\nジョイン失敗しました。");
                OnRoomConnectFailed?.Invoke();
            }

            if (response == Enums.JoinRoomResponseTypeEnum.AllRoomsFull)
            {
                ShowMessage?.Invoke("All rooms full. Try again later.\nルームがいっぱいになってます。時間空いてから改めて試してください");
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
                await Disconnect();
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
                await Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public async UniTask<Enums.StartResponseTypeEnum> StartGame(bool isFirstRound, Action onFinish, Action onDisconnect)
        {
            try
            {
                var response = await CallStartGame(onFinish, isFirstRound);
                if (response is Enums.StartResponseTypeEnum.NotEnoughChips or Enums.StartResponseTypeEnum.GroupDoesNotExist or Enums.StartResponseTypeEnum.NotEnoughPlayers)
                {
                    await Disconnect();
                    onDisconnect?.Invoke();
                }
                return response;
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke();
                return Enums.StartResponseTypeEnum.Failed;
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
                await Disconnect();
                onDisconnect?.Invoke();
            }
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
                await Disconnect();
                onDisconnect?.Invoke();
            }
        }

        public List<PlayerData> GetPlayerList() => players.ToList();
        
        public bool CanPlaceBet() => BetAmount.Value <= Self.Chips;

        private async UniTask Disconnect()
        {
            await client.DisposeAsync();
            client = null;
            Self = null;
            CurrentPlayer = null;
        }

        #region RPC calls
        
        private async UniTask<Enums.JoinRoomResponseTypeEnum> CallJoinRoom(string userName)
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
            var playerEntities = await client.GetAllPlayers();
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            onFinish?.Invoke(players.Length);
        }

        private async UniTask<Enums.StartResponseTypeEnum> CallStartGame(Action onFinish, bool isFirstRound)
        {
            Debug.Log("Calling StartGame");
            OnFinishStart = onFinish;
            return await client.StartGame(Self.Id, isFirstRound);
        }

        private async UniTask CallCancelGame()
        {
            Debug.Log("Calling CancelGame");
            await client.CancelStart(Self.Id);
        }

        private async UniTask CallDoAction(Enums.CommandTypeEnum commandType, int chipsBet, Guid targetPlayerId)
        {
            Debug.Log("Calling DoAction");
            await client.DoAction(commandType, chipsBet, targetPlayerId);
        }
        
        #endregion
        
        #region RPC callbacks
        
        public void OnJoinRoom(PlayerEntity player, int playerCount)
        {
            if (Self == null)
            {
                Self = new PlayerData(player);
                UserName.Value = "";
            }
            
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomId}");
            OnRoomConnectSuccess?.Invoke();
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void OnLeaveRoom(PlayerEntity player, int playerCount)
        {
            Debug.Log($"{player.Name}:{player.Id} left");
            if (player.Id == Self.Id)
                Self = null;
            UpdatePlayerCount?.Invoke(playerCount);
        }

        public void OnGetAllPlayers(PlayerEntity[] playerEntities)
        {
            Debug.Log($"Player count: {playerEntities.Length}");
        }

        public void OnGameStart(PlayerEntity[] playerEntities, PlayerEntity currentPlayer, Enums.GameStateEnum gameState, int roundNumber, bool isFirstRound)
        {
            Debug.Log("Game started");
            GameState = gameState;
            CurrentRound = roundNumber;
            UpdatePlayerCount = null;
            //do this in scene manager
            if (isFirstRound)
                SceneManager.LoadSceneAsync("GameScene");
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            Self = new PlayerData(playerEntities.First(x => x.Id == Self.Id));
            CurrentPlayer = new PlayerData(currentPlayer);
            OnFinishStart?.Invoke();
        }

        public void OnCancelGameStart()
        {
            Debug.Log("Cancelled");
        }

        public void OnDoAction(Enums.CommandTypeEnum commandType, PlayerEntity[] playerEntities, Guid previousPlayerId, Guid currentPlayerId, Guid targetPlayerId, List<(Guid, int)> pots, List<CardEntity> communityCards, Enums.GameStateEnum gameState, bool isError, string actionMessage, List<WinningHandEntity> winnerList)
        {
            Debug.Log($"Doing action {commandType}");
            GameState = gameState;
            players = playerEntities.Select(p => new PlayerData(p)).ToArray();
            if (isError)
                ShowMessage?.Invoke(actionMessage);
            else if (winnerList.Count == 1 && winnerList.First().HandRanking == Enums.HandRankingType.Nothing)
            {
                Debug.Log("Game over");
                players = playerEntities.Select(p => new PlayerData(p)).ToArray();
                var player = playerEntities.First(x => x.Id == winnerList.First().Winner.Id);
                ShowMessage?.Invoke($"{player.Name} is the winner!");
                OnGameOverAction?.Invoke();
            }
            else if (winnerList.Count > 0 && winnerList.First().HandRanking != Enums.HandRankingType.Nothing)
            {
                Debug.Log("Hand chosen");
                players = playerEntities.Select(p => new PlayerData(p)).ToArray();
                var sbEng = new StringBuilder();
                var sbJap = new StringBuilder();
                foreach (var winner in winnerList)
                {
                    if (winner.Winner != null)
                    {
                        sbEng.Append($"Player {winner.Winner.Name} had a {winner.HandRanking} and won {winner.PotToWinner} from {winner.PotName}.");
                        sbJap.Append($"プレイヤー{winner.Winner.Name}は{winner.HandRanking.GetDescription()}があって{winner.PotName}から{winner.PotToWinner}をもらった。");
                    }
                    else if (winner.TiedWith.Count > 0)
                    {
                        var sb2Eng = new StringBuilder();
                        var sb2Jap = new StringBuilder();
                        foreach (var tie in winner.TiedWith)
                        {
                            sb2Eng.Append($"{tie.Name} and");
                            sb2Jap.Append($"{tie.Name}と");
                        }

                        sbEng.Append($"Players {sb2Eng} tied with {winner.HandRanking} and won {winner.PotToTiedWith} each from {winner.PotName}.");
                        sbJap.Append($"プレイヤー{sb2Jap}が{winner.HandRanking.GetDescription()}で引き分けしてそれぞれ{winner.PotName}から{winner.PotToTiedWith}をもらった。");
                    }
                }

                ShowMessage?.Invoke($"{sbEng}\n{sbJap}");
                OnGameOverAction?.Invoke();
            }

            var cards = new List<CardData>();
            foreach (var card in communityCards)
            {
                if (card != null)
                    cards.Add(new CardData(card));
            }

            UpdateGameUi?.Invoke(commandType, currentPlayerId == Self.Id, previousPlayerId, currentPlayerId, pots, cards);
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