using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class GameResponse
	{
		[DataMember(Name = "game")] public Game Game;
		[DataMember(Name = "hero")] public Hero Hero;
		[DataMember(Name = "playUrl")] public string PlayUrl;
		[DataMember(Name = "token")] public string Token;
		[DataMember(Name = "viewUrl")] public string ViewUrl;
	}
}