using System.Runtime.Serialization;

namespace Vindinium.Contracts
{
	[DataContract]
	internal class Hero
	{
		[DataMember(Name = "crashed")] internal bool Crashed;
		[DataMember(Name = "elo")] internal int Elo;
		[DataMember(Name = "gold")] internal int Gold;
		[DataMember(Name = "id")] internal int Id;
		[DataMember(Name = "life")] internal int Life;
		[DataMember(Name = "mineCount")] internal int MineCount;
		[DataMember(Name = "name")] internal string Name;
		[DataMember(Name = "pos")] internal Pos Pos;
		[DataMember(Name = "spanPos")] internal Pos SpawnPos;
	}
}