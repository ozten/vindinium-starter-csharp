using System;

namespace Vindinium
{
    /// <summary>
    /// Random bot.
    /// </summary>
    public sealed class RandomBot : IBot
    {
        Random random = new Random();

        /// <summary>
        /// Gets the name of this Bot.
        /// </summary>
        /// <value>The name of this Bot.</value>
        public String Name
        {
            get { return "random bot"; }
        }

        /// <summary>
        /// Gets the direction to move the hero following the state specified.
        /// </summary>
        /// <remarks>This implementation ignores the state and chooses a random direction.</remarks>
        /// <param name="gameState">The current game state.</param>
        public Direction Move(GameState gameState)
        {
            var i = random.Next(0, 6);
            var outp = i == 0 ? Direction.East : 
				i == 1 ? Direction.North :
				i == 2 ? Direction.South : 
				i == 3 ? Direction.Stay :
				Direction.West;
            Console.WriteLine(outp);
            return outp;
        }
    }
}
