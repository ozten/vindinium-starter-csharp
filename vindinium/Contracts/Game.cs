using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vindinium.Contracts
{
	[DataContract]
	internal class Game
	{
		[DataMember(Name = "board")] internal Board Board;
		[DataMember(Name = "finished")] internal bool Finished;
		[DataMember(Name = "heroes")] internal List<Hero> Heroes;
		[DataMember(Name = "id")] internal string Id;
		[DataMember(Name = "maxTurns")] internal int MaxTurns;
		[DataMember(Name = "turn")] internal int Turn;
	}
}