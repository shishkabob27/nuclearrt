using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Click : ParameterCommon
	{
		public byte IsDouble;
		public byte Button;



		public override void Read(ByteReader reader)
		{
			Button = reader.ReadByte();
			IsDouble = reader.ReadByte();
		}

		public override string ToString()
		{
			return $"{Button}-{IsDouble}";
		}
	}
}
