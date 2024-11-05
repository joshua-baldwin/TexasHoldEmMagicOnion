using THE.MagicOnion.Client;
using THE.Player;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;

        public void Initialize()
        {
            foreach (var player in GamingHubReceiver.Instance.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
        }
    }
}