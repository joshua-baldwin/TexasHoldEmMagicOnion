using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.Player;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class TargetSelectionUi : MonoBehaviour
    {
        [SerializeField] private GameObject targetCellPrefab;
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject targetParent;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        
        private List<Guid> selectedTargetIds = new();
        private List<TargetCellUi> targetCells = new();
        private Action<List<Guid>> onConfirmAction;
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

        public void ShowUi(List<PlayerData> players, int maxTargetsToSelect, Action<List<Guid>> onConfirm, Action onClose)
        {
            maxSelection = maxTargetsToSelect;
            onConfirmAction = onConfirm;
            onCloseAction = onClose;
            foreach (var player in players)
            {
                var targetCell = Instantiate(targetCellPrefab, targetParent.transform).GetComponent<TargetCellUi>();
                targetCell.Initialize(player);
                targetCell.OnSelectAction = OnSelect;
                targetCell.OnDeselectAction = OnDeselect;
                targetCells.Add(targetCell);
            }
            contents.SetActive(true);
            confirmButton.interactable = selectedTargetIds.Count == maxSelection;
        }

        public void Reset()
        {
            selectedTargetIds.Clear();
            targetCells.Clear();
        }

        private void ConfirmSelection()
        {
            onConfirmAction?.Invoke(new List<Guid>(selectedTargetIds));
            selectedTargetIds.Clear();
            HideUi();
        }

        private void CancelSelection()
        {
            onCloseAction?.Invoke();
            selectedTargetIds.Clear();
            targetCells.ForEach(target => target.Reset());
            HideUi();
        }

        private void HideUi()
        {
            foreach (Transform child in targetParent.transform)
                Destroy(child.gameObject);
            
            targetCells.Clear();
            contents.SetActive(false);
        }

        private void OnSelect(Guid targetId)
        {
            selectedTargetIds.Add(targetId);
            confirmButton.interactable = selectedTargetIds.Count == maxSelection;
        }
        
        private void OnDeselect(Guid targetId)
        {
            selectedTargetIds.Remove(targetId);
            confirmButton.interactable = selectedTargetIds.Count == maxSelection;
        }
    }
}