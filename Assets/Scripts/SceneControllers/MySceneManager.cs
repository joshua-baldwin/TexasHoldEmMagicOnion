using THE.MagicOnion.Client;
using THE.Utilities;

namespace THE.SceneControllers
{
    public class MySceneManager : Singleton<MySceneManager>
    {
        public readonly GamingHubReceiver HubReceiver = new();

        protected override void OnAwake()
        {
            HubReceiver.OnRoomConnectSuccess = () => StartCoroutine(ClientUtilityMethods.LoadAsyncScene("WaitingRoomScene"));
            base.OnAwake();
        }

        private async void OnApplicationQuit()
        {
            await HubReceiver.LeaveRoom(null, null);
        }
    }
}