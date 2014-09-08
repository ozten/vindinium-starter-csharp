using System;
using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Hero : IEquatable<Hero>
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }

		[DataMember(Name = "spawnPos")]
		public Pos SpawnPos { get; set; }

		[DataMember(Name = "pos")]
		public Pos Pos { get; set; }

		[DataMember(Name = "mineCount")]
		public int MineCount { get; set; }

		[DataMember(Name = "elo")]
		public int Elo { get; set; }

		[DataMember(Name = "life")]
		public int Life { get; set; }

		[DataMember(Name = "id")]
		public int Id { get; set; }

		[DataMember(Name = "gold")]
		public int Gold { get; set; }

		[DataMember(Name = "userId")]
		public string UserId { get; set; }

		[DataMember(Name = "crashed")]
		public bool Crashed { get; set; }

		#region IEquatable<Hero> Members

		public bool Equals(Hero other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Crashed.Equals(Crashed) && other.Elo == Elo && other.Gold == Gold && other.Id == Id &&
			       other.Life == Life && other.MineCount == MineCount && Equals(other.Name, Name) && Equals(other.Pos, Pos) &&
			       Equals(other.SpawnPos, SpawnPos) && Equals(other.UserId, UserId);
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Hero)) return false;
			return Equals((Hero) obj);
		}

		public override string ToString()
		{
			return string.Format("#{0} {1} {2}hp {3}gp {4}", Id, Name, Life, Gold, Pos);
		}

		public override int GetHashCode()
		{
			return (UserId != null ? UserId.GetHashCode() : 0);
		}

		public static bool operator ==(Hero left, Hero right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Hero left, Hero right)
		{
			return !Equals(left, right);
		}
	}
}