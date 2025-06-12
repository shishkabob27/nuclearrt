using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Shoot : ParameterCommon
	{
		public Position ShootPos;
		public ushort ObjectInstance;
		public ushort ObjectInfo;
		public short ShootSpeed;

		public override void Read(ByteReader reader)
		{
			ShootPos = new Position();
			ShootPos.Read(reader);
			ObjectInstance = reader.ReadUInt16();
			ObjectInfo = reader.ReadUInt16();
			reader.Skip(4);
			ShootSpeed = reader.ReadInt16();
		}

		public override string ToString()
		{
			return $"Shoot {ShootPos.X}x{ShootPos.Y}";
		}
	}
}
