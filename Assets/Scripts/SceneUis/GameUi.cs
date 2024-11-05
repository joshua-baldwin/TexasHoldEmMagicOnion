using THE.MagicOnion.Client;
using THE.MagicOnion.Shared.Entities;
using THE.Player;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;

        private void Awake()
        {
            GamingHubReceiver.Instance.OnGameStartAction = OnGameStart;
        }

        public void Initialize()
        {
            foreach (var player in GamingHubReceiver.Instance.GetAllPlayers())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
            //GamingHubReceiver.Instance.CallStartGameMethod(GamingHubReceiver.Instance.RoomName, null);
        }

        private void OnGameStart(PlayerEntity[] playerEntities)
        {
            // players = playerEntities;
            // foreach (var player in players)
            // {
            //     var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
            //     playerObject.Initialize(player);
            // }
        }
    }
}