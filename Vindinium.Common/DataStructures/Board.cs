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
            return string.Equals(MapText, other.MapText) && Size == other.Size;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((MapText != null ? MapText.GetHashCode() : 0)*397) ^ Size;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            string border = string.Format("+{0}+", "-".PadLeft(Size*2, '-'));
            sb.AppendLine(border);
            for (int i = 0; i < Size; i++)
            {
                sb.Append("|");
                sb.Append(MapText.Substring(i*Size*2, Size*2));
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