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
            Quit
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

            gamingHubReceiver.UpdateGameUi = UpdateUi;
        }
        
        public void Initialize(bool isMyTurn)
        {
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
            UpdateUi(isMyTurn);
        }

        private void UpdateUi(bool isMyTurn)
        {
            foreach (var button in buttonList)
                button.ButtonObject.interactable = isMyTurn;
        }

        private async UniTaskVoid OnClickButton(ButtonTypeEnum buttonType)
        {
            if (buttonType == ButtonTypeEnum.Quit)
                await gamingHubReceiver.LeaveRoom(() => StartCoroutine(UtilityMethods.LoadAsyncScene("StartScene")));
            else
            {
                var commandType = buttonType switch
                {
                    ButtonTypeEnum.Check => Enums.CommandTypeEnum.Check,
                    ButtonTypeEnum.Bet => Enums.CommandTypeEnum.Bet,
                    ButtonTypeEnum.Fold => Enums.CommandTypeEnum.Fold,
                    ButtonTypeEnum.Call => Enums.CommandTypeEnum.Call,
                    ButtonTypeEnum.Raise => Enums.CommandTypeEnum.Raise,
                    _ => throw new ArgumentOutOfRangeException()
                };
                await gamingHubReceiver.DoAction(commandType);
            }
        }
    }
}