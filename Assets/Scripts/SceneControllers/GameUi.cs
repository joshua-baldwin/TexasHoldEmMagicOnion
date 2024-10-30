using System;
using THE.MagicOnion.Shared.Entities;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        private PlayerEntity[] players;
        private void Awake()
        {
            StreamingHubManager.Receiver.OnGameStartAction = OnGameStart;
            StreamingHubManager.Receiver.CallStartGameMethod(StreamingHubManager.RoomName, () =>
            {
                
            });
        }

        private void OnGameStart(PlayerEntity[] playerEntities)
        {
            players = playerEntities;
        }
    }
}