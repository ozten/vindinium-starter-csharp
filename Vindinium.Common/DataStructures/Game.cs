using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Game
	{
		[DataMember(Name = "board")] public Board Board;
		[DataMember(Name = "finished")] public bool Finished;
		[DataMember(Name = "heroes")] public List<Hero> Heroes;
		[DataMember(Name = "id")] public string Id;
		[DataMember(Name = "maxTurns")] public int MaxTurns;
		[DataMember(Name = "turn")] public int Turn;
	}
}