using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
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
        [SerializeField] private InputField betAmountInput;
        [SerializeField] private Button confirmAmountButton;
        [SerializeField] private Button cancelButton;
        
        private GamingHubReceiver gamingHubReceiver;

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

        public void Initialize(bool isMyTurn, string currentPlayerName)
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
            UpdateUi(isMyTurn, currentPlayerName);
        }

        private void UpdateUi(bool isMyTurn, string currentPlayerName)
        {
            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;
            
            currentTurnText.text = $"Current turn: {currentPlayerName}";
        }

        private async UniTaskVoid OnClickButton(ButtonTypeEnum buttonType)
        {
            if (buttonType == ButtonTypeEnum.Quit)
                await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
            else if (buttonType == ButtonTypeEnum.Bet || buttonType == ButtonTypeEnum.Call)
            {
                //show bet amount input field
                betAmountInput.gameObject.SetActive(true);
                confirmAmountButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
            }
            else
            {
                var commandType = buttonType switch
                {
                    ButtonTypeEnum.Check => Enums.CommandTypeEnum.Check,
                    ButtonTypeEnum.Fold => Enums.CommandTypeEnum.Fold,
                    ButtonTypeEnum.Raise => Enums.CommandTypeEnum.Raise,
                    _ => throw new ArgumentOutOfRangeException()
                };
                await gamingHubReceiver.DoAction(commandType);
            }
        }

        private void ConfirmAmount()
        {
            //todo
        }
        
        private void CancelBet()
        {
            betAmountInput.gameObject.SetActive(false);
            confirmAmountButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);   
        }
    }
}