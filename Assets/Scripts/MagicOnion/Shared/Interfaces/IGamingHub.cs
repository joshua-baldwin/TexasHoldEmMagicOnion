using System;
using System.Threading.Tasks;
using MagicOnion;
using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        ValueTask<PlayerEntity> JoinRoomAsync(string name);
        ValueTask<PlayerEntity> LeaveRoomAsync();
        ValueTask<PlayerEntity[]> GetAllPlayers(string roomName);
        ValueTask StartGame(Guid playerId);
        ValueTask CancelStart(Guid playerId);
        ValueTask QuitGame(Guid playerId);
    }
}