using System;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public static class MapMaker
    {
        public static string GenerateMap()
        {
            var grid = new Grid();
            int seed = (int) DateTime.Now.Ticks%int.MaxValue;
            var random = new Random(seed);
            int quarter = random.Next(5, 14);
            int size = quarter*2;
            grid.MapText = "".PadLeft(size*size*2, ' ');
            AddToken(grid, TokenHelper.Tavern, random);
            AddToken(grid, TokenHelper.Player(0), random);
            for (int i = 0; i < 4; i++)
            {
                AddToken(grid, TokenHelper.NeutralMine, random);
            }
            for (int i = 0; i < 8; i++)
            {
                AddToken(grid, TokenHelper.Obstruction, random);
            }

            grid.MakeSymmetric();
            return grid.MapText;
        }

        private static void AddToken(Grid grid, string token, Random random)
        {
            int max = grid.Size/2;

            var pos = new Pos {X = random.Next(1, max), Y = random.Next(1, max)};
            while (grid[pos] != TokenHelper.OpenPath)
            {
                pos.X = random.Next(1, max);
                pos.Y = random.Next(1, max);
            }
            grid[pos] = token;
        }
    }
}