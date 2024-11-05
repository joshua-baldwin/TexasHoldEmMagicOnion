using THE.MagicOnion.Client;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class WaitingRoomUi : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text roomName;
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;
        private int playerCount;

        private void Awake()
        {
            userName.text = $"UserName: {GamingHubReceiver.Instance.UserName}";
            roomName.text = $"Room name:\n{GamingHubReceiver.Instance.RoomName}";
            startButton.gameObject.SetActive(GamingHubReceiver.Instance.IsHost);
            startButton.interactable = playerCount > 1;
            startButton.onClick.AddListener(StartAction);
            leaveButton.onClick.AddListener(LeaveRoom);
            GamingHubReceiver.Instance.UpdatePlayerCount = UpdatePlayerCount;
        }

        public void Initialize()
        {
            GamingHubReceiver.Instance.GetPlayers(UpdatePlayerCount);
        }

        private void UpdatePlayerCount(int count)
        {
            currentPlayerCount.text = $"{count}/10";
            playerCount = count;
            startButton.interactable = playerCount > 1;
        }

        private void StartAction()
        {
            GamingHubReceiver.Instance.StartGame(GamingHubReceiver.Instance.RoomName, null);
        }
        
        private void LeaveRoom()
        {
            GamingHubReceiver.Instance.LeaveRoom(() => SceneManager.LoadScene("StartScene"));
        }
    }
}