using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.Player;
using THE.SceneControllers;
using THE.Utilities;
using Unity.VisualScripting.Dependencies.NCalc;
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
            ConfirmHand,
            Quit,
            Cancel
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
        [SerializeField] private Button confirmHandButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text currentTurnText;
        [SerializeField] private Text potText;
        [SerializeField] private Text gameStateText;
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
            
            confirmHandButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnClickButton(ButtonTypeEnum.ConfirmHand))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            quitButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OnClickButton(ButtonTypeEnum.Quit))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            betAmountInput.OnValueChangedAsAsyncEnumerable()
                .Where(x => gamingHubReceiver.BetAmount.Value != int.Parse(x))
                .Subscribe(x => gamingHubReceiver.BetAmount.Value = int.Parse(x))
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
            
            confirmHandButton.gameObject.SetActive(false);
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);

            gamingHubReceiver.UpdateGameUi = UpdateUi;
            gamingHubReceiver.ShowMessage = ShowMessage;
            gamingHubReceiver.ShowPlayerHands = ShowAllPlayersCards;
        }

        public void Initialize(bool isMyTurn, PlayerData currentPlayerEntity)
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
                playerList.Add(playerObject);
            }
            UpdateUi(isMyTurn, null, currentPlayerEntity, 0, null);
            playerList.ForEach(x => x.ChangeCardVisibility(gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet));
        }
        
        private void UpdateBets(PlayerData playerEntity)
        {
            playerList.First(x => x.PlayerId == playerEntity.Id).UpdateBet(playerEntity.CurrentBet);
        }

        private void UpdateUi(bool isMyTurn, PlayerData previousPlayerEntity, PlayerData currentPlayerEntity, int currentPot, List<CardData> communityCards)
        {
            gameStateText.text = $"Current state: {gamingHubReceiver.GameState}";
            switch (gamingHubReceiver.GameState)
            {
                case Enums.GameStateEnum.BlindBet:
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType == ButtonTypeEnum.Bet));
                    break;
                case Enums.GameStateEnum.PreFlop:
                    playerList.ForEach(player => player.InitializeCards(OnCardSelected));
                    buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Check && x.ButtonType != ButtonTypeEnum.Bet));
                    break;
                case Enums.GameStateEnum.TheFlop:
                    communityCardList[0].gameObject.SetActive(true);
                    communityCardList[0].Initialize(communityCards[0], true, OnCardSelected);
                    communityCardList[1].gameObject.SetActive(true);
                    communityCardList[1].Initialize(communityCards[1], true, OnCardSelected);
                    communityCardList[2].gameObject.SetActive(true);
                    communityCardList[2].Initialize(communityCards[2], true, OnCardSelected);
                    break;
                case Enums.GameStateEnum.TheTurn:
                    communityCardList[3].gameObject.SetActive(true);
                    communityCardList[3].Initialize(communityCards[3], true, OnCardSelected);
                    break;
                case Enums.GameStateEnum.TheRiver:
                    communityCardList[4].gameObject.SetActive(true);
                    communityCardList[4].Initialize(communityCards[4], true, OnCardSelected);
                    break;
                case Enums.GameStateEnum.Showdown:
                    HighlightCards();
                    ShowMessage("Choose your hand");
                    break;
            }

            if (gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet && gamingHubReceiver.GameState != Enums.GameStateEnum.PreFlop)
                buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(x.ButtonType != ButtonTypeEnum.Bet));

            if (gamingHubReceiver.GameState == Enums.GameStateEnum.Showdown)
            {
                buttonList.ForEach(x => x.ButtonObject.gameObject.SetActive(false));
                confirmHandButton.gameObject.SetActive(true);
                confirmHandButton.interactable = false;
                gamingHubReceiver.Self.CanSelectCard = true;
            }

            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;
            
            currentTurnText.text = $"Current turn: {currentPlayerEntity.Name}";
            potText.text = $"Pot: {currentPot}";
            if (previousPlayerEntity != null)
                UpdateBets(previousPlayerEntity);
        }

        private void OnCardSelected(bool isSelected, CardData cardEntity)
        {
            if (isSelected)
                selectedCards.Add(cardEntity);
            else
                selectedCards.Remove(cardEntity);

            confirmHandButton.interactable = selectedCards.Count == 5;
            gamingHubReceiver.Self.CanSelectCard = selectedCards.Count < 5;
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
                await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
                return;
            }

            if (buttonType == ButtonTypeEnum.ConfirmHand)
            {
                gameStateText.text = $"Current state: Waiting";
                confirmHandButton.interactable = false;
                await gamingHubReceiver.ChooseHand(gamingHubReceiver.Self.Id, selectedCards);
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
                betAmountInput.gameObject.SetActive(true);
                confirmAmountButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            }
            else
            {
                //todo get target id
                await gamingHubReceiver.DoAction(currentAction, Guid.Empty);
            }
        }

        private async UniTaskVoid ConfirmAmount()
        {
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            //todo get target id
            await gamingHubReceiver.DoAction(currentAction, Guid.Empty);
        }
        
        private void CancelBet()
        {
            buttonList.ForEach(x => x.ButtonObject.interactable = true);
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gamingHubReceiver.BetAmount.Value = 0;
        }
        
        private void HighlightCards()
        {
            communityCardList.ForEach(card => card.HighlightCard());
            playerList.First(player => player.PlayerId == gamingHubReceiver.Self.Id).HighlightCards();
        }

        private void ShowAllPlayersCards()
        {
            playerList.ForEach(player => player.ShowCards());
        }
    }
}