using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.MagicOnion.Shared.Utilities;
using THE.Player;
using THE.SceneControllers;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class GameUi : BaseLayoutUi
    {
        private enum ButtonTypeEnum
        {
            Check,
            Bet,
            Fold,
            Call,
            Raise,
            AllIn,
            UseJoker,
            Quit,
            PlayAgain
        }
        
        [Serializable]
        private class ButtonClass
        {
            public ButtonTypeEnum ButtonType;
            public Button ButtonObject;
        }
        
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<CardClass> communityCardList;
        [SerializeField] private List<ButtonClass> buttonList;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Text currentTurnText;
        [SerializeField] private Text potText;
        [SerializeField] private Text gameStateText;
        [SerializeField] private GameObject betRoot;
        [SerializeField] private InputField betAmountInput;
        [SerializeField] private Button confirmAmountButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text commandText;
        [SerializeField] private JokerListUi jokerListUi;
        
        private readonly List<PlayerClass> playerList = new();
        private GamingHubReceiver gamingHubReceiver;
        private Enums.CommandTypeEnum currentAction;
        private JokerData selectedJoker;

        private void Awake()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            foreach (var button in buttonList)
            {
                button.ButtonObject.OnClickAsAsyncEnumerable()
                    .Subscribe(_ => OnClickButton(button.ButtonType))
                    .AddTo(this.GetCancellationTokenOnDestroy());
            }
            
            quitButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnClickButton(ButtonTypeEnum.Quit))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            playAgainButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnClickButton(ButtonTypeEnum.PlayAgain))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            betAmountInput.OnValueChangedAsAsyncEnumerable()
                .Where(x =>
                {
                    int.TryParse(x, out var val);
                    return gamingHubReceiver.BetAmount.Value != val;
                })
                .Subscribe(x =>
                {
                    int.TryParse(x, out var val);
                    gamingHubReceiver.BetAmount.Value = val;
                })
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            gamingHubReceiver.BetAmount
                .Subscribe(x => betAmountInput.text = x.ToString())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            confirmAmountButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => ConfirmAmount())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelBet())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            playAgainButton.gameObject.SetActive(false);

            gamingHubReceiver.UpdateGameUi = UpdateUi;
            gamingHubReceiver.UpdatePots = UpdatePots;
            gamingHubReceiver.ShowMessage = ShowMessage;
            gamingHubReceiver.OnGameOverAction = OnGameOver;
            gamingHubReceiver.OnUseJokerAction = OnUseJoker;
            gamingHubReceiver.OnUseJokerDrawAction = OnUseJokerDraw;
        }

        public void Initialize()
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
                playerList.Add(playerObject);
            }
            UpdateUi(0, gamingHubReceiver.IsMyTurn, Guid.Empty, gamingHubReceiver.CurrentPlayer.Id, null, false);
            UpdatePots(new List<PotEntity> { new(Guid.Empty, 0, 0, false, null) });
            playerList.ForEach(x => x.ChangeCardVisibility(gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet));
        }

        private void UpdateUi(Enums.CommandTypeEnum previousCommand, bool isMyTurn, Guid previousPlayerEntityId, Guid currentPlayerEntityId, List<CardData> communityCards, bool isError)
        {
            quitButton.interactable = true;
            var players = gamingHubReceiver.GetPlayerList();
            var currentPlayer = players.First(x => x.Id == currentPlayerEntityId);
            gameStateText.text = $"Current state: {gamingHubReceiver.GameState}";
            if (previousPlayerEntityId != Guid.Empty && !isError)
            {
                var previousPlayer = players.First(x => x.Id == previousPlayerEntityId);
                commandText.text = previousCommand switch
                {
                    Enums.CommandTypeEnum.Raise or Enums.CommandTypeEnum.SmallBlindBet or Enums.CommandTypeEnum.BigBlindBet => $"Player {previousPlayer.Name} raised {previousPlayer.RaiseAmount}",
                    Enums.CommandTypeEnum.AllIn => $"Player {previousPlayer.Name} went all in",
                    _ => $"Player {previousPlayer.Name} {previousCommand}ed"
                };
            }

            switch (gamingHubReceiver.GameState)
            {
                case Enums.GameStateEnum.BlindBet:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType == ButtonTypeEnum.Bet));
                    break;
                case Enums.GameStateEnum.PreFlop:
                    playerList.ForEach(player => player.InitializeCards());
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Check && x.ButtonType != ButtonTypeEnum.Bet));
                    if (gamingHubReceiver.Self.PlayerRole == Enums.PlayerRoleEnum.BigBlind)
                    {
                        buttonList.First(x => x.ButtonType == ButtonTypeEnum.Call).ButtonObject.gameObject.SetActive(players.Any(x => x.LastCommand == Enums.CommandTypeEnum.Raise));
                        if (players.Any(x => x.LastCommand == Enums.CommandTypeEnum.Call) && players.All(x => x.LastCommand != Enums.CommandTypeEnum.Raise))
                            buttonList.First(x => x.ButtonType == ButtonTypeEnum.Check).ButtonObject.gameObject.SetActive(true);
                    }
                    break;
                case Enums.GameStateEnum.TheFlop:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Call).ButtonObject.gameObject.SetActive(players.Any(x => x.LastCommand is Enums.CommandTypeEnum.Raise or Enums.CommandTypeEnum.AllIn));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Check).ButtonObject.gameObject.SetActive(players.All(x => x.LastCommand != Enums.CommandTypeEnum.Call && x.LastCommand != Enums.CommandTypeEnum.Raise && x.LastCommand != Enums.CommandTypeEnum.AllIn));
                    communityCardList[0].gameObject.SetActive(true);
                    communityCardList[0].Initialize(communityCards[0], true);
                    communityCardList[1].gameObject.SetActive(true);
                    communityCardList[1].Initialize(communityCards[1], true);
                    communityCardList[2].gameObject.SetActive(true);
                    communityCardList[2].Initialize(communityCards[2], true);
                    break;
                case Enums.GameStateEnum.TheTurn:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Call).ButtonObject.gameObject.SetActive(players.Any(x => x.LastCommand is Enums.CommandTypeEnum.Raise or Enums.CommandTypeEnum.AllIn));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Check).ButtonObject.gameObject.SetActive(players.All(x => x.LastCommand != Enums.CommandTypeEnum.Call && x.LastCommand != Enums.CommandTypeEnum.Raise && x.LastCommand != Enums.CommandTypeEnum.AllIn));
                    communityCardList[3].gameObject.SetActive(true);
                    communityCardList[3].Initialize(communityCards[3], true);
                    break;
                case Enums.GameStateEnum.TheRiver:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Call).ButtonObject.gameObject.SetActive(players.Any(x => x.LastCommand is Enums.CommandTypeEnum.Raise or Enums.CommandTypeEnum.AllIn));
                    buttonList.First(x => x.ButtonType == ButtonTypeEnum.Check).ButtonObject.gameObject.SetActive(players.All(x => x.LastCommand != Enums.CommandTypeEnum.Call && x.LastCommand != Enums.CommandTypeEnum.Raise && x.LastCommand != Enums.CommandTypeEnum.AllIn));
                    communityCardList[4].gameObject.SetActive(true);
                    communityCardList[4].Initialize(communityCards[4], true);
                    break;
                case Enums.GameStateEnum.Showdown:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(false));
                    for (var i = 0; i < communityCardList.Count; i++)
                    {
                        if (communityCardList[i].gameObject.activeSelf)
                            continue;
                        communityCardList[i].gameObject.SetActive(true);
                        communityCardList[i].Initialize(communityCards[i], true);
                    }
                    break;
                case Enums.GameStateEnum.GameOver:
                    break;
            }

            buttonList.First(button => button.ButtonType == ButtonTypeEnum.UseJoker).ButtonObject.gameObject.SetActive(true);
            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;

            
            currentTurnText.text = $"Current player: {currentPlayer.Name}";
            if (previousPlayerEntityId != Guid.Empty)
                UpdatePlayers();
        }

        private void UpdatePots(List<PotEntity> pots)
        {
            var sb = new StringBuilder();
            var index = 'A';
            for (var i = pots.Count - 1; i >= 0; i--)
            {
                if (i == pots.Count - 1)
                    sb.Append($"Main pot: {pots[i].PotAmount} ");
                else
                {
                    sb.Append($"Side pot {index}: {pots[i].PotAmount} ");
                    index++;
                }
            }
            potText.text = sb.ToString();
        }
        
        private void UpdatePlayers()
        {
            var players = gamingHubReceiver.GetPlayerList();
            foreach (var player in playerList)
            {
                player.UpdatePlayerUi(players.First(x => x.Id == player.PlayerData.Id));
            }
        }

        private async UniTaskVoid OnClickButton(ButtonTypeEnum buttonType)
        {
            //quitButton.interactable = false;
            buttonList.ForEach(x => x.ButtonObject.interactable = false);
            
            if (buttonType == ButtonTypeEnum.Quit)
            {
                ShowConfirmation("Are you sure you want to quit?\nやめますか？", async () =>
                {
                    await gamingHubReceiver.LeaveRoom(() => StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene")), OnDisconnect);    
                }, () =>
                {
                    quitButton.interactable = true;
                    buttonList.ForEach(x => x.ButtonObject.interactable = true);
                });
                
                return;
            }

            if (buttonType == ButtonTypeEnum.PlayAgain)
            {
                playAgainButton.interactable = false;
                await gamingHubReceiver.StartGame(false, OnGameStarted, OnDisconnect);
                return;
            }

            if (buttonType is ButtonTypeEnum.UseJoker)
            {
                OpenMyJokerList();
                return;
            }

            currentAction = buttonType switch
            {
                ButtonTypeEnum.Check => Enums.CommandTypeEnum.Check,
                ButtonTypeEnum.Bet => gamingHubReceiver.GameState == Enums.GameStateEnum.BlindBet && gamingHubReceiver.Self.PlayerRole == Enums.PlayerRoleEnum.SmallBlind
                    ? Enums.CommandTypeEnum.SmallBlindBet
                    : Enums.CommandTypeEnum.BigBlindBet,
                ButtonTypeEnum.Fold => Enums.CommandTypeEnum.Fold,
                ButtonTypeEnum.Call => Enums.CommandTypeEnum.Call,
                ButtonTypeEnum.Raise => Enums.CommandTypeEnum.Raise,
                ButtonTypeEnum.AllIn => Enums.CommandTypeEnum.AllIn,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (buttonType is ButtonTypeEnum.Raise)
            {
                betRoot.gameObject.SetActive(true);
                confirmAmountButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            }
            else
                await gamingHubReceiver.DoAction(currentAction, OnDisconnect);
        }

        private async UniTaskVoid ConfirmAmount()
        {
            if (!gamingHubReceiver.CanPlaceBet())
            {
                ShowMessage("Not enough chips", null);
                return;
            }
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            await gamingHubReceiver.DoAction(currentAction, OnDisconnect);
        }
        
        private void UseJokerAction(JokerData joker)
        {
            selectedJoker = joker;
        }

        private async UniTaskVoid UseJokerToDrawAction(JokerData joker)
        {
            selectedJoker = joker;
            await gamingHubReceiver.UseJoker(joker.UniqueId, new List<Guid> { gamingHubReceiver.Self.Id }, new List<CardData>(), OnDisconnect);
        }

        private async UniTaskVoid OnConfirm(List<Guid> selectedTargets, List<CardData> selectedCards)
        {
            var res = await gamingHubReceiver.UseJoker(selectedJoker.UniqueId, selectedTargets, selectedCards, OnDisconnect);
            if (res == Enums.UseJokerResponseTypeEnum.Success)
                jokerListUi.HideList();
        }

        private async UniTaskVoid OnConfirmDiscard(List<Guid> selectedTargets, List<CardData> selectedCards)
        {
            await gamingHubReceiver.DiscardHoleCard(gamingHubReceiver.Self.Id, selectedCards, OnDisconnect);
            jokerListUi.HideList();
        }

        private void OnUseJoker()
        {
            playerList.ForEach(player =>
            {
                var data = gamingHubReceiver.GetPlayerList().First(x => x.Id == player.PlayerData.Id);
                player.UpdateHoleCards(data);
            });
        }

        private void OnUseJokerDraw(JokerData joker)
        {
            playerList.ForEach(player =>
            {
                var data = gamingHubReceiver.GetPlayerList().First(x => x.Id == player.PlayerData.Id);
                player.UpdateHoleCards(data);
            });
            jokerListUi.ShowJokerConfirmationForDiscard(joker);
        }

        private void CancelBet()
        {
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            quitButton.interactable = true;
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gamingHubReceiver.BetAmount.Value = 0;
        }

        private void OnGameStarted(bool isFirstRound)
        {
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            playAgainButton.gameObject.SetActive(false);
            playAgainButton.interactable = true;
            commandText.text = string.Empty;
            communityCardList.ForEach(card =>
            {
                card.Clear();
                card.gameObject.SetActive(false);
            });

            if (!isFirstRound)
            {
                var allPlayers = gamingHubReceiver.GetPlayerList();
                foreach (Transform child in playerRoot.transform)
                {
                    var playerClass = child.GetComponent<PlayerClass>();
                    if (allPlayers.Find(x => x.Id == playerClass.PlayerData.Id) != null)
                        continue;
                    
                    Destroy(child.gameObject);
                    playerList.RemoveAll(x => x.PlayerData.Id == playerClass.PlayerData.Id);
                }
            }

            //re-initialize
            foreach (var player in gamingHubReceiver.GetPlayerList())
                playerList.First(x => x.PlayerData.Id == player.Id).Initialize(player);
                    
            UpdateUi(0, gamingHubReceiver.IsMyTurn, Guid.Empty, gamingHubReceiver.CurrentPlayer.Id, null, false);
            UpdatePots(new List<PotEntity> { new(Guid.Empty, 0, 0, false, null) });
            playerList.ForEach(x => x.ChangeCardVisibility(gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet));
        }

        private void OnGameOver(bool gameOverByFold)
        {
            buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(false));
            if (gamingHubReceiver.CurrentRound <= Constants.MaxRounds && gamingHubReceiver.Self.Chips > Constants.MinBet)
                playAgainButton.gameObject.SetActive(true);
            playerList.ForEach(player =>
            {
                if (!gameOverByFold)
                    player.ShowCards();
            });
        }

        private void OnDisconnect(string disconnectMessage)
        {
            ShowMessage(disconnectMessage, () =>
            {
                StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene"));    
            });
        }
        
        private void OpenMyJokerList()
        {
            jokerListUi.ShowListForGame(gamingHubReceiver, UseJokerAction, UseJokerToDrawAction, () =>
            {
                quitButton.interactable = true;
                buttonList.ForEach(x => x.ButtonObject.interactable = true);
            }, OnConfirm, OnConfirmDiscard);
        }
    }
}