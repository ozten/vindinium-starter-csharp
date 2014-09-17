namespace Vindinium
{
    using System;

    /// <summary>
    /// Random bot.
    /// </summary>
    public sealed class RandomBot : IBot
    {
        private Random random = new Random();

        /// <summary>
        /// Gets the name of this Bot.
        /// </summary>
        /// <value>The name of this Bot.</value>
        public string Name
        {
            get { return "random bot"; }
        }

        /// <summary>
        /// Gets the direction to move the hero following the state specified.
        /// </summary>
        /// <remarks>This implementation ignores the state and chooses a random direction.</remarks>
        /// <param name="gameState">The current game state.</param>
        /// <returns>A randomly-chosen direction</returns>
        public Direction Move(GameState gameState)
        {
            var i = this.random.Next(0, 6);
            var outp = i == 0 ? Direction.East : 
                i == 1 ? Direction.North :
                i == 2 ? Direction.South : 
                i == 3 ? Direction.Stay :
                Direction.West;
            return outp;
        }
    }
}
