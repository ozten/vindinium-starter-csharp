using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Vindinium
{
    public sealed class GameState
    {
        private Tile[][] _jaggedTiles;

        public int? X { get; private set; }

        public int? Y { get; private set; }

        // TODO should we make this internal? --George
        public Uri ViewURL { get; private set; }

        public Hero MyHero { get; private set; }

        public IList<Hero> Heroes { get; private set; }

        public int CurrentTurn { get; private set; }

        public int MaxTurns { get; private set; }

        public bool Finished { get; private set; }

        public bool Errored { get; private set; }

        public string ErrorText { get; private set; }

        internal Uri PlayURL { get; private set; }

        internal GameState(WebException exception)
        {
            this.Errored = true;
            if (exception.Response != null)
            {
                using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                {
                    this.ErrorText = reader.ReadToEnd();
                }
            }
            else
            {
                this.ErrorText = "The server is down";
            }
        }

        internal GameState(string json)
        {

            var gameResponse = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);

            var playUrl = gameResponse["playUrl"] as string;
            if (playUrl != null)
            {
                PlayURL = new Uri(playUrl);
            }
            var viewUrl = gameResponse["viewUrl"] as string;
            if(viewUrl != null)
            {
                this.ViewURL = new Uri(viewUrl);
            }

            this.MyHero = new Hero(gameResponse["hero"] as JObject);

            var game = (JObject) gameResponse["game"];

            this.Heroes = (game["heroes"] as JArray ?? new JArray()).Select(x => new Hero(x as JObject)).ToList();

            var currentTurn = game["turn"];
            if (currentTurn != null)
            {
                this.CurrentTurn = (int) currentTurn;
            }
            var maxTurns = game["maxTurns"];
            if (maxTurns != null)
            {
                this.MaxTurns = (int) maxTurns;
            }
            var finished = game["finished"];
            if (finished != null)
            {
                this.Finished = (bool) finished;
            }
            var board = game["board"] as JObject;
            var size = Util.JToken2NullableT<int>(board, "size");
            var tiles = Util.JToken2T<string>(board, "tiles");
            this.CreateBoard(size, tiles);
        }

        private void CreateBoard(int? size, string data)
        {

            if (size != null)
            {
                //check to see if the board list is already created, if it is, we just overwrite its values
                if (_jaggedTiles == null || _jaggedTiles.Length != size)
                {
                    _jaggedTiles = new Tile[(int)size][];

                    //need to initialize the lists within the list
                    for (int i = 0; i < size; i++)
                    {
                        _jaggedTiles[i] = new Tile[(int)size];
                    }
                }
            }

            this.X = size;
            this.Y = size;

            //convert the string to the List<List<Tile>>
            int x = 0;
            int y = 0;
            char[] charData = data.ToCharArray();

            for (int i = 0; i < charData.Length; i += 2)
            {
                switch (charData[i])
                {
                    case '#':
                        _jaggedTiles[x][y] = Tile.ImpassableWood;
                        break;
                    case ' ':
                        _jaggedTiles[x][y] = Tile.Free;
                        break;
                    case '@':
                        switch (charData[i + 1])
                        {
                            case '1':
                                _jaggedTiles[x][y] = Tile.Hero1;
                                break;
                            case '2':
                                _jaggedTiles[x][y] = Tile.Hero2;
                                break;
                            case '3':
                                _jaggedTiles[x][y] = Tile.Hero3;
                                break;
                            case '4':
                                _jaggedTiles[x][y] = Tile.Hero4;
                                break;

                        }
                        break;
                    case '[':
                        _jaggedTiles[x][y] = Tile.Tavern;
                        break;
                    case '$':
                        switch (charData[i + 1])
                        {
                            case '-':
                                _jaggedTiles[x][y] = Tile.GoldMineNeutral;
                                break;
                            case '1':
                                _jaggedTiles[x][y] = Tile.GoldMine1;
                                break;
                            case '2':
                                _jaggedTiles[x][y] = Tile.GoldMine2;
                                break;
                            case '3':
                                _jaggedTiles[x][y] = Tile.GoldMine3;
                                break;
                            case '4':
                                _jaggedTiles[x][y] = Tile.GoldMine4;
                                break;
                        }
                        break;
                }

                //time to increment x and y
                x++;
                if (x == size)
                {
                    x = 0;
                    y++;
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            return _jaggedTiles[y][x];
        }
    }

    public enum Tile
    {
        Hero1,
        Hero2,
        Hero3,
        Hero4,
        ImpassableWood,
        Free,
        Tavern,
        GoldMineNeutral,
        GoldMine1,
        GoldMine2,
        GoldMine3,
        GoldMine4
    }

    public enum Direction
    {
        Stay,
        North,
        East,
        South,
        West
    }

    public sealed class Pos
    {
        public int? X
        {
            get;
            internal set;
        }

        public int? Y
        {
            get;
            internal set;
        }


        internal Pos(IDictionary<string, JToken> inp) {
            this.X = Util.JToken2NullableT<int>(inp, "x");
            this.Y = Util.JToken2NullableT<int>(inp, "y");
        }

        public static implicit operator Tuple<int?,int?>(Pos p)
        {
            return new Tuple<int?, int?>(p.X, p.Y);
        }

    }

    public sealed class Hero
    {
        internal Hero(IDictionary<string, JToken> inp) {
            this.Crashed = Util.JToken2NullableT<bool>(inp, "crashed");
            this.Elo = Util.JToken2NullableT<int>(inp, "elo");
            this.Gold = Util.JToken2NullableT<int>(inp, "gold");
            this.Id = Util.JToken2NullableT<int>(inp, "id");
            this.Life = Util.JToken2NullableT<int>(inp, "life");
            this.MineCount = Util.JToken2NullableT<int>(inp, "mineCount");
            this.Name = Util.JToken2T<string>(inp, "name");
            this.Pos = new Pos(inp["pos"] as JObject);
            this.SpawnPos = new Pos(inp["spawnPos"] as JObject);
        }

        public int? Id
        {
            get; private set;
        }

        public string Name
        {
            get; private set;
        }

        public int? Elo
        {
            get; private set;
        }

        public Pos Pos
        {
            get; private set;
        }

        public int? Life
        {
            get; private set;
        }

        public int? Gold
        {
            get; private set;
        }

        public int? MineCount
        {
            get; private set;
        }

        public Pos SpawnPos
        {
            get; private set;
        }

        public bool? Crashed
        {
            get; private set;
        }

    }
}
