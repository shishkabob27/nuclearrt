using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Program : ParameterCommon
	{
		public short Flags;
		public string Filename;
		public string Command;

		public override void Read(ByteReader reader)
		{
			Flags = reader.ReadInt16();
			Filename = reader.ReadAscii(260);
			Command = reader.ReadAscii();
		}
	}
}
