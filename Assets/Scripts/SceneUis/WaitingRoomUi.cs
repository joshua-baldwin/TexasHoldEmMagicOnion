using System;
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
    public class WaitingRoomUi : BaseLayoutUi
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text roomName;
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button buyJokerButton;
        [SerializeField] private JokerListUi jokerListUi;
        [SerializeField] private Button myJokerListButton;
        
        private int playerCount;
        private GamingHubReceiver gamingHubReceiver;
        private bool jokerListIsOpen;

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
            
            myJokerListButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => OpenMyJokerList())
                .AddTo(this.GetCancellationTokenOnDestroy());
            
            gamingHubReceiver.UpdatePlayerCount = UpdatePlayerCount;
            gamingHubReceiver.ShowMessage = ShowMessage;
        }

        public async UniTask Initialize()
        {
            await gamingHubReceiver.GetPlayers(UpdatePlayerCount, OnDisconnect);
        }

        private async UniTask BuyJokerAction(int jokerId)
        {
            var response = await gamingHubReceiver.BuyJoker(jokerId, OnDisconnect);
            jokerListUi.UpdateJokerButton(jokerId, response != Enums.BuyJokerResponseTypeEnum.Success);
        }

        private void UpdatePlayerCount(int count)
        {
            currentPlayerCount.text = $"{count}/{Constants.MaxPlayers}";
            playerCount = count;
            startButton.interactable = playerCount >= Constants.MinimumPlayers;
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
            jokerListUi.ShowListForWaitingRoom(gamingHubReceiver.GetJokerList(), gamingHubReceiver.Self.JokerCards, BuyJokerAction, true);
        }
        
        private void OpenMyJokerList()
        {
            jokerListUi.ShowListForWaitingRoom(gamingHubReceiver.Self.JokerCards.Select(x => new JokerData(x)), null, null, false);
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