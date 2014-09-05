using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructure
{
	[DataContract]
	public class Pos
	{
		[DataMember(Name = "x")] public int X;
		[DataMember(Name = "y")] public int Y;
	}
}