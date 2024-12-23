using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.Player;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class JokerListUi : MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject jokerPrefab;
        [SerializeField] private GameObject jokerListRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private JokerConfirmationUi jokerConfirmationUi;
        
        private List<JokerClass> jokerList = new();
        
        private Action onCloseAction;
        private GamingHubReceiver gamingHubReceiver;

        private void Awake()
        {
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => HideList())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowListForWaitingRoom(GamingHubReceiver receiver, Func<int, UniTask> buyJokerFunc, bool isAllJokerList)
        {
            gamingHubReceiver = receiver;
            var jokerDataList = isAllJokerList
                ? gamingHubReceiver.GetJokerList()
                : gamingHubReceiver.Self.JokerCards;
            
            foreach (var jokerData in jokerDataList)
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData, !isAllJokerList);
                joker.SetBuyButtonActive(isAllJokerList);
                joker.SetUseButtonActive(false);

                if (isAllJokerList)
                {
                    var myJokerDataList = gamingHubReceiver.Self.JokerCards;
                    joker.BuyJokerAction = buyJokerFunc;
                    joker.SetButtonInteractable(myJokerDataList.Count == 0 || !myJokerDataList.Select(card => card.JokerId).Contains(joker.JokerData.JokerId));
                }

                jokerList.Add(joker);
            }
            
            contents.SetActive(true);   
        }
        
        public void ShowListForGame(GamingHubReceiver receiver, Action<JokerData> useJokerAction, Func<JokerData, UniTaskVoid> useJokerToDrawAction, Action onCloseList, Func<List<Guid>, List<CardData>, UniTaskVoid> onConfirm, Func<List<Guid>, List<CardData>, UniTaskVoid> onConfirmDiscard)
        {
            gamingHubReceiver = receiver;
            var jokerDataList = gamingHubReceiver.Self.JokerCards;
            onCloseAction = onCloseList;
            foreach (var jokerData in jokerDataList)
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData, true);
                joker.SetBuyButtonActive(false);
                joker.SetUseButtonActive(true);
                joker.UseJokerAction = (data) =>
                {
                    if (data.JokerAbilities.First().AbilityEffects.First().HandInfluenceType == Enums.HandInfluenceTypeEnum.DiscardThenDraw)
                    {
                        jokerConfirmationUi.ShowUi(gamingHubReceiver, data, false);
                        jokerConfirmationUi.OnConfirmAction = onConfirm;
                        useJokerAction?.Invoke(data);
                    }
                    else
                    {
                        jokerConfirmationUi.OnConfirmDiscardAction = onConfirmDiscard;
                        useJokerToDrawAction?.Invoke(data);
                    }
                };

                jokerList.Add(joker);
            }
            
            contents.SetActive(true);
        }
        
        public void HideList()
        {
            jokerConfirmationUi.HideUi();
            foreach (Transform child in jokerListRoot.transform)
                Destroy(child.gameObject);
            
            jokerList.Clear();
            contents.SetActive(false);
            onCloseAction?.Invoke();
        }
        
        public void UpdateJokerButton(int jokerId, bool interactable)
        {
            jokerList.First(x => x.JokerData.JokerId == jokerId).SetButtonInteractable(interactable);
        }

        public void ShowJokerConfirmationForDiscard(JokerData jokerData)
        {
            jokerConfirmationUi.ShowUi(gamingHubReceiver, jokerData, true);
        }
    }
}