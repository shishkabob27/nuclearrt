using CTFAK.Core.CCN.Chunks.Banks.SoundBank;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Sample : ParameterCommon
	{
		public int Handle;
		public string Name;
		public int Flags;

		public override void Read(ByteReader reader)
		{
			Handle = reader.ReadInt16();
			Flags = reader.ReadUInt16();
			Name = reader.ReadYuniversal();
		}

		public override string ToString()
		{
			return $"Sample '{Name}' handle: {Handle}";
		}
	}
}
