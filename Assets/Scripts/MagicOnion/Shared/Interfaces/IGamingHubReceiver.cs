using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHubReceiver
    {
        void OnJoin(PlayerEntity player);
        void OnLeave(PlayerEntity player);
        void SendMessage(string message);
        void OnGetAllPlayers(string roomName, PlayerEntity[] playerEntities);
    }
}