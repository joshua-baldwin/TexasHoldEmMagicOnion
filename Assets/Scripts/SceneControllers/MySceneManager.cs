using THE.MagicOnion.Client;
using THE.Utilities;

namespace THE.SceneControllers
{
    public class MySceneManager : Singleton<MySceneManager>
    {
        public GamingHubReceiver HubReceiver = new();
    }
}