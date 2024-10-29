using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHubReceiver
    {
        void OnJoinRoom(PlayerEntity player);
        void OnLeaveRoom(PlayerEntity player, int playerCount);
        void SendMessage(string message);
        void OnGetAllPlayers(PlayerEntity[] playerEntities);
    }
}