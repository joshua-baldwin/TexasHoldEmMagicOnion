using THE.MagicOnion.Client;
using THE.Player;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameUi : MonoBehaviour
    {
        [SerializeField] private GameObject playerRoot;
        [SerializeField] private GameObject playerPrefab;
        
        private GamingHubReceiver gamingHubReceiver;

        public void Initialize()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            foreach (var player in gamingHubReceiver.GetPlayerList())
            {
                var playerObject = Instantiate(playerPrefab, playerRoot.transform).GetComponent<PlayerClass>();
                playerObject.Initialize(player);
            }
        }
    }
}