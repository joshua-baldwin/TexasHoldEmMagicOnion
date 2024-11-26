using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.Player;
using THE.SceneControllers;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class GameUi : MonoBehaviour
    {
        private enum ButtonTypeEnum
        {
            Check,
            Bet,
            Fold,
            Call,
            Raise,
            Quit,
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
        [SerializeField] private Text currentTurnText;
        [SerializeField] private Text potText;
        [SerializeField] private Text gameStateText;
        [SerializeField] private GameObject betRoot;
        [SerializeField] private InputField betAmountInput;
        [SerializeField] private Button confirmAmountButton;
        [SerializeField] private Button cancelButton;
        
        private readonly List<PlayerClass> playerList = new();
        private readonly List<CardData> selectedCards = new();
        private GamingHubReceiver gamingHubReceiver;
        private Enums.CommandTypeEnum currentAction;
        
        private PopupUi popupUi;

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

            gamingHubReceiver.UpdateGameUi = UpdateUi;
            gamingHubReceiver.ShowMessage = ShowMessage;
            gamingHubReceiver.OnGameOverAction = OnGameOver;
        }

        public void Initialize(bool isMyTurn, Guid currentPlayerEntityId)
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
                playerList.Add(playerObject);
            }
            UpdateUi(isMyTurn, Guid.Empty, currentPlayerEntityId, 0, null);
            playerList.ForEach(x => x.ChangeCardVisibility(gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet));
        }

        private void UpdateUi(bool isMyTurn, Guid previousPlayerEntityId, Guid currentPlayerEntityId, int currentPot, List<CardData> communityCards)
        {
            gameStateText.text = $"Current state: {gamingHubReceiver.GameState}";
            switch (gamingHubReceiver.GameState)
            {
                case Enums.GameStateEnum.BlindBet:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType == ButtonTypeEnum.Bet));
                    break;
                case Enums.GameStateEnum.PreFlop:
                    playerList.ForEach(player => player.InitializeCards());
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Check && x.ButtonType != ButtonTypeEnum.Bet));
                    break;
                case Enums.GameStateEnum.TheFlop:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    communityCardList[0].gameObject.SetActive(true);
                    communityCardList[0].Initialize(communityCards[0], true);
                    communityCardList[1].gameObject.SetActive(true);
                    communityCardList[1].Initialize(communityCards[1], true);
                    communityCardList[2].gameObject.SetActive(true);
                    communityCardList[2].Initialize(communityCards[2], true);
                    break;
                case Enums.GameStateEnum.TheTurn:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    communityCardList[3].gameObject.SetActive(true);
                    communityCardList[3].Initialize(communityCards[3], true);
                    break;
                case Enums.GameStateEnum.TheRiver:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));
                    communityCardList[4].gameObject.SetActive(true);
                    communityCardList[4].Initialize(communityCards[4], true);
                    break;
                case Enums.GameStateEnum.Showdown:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(false));
                    //HighlightCards();
                    break;
            }

            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;

            var players = gamingHubReceiver.GetPlayerList();
            var currentPlayer = players.First(x => x.Id == currentPlayerEntityId);
            currentTurnText.text = $"Current player: {currentPlayer.Name}";
            potText.text = $"Pot: {currentPot}";
            if (previousPlayerEntityId != Guid.Empty)
                UpdateBets();
        }
        
        private void UpdateBets()
        {
            var players = gamingHubReceiver.GetPlayerList();
            foreach (var player in playerList)
            {
                player.UpdateBetAndChips(players.First(x => x.Id == player.PlayerId));
            }
        }

        private void ShowMessage(string message)
        {
            popupUi = FindFirstObjectByType<PopupUi>();
            popupUi.ShowMessage(message);
        }

        private async UniTaskVoid OnClickButton(ButtonTypeEnum buttonType)
        {
            if (buttonType == ButtonTypeEnum.Quit)
            {
                await gamingHubReceiver.LeaveRoom(() => StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene")), OnDisconnect);
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
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (buttonType is ButtonTypeEnum.Bet or ButtonTypeEnum.Raise)
            {
                buttonList.ForEach(x => x.ButtonObject.interactable = false);
                betRoot.gameObject.SetActive(true);
                confirmAmountButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            }
            else
            {
                //todo get target id
                await gamingHubReceiver.DoAction(currentAction, Guid.Empty, OnDisconnect);
            }
        }

        private async UniTaskVoid ConfirmAmount()
        {
            if (!gamingHubReceiver.CanPlaceBet())
            {
                ShowMessage("Not enough chips");
                return;
            }
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            //todo get target id
            await gamingHubReceiver.DoAction(currentAction, Guid.Empty, OnDisconnect);
        }

        private void CancelBet()
        {
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            betRoot.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gamingHubReceiver.BetAmount.Value = 0;
        }
        
        private void HighlightCards()
        {
            communityCardList.ForEach(card => card.HighlightCard());
            playerList.First(player => player.PlayerId == gamingHubReceiver.Self.Id).HighlightCards();
        }

        private void OnGameOver()
        {
            playerList.ForEach(player => player.ShowCards());
            buttonList.ForEach(x => x.ButtonObject.interactable = false);
        }

        private void OnDisconnect()
        {
            StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene"));
            ShowMessage("Disconnected from server");
        }
    }
}