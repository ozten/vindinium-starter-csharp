using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
    [DataContract]
    public class GameResponse
    {
        [DataMember(Name = "game")]
        public Game Game { get; set; }

        [DataMember(Name = "hero")]
        public Hero Self { get; set; }

        [DataMember(Name = "playUrl")]
        public string PlayUrl { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "viewUrl")]
        public string ViewUrl { get; set; }
    }
}