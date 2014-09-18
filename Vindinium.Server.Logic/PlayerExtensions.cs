using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    internal static class PlayerExtensions
    {
        internal static string PlayerToken(this Hero player)
        {
            return TokenHelper.Player(player.Id);
        }

        internal static string MineToken(this Hero player)
        {
            return TokenHelper.Mine(player.Id);
        }

        internal static bool IsDead(this Hero player)
        {
            return player.Life <= 0;
        }

        internal static void Die(this Hero player)
        {
            player.Life = 0;
        }

        internal static bool IsLiving(this Hero player)
        {
            return !player.IsDead();
        }
    }
}