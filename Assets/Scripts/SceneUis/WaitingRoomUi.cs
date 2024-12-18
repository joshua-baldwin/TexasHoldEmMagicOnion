using System;
using System.Collections.Generic;
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
        [SerializeField] private GameObject scrollerRoot;
        [SerializeField] private GameObject jokerListRoot;
        [SerializeField] private GameObject jokerPrefab;
        
        private int playerCount;
        private GamingHubReceiver gamingHubReceiver;
        private List<JokerClass> jokers = new();
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
                .Subscribe(_ => BuyJokerAction())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            closeJokerListButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => CloseList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            gamingHubReceiver.UpdatePlayerCount = UpdatePlayerCount;
            gamingHubReceiver.ShowMessage = ShowMessage;
        }

        public async UniTask Initialize()
        {
            foreach (var jokerData in gamingHubReceiver.GetJokerList())
            {
                var joker = Instantiate(jokerPrefab, jokerListRoot.transform).GetComponent<JokerClass>();
                joker.Initialize(jokerData);
                joker.BuyJokerAction = BuyJokerAction;
                jokers.Add(joker);
            }
            
            await gamingHubReceiver.GetPlayers(UpdatePlayerCount, OnDisconnect);
        }

        private async UniTask BuyJokerAction(Guid jokerId)
        {
            var response = await gamingHubReceiver.BuyJoker(jokerId, OnDisconnect);
            if (response == Enums.BuyJokerResponseTypeEnum.Success)
                await CloseList();
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
        
        private async UniTask BuyJokerAction()
        {
            scrollerRoot.SetActive(true);
        }

        private async UniTask CloseList()
        {
            scrollerRoot.SetActive(false);
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