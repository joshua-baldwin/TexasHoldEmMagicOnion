using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using THE.MagicOnion.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace THE.Player
{
    public class JokerClass : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI remainingUsesText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonText;
        [SerializeField] private Button useButton;
        
        private GamingHubReceiver gamingHubReceiver;
        public JokerData JokerData { get; private set; }
        public Func<int, UniTask> BuyJokerAction;
        public Action<JokerData> UseJokerAction;

        private void Awake()
        {
            buyButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => BuyJoker())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            useButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => UseJoker())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void Initialize(JokerData jokerData, bool ownedJoker)
        {
            JokerData = jokerData;
            nameText.text = $"{JokerData.JokerType} influence";
            costText.text = $"Cost: {JokerData.BuyCost}";
            remainingUsesText.text = ownedJoker
                ? $"Remaining uses: {jokerData.MaxUses - jokerData.CurrentUses}/{jokerData.MaxUses}"
                : $"Max uses: {jokerData.MaxUses}";
            descriptionText.text = JokerData.JokerAbility.GetDescription();
        }

        public void SetBuyButtonActive(bool isActive)
        {
            buyButton.gameObject.SetActive(isActive);
        }
        
        public void SetUseButtonActive(bool isActive)
        {
            useButton.gameObject.SetActive(isActive);
        }

        public void SetButtonInteractable(bool isInteractable)
        {
            if (isInteractable)
            {
                buyButton.interactable = true;
                buyButtonText.text = "Buy";
            }
            else
            {
                buyButton.interactable = false;
                buyButtonText.text = "Purchased";
            }
        }

        private void BuyJoker()
        {
            buyButton.interactable = false;
            BuyJokerAction?.Invoke(JokerData.JokerId);
        }
        
        private void UseJoker()
        {
            UseJokerAction?.Invoke(JokerData);
        }
    }
}