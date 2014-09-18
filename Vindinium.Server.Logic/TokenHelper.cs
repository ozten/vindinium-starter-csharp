using System.Globalization;

namespace Vindinium.Game.Logic
{
    internal static class TokenHelper
    {
        internal const string Tavern = "[]";
        private const string MinePrefix = "$";
        internal const string NeutralMine = "$-";
        internal const string OpenPath = "  ";
        internal const string Obstruction = "##";
        private const string PlayerPrefix = "@";

        internal static string Player(int playerId)
        {
            return string.Format("{0}{1}", PlayerPrefix, IdSuffix(playerId));
        }

        private static string IdSuffix(int playerId)
        {
            return playerId < 1 ? "-" : playerId.ToString(CultureInfo.InvariantCulture);
        }

        internal static string Mine(int playerId)
        {
            return string.Format("{0}{1}", MinePrefix, IdSuffix(playerId));
        }

        internal static bool IsMine(string token)
        {
            return token.StartsWith(MinePrefix);
        }

        internal static bool IsPlayer(string token)
        {
            return token.StartsWith(PlayerPrefix);
        }
    }
}