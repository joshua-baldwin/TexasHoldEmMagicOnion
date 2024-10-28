using System.Threading.Tasks;
using MagicOnion;
using THE.MagicOnion.Shared.Entities;

namespace THE.MagicOnion.Shared.Interfaces
{
    public interface IGamingHub : IStreamingHub<IGamingHub, IGamingHubReceiver>
    {
        ValueTask<long> JoinAsync(string name);
        ValueTask<long> LeaveAsync();
        ValueTask SendMessageAsync(string message);
        ValueTask<PlayerEntity[]> GetAllPlayers(string roomName);
    }
}