using System;

namespace Vindinium
{
    public interface IBot
    {
        Direction Move(GameState gameState);

        String Name
        {
            get;
        }
    }
}

