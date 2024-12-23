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
    public class JokerConfirmationUi : BaseLayoutUi
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
        public Func<List<Guid>, List<CardData>, UniTaskVoid> OnConfirmAction;
        public Func<List<Guid>, List<CardData>, UniTaskVoid> OnConfirmDiscardAction;
        private JokerData jokerData;
        private bool isOpen;
        
        private List<CardData> selectedCards = new();
        private List<Guid> selectedTargets = new();

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

        public void ShowUi(GamingHubReceiver receiver, JokerData joker, bool mustDiscard)
        {
            gamingHubReceiver = receiver;
            jokerData = joker;
            contents.SetActive(true);
            SetButtonsInteractable(true);
            if (mustDiscard)
                closeButton.interactable = false;
            isOpen = true;
        }
        
        public void HideUi()
        {
            if (!isOpen)
                return;
            
            SetButtonsInteractable(true);
            contents.SetActive(false);
            isOpen = false;
        }

        private void OpenTargetSelection()
        {
            SetButtonsInteractable(false);
            targetSelectionUi.ShowUi(gamingHubReceiver.GetPlayerList(), jokerData.JokerAbilities.First().AbilityEffects.First().EffectValue, (targets) =>
            {
                selectedTargets = targets;
                SetButtonsInteractable(true);
            }, () =>
            {
                selectedTargets.Clear();
                SetButtonsInteractable(true);
            });
        }
        
        private void OpenCardSelection()
        {
            SetButtonsInteractable(false);
            //TODO assuming only one effect and one ability
            cardSelectionUi.ShowUi(gamingHubReceiver.Self.HoleCards.Concat(gamingHubReceiver.Self.TempHoleCards).ToList(), jokerData.JokerAbilities.First().AbilityEffects.First().EffectValue, (cards) =>
            {
                selectedCards = cards;
                SetButtonsInteractable(true);
            }, () =>
            {
                selectedCards.Clear();
                SetButtonsInteractable(true);
            });
        }

        private void Confirm()
        {
            if (jokerData.JokerType == Enums.JokerTypeEnum.Hand && selectedCards.Count == 0)
            {
                ShowMessage("Please select a hole card to continue.\nホールカードを選択してください。", null);
                return;
            }
            SetButtonsInteractable(false);
            var targets = jokerData.TargetType == Enums.TargetTypeEnum.Self
                ? new List<Guid> { gamingHubReceiver.Self.Id }
                : selectedTargets;
            if (jokerData.JokerAbilities.First().AbilityEffects.First().HandInfluenceType == Enums.HandInfluenceTypeEnum.DiscardThenDraw)
                OnConfirmAction?.Invoke(targets, selectedCards);
            else
                OnConfirmDiscardAction?.Invoke(targets, selectedCards);
            selectedCards.Clear();
            selectedTargets.Clear();
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

            if (jokerData.JokerType == Enums.JokerTypeEnum.Hand)
            {
                //TODO assuming one ability
                confirmButton.interactable = selectedCards.Count == jokerData.JokerAbilities.First().AbilityEffects.First().EffectValue;
            }
            else if (jokerData.JokerType == Enums.JokerTypeEnum.Action)
            {
                confirmButton.interactable = selectedTargets.Count == jokerData.JokerAbilities.First().AbilityEffects.First().EffectValue;
            }
            else if (jokerData.JokerType == Enums.JokerTypeEnum.Info)
            {
                confirmButton.interactable = true;
            }
            else
            {
                confirmButton.interactable = true;
            }
        }
    }
}