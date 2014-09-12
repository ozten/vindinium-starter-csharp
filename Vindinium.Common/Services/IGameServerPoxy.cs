using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IGameServerPoxy
    {
        IApiResponse StartTraining(string userId, uint turns);
        IApiResponse StartArena(string userId);
        IApiResponse Play(string gameId, string token, Direction direction);
    }
}