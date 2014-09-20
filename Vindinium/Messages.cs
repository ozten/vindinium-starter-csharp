namespace Vindinium.Messages
{
    using System;
    using System.Collections.Generic;
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
    public sealed class GameState
    {
        private Tile[][] tiles;
    
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
            this.Heroes = (game["heroes"] as JArray ?? new JArray()).Select(x => new Hero(x as JObject)).ToList();
            this.CurrentTurn = Util.JToken2T<int>(game, "turn");
            this.MaxTurns = Util.JToken2T<int>(game, "maxTurns");
            this.Finished = Util.JToken2T<bool>(game, "finished");
            var board = game["board"] as JObject;
            var size = Util.JToken2T<int>(board, "size");
            var tiles = Util.JToken2T<string>(board, "tiles");
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
        public IList<Hero> Heroes { get; private set; }

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
            else if ( y < 0 || y >= ys)
            {
                throw new ArgumentException("y must be between 0 and [" + ys.ToString()+ "]", "y");
            }
            else
            {
                return this.tiles[y][x];
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
    public sealed class Hero
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
    }
}
