using THE.MagicOnion.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace THE.SceneControllers
{
    public class StartSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject startUiPrefab;
        
        private StartUi startUi;

        private void Start()
        {
            startUi = Instantiate(startUiPrefab, canvas.transform).GetComponent<StartUi>();
            GamingHubReceiver.Instance.OnRoomConnectSuccess = () => SceneManager.LoadSceneAsync("WaitingRoomScene");
            GamingHubReceiver.Instance.OnRoomConnectFailed = () => startUi.SetRoomButton(true);
            GamingHubReceiver.Instance.OnCancelRoomConnect = () => startUi.SetRoomButton(true);
            startUi.Initialize();
        }
    }
}