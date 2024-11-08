using System.Threading;
using MessagePack;
using MessagePack.Resolvers;
using THE.MagicOnion.Client;
using UnityEngine;

namespace THE.MagicOnion.Settings
{
    public class InitialSettings : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void RegisterResolvers()
        {
            StaticCompositeResolver.Instance.Register(
                MagicOnionGeneratedClientInitializer.Resolver,
                GeneratedResolver.Instance,
                BuiltinResolver.Instance,
                PrimitiveObjectResolver.Instance);
            
            MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions.WithResolver(StaticCompositeResolver.Instance);
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeSynchronizationContext()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }
#endif
    }
}