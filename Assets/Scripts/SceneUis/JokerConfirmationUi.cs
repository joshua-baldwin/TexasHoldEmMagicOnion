using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.Player;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class JokerConfirmationUi : MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private Button selectTargetButton;
        [SerializeField] private Button selectCardsButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject buttonRoot;
        [SerializeField] private TargetSelectionUi targetSelectionUi;
        [SerializeField] private CardSelectionUi cardSelectionUi;
        [SerializeField] private Button closeButton;
        
        private GamingHubReceiver gamingHubReceiver;
        public Func<List<Guid>, List<int>, UniTaskVoid> OnConfirmAction;
        private JokerData jokerData;

        private void Awake()
        {
            selectTargetButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OpenTargetSelection())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            selectCardsButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OpenCardSelection())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            confirmButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => Confirm())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => HideUi())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowUi(GamingHubReceiver receiver, JokerData joker)
        {
            gamingHubReceiver = receiver;
            jokerData = joker;
            contents.SetActive(true);
            selectTargetButton.interactable = joker.TargetType is Enums.TargetTypeEnum.SinglePlayer or Enums.TargetTypeEnum.MultiPlayers;
            selectCardsButton.interactable = joker.JokerType == Enums.JokerTypeEnum.Hand;
        }
        
        public void HideUi()
        {
            SetButtonsInteractable(true);
            contents.SetActive(false);
        }

        private void OpenTargetSelection()
        {
            SetButtonsInteractable(false);
            targetSelectionUi.ShowUi(gamingHubReceiver.GetPlayerList(), () => SetButtonsInteractable(true));
        }
        
        private void OpenCardSelection()
        {
            SetButtonsInteractable(false);
            cardSelectionUi.ShowUi(gamingHubReceiver.Self.HoleCards, () => SetButtonsInteractable(true));
        }

        private void Confirm()
        {
            SetButtonsInteractable(false);
            var targets = jokerData.TargetType == Enums.TargetTypeEnum.Self
                ? new List<Guid> { gamingHubReceiver.Self.Id }
                : targetSelectionUi.GetSelectedTargets;
            OnConfirmAction?.Invoke(targets, cardSelectionUi.GetSelectedCards);
            targetSelectionUi.Reset();
            cardSelectionUi.Reset();
            HideUi();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (interactable)
            {
                selectTargetButton.interactable = jokerData.TargetType is Enums.TargetTypeEnum.SinglePlayer or Enums.TargetTypeEnum.MultiPlayers;
                selectCardsButton.interactable = jokerData.JokerType == Enums.JokerTypeEnum.Hand;
            }
            else
            {
                selectTargetButton.interactable = false;
                selectCardsButton.interactable = false;
            }

            confirmButton.interactable = interactable;
        }
    }
}