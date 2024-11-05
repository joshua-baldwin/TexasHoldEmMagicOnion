using UnityEngine;

namespace THE.SceneControllers
{
    public class WaitingRoomController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject waitingRoomUiPrefab;

        private void Start()
        {
            var ui = Instantiate(waitingRoomUiPrefab, canvas.transform).GetComponent<WaitingRoomUi>();
            ui.Initialize();
        }
    }
}