using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Extension : ParameterCommon
	{
		public short Size;
		public short Type;
		public short Code;
		public byte[] Data;

		public override void Read(ByteReader reader)
		{
			Size = reader.ReadInt16();
			Type = reader.ReadInt16();
			Code = reader.ReadInt16();
			Data = reader.ReadBytes(Size);
		}
	}
}
