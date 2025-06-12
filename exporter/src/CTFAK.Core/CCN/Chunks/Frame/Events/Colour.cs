using System.Drawing;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Colour : ParameterCommon
	{
		public Color Value;

		public override void Read(ByteReader reader)
		{
			var bytes = reader.ReadBytes(4);
			Value = Color.FromArgb(bytes[0], bytes[1], bytes[2]);
		}
	}
}
