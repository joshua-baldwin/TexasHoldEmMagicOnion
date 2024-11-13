using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.Player;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneControllers
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
        [SerializeField] private List<ButtonClass> buttonList;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text currentTurnText;
        [SerializeField] private Text potText;
        [SerializeField] private InputField betAmountInput;
        [SerializeField] private Button confirmAmountButton;
        [SerializeField] private Button cancelButton;
        
        private readonly List<PlayerClass> playerList = new();
        private GamingHubReceiver gamingHubReceiver;
        private Enums.CommandTypeEnum currentAction;

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
                .Where(x => gamingHubReceiver.BetAmount.Value != int.Parse(x))
                .Subscribe(x => gamingHubReceiver.BetAmount.Value = int.Parse(x))
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            confirmAmountButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => ConfirmAmount())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelBet())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);

            gamingHubReceiver.UpdateGameUi = UpdateUi;
        }

        public void Initialize(bool isMyTurn, PlayerEntity playerEntity)
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
                playerList.Add(playerObject);
            }
            UpdateUi(isMyTurn, playerEntity, 0);
            playerList.ForEach(x => x.ChangeCardVisibility(gamingHubReceiver.GameState != Enums.GameStateEnum.BlindBet));
            if (gamingHubReceiver.GameState == Enums.GameStateEnum.BlindBet)
            {
                buttonList.ForEach(x =>
                {
                    if (x.ButtonType != ButtonTypeEnum.Bet)
                        x.ButtonObject.gameObject.SetActive(false);
                });
            }
        }

        private void UpdateUi(bool isMyTurn, PlayerEntity playerEntity, int currentPot)
        {
            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;
            
            currentTurnText.text = $"Current turn: {playerEntity.Name}";
            potText.text = $"Pot: {currentPot}";
            playerList.First(x => x.PlayerId == playerEntity.Id).UpdateBet(playerEntity.CurrentBet);
        }

        private async UniTaskVoid OnClickButton(ButtonTypeEnum buttonType)
        {
            if (buttonType == ButtonTypeEnum.Quit)
                await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
            
            currentAction = buttonType switch
            {
                ButtonTypeEnum.Check => Enums.CommandTypeEnum.Check,
                ButtonTypeEnum.Bet => Enums.CommandTypeEnum.Bet,
                ButtonTypeEnum.Fold => Enums.CommandTypeEnum.Fold,
                ButtonTypeEnum.Call => Enums.CommandTypeEnum.Call,
                ButtonTypeEnum.Raise => Enums.CommandTypeEnum.Raise,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (buttonType is ButtonTypeEnum.Bet or ButtonTypeEnum.Call)
            {
                betAmountInput.gameObject.SetActive(true);
                confirmAmountButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            }
            else
            {
                await gamingHubReceiver.DoAction(currentAction);
            }
        }

        private async UniTaskVoid ConfirmAmount()
        {
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            await gamingHubReceiver.DoAction(currentAction);
        }
        
        private void CancelBet()
        {
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            gamingHubReceiver.BetAmount.Value = 0;
        }
    }
}