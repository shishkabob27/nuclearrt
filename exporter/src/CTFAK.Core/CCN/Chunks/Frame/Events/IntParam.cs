using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class IntParam : ParameterCommon
	{
		public int Value;

		public override void Read(ByteReader reader)
		{
			Value = reader.ReadInt32();
		}
	}
}
