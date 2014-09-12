using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
    [DataContract]
    public class Game
    {
        [DataMember(Name = "heroes")] private List<Hero> _players = new List<Hero>();

        [DataMember(Name = "board")]
        public Board Board { get; set; }

        [DataMember(Name = "finished")]
        public bool Finished { get; set; }

        public List<Hero> Players
        {
            get { return _players; }
            set { _players = value; }
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "maxTurns")]
        public int MaxTurns { get; set; }

        [DataMember(Name = "turn")]
        public int Turn { get; set; }
    }
}