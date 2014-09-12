namespace Vindinium.Common.Services
{
    public interface IGameServerPoxy
    {
        string StartTraining(string userId, uint turns);
        string StartArena(string userId);
        string Play(string gameId, string token, Direction direction);
    }
}