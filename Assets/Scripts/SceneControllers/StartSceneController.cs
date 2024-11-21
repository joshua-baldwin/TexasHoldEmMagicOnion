using THE.MagicOnion.Client;
using THE.SceneUis;
using THE.Utilities;
using UnityEngine;

namespace THE.SceneControllers
{
    public class StartSceneController : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject startUiPrefab;
        
        private StartUi startUi;
        private GamingHubReceiver gamingHubReceiver;

        private void Start()
        {
            gamingHubReceiver = MySceneManager.Instance.HubReceiver;
            gamingHubReceiver.OnRoomConnectSuccess = () => StartCoroutine(ClientUtilityMethods.LoadAsyncScene("WaitingRoomScene"));
            gamingHubReceiver.OnRoomConnectFailed = () => startUi.SetRoomButton(true);
            gamingHubReceiver.OnCancelRoomConnect = () => startUi.SetRoomButton(true);
            startUi = Instantiate(startUiPrefab, canvas.transform).GetComponent<StartUi>();
            startUi.Initialize();
        }
    }
}