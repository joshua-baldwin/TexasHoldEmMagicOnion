using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.Player;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class CardCellUi : MonoBehaviour
    {
        [SerializeField] private Image suitImage;
        [SerializeField] private Text rankText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject highlight;
        
        private (CardData CardData, int HoleCardIndex) card;
        public Action<int> OnSelectAction;
        public Action<int> OnDeselectAction;

        private bool isSelected;

        private void Awake()
        {
            selectButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => Select())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        private void OnEnable()
        {
            StartCoroutine(LoadFromResourcesFolder(card.CardData.Suit));
        }

        public void Initialize(CardData data, int index, bool wasSelected)
        {
            rankText.text = data.Rank.GetDescription();
            card = (data, index);
            if (wasSelected)
            {
                isSelected = true;
                highlight.SetActive(true);
            }
        }

        public void Reset()
        {
            isSelected = false;
            highlight.SetActive(false);
        }
        
        private void Select()
        {
            if (isSelected)
            {
                OnDeselectAction?.Invoke(card.HoleCardIndex);
                highlight.SetActive(false);
            }
            else
            {
                OnSelectAction?.Invoke(card.HoleCardIndex);
                highlight.SetActive(true);
            }

            isSelected = !isSelected;
        }
        
        private IEnumerator LoadFromResourcesFolder(Enums.CardSuitEnum suit)
        {
            var loadAsync = Resources.LoadAsync($"Suits/{suit}", typeof(Sprite));
            
            while (!loadAsync.isDone)
            {
                yield return null;
            }

            if (loadAsync.asset != null)
            {
                suitImage.sprite = loadAsync.asset as Sprite;
            }
        }
    }
}