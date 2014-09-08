using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Vindinium.Common.DataStructures;

namespace Vindinium.Game.Logic
{
	public class GameServer
	{
		public GameResponse Start()
		{
			const string json =
				@"{""game"":{""id"":""the-game-id"",""turn"":0,""maxTurns"":20,""heroes"":[{""id"":1,""name"":""GrimTrick"",""userId"":""8aq2nq2b"",""elo"":1213,""pos"":{""x"":2,""y"":2},""life"":100,""gold"":0,""mineCount"":0,""spawnPos"":{""x"":2,""y"":2},""crashed"":false},{""id"":2,""name"":""random"",""pos"":{""x"":7,""y"":2},""life"":100,""gold"":0,""mineCount"":0,""spawnPos"":{""x"":7,""y"":2},""crashed"":false},{""id"":3,""name"":""random"",""pos"":{""x"":7,""y"":7},""life"":100,""gold"":0,""mineCount"":0,""spawnPos"":{""x"":7,""y"":7},""crashed"":false},{""id"":4,""name"":""random"",""pos"":{""x"":2,""y"":7},""life"":100,""gold"":0,""mineCount"":0,""spawnPos"":{""x"":2,""y"":7},""crashed"":false}],""board"":{""size"":10,""tiles"":""        [][]        ##                ##$-  @1$-####$-@4  $-##  ##        ##  ##                                        ##  ##        ##  ##$-  @2$-####$-@3  $-##                ##        [][]        ""},""finished"":false},""hero"":{""id"":1,""name"":""GrimTrick"",""userId"":""8aq2nq2b"",""elo"":1213,""pos"":{""x"":2,""y"":2},""life"":100,""gold"":0,""mineCount"":0,""spawnPos"":{""x"":2,""y"":2},""crashed"":false},""token"":""the-token"",""viewUrl"":""http://vindinium.org/the-game-id"",""playUrl"":""http://vindinium.org/api/the-game-id/the-token/play""}";
			byte[] byteArray = Encoding.UTF8.GetBytes(json);
			using (var stream = new MemoryStream(byteArray))
			{
				var ser = new DataContractJsonSerializer(typeof (GameResponse));
				return ser.ReadObject(stream) as GameResponse;
			}
		}
	}
}