using System;
using THE.MagicOnion.Shared.Entities;
using THE.Player;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;
        private PlayerEntity[] players;
        
        private void Awake()
        {
            StreamingHubManager.Receiver.OnGameStartAction = OnGameStart;
            StreamingHubManager.Receiver.CallStartGameMethod(StreamingHubManager.RoomName, null);
        }

        private void OnGameStart(PlayerEntity[] playerEntities)
        {
            players = playerEntities;
            foreach (var player in players)
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
        }
    }
}