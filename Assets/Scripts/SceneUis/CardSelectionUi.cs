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
        [SerializeField] private Button closeButton;
        
        private List<int> selectedCards = new();

        private Action onCloseAction;
        
        public List<int> GetSelectedCards => selectedCards;

        private void Awake()
        {
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => HideUi())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowUi(List<CardData> cards, Action onClose)
        {
            onCloseAction = onClose;
            var index = 0;
            foreach (var card in cards)
            {
                var cardCell = Instantiate(cardCellPrefab, cardCellParent.transform).GetComponent<CardCellUi>();
                cardCell.Initialize(card, index, selectedCards.Contains(index));
                cardCell.OnSelectAction = OnSelect;
                cardCell.OnDeselectAction = OnDeselect;
                index++;
            }
            contents.SetActive(true);
        }

        public void Reset()
        {
            selectedCards.Clear();
        }

        private void HideUi()
        {
            foreach (Transform child in cardCellParent.transform)
                Destroy(child.gameObject);
            
            contents.SetActive(false);
            onCloseAction?.Invoke();
        }

        private void OnSelect(int index)
        {
            selectedCards.Add(index);
        }
        
        private void OnDeselect(int index)
        {
            selectedCards.Remove(index);
        }
    }
}
