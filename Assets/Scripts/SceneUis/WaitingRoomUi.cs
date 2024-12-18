using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TexasHoldEmShared.Enums;
using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Utilities;
using THE.Player;
using THE.SceneControllers;
using THE.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace THE.SceneUis
{
    public class WaitingRoomUi : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text roomName;
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button buyJokerButton;
        [SerializeField] private Button closeJokerListButton;
        [SerializeField] private GameObject allJokerScrollerRoot;
        [SerializeField] private GameObject allJokerListRoot;
        [SerializeField] private Button myJokerListButton;
        [SerializeField] private Button closeMyJokerListButton;
        [SerializeField] private GameObject myJokerScrollerRoot;
        [SerializeField] private GameObject myJokerListRoot;
        [SerializeField] private GameObject jokerPrefab;
        
        private int playerCount;
        private GamingHubReceiver gamingHubReceiver;
        private List<JokerClass> allJokerList = new();
        private List<JokerClass> myJokerList = new();
        private bool jokerListIsOpen;
        
        private PopupUi popupUi;

        private void Awake()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            userName.text = $"Name: {gamingHubReceiver.Self.Name}";
            roomName.text = $"Room id:\n{gamingHubReceiver.Self.RoomId}";
            startButton.interactable = playerCount >= Constants.MinimumPlayers;
            cancelButton.interactable = false;
            startButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => StartAction())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            cancelButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CancelAction())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            leaveButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => LeaveRoom())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            buyJokerButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OpenJokerList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            closeJokerListButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseJokerList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            myJokerListButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OpenMyJokerList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            closeMyJokerListButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseMyJokerList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            gamingHubReceiver.UpdatePlayerCount = UpdatePlayerCount;
            gamingHubReceiver.ShowMessage = ShowMessage;
        }

        public async UniTask Initialize()
        {
            foreach (var jokerData in gamingHubReceiver.GetJokerList())
            {
                var joker = Instantiate(jokerPrefab, allJokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData);
                joker.BuyJokerAction = BuyJokerAction;
                allJokerList.Add(joker);
            }

            
            
            await gamingHubReceiver.GetPlayers(UpdatePlayerCount, OnDisconnect);
        }

        private async UniTask BuyJokerAction(int jokerId)
        {
            var response = await gamingHubReceiver.BuyJoker(jokerId, OnDisconnect);
            allJokerList.First(x => x.JokerData.JokerId == jokerId).SetButtonInteractable(response != Enums.BuyJokerResponseTypeEnum.Success);
        }

        private void UpdatePlayerCount(int count)
        {
            currentPlayerCount.text = $"{count}/{Constants.MaxPlayers}";
            playerCount = count;
            startButton.interactable = playerCount >= Constants.MinimumPlayers;
        }
        
        private void ShowMessage(string message, Action onClose)
        {
            popupUi = FindFirstObjectByType<PopupUi>();
            popupUi.ShowMessage(message, onClose);
        }

        private async UniTaskVoid StartAction()
        {
            startButton.interactable = false;
            cancelButton.interactable = true;
            await gamingHubReceiver.StartGame(true, _ =>
            {
                gamingHubReceiver.UpdatePlayerCount = null;
                StartCoroutine(ClientUtilityMethods.LoadAsyncScene("GameScene"));
            }, OnDisconnect);
        }

        private async UniTaskVoid CancelAction()
        {
            startButton.interactable = true;
            cancelButton.interactable = false;
            await gamingHubReceiver.CancelStartGame(OnDisconnect);
        }
        
        private async UniTaskVoid LeaveRoom()
        {
            await gamingHubReceiver.LeaveRoom(() => StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene")), OnDisconnect);
        }
        
        private void OpenJokerList()
        {
            var heldJokers = gamingHubReceiver.Self.JokerCards;
            foreach (var joker in allJokerList)
                joker.SetButtonInteractable(heldJokers.Count == 0 || !heldJokers.Select(card => card.JokerId).Contains(joker.JokerData.JokerId));
            
            allJokerScrollerRoot.SetActive(true);
        }

        private void CloseJokerList()
        {
            allJokerScrollerRoot.SetActive(false);
        }
        
        private void OpenMyJokerList()
        {
            foreach (Transform child in myJokerListRoot.transform)
                Destroy(child.gameObject);
            myJokerList.Clear();
            
            foreach (var jokerData in gamingHubReceiver.Self.JokerCards.Select(x => new JokerData(x)))
            {
                var joker = Instantiate(jokerPrefab, myJokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData);
                joker.SetButtonActive(false);
                myJokerList.Add(joker);
            }
            
            myJokerScrollerRoot.SetActive(true);
        }

        private void CloseMyJokerList()
        {
            myJokerScrollerRoot.SetActive(false);
        }

        private void OnDisconnect(string disconnectMessage)
        {
            ShowMessage(disconnectMessage, () =>
            {
                StartCoroutine(ClientUtilityMethods.LoadAsyncScene("StartScene"));    
            });
        }
    }
}