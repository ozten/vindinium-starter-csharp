using System;
using System.Collections.Generic;
using System.Linq;
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

        private static void Revive(this Hero player)
        {
            player.Life = 100;
            player.Pos = player.SpawnPos;
        }

        internal static bool IsLiving(this Hero player)
        {
            return !player.IsDead();
        }

        internal static void Heal(this Hero player)
        {
            player.Life += 50;
            if (player.Life > 100) player.Life = 100;
        }

        internal static void GetThirsty(this Hero player)
        {
            if (player.Life > 1)
            {
                player.Life--;
            }
        }

        internal static void GetWealthy(this Hero player)
        {
            player.Gold += player.MineCount;
        }

        internal static void Purchase(this Hero player, int amount, Action<Hero> purchased)
        {
            if (player.Gold < amount) return;
            player.Gold -= amount;
            purchased(player);
        }

        internal static void RaiseTheDead(this IEnumerable<Hero> players)
        {
            players.Where(p => p.IsDead()).ToList().ForEach(p => p.Revive());
        }

        internal static void Attack(this Hero player, Hero enemy, Grid map)
        {
            enemy.Life -= 20;
            if (enemy.IsDead())
            {
                map.ForEach(p => { if (map[p] == enemy.MineToken()) map[p] = player.MineToken(); });
            }
        }

        internal static void AssignPosAndMinesFromMap(this Hero player, Grid map)
        {
            player.Pos = map.PositionOf(player.PlayerToken());
            player.MineCount = map.TokenCount(player.MineToken());
        }
    }
}