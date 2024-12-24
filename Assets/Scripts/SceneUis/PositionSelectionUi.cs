using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class PositionSelectionUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI newPositionText;
        [SerializeField] private GameObject positionCellPrefab;
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject positionCellParent;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        
        private List<int> newPositions = new();
        private List<PositionCellUi> positionCells = new();

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

        public void ShowUi(List<PlayerData> playerList, int maxSelectionCount, Action<List<int>> onConfirm, Action onClose)
        {
            newPositionText.text = "New Position: -";
            maxSelection = maxSelectionCount;
            onConfirmAction = onConfirm;
            onCloseAction = onClose;
            
            var index = 0;
            foreach (var playerData in playerList)
            {
                var positionCell = Instantiate(positionCellPrefab, positionCellParent.transform).GetComponent<PositionCellUi>();
                positionCell.Initialize(playerData);
                positionCell.OnSelectAction = OnSelect;
                positionCell.OnDeselectAction = OnDeselect;
                positionCells.Add(positionCell);
                index++;
            }
            contents.SetActive(true);
            confirmButton.interactable = newPositions.Count == maxSelection;
        }

        public void Reset()
        {
            newPositions.Clear();
            positionCells.Clear();
        }
        
        private void ConfirmSelection()
        {
            onConfirmAction?.Invoke(new List<int>(newPositions));
            newPositions.Clear();
            HideUi();
        }

        private void CancelSelection()
        {
            onCloseAction?.Invoke();
            newPositions.Clear();
            positionCells.ForEach(position => position.Reset());
            HideUi();
        }
        
        private void HideUi()
        {
            foreach (Transform child in positionCellParent.transform)
                Destroy(child.gameObject);
            
            positionCells.Clear();
            contents.SetActive(false);
        }

        private void OnSelect(int position)
        {
            newPositionText.text = $"New Position: {position}";
            newPositions.Add(position);
            confirmButton.interactable = newPositions.Count == maxSelection;
        }
        
        private void OnDeselect(int position)
        {
            newPositionText.text = $"New Position: -";
            newPositions.Remove(position);
            confirmButton.interactable = newPositions.Count == maxSelection;
        }
    }
}