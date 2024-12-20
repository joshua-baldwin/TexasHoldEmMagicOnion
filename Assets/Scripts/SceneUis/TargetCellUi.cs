using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class TargetCellUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text selectText;
        
        private PlayerData playerData;
        public Action<Guid> OnSelectAction;
        public Action<Guid> OnDeselectAction;

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
            nameText.text = playerData.Name;
        }

        private void Select()
        {
            if (isSelected)
            {
                OnDeselectAction?.Invoke(playerData.Id);
                selectText.text = "Select";
            }
            else
            {
                OnSelectAction?.Invoke(playerData.Id);
                selectText.text = "Deselect";
            }

            isSelected = !isSelected;
        }
    }
}
