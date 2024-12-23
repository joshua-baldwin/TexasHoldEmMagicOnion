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

namespace THE.MagicOnion.Client
{
    public class GamingHubReceiver : IGamingHubReceiver
    {
        private const int MaxRetry = 5;
        private CancellationTokenSource shutdownCancellation;
        private GrpcChannel channel;
        private IGamingHub client;
        private List<PlayerData> players;
        private List<JokerData> jokers;
        private Action<bool> onFinishStart;
        
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
        public Action<Enums.CommandTypeEnum, bool, Guid, Guid, List<CardData>, bool> UpdateGameUi;
        public Action<List<PotEntity>> UpdatePots;
        public Action<string, Action> ShowMessage;
        public Action<bool> OnGameOverAction;
        public Action OnUseJokerAction;
        public Action<JokerData> OnUseJokerDrawAction;
        
        public bool IsMyTurn => CurrentPlayer.Id == Self.Id;
        public List<PlayerData> GetPlayerList() => players.ToList();
        public List<JokerData> GetJokerList() => jokers.ToList();
        
        public async UniTask JoinRoom(int retryCount = 0)
        {
            if (client == null)
                await InitializeClientAsync();
            
            if (shutdownCancellation.IsCancellationRequested)
                return;

            var response = Enums.JoinRoomResponseTypeEnum.Success;
            try
            {
                response = await CallJoinRoom(UserName.Value);
            }
            catch (ObjectDisposedException)
            {
                if (retryCount < MaxRetry)
                {
                    retryCount++;
                    await Disconnect();
                    await JoinRoom(retryCount);
                    return;
                }
                
                await Disconnect();
                ShowMessage?.Invoke("Disconnected from server.", null);
                OnRoomConnectFailed?.Invoke();
            }
            catch (Exception ex)
            {
                response = Enums.JoinRoomResponseTypeEnum.Failed;
                ShowMessage?.Invoke($"Failed to join room.\nジョイン失敗しました。", null);
                OnRoomConnectFailed?.Invoke();
            }

            if (response == Enums.JoinRoomResponseTypeEnum.AllRoomsFull)
            {
                ShowMessage?.Invoke("All rooms full. Try again later.\nルームがいっぱいになってます。時間空いてから改めて試してください", null);
                OnRoomConnectFailed?.Invoke();
            }
            
            if (response == Enums.JoinRoomResponseTypeEnum.Failed)
            {
                ShowMessage?.Invoke($"Failed to join room.\nジョイン失敗しました。", null);
                OnRoomConnectFailed?.Invoke();
            }
            
            if (response == Enums.JoinRoomResponseTypeEnum.InternalServerError)
            {
                ShowMessage?.Invoke($"Internal server error.", null);
                OnRoomConnectFailed?.Invoke();
            }
        }

