using UnityEngine;

namespace THE.SceneControllers
{
    public class StartSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject startUiPrefab;

        private void Start()
        {
            var ui = Instantiate(startUiPrefab, canvas.transform).GetComponent<StartUi>();
            ui.Initialize();
        }
    }
}