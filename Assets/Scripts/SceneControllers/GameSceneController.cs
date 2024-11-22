using THE.SceneUis;
using UnityEngine;

namespace THE.SceneControllers
{
    public class GameSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject gameUiPrefab;
        [SerializeField] private GameObject popupPrefab;

        private void Start()
        {
            Instantiate(popupPrefab, canvas.transform);
            var hubReceiver = MySceneManager.Instance.HubReceiver;
            var ui = Instantiate(gameUiPrefab, canvas.transform).GetComponent<GameUi>();
            ui.Initialize(hubReceiver.IsMyTurn, hubReceiver.CurrentPlayer);
        }
    }
}