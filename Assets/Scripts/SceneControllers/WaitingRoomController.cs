using Cysharp.Threading.Tasks;
using UnityEngine;

namespace THE.SceneControllers
{
    public class WaitingRoomController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject waitingRoomUiPrefab;

        private async void Start()
        {
            var ui = Instantiate(waitingRoomUiPrefab, canvas.transform).GetComponent<WaitingRoomUi>();
            await ui.Initialize();
        }
    }
}