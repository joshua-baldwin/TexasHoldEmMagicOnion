using System;
using System.Collections.Generic;
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
        
        private List<int> selectedCards = new();
        private List<CardCellUi> cardCells = new();

        private Action<List<int>> onConfirmAction;
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

        public void ShowUi(List<CardData> cards, int maxCardsToSelect, Action<List<int>> onConfirm, Action onClose)
        {
            maxSelection = maxCardsToSelect;
            onConfirmAction = onConfirm;
            onCloseAction = onClose;
            var index = 0;
            foreach (var card in cards)
            {
                var cardCell = Instantiate(cardCellPrefab, cardCellParent.transform).GetComponent<CardCellUi>();
                cardCell.Initialize(card, index, selectedCards.Contains(index));
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
            onConfirmAction?.Invoke(new List<int>(selectedCards));
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

        private void OnSelect(int index)
        {
            selectedCards.Add(index);
            confirmButton.interactable = selectedCards.Count == maxSelection;
        }
        
        private void OnDeselect(int index)
        {
            selectedCards.Remove(index);
            confirmButton.interactable = selectedCards.Count == maxSelection;
        }
    }
}
