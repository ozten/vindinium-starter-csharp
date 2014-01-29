using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace vindinium
{
    [DataContract]
    class GameResponse
    {
        [DataMember]
        internal Game game;

        [DataMember]
        internal Hero hero;

        [DataMember]
        internal string token;

        [DataMember]
        internal string viewUrl;

        [DataMember]
        internal string playUrl;
    }

    [DataContract]
    class Game
    {
        [DataMember]
        internal string id;

        [DataMember]
        internal int turn;

        [DataMember]
        internal int maxTurns;

        [DataMember]
        internal List<Hero> heroes;

        [DataMember]
        internal Board board;

        [DataMember]
        internal bool finished;
    }

    [DataContract]
    class Hero
    {
        [DataMember]
        internal int id;

        [DataMember]
        internal string name;

        [DataMember]
        internal int elo;

        [DataMember]
        internal Pos pos;

        [DataMember]
        internal int life;

        [DataMember]
        internal int gold;

        [DataMember]
        internal int mineCount;

        [DataMember]
        internal Pos spawnPos;

        [DataMember]
        internal bool crashed;
    }

    [DataContract]
    class Pos
    {
        [DataMember]
        internal int x;

        [DataMember]
        internal int y;
    }

    [DataContract]
    class Board
    {
        [DataMember]
        internal int size;

        [DataMember]
        internal string tiles;
    }
}
