using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHubReceiver
    {
        void OnJoinRoom(PlayerEntity player);
        void OnLeaveRoom(PlayerEntity player);
        void SendMessage(string message);
        void OnGetAllPlayers(PlayerEntity[] playerEntities);
    }
}