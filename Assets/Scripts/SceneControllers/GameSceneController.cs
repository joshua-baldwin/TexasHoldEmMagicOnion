using UnityEngine;

namespace THE.SceneControllers
{
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject gameUiPrefab;

        private void Start()
        {
            var currentPlayer = MySceneManager.Instance.HubReceiver.CurrentPlayer;
            var ui = Instantiate(gameUiPrefab, canvas.transform).GetComponent<GameUi>();
            ui.Initialize(MySceneManager.Instance.HubReceiver.IsMyTurn, currentPlayer.Name);
        }
    }
}