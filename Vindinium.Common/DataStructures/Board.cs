using System.Runtime.Serialization;
using System.Text;

namespace Vindinium.Common.DataStructures
{
	[DataContract]
	public class Board
	{
		[DataMember(Name = "tiles")]
		public string MapText { get; set; }

		[DataMember(Name = "size")]
		public int Size { get; set; }

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
	}
}