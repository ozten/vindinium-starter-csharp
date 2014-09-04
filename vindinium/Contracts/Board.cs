using System.Runtime.Serialization;

namespace Vindinium.Contracts
{
	[DataContract]
	internal class Board
	{
		[DataMember(Name = "MapText")] internal string MapText;
		[DataMember(Name = "size")] internal int Size;
	}
}