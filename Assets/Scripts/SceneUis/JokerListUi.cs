using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
        
        private List<JokerClass> jokerList = new();
        
        private Action onCloseAction;
        private GamingHubReceiver gamingHubReceiver;

        private void Awake()
        {
            closeButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => HideList())
                .AddTo(this.GetCancellationTokenOnDestroy());
        }

        public void ShowListForWaitingRoom(IEnumerable<JokerData> jokerDataList, List<JokerEntity> myJokerDataList, Func<int, UniTask> buyJokerFunc, bool isAllJokerList)
        {
            foreach (var jokerData in jokerDataList)
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData, !isAllJokerList);
                joker.SetBuyButtonActive(isAllJokerList);
                joker.SetUseButtonActive(false);

                if (isAllJokerList)
                {
                    joker.BuyJokerAction = buyJokerFunc;
                    joker.SetButtonInteractable(myJokerDataList.Count == 0 || !myJokerDataList.Select(card => card.JokerId).Contains(joker.JokerData.JokerId));
                }

                jokerList.Add(joker);
            }
            
            contents.SetActive(true);   
        }
        
        public void ShowListForGame(IEnumerable<JokerData> jokerDataList, List<PlayerData> players, Action<JokerData> useJokerAction, Action onCloseList)
        {
            onCloseAction = onCloseList;
            foreach (var jokerData in jokerDataList)
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData, true);
                joker.SetBuyButtonActive(false);
                joker.SetUseButtonActive(true);
                joker.UseJokerAction = useJokerAction;

                jokerList.Add(joker);
            }
            
            contents.SetActive(true);
        }
        
        public void HideList()
        {
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
    }
}