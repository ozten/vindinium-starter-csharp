using System;
using System.Runtime.Serialization;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Pos : IEquatable<Pos>
	{
		[DataMember(Name = "x")]
		public int Y { get; set; }

		[DataMember(Name = "y")]
		public int X { get; set; }

		//TODO: Change x/y back. Map reads backwards?

		#region IEquatable<Pos> Members

		public bool Equals(Pos other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.X == X && other.Y == Y;
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Pos)) return false;
			return Equals((Pos) obj);
		}

		public override string ToString()
		{
			return string.Format("[{0}, {1}]", X, Y);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X*397) ^ Y;
			}
		}

		public static bool operator ==(Pos left, Pos right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Pos left, Pos right)
		{
			return !Equals(left, right);
		}
	}
}