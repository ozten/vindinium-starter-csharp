using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Hero
	{
		[DataMember(Name = "crashed")] public bool Crashed;
		[DataMember(Name = "elo")] public int Elo;
		[DataMember(Name = "gold")] public int Gold;
		[DataMember(Name = "id")] public int Id;
		[DataMember(Name = "life")] public int Life;
		[DataMember(Name = "mineCount")] public int MineCount;
		[DataMember(Name = "name")] public string Name;
		[DataMember(Name = "pos")] public Pos Pos;
		[DataMember(Name = "spanPos")] public Pos SpawnPos;
	}
}