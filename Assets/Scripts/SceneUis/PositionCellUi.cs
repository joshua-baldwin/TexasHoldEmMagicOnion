using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class PositionCellUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI roleText;
        [SerializeField] private TextMeshProUGUI queueOrderText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text selectText;
        
        private PlayerData playerData;
        public Action<int> OnSelectAction;
        public Action<int> OnDeselectAction;

        private bool isSelected;

        private void Awake()
        {
            selectButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => Select())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void Initialize(PlayerData data)
        {
            playerData = data;
            nameText.text = $"Name: {playerData.Name}";
            roleText.text = $"Role: {playerData.PlayerRole}";
            queueOrderText.text = $"Order: {playerData.OrderInQueue}";
        }

        public void Reset()
        {
            isSelected = false;
            selectText.text = "Select";
        }

        private void Select()
        {
            if (isSelected)
            {
                OnDeselectAction?.Invoke(playerData.OrderInQueue);
                selectText.text = "Select";
            }
            else
            {
                OnSelectAction?.Invoke(playerData.OrderInQueue);
                selectText.text = "Deselect";
            }

            isSelected = !isSelected;
        }
    }
}