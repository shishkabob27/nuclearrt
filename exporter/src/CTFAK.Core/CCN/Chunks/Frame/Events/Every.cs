using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Every : ParameterCommon
	{
		public int Delay;
		public int Compteur;

		public override void Read(ByteReader reader)
		{
			Delay = reader.ReadInt32();
			Compteur = reader.ReadInt32();

		}

		public override string ToString()
		{
			return $"Every {Delay / 1000} sec";
		}
	}
}
