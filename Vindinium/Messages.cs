namespace Vindinium.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Newtonsoft.Json.Linq;
    using Vindinium.Util;

    /// <summary>
    /// Tile of the board.
    /// </summary>
    public enum Tile
    {
        /// <summary>
        /// The first hero.
        /// </summary>
        Hero1,

        /// <summary>
        /// The second hero.
        /// </summary>
        Hero2,

        /// <summary>
        /// The third hero.
        /// </summary>
        Hero3,

        /// <summary>
        /// The fourth hero.
        /// </summary>
        Hero4,

        /// <summary>
        /// Some impassable wood.
        /// </summary>
        ImpassableWood,

        /// <summary>
        /// A free square.
        /// </summary>
        Free,

        /// <summary>
        /// A tavern.
        /// </summary>
        Tavern,

        /// <summary>
        /// A neutral gold mine.
        /// </summary>
        GoldMineNeutral,

        /// <summary>
        /// A gold mine belonging to the first hero.
        /// </summary>
        GoldMine1,

        /// <summary>
        /// A gold mine belonging to the second hero.
        /// </summary>
        GoldMine2,

        /// <summary>
        /// A gold mine belonging to the third hero.
        /// </summary>
        GoldMine3,

        /// <summary>
        /// A gold mine belonging to the fourth hero.
        /// </summary>
        GoldMine4
    }

    /// <summary>
    /// Directions you can go in.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Stay where you are.
        /// </summary>
        Stay,

        /// <summary>
        /// Go north.
        /// </summary>
        North,

        /// <summary>
        /// Go east.
        /// </summary>
        East,

        /// <summary>
        /// Go south.
        /// </summary>
        South,

        /// <summary>
        /// Go west.
        /// </summary>
        West
    }

    /// <summary>
    /// The current game state.
    /// </summary>
    public sealed class GameState : IEquatable<GameState>
    {
        private Tile[][] tiles;
        private string tilesString;
        private ISet<Hero> heroes;

        internal GameState(JObject gameResponse)
        {
            var playUrl = Util.JToken2T<string>(gameResponse, "playUrl");
            if (playUrl != null)
            {
                this.PlayURL = new Uri(playUrl);
            }

            var viewUrl = Util.JToken2T<string>(gameResponse, "viewUrl");
            if (viewUrl != null)
            {
                this.ViewURL = new Uri(viewUrl);
            }

            this.MyHero = new Hero(gameResponse["hero"] as JObject);
            var game = (JObject)gameResponse["game"];
            this.heroes = new HashSet<Hero>((game["heroes"] as JArray ?? new JArray()).Select(x => new Hero(x as JObject)));
            this.CurrentTurn = Util.JToken2T<int>(game, "turn");
            this.MaxTurns = Util.JToken2T<int>(game, "maxTurns");
            this.Finished = Util.JToken2T<bool>(game, "finished");
            var board = game["board"] as JObject;
            var size = Util.JToken2T<int>(board, "size");
            var tiles = Util.JToken2T<string>(board, "tiles");
            this.tilesString = tiles;
            this.CreateBoard(size, tiles);
        }

        /// <summary>
        /// Gets the dimensions of the board.
        /// </summary>
        /// <value>The dimensions.</value>
        /// <remarks>Presently the x and y heights are always the same</remarks>
        public Tuple<int, int> Dimensions { get; private set; }

        /// <summary>
        /// Gets the user's own hero.
        /// </summary>
        /// <value>My hero.</value>
        public Hero MyHero { get; private set; }

        /// <summary>
        /// Gets all the heroes.
        /// </summary>
        /// <value>The heroes.</value>
        /// <remarks>TODO look at immutable collections to go here.</remarks>
        public ISet<Hero> Heroes {
            get
            {
                return new HashSet<Hero>(this.heroes);
            }
        }

        /// <summary>
        /// Gets the number of the current turn.
        /// </summary>
        /// <value>The current turn.</value>
        public int CurrentTurn { get; private set; }

        /// <summary>
        /// Gets the total number of turns.
        /// </summary>
        /// <value>The max turns.</value>
        public int MaxTurns { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the game is finished.
        /// </summary>
        /// <value><c>true</c> if finished; otherwise, <c>false</c>.</value>
        public bool Finished { get; private set; }

        internal Uri ViewURL { get; private set; }

        internal Uri PlayURL { get; private set; }

        private static bool ValueEqual(GameState left, GameState right)
        {
            return ReferenceEquals(left, right) || (!ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.tilesString == right.tilesString && 
                (ReferenceEquals(left.Dimensions, right.Dimensions) || (!ReferenceEquals(left.Dimensions, null) && left.Dimensions.Equals(right.Dimensions))) && 
                left.MyHero == right.MyHero && 
                (ReferenceEquals(left.Heroes, right.Heroes) || (!ReferenceEquals(left.Heroes, null) && left.Heroes.SetEquals(right.Heroes))) &&
                left.CurrentTurn == right.CurrentTurn &&
                left.MaxTurns == right.MaxTurns &&
                left.Finished == right.Finished &&
                left.ViewURL == right.ViewURL &&
                left.PlayURL == right.PlayURL);
        }

        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        public static bool operator ==(GameState left, GameState right)
        {
            return GameState.ValueEqual(left, right);
        }

        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        public static bool operator !=(GameState left, GameState right)
        {
            return !GameState.ValueEqual(left, right);
        }

        /// <summary>
        /// Gets the tile with x and y coordinates specified.
        /// </summary>
        /// <remarks>May throw an exception if x or y are out of range.</remarks>
        /// <returns>The tile in question.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Tile GetTile(int x, int y)
        {
            var xes = this.Dimensions.Item1;
            var ys = this.Dimensions.Item2;
            if (x < 0 || x >= xes)
            {
                throw new ArgumentException("x must be between 0 and [" + xes.ToString() + "]", "x");
            }
            else if (y < 0 || y >= ys)
            {
                throw new ArgumentException("y must be between 0 and [" + ys.ToString() + "]", "y");
            }
            else
            {
                return this.tiles[y][x];
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Vindinium.Messages.GameState"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Vindinium.Messages.GameState"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[GameState: tilesString={0}, Dimensions={1}, MyHero={2}, Heroes={3}, CurrentTurn={4}, MaxTurns={5}, Finished={6}, ViewURL={7}, PlayURL={8}]", this.tilesString, this.Dimensions, this.MyHero, this.Heroes, this.CurrentTurn, this.MaxTurns, this.Finished, this.ViewURL, this.PlayURL);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Vindinium.Messages.GameState"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Vindinium.Messages.GameState"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Vindinium.Messages.GameState"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return GameState.ValueEqual(this, obj as GameState);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vindinium.Messages.GameState"/> is equal to the current <see cref="Vindinium.Messages.GameState"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vindinium.Messages.GameState"/> to compare with the current <see cref="Vindinium.Messages.GameState"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Vindinium.Messages.GameState"/> is equal to the current
        /// <see cref="Vindinium.Messages.GameState"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(GameState other)
        {
            return GameState.ValueEqual(this, other);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Vindinium.Messages.GameState"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.tilesString != null ? this.tilesString.GetHashCode() : 0) ^ (this.Dimensions != null ? this.Dimensions.GetHashCode() : 0) ^ (this.MyHero != null ? this.MyHero.GetHashCode() : 0) ^ (this.Heroes != null ? this.Heroes.Aggregate(0, (acc, hero) => acc ^ (hero != null ? hero.GetHashCode() : 0)) : 0) ^ this.CurrentTurn.GetHashCode() ^ this.MaxTurns.GetHashCode() ^ this.Finished.GetHashCode() ^ (this.ViewURL != null ? this.ViewURL.GetHashCode() : 0) ^ (this.PlayURL != null ? this.PlayURL.GetHashCode() : 0);
            }
        }

        private void CreateBoard(int size, string data)
        {
            // check to see if the board list is already created, if it is, we just overwrite its values
            if (this.tiles == null || this.tiles.Length != size)
            {
                this.tiles = new Tile[(int)size][];

                // need to initialize the lists within the list
                for (int i = 0; i < size; i++)
                {
                    this.tiles[i] = new Tile[(int)size];
                }
            }

            this.Dimensions = new Tuple<int, int>(size, size);

            // convert the string to the List<List<Tile>>
            int x = 0;
            int y = 0;
            char[] charData = data.ToCharArray();

            for (int i = 0; i < charData.Length; i += 2)
            {
                switch (charData[i])
                {
                    case '#':
                        this.tiles[x][y] = Tile.ImpassableWood;
                        break;
                    case ' ':
                        this.tiles[x][y] = Tile.Free;
                        break;
                    case '@':
                        switch (charData[i + 1])
                        {
                            case '1':
                                this.tiles[x][y] = Tile.Hero1;
                                break;
                            case '2':
                                this.tiles[x][y] = Tile.Hero2;
                                break;
                            case '3':
                                this.tiles[x][y] = Tile.Hero3;
                                break;
                            case '4':
                                this.tiles[x][y] = Tile.Hero4;
                                break;
                        }

                        break;
                    case '[':
                        this.tiles[x][y] = Tile.Tavern;
                        break;
                    case '$':
                        switch (charData[i + 1])
                        {
                            case '-':
                                this.tiles[x][y] = Tile.GoldMineNeutral;
                                break;
                            case '1':
                                this.tiles[x][y] = Tile.GoldMine1;
                                break;
                            case '2':
                                this.tiles[x][y] = Tile.GoldMine2;
                                break;
                            case '3':
                                this.tiles[x][y] = Tile.GoldMine3;
                                break;
                            case '4':
                                this.tiles[x][y] = Tile.GoldMine4;
                                break;
                        }

                        break;
                }

                // time to increment x and y
                x++;
                if (x == size)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }

    /// <summary>
    /// Represents a Hero.
    /// </summary>
    public sealed class Hero : IEquatable<Hero>
    {
        internal Hero(IDictionary<string, JToken> inp)
        {
            this.Crashed = Util.JToken2T<bool>(inp, "crashed");
            this.Elo = Util.JToken2NullableT<int>(inp, "elo");
            this.Gold = Util.JToken2T<int>(inp, "gold");
            this.Id = Util.JToken2T<int>(inp, "id");
            this.Life = Util.JToken2T<int>(inp, "life");
            this.MineCount = Util.JToken2T<int>(inp, "mineCount");
            this.Name = Util.JToken2T<string>(inp, "name");
            this.Pos = Util.JObject2TupleIntInt(inp["pos"] as JObject);
            this.SpawnPos = Util.JObject2TupleIntInt(inp["spawnPos"] as JObject);
        }

        /// <summary>
        /// Gets the identifier of the hero.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the hero.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the elo rating of the hero.
        /// </summary>
        /// <value>The elo.</value>
        /// <remarks>Is absent sometimes. (On the first run?)</remarks>
        public int? Elo
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the position of the hero.
        /// </summary>
        /// <value>The position.</value>
        public Tuple<int, int> Pos
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the life remaining of the hero.
        /// </summary>
        /// <value>The life.</value>
        public int Life
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the gold of the hero.
        /// </summary>
        /// <value>The gold.</value>
        public int Gold
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of mines the hero has.
        /// </summary>
        /// <value>The mine count.</value>
        public int MineCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the spawn position of the hero.
        /// </summary>
        /// <value>The spawn position.</value>
        public Tuple<int, int> SpawnPos
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the hero has crashed or not.
        /// </summary>
        /// <value>The crashed.</value>
        public bool Crashed
        {
            get;
            private set;
        }

        private static bool ValueEqual(Hero left, Hero right)
        {
            // if left and right are both null then they are both reference-equal
            return ReferenceEquals(left, right) || (!ReferenceEquals(left, null) && 
                !ReferenceEquals(right, null) &&
                left.Id == right.Id &&
                left.Name == right.Name &&
                left.Elo == right.Elo &&
                !ReferenceEquals(left.Pos, null) &&
                left.Pos.Equals(right.Pos) &&
                left.Life == right.Life &&
                left.Gold == right.Gold &&
                left.MineCount == right.MineCount &&
                left.SpawnPos != null &&
                left.SpawnPos.Equals(right.SpawnPos) &&
                left.Crashed == right.Crashed);
        }


        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        public static bool operator ==(Hero left, Hero right)
        {
            return Hero.ValueEqual(left, right);
        }

        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        public static bool operator !=(Hero left, Hero right)
        {
            return !Hero.ValueEqual(left, right);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Vindinium.Messages.Hero"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Vindinium.Messages.Hero"/>.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[Hero: Id={0}, Name={1}, Elo={2}, Pos={3}, Life={4}, Gold={5}, MineCount={6}, SpawnPos={7}, Crashed={8}]", this.Id, this.Name, this.Elo, this.Pos, this.Life, this.Gold, this.MineCount, this.SpawnPos, this.Crashed);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Vindinium.Messages.Hero"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Vindinium.Messages.Hero"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Vindinium.Messages.Hero"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return Hero.ValueEqual(this, obj as Hero);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vindinium.Messages.Hero"/> is equal to the current <see cref="Vindinium.Messages.Hero"/>.
        /// </summary>
        /// <param name="other">The <see cref="Vindinium.Messages.Hero"/> to compare with the current <see cref="Vindinium.Messages.Hero"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Vindinium.Messages.Hero"/> is equal to the current
        /// <see cref="Vindinium.Messages.Hero"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Hero other)
        {
            return Hero.ValueEqual(this, other);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Vindinium.Messages.Hero"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return this.Id.GetHashCode() ^ (this.Name != null ? this.Name.GetHashCode() : 0) ^ this.Elo.GetHashCode() ^ (this.Pos != null ? this.Pos.GetHashCode() : 0) ^ this.Life.GetHashCode() ^ this.Gold.GetHashCode() ^ this.MineCount.GetHashCode() ^ (this.SpawnPos != null ? this.SpawnPos.GetHashCode() : 0) ^ this.Crashed.GetHashCode();
            }
        }
    }
}
