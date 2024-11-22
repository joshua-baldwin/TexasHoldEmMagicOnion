using THE.SceneUis;
using UnityEngine;

namespace THE.SceneControllers
{
    public class WaitingRoomController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject waitingRoomUiPrefab;
        [SerializeField] private GameObject popupPrefab;

        private async void Start()
        {
            var ui = Instantiate(waitingRoomUiPrefab, canvas.transform).GetComponent<WaitingRoomUi>();
            await ui.Initialize();
            Instantiate(popupPrefab, canvas.transform);
        }
    }
}