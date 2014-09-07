using System;

namespace Vindinium
{
    /// <summary>
    /// Interface that bots should implement.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Gets the direction to move the hero following the state specified.
        /// </summary>
        /// <param name="gameState">The current game state.</param>
        Direction Move(GameState gameState);

        /// <summary>
        /// Gets the name of this Bot.
        /// </summary>
        /// <value>The name of this Bot.</value>
        String Name
        {
            get;
        }
    }
}

