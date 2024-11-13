using THE.MagicOnion.Client;
using THE.Utilities;

namespace THE.SceneControllers
{
    public class MySceneManager : Singleton<MySceneManager>
    {
        public readonly GamingHubReceiver HubReceiver = new();

        private async void OnApplicationQuit()
        {
            await HubReceiver.LeaveRoom(null);
        }
    }
}