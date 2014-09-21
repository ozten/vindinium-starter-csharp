using System;
using System.Runtime.Serialization;
using System.Text;

namespace Vindinium.Common.DataStructures
{
    [DataContract]
    public class Board : IEquatable<Board>
    {
        [DataMember(Name = "tiles")]
        public string MapText { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        public bool Equals(Board other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(MapText, other.MapText);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return MapText == null ? 0 : MapText.GetHashCode();
            }
        }

        public override string ToString()
        {
            var size = (int) Math.Sqrt(MapText.Length/2.0);
            var sb = new StringBuilder();
            string border = string.Format("+{0}+", "-".PadLeft(size*2, '-'));
            sb.AppendLine(border);
            for (int i = 0; i < size; i++)
            {
                sb.Append("|");
                sb.Append(MapText.Substring(i*size*2, size*2));
                sb.AppendLine("|");
            }
            sb.Append(border);
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Board) obj);
        }
    }
}