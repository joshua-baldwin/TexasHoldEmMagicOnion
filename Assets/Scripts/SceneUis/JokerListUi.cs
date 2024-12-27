using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.Player;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class JokerListUi : BaseLayoutUi
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject jokerPrefab;
        [SerializeField] private GameObject jokerListRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private JokerConfirmationUi jokerConfirmationUi;
        
        private List<JokerClass> jokerList = new();
        
        private Action onCloseAction;
        private GamingHubReceiver gamingHubReceiver;
        private Action<JokerData> useJokerAction;
        private Func<JokerData,UniTaskVoid> useJokerToDrawFunc;
        private Func<JokerData,UniTaskVoid> useJokerToChangePositionFunc;
        private Func<List<Guid>,List<CardData>,UniTaskVoid> onConfirmFunc;
        private Func<List<Guid>,List<CardData>,UniTaskVoid> onConfirmDiscardFunc;

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
        
        public void ShowListForGame(GamingHubReceiver receiver, Action<JokerData> useJoker, Func<JokerData, UniTaskVoid> useJokerToDraw, Func<JokerData, UniTaskVoid> useJokerToChangePosition, Action onCloseList, Func<List<Guid>, List<CardData>, UniTaskVoid> onConfirm, Func<List<Guid>, List<CardData>, UniTaskVoid> onConfirmDiscard)
        {
            gamingHubReceiver = receiver;
            var jokerDataList = gamingHubReceiver.Self.JokerCards;
            onCloseAction = onCloseList;
            useJokerAction = useJoker;
            useJokerToDrawFunc = useJokerToDraw;
            useJokerToChangePositionFunc = useJokerToChangePosition;
            onConfirmFunc = onConfirm;
            onConfirmDiscardFunc = onConfirmDiscard;
            foreach (var jokerData in jokerDataList)
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData, true);
                joker.SetBuyButtonActive(false);
                joker.SetUseButtonActive(true);
                joker.UseJokerAction = UseJoker;
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

        private void UseJoker(JokerData jokerData)
        {
            if (gamingHubReceiver.GameState == Enums.GameStateEnum.PreFlop && jokerData.ActionInfluenceType == Enums.ActionInfluenceTypeEnum.ChangePosition)
                ShowMessage("Can't use this joker during pre-flop.\n Pre-flopの時にこのジョーカー使えない。", null);
            else
            {
                if (jokerData.HandInfluenceType == Enums.HandInfluenceTypeEnum.DrawThenDiscard)
                {
                    jokerConfirmationUi.OnConfirmDiscardAction = onConfirmDiscardFunc;
                    useJokerToDrawFunc?.Invoke(jokerData);
                }
                else if (jokerData.ActionInfluenceType == Enums.ActionInfluenceTypeEnum.ChangePosition)
                    useJokerToChangePositionFunc?.Invoke(jokerData);
                else
                {
                    jokerConfirmationUi.ShowUi(gamingHubReceiver, jokerData, false);
                    jokerConfirmationUi.OnConfirmAction = onConfirmFunc;
                    useJokerAction?.Invoke(jokerData);
                }
            }
        }
    }
}