using Vindinium.Common.DataStructures;
using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IGameServerProxy
    {
        GameResponse GameResponse { get; }
        IApiResponse ApiResponse { get; }
        string StartTraining(uint turns);
        string StartArena();
        string Start(string mapText);
        string Play(string gameId, string token, Direction direction);
        void ChangeMap(string mapText);
    }
}