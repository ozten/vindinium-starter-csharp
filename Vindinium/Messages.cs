using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vindinium
{
	public class GameState
	{
		private Tile[][] _jaggedTiles;

		public int X { get; internal set; }
		public int Y { get; internal set; }

		public Uri viewURL { get; internal set; }

		public Hero myHero { get; internal set; }
		public IList<Hero> heroes { get; internal set; }

		public int currentTurn { get; internal set; }
		public int maxTurns { get; internal set; }
		public bool finished { get; internal set; }
		public bool errored { get; internal set; }
		public string errorText { get; internal set; }


		internal void CreateBoard(int size, string data)
		{
			//check to see if the board list is already created, if it is, we just overwrite its values
			if (_jaggedTiles == null || _jaggedTiles.Length != size)
			{
				_jaggedTiles = new Tile[size][];

				//need to initialize the lists within the list
				for (int i = 0; i < size; i++)
				{
					_jaggedTiles[i] = new Tile[size];
				}
			}

			this.X = size;
			this.Y = size;

			//convert the string to the List<List<Tile>>
			int x = 0;
			int y = 0;
			char[] charData = data.ToCharArray();

			for(int i = 0;i < charData.Length;i += 2)
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

		public Tile GetTile(int x, int y) {
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
		Stay, North, East, South, West
    }
}
