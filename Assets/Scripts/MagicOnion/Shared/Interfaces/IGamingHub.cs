using System;
using System.Threading.Tasks;
using MagicOnion;
using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        ValueTask<Guid> JoinRoomAsync(string name, bool createRoom);
        ValueTask<Guid> LeaveRoomAsync(Guid id);
        ValueTask SendMessageAsync(string message);
        ValueTask<PlayerEntity[]> GetAllPlayers(Guid playerId);
    }
}