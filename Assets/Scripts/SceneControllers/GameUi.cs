using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private Text userName;
        [SerializeField] private Text roomName;
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;
        private int playerCount;

        private void Awake()
        {
            userName.text = $"UserName: {StreamingHubManager.UserName}";
            roomName.text = $"Room name:\n{StreamingHubManager.RoomName}";
            startButton.onClick.AddListener(StartAction);
            leaveButton.onClick.AddListener(LeaveRoom);
            StreamingHubManager.Receiver.UpdatePlayerCount = UpdatePlayerCount;
        }

        private void UpdatePlayerCount(int count)
        {
            Debug.Log("updating player count");
            currentPlayerCount.text = $"{count}/10";
        }

        private void StartAction()
        {
            
        }
        
        private void LeaveRoom()
        {
            StreamingHubManager.Receiver.CallLeaveMethod(() => SceneManager.LoadScene("StartScene"));
        }
    }
}