        public async UniTask LeaveRoom(Action onFinish, Action<string> onDisconnect)
        {
            try
            {
                await CallLeaveRoom();
                onFinish?.Invoke();
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
            catch (Exception)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public async UniTask GetPlayers(Action<int> onFinish, Action<string> onDisconnect)
        {
            try
            {
                await CallGetPlayers(onFinish);
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public async UniTask StartGame(bool isFirstRound, Action<bool> onFinish, Action<string> onDisconnect)
        {
            try
            {
                var response = await CallStartGame(onFinish, isFirstRound);
                if (response is not Enums.StartResponseTypeEnum.Success && response is not Enums.StartResponseTypeEnum.AllPlayersNotReady)
                {
                    var message = response switch
                    {
                        Enums.StartResponseTypeEnum.NotEnoughChips => "Not enough chips to play again. Disconnecting.\nチップが足りないのでプレイできません。接続切ります",
                        Enums.StartResponseTypeEnum.NotEnoughPlayers => "Not enough players to play again. Disconnecting.\nプレイヤーが足りないのでプレイできません。接続切ります",
                        Enums.StartResponseTypeEnum.GroupDoesNotExist => "Room does not exist. Disconnecting.\nルームは存在していないのでプレイできません。接続切ります。",
                        Enums.StartResponseTypeEnum.AlreadyPlayedMaxRounds => "Already played max rounds. Disconnecting.\nラウンドの制限を超えた。接続を切ります。",
                        Enums.StartResponseTypeEnum.InternalServerError => "Internal server error.",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    await Disconnect();
                    onDisconnect?.Invoke(message);
                }
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public async UniTask CancelStartGame(Action<string> onDisconnect)
        {
            try
            {
                await CallCancelGame();
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public async UniTask DoAction(Enums.CommandTypeEnum commandType, Action<string> onDisconnect)
        {
            try
            {
                var response = await CallDoAction(commandType, BetAmount.Value);
                if (response == Enums.DoActionResponseTypeEnum.PlayerHasInvalidCardData)
                {
                    await Disconnect();
                    onDisconnect?.Invoke("The player who used a joker has invalid card data.\nジョーカーを使った人のカードデータに不正データがあります");
                }
                BetAmount.Value = 0;
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public async UniTask<Enums.BuyJokerResponseTypeEnum> BuyJoker(int jokerId, Action<string> onDisconnect)
        {
            try
            {
                var response = await CallBuyJokerAction(jokerId);
                if (response is Enums.BuyJokerResponseTypeEnum.GroupDoesNotExist or Enums.BuyJokerResponseTypeEnum.NotEnoughChips)
                {
                    var message = response switch
                    {
                        Enums.BuyJokerResponseTypeEnum.NotEnoughChips => "You don't have enough chips.\nチップが足りない。",
                        Enums.BuyJokerResponseTypeEnum.GroupDoesNotExist => "Room does not exist. Disconnecting.\nルームは存在していないのでプレイできません。接続切ります。"
                    };

                    await Disconnect();
                    onDisconnect?.Invoke(message);
                }
                return response;
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
                return Enums.BuyJokerResponseTypeEnum.Failed;
            }
        }
        
        public async UniTask<Enums.UseJokerResponseTypeEnum> UseJoker(Guid jokerUniqueId, List<Guid> targetIds, List<CardData> cardsToDiscard, Action<string> onDisconnect)
        {
            try
            {
                var response = await CallUseJokerAction(jokerUniqueId, targetIds, cardsToDiscard);
                if (response is Enums.UseJokerResponseTypeEnum.GroupDoesNotExist or Enums.UseJokerResponseTypeEnum.NotEnoughChips or Enums.UseJokerResponseTypeEnum.PlayerHasInvalidCardData)
                {
                    var message = response switch
                    {
                        Enums.UseJokerResponseTypeEnum.NotEnoughChips => "You don't have enough chips.\nチップが足りない。",
                        Enums.UseJokerResponseTypeEnum.GroupDoesNotExist => "Room does not exist. Disconnecting.\nルームは存在していないのでプレイできません。接続切ります。",
                        Enums.UseJokerResponseTypeEnum.PlayerHasInvalidCardData => "The player who used a joker has invalid card data.\nジョーカーを使った人のカードデータに不正データがあります",
                    };

                    await Disconnect();
                    onDisconnect?.Invoke(message);
                }
                return response;
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
                return Enums.UseJokerResponseTypeEnum.Failed;
            }
        }

        public async UniTask DiscardHoleCard(Guid jokerUserId, Guid selectedJokerUniqueId, List<CardData> cardsToDiscard, Action<string> onDisconnect)
        {
            try
            {
                await CallDiscardHoleCard(jokerUserId, selectedJokerUniqueId, cardsToDiscard);
            }
            catch (ObjectDisposedException)
            {
                await Disconnect();
                onDisconnect?.Invoke("Disconnected from server.");
            }
        }

        public bool CanPlaceBet() => BetAmount.Value <= Self.Chips;

        private async UniTask Disconnect()
        {
            try
            {
                if (client != null)
                    await client.LeaveRoomAsync();
            }
            catch (ObjectDisposedException)
            {
                //we're already disconnected so do nothing
            }
            
            if (client != null)
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
            players = playerEntities.Select(p => new PlayerData(p)).ToList();
            onFinish?.Invoke(players.Count);
        }

        private async UniTask<Enums.StartResponseTypeEnum> CallStartGame(Action<bool> onFinish, bool isFirstRound)
        {
            Debug.Log("Calling StartGame");
            onFinishStart = onFinish;
            return await client.StartGame(Self.Id, isFirstRound);
        }

        private async UniTask CallCancelGame()
        {
            Debug.Log("Calling CancelGame");
            await client.CancelStart(Self.Id);
        }

        private async UniTask<Enums.DoActionResponseTypeEnum> CallDoAction(Enums.CommandTypeEnum commandType, int chipsBet)
        {
            Debug.Log("Calling DoAction");
            return await client.DoAction(commandType, chipsBet);
        }

        private async UniTask<Enums.BuyJokerResponseTypeEnum> CallBuyJokerAction(int jokerId)
        {
            Debug.Log("Calling BuyJokerAction");
            return await client.BuyJoker(Self.Id, jokerId);
        }
        
        private async UniTask<Enums.UseJokerResponseTypeEnum> CallUseJokerAction(Guid jokerUniqueId, List<Guid> targetIds, List<CardData> cardsToDiscard)
        {
            Debug.Log("Calling UseJokerAction");
            var cardEntities = cardsToDiscard.Select(c => new CardEntity(c.Suit, c.Rank)).ToList();
            return await client.UseJoker(Self.Id, jokerUniqueId, targetIds, cardEntities);
        }
        
        private async UniTask CallDiscardHoleCard(Guid jokerUserId, Guid selectedJokerUniqueId, List<CardData> cardsToDiscard)
        {
            Debug.Log("Calling DiscardHoleCard");
            var cardEntities = cardsToDiscard.Select(c => new CardEntity(c.Suit, c.Rank)).ToList();
            await client.DiscardHoleCard(jokerUserId, selectedJokerUniqueId, cardEntities);
        }
        
        #endregion
        
        #region RPC callbacks
        
        public void OnJoinRoom(PlayerEntity player, int playerCount, List<JokerEntity> jokerEntities)
        {
            if (Self == null)
            {
                Self = new PlayerData(player);
                UserName.Value = "";
            }
            
            jokers = jokerEntities.Select(x => new JokerData(x)).ToList();
            Debug.Log($"{player.Name}:{player.Id} joined room {player.RoomId}");
            if (Self.Id == player.Id)
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

        public void OnGetAllPlayers(List<PlayerEntity> playerEntities)
        {
            Debug.Log($"Player count: {playerEntities.Count}");
        }

        public void OnGameStart(List<PlayerEntity> playerEntities, PlayerEntity currentPlayer, Enums.GameStateEnum gameState, int roundNumber, bool isFirstRound)
        {
            Debug.Log("Game started");
            GameState = gameState;
            CurrentRound = roundNumber;
            UpdatePlayerCount = null;
            players = playerEntities.Select(p => new PlayerData(p)).ToList();
            Self = new PlayerData(playerEntities.First(x => x.Id == Self.Id));
            CurrentPlayer = new PlayerData(currentPlayer);
            onFinishStart?.Invoke(isFirstRound);
        }

        public void OnCancelGameStart()
        {
            Debug.Log("Cancelled");
        }

        public void OnDoAction(Enums.CommandTypeEnum commandType, List<PlayerEntity> playerEntities, Guid previousPlayerId, Guid currentPlayerId, List<PotEntity> pots, List<CardEntity> communityCards, Enums.GameStateEnum gameState, bool isError, string actionMessage, List<WinningHandEntity> winnerList)
        {
            var isGameOver = false;
            var gameOverByFold = false;
            Debug.Log($"Doing action {commandType}");
            GameState = gameState;
            players = playerEntities.Select(p => new PlayerData(p)).ToList();
            Self = new PlayerData(playerEntities.First(x => x.Id == Self.Id));
            if (isError)
                ShowMessage?.Invoke(actionMessage, null);
            else if (winnerList.Count == 1 && winnerList.First().HandRanking == Enums.HandRankingType.Nothing)
            {
                Debug.Log("Game over");
                var player = playerEntities.First(x => x.Id == winnerList.First().Winner.Id);
                ShowMessage?.Invoke($"{player.Name} is the winner!", null);
                isGameOver = true;
                gameOverByFold = true;
            }
            else if (winnerList.Count > 0 && winnerList.First().HandRanking != Enums.HandRankingType.Nothing)
            {
                Debug.Log("Hand chosen");
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
                            if (tie == winner.TiedWith.Last())
                            {
                                sb2Eng.Append($"{tie.Name}");
                                sb2Jap.Append($"{tie.Name}");
                            }
                            else
                            {
                                sb2Eng.Append($"{tie.Name} and");
                                sb2Jap.Append($"{tie.Name}と");
                            }
                        }

                        sbEng.Append($"Players {sb2Eng} tied with a {winner.HandRanking} and won {winner.PotToTiedWith} each from {winner.PotName}.");
                        sbJap.Append($"プレイヤー{sb2Jap}は{winner.HandRanking.GetDescription()}のハンドを持って引き分けしてそれぞれ{winner.PotName}から{winner.PotToTiedWith}をもらった。");
                    }
                }

                ShowMessage?.Invoke($"{sbEng}\n{sbJap}", null);
                isGameOver = true;
            }

            var cards = new List<CardData>();
            foreach (var card in communityCards)
            {
                if (card != null)
                    cards.Add(new CardData(card));
            }

            UpdateGameUi?.Invoke(commandType, currentPlayerId == Self.Id, previousPlayerId, currentPlayerId, cards, isError);
            UpdatePots?.Invoke(pots);
            if (isGameOver)
                OnGameOverAction?.Invoke(gameOverByFold);
        }

        public void OnBuyJoker(PlayerEntity player, JokerEntity joker)
        {
            Self = new PlayerData(player);
        }

        public void OnUseJoker(PlayerEntity jokerUser, List<PlayerEntity> targets, JokerEntity joker, List<PotEntity> pots, string actionMessage)
        {
            if (jokerUser.Id == Self.Id)
                Self = new PlayerData(jokerUser);
            else if (targets.FirstOrDefault(target => target.Id == Self.Id) != null)
                Self = new PlayerData(targets.FirstOrDefault(target => target.Id == Self.Id));
            
            for (int i = 0; i < players.Count; i++)
            {
                var target = targets.FirstOrDefault(x => x.Id == players[i].Id);
                if (target != null)
                    players[i] = new PlayerData(target);
            }
            UpdatePots?.Invoke(pots);
            if (joker.JokerAbilityEntities.First().AbilityEffects.First().HandInfluenceType == Enums.HandInfluenceTypeEnum.DiscardThenDraw)
            {
                ShowMessage?.Invoke(actionMessage, null);
                OnUseJokerAction?.Invoke();
            }
            else
            {
                var sb = new StringBuilder(actionMessage);
                if (jokerUser.Id == Self.Id)
                {
                    var effect = joker.JokerAbilityEntities.First().AbilityEffects.First();
                    sb.AppendLine();
                    sb.Append($"Choose {effect.EffectValue} hole card to discard.\n{effect.EffectValue}枚のカードを捨てるので選んでください。");
                }

                ShowMessage?.Invoke(sb.ToString(), null);
                OnUseJokerDrawAction?.Invoke(new JokerData(joker));
            }
        }

        public void OnDiscardHoleCard(PlayerEntity jokerUser, List<CardEntity> discardedCards, string message)
        {
            if (jokerUser.Id == Self.Id)
                Self = new PlayerData(jokerUser);
            
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Id == jokerUser.Id)
                    players[i] = new PlayerData(jokerUser);
            }
            
            ShowMessage?.Invoke(message, null);
            OnUseJokerAction?.Invoke();
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