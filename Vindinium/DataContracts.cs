using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Vindinium
{
    [DataContract]
    public sealed class Pos
    {
        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        [DataMember]
        internal int x;
        [DataMember]
        internal int y;
    }

    [DataContract]
    sealed class Board
    {
        [DataMember]
        internal int size;
        [DataMember]
        internal string tiles;
    }

    [DataContract]
    sealed class GameResponse
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
    sealed class Game
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
    public sealed class Hero
    {
        public int Id
        {
            get { return id; }
        }

        public string Name
        {
            get { return name; }
        }

        public int Elo
        {
            get { return elo; }
        }

        public Pos Pos
        {
            get { return pos; }
        }

        public int Life
        {
            get { return life; }
        }

        public int Gold
        {
            get { return gold; }
        }

        public int MineCount
        {
            get { return mineCount; }
        }

        public Pos SpawnPos
        {
            get { return spawnPos; }
        }

        public bool Crashed
        {
            get { return crashed; }
        }

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
}
