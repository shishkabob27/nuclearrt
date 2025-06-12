using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class ChildEvent : ParameterCommon
	{
		public int evgOffsetList;
		public short[] ois;

		public override void Read(ByteReader reader)
		{
			var count = reader.ReadInt32();
			evgOffsetList = 0; //wtf cockteam
			ois = new short[count * 2];
			for (int i = 0; i < count * 2; i++)
			{
				ois[i] = reader.ReadInt16();
			}
			reader.Skip(4);
		}

		public override string ToString()
		{
			return $"{ois.Length}";
		}
	}
}
