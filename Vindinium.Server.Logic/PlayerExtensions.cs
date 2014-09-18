using System;
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

        internal static void Heal(this Hero player, int amount)
        {
            player.Life += amount;
            if (player.Life > 100) player.Life = 100;
        }

        internal static void Purchase(this Hero player, int amount, Action<Hero> purchased)
        {
            if (player.Gold < amount) return;
            player.Gold -= amount;
            purchased(player);
        }
    }
}