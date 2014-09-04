using System.Runtime.Serialization;

namespace Vindinium.Contracts
{
	[DataContract]
	internal class Pos
	{
		[DataMember(Name = "x")] internal int X;
		[DataMember(Name = "y")] internal int Y;
	}
}