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
        [SerializeField] private Button closeButton;
        
        private List<Guid> selectedTargetIds = new();
        private Action onCloseAction;

        public List<Guid> GetSelectedTargets => selectedTargetIds;

        private void Awake()
        {
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => HideUi())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowUi(List<PlayerData> players, Action onClose)
        {
            onCloseAction = onClose;
            foreach (var player in players)
            {
                var targetCell = Instantiate(targetCellPrefab, targetParent.transform).GetComponent<TargetCellUi>();
                targetCell.Initialize(player);
                targetCell.OnSelectAction = OnSelect;
                targetCell.OnDeselectAction = OnDeselect;
            }
            contents.SetActive(true);
        }

        public void Reset()
        {
            selectedTargetIds.Clear();
        }

        private void HideUi()
        {
            foreach (Transform child in targetParent.transform)
                Destroy(child.gameObject);
            
            contents.SetActive(false);
            onCloseAction?.Invoke();
        }

        private void OnSelect(Guid targetId)
        {
            selectedTargetIds.Add(targetId);
        }
        
        private void OnDeselect(Guid targetId)
        {
            selectedTargetIds.Remove(targetId);
        }
    }
}