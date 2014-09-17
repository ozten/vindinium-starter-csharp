namespace Vindinium
{
    /// <summary>
    /// Interface that bots should implement.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Gets the name of this Bot.
        /// </summary>
        /// <value>The name of this Bot.</value>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets the direction to move the hero following the state specified.
        /// </summary>
        /// <param name="gameState">The current game state.</param>
        /// <returns>The direction in which to go following that state</returns>
        Direction Move(GameState gameState);
    }
}
