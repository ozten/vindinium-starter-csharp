using System.Runtime.Serialization;

namespace Vindinium.Contracts
{
	[DataContract]
	internal class GameResponse
	{
		[DataMember(Name = "game")] internal Game Game;
		[DataMember(Name = "hero")] internal Hero Hero;
		[DataMember(Name = "playUrl")] internal string PlayUrl;
		[DataMember(Name = "token")] internal string Token;
		[DataMember(Name = "viewUrl")] internal string ViewUrl;
	}
}