using System;
using System.Text;
using System.Text.RegularExpressions;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
    public class Grid
    {
        public volatile object SynchronizationRoot = new object();
        private string[,] _grid;
        private byte _size;

        public byte Size
        {
            get
            {
                lock (SynchronizationRoot)
                {
                    return _size;
                }
            }
        }

        public string MapText
        {
            set
            {
                lock (SynchronizationRoot)
                {
                    _size = (byte) Math.Sqrt(value.Length/2d);
                    _grid = new string[_size, _size];
                    for (int y = 0; y < _size; y++)
                    {
                        for (int x = 0; x < _size; x++)
                        {
                            int i = y*_size*2;
                            i += x*2;
                            _grid[x, y] = value.Substring(i, 2);
                        }
                    }
                }
            }

            get
            {
                lock (SynchronizationRoot)
                {
                    var sb = new StringBuilder();
                    for (int y = 0; y < _size; y++)
                    {
                        for (int x = 0; x < _size; x++)
                        {
                            sb.Append(_grid[x, y]);
                        }
                    }
                    return sb.ToString();
                }
            }
        }

        public string this[int x, int y]
        {
            get
            {
                lock (SynchronizationRoot)
                {
                    return _grid[x - 1, y - 1];
                }
            }
            private set
            {
                lock (SynchronizationRoot)
                {
                    _grid[x - 1, y - 1] = value;
                }
            }
        }

        public string this[Pos pos]
        {
            get { return this[pos.X, pos.Y]; }
            set { this[pos.X, pos.Y] = value; }
        }

        public Pos PositionOf(string token)
        {
            lock (SynchronizationRoot)
            {
                for (int y = 0; y < _size; y++)
                {
                    for (int x = 0; x < _size; x++)
                    {
                        if (_grid[x, y] == token) return new Pos {X = x + 1, Y = y + 1};
                    }
                }
                return null;
            }
        }

        public void ForEach(Action<Pos> action)
        {
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    action(new Pos {X = x + 1, Y = y + 1});
                }
            }
        }

        internal void MakeSymmetric()
        {
            lock (SynchronizationRoot)
            {
                for (int y = 0; y < _size/2; y++)
                {
                    for (int x = 0; x < _size/2; x++)
                    {
                        string token = _grid[x, y];
                        int max = _size - 1;

                        if (token.StartsWith("@"))
                        {
                            _grid[x, y] = TokenHelper.Player(1);
                            _grid[x, max - y] = TokenHelper.Player(2);
                            _grid[max - x, y] = TokenHelper.Player(3);
                            _grid[max - x, max - y] = TokenHelper.Player(4);
                        }
                        else
                        {
                            _grid[max - x, y] = token;
                            _grid[max - x, max - y] = token;
                            _grid[x, max - y] = token;
                        }
                    }
                }
            }
        }

        internal int TokenCount(string token)
        {
            return Regex.Matches(MapText, Regex.Escape(token)).Count;
        }

        public bool PathExistsBetween(Pos start, Pos end)
        {
            if (start == end) return true;
            if (start + new Pos {X = 1} == end) return true;
            if (start + new Pos {X = -1} == end) return true;
            if (start + new Pos {Y = 1} == end) return true;
            if (start + new Pos {Y = -1} == end) return true;


            return false;
        }
    }
}