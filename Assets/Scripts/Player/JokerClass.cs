using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
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
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyButtonText;
        
        private GamingHubReceiver gamingHubReceiver;
        public JokerData JokerData { get; private set; }
        public Func<int, UniTask> BuyJokerAction;

        private void Awake()
        {
            buyButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => BuyJoker())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void Initialize(JokerData jokerData)
        {
            JokerData = jokerData;
            nameText.text = $"{JokerData.JokerType} influence";
            costText.text = $"Cost: {JokerData.BuyCost}";
            descriptionText.text = JokerData.JokerAbilities.First().GetDescription();
        }

        public void SetButtonActive(bool isActive)
        {
            buyButton.gameObject.SetActive(isActive);
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
    }
}