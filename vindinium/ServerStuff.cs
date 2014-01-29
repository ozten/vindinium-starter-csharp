using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace vindinium
{
    class ServerStuff
    {
        private string key;
        private bool trainingMode;
        private uint turns;
        private string map;

        private string playURL;
        public string viewURL { get; private set; }

        public Hero myHero { get; private set; }
        public List<Hero> heroes { get; private set; }

        public int currentTurn { get; private set; }
        public int maxTurns { get; private set; }
        public bool finished { get; private set; }
        public bool errored { get; private set; }
        public string errorText { get; private set; }
        private string serverURL;

        public Tile[][] board { get; private set; }

        //if training mode is false, turns and map are ignored8
        public ServerStuff(string key, bool trainingMode, uint turns, string serverURL, string map)
        {
            this.key = key;
            this.trainingMode = trainingMode;
            this.serverURL = serverURL;

            //the reaons im doing the if statement here is so that i dont have to do it later
            if (trainingMode)
            {
                this.turns = turns;
                this.map = map;
            }
        }

        //initializes a new game, its syncronised
        public void createGame()
        {
            errored = false;

            string uri;
            
            if (trainingMode)
            {
                uri = serverURL + "/api/training";
            }
            else
            {
                uri = serverURL + "/api/arena";
            }

            string myParameters = "key=" + key;
            if (trainingMode) myParameters += "&turns=" + turns;
            if (map != null) myParameters += "&map=" + map;

            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    string result = client.UploadString(uri, myParameters);
                    deserialize(result);
                }
                catch (WebException exception)
                {
                    errored = true;
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        errorText = reader.ReadToEnd();
                    }
                }
            }
        }

        private void deserialize(string json)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            //byte[] byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GameResponse));
            GameResponse gameResponse = (GameResponse)ser.ReadObject(stream);

            playURL = gameResponse.playUrl;
            viewURL = gameResponse.viewUrl;

            myHero = gameResponse.hero;
            heroes = gameResponse.game.heroes;

            currentTurn = gameResponse.game.turn;
            maxTurns = gameResponse.game.maxTurns;
            finished = gameResponse.game.finished;

            createBoard(gameResponse.game.board.size, gameResponse.game.board.tiles);
        }

        public void moveHero(string direction)
        {
            string myParameters = "key=" + key + "&dir=" + direction;
            
            //make the request
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                try
                {
                    string result = client.UploadString(playURL, myParameters);
                    deserialize(result);
                }
                catch(WebException exception)
                {
                    errored = true;
                    using(var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        errorText = reader.ReadToEnd();
                    }
                }
            }
        }

        private void createBoard(int size, string data)
        {
            //check to see if the board list is already created, if it is, we just overwrite its values
            if (board == null || board.Length != size)
            {
                board = new Tile[size][];

                //need to initialize the lists within the list
                for (int i = 0; i < size; i++)
                {
                    board[i] = new Tile[size];
                }
            }

            //convert the string to the List<List<Tile>>
            int x = 0;
            int y = 0;
            char[] charData = data.ToCharArray();

            for(int i = 0;i < charData.Length;i += 2)
            {
                switch (charData[i])
                {
                    case '#':
                        board[x][y] = Tile.IMPASSABLE_WOOD;
                        break;
                    case ' ':
                        board[x][y] = Tile.FREE;
                        break;
                    case '@':
                        switch (charData[i + 1])
                        {
                            case '1':
                                board[x][y] = Tile.HERO_1;
                                break;
                            case '2':
                                board[x][y] = Tile.HERO_2;
                                break;
                            case '3':
                                board[x][y] = Tile.HERO_3;
                                break;
                            case '4':
                                board[x][y] = Tile.HERO_4;
                                break;

                        }
                        break;
                    case '[':
                        board[x][y] = Tile.TAVERN;
                        break;
                    case '$':
                        switch (charData[i + 1])
                        {
                            case '-':
                                board[x][y] = Tile.GOLD_MINE_NEUTRAL;
                                break;
                            case '1':
                                board[x][y] = Tile.GOLD_MINE_1;
                                break;
                            case '2':
                                board[x][y] = Tile.GOLD_MINE_2;
                                break;
                            case '3':
                                board[x][y] = Tile.GOLD_MINE_3;
                                break;
                            case '4':
                                board[x][y] = Tile.GOLD_MINE_4;
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
    }
}