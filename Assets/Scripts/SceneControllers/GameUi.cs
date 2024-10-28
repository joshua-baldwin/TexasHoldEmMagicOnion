using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private Text currentPlayerCount;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;
        private int playerCount;

        private void Awake()
        {
            startButton.onClick.AddListener(StartAction);
            leaveButton.onClick.AddListener(LeaveRoom);
            currentPlayerCount.text = $"{MySceneManager.PlayerCount}/10";
        }

        private void StartAction()
        {
            
        }
        
        private void LeaveRoom()
        {
            MySceneManager.Receiver.CallLeaveMethod(() =>
            {
                
                SceneManager.LoadScene("StartScene");
            });
        }
    }
}