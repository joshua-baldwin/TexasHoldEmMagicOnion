using System;
using System.Threading.Tasks;
using MagicOnion;
using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        ValueTask<PlayerEntity> JoinRoomAsync(string name, bool createRoom);
        ValueTask<PlayerEntity> LeaveRoomAsync(string roomName);
        ValueTask SendMessageAsync(string message);
        ValueTask<PlayerEntity[]> GetAllPlayers(string roomName);
    }
}