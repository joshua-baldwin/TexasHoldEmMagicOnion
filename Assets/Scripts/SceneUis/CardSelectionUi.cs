using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.Player;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class CardSelectionUi : MonoBehaviour
    {
        [SerializeField] private GameObject cardCellPrefab;
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject cardCellParent;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        
        private List<CardData> selectedCards = new();
        private List<CardCellUi> cardCells = new();

        private Action<List<CardData>> onConfirmAction;
        private Action<List<CardData>> onConfirmDiscardAction;
        private Action onCloseAction;
        private int maxSelection;

        private void Awake()
        {
            confirmButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => ConfirmSelection())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelSelection())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowUi(List<CardData> cards, int maxCardsToSelect, Action<List<CardData>> onConfirm, Action onClose)
        {
            maxSelection = maxCardsToSelect;
            onConfirmAction = onConfirm;
            onCloseAction = onClose;
            var index = 0;
            foreach (var card in cards)
            {
                var cardCell = Instantiate(cardCellPrefab, cardCellParent.transform).GetComponent<CardCellUi>();
                cardCell.Initialize(card, index, selectedCards.Any(x => x.Rank == card.Rank && x.Suit == card.Suit));
                cardCell.OnSelectAction = OnSelect;
                cardCell.OnDeselectAction = OnDeselect;
                cardCells.Add(cardCell);
                index++;
            }
            contents.SetActive(true);
            confirmButton.interactable = selectedCards.Count == maxSelection;
        }

        public void Reset()
        {
            selectedCards.Clear();
            cardCells.Clear();
        }

        private void ConfirmSelection()
        {
            onConfirmAction?.Invoke(new List<CardData>(selectedCards));
            selectedCards.Clear();
            HideUi();
        }

        private void CancelSelection()
        {
            onCloseAction?.Invoke();
            selectedCards.Clear();
            cardCells.ForEach(card => card.Reset());
            HideUi();
        }

        private void HideUi()
        {
            foreach (Transform child in cardCellParent.transform)
                Destroy(child.gameObject);
            
            cardCells.Clear();
            contents.SetActive(false);
        }

        private void OnSelect(CardData cardData)
        {
            selectedCards.Add(cardData);
            confirmButton.interactable = selectedCards.Count == maxSelection;
        }
        
        private void OnDeselect(CardData cardData)
        {
            selectedCards.Remove(cardData);
            confirmButton.interactable = selectedCards.Count == maxSelection;
        }
    }
}
