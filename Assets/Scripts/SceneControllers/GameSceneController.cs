using UnityEngine;

namespace THE.SceneControllers
{
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject gameUiPrefab;

        private void Start()
        {
            var hubReceiver = MySceneManager.Instance.HubReceiver;
            var ui = Instantiate(gameUiPrefab, canvas.transform).GetComponent<GameUi>();
            ui.Initialize(hubReceiver.IsMyTurn, hubReceiver.CurrentPlayer);
        }
    }
}