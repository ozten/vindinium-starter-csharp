using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Board
	{
		[DataMember(Name = "MapText")] public string MapText;
		[DataMember(Name = "size")] public int Size;
	}
}