using CTFAK.Memory;
using CTFAK.Utils;
using System;

namespace CTFAK.MMFParser.EXE.Loaders.Events.Parameters
{
	public class Position : ParameterCommon
	{
		public uint ObjectInfoParent;
		public short Flags;
		public int X;
		public int Y;
		public int Slope;
		public int Angle;
		public int Direction;
		public int TypeParent;
		public int ObjectInfoList;
		public int Layer;

		public override void Read(ByteReader reader)
		{
			ObjectInfoParent = reader.ReadUInt16();
			Flags = reader.ReadInt16();
			X = reader.ReadInt16();
			Y = reader.ReadInt16();
			Slope = reader.ReadInt16();
			Angle = reader.ReadInt16();
			Direction = reader.ReadInt32();
			TypeParent = reader.ReadInt16();
			ObjectInfoList = reader.ReadInt16();
			Layer = reader.ReadInt16();
		}

		public override string ToString()
		{
			return $"Object Info: {ObjectInfoParent}, Flags: {Flags}, X:{X}, Y:{Y}, Slope: {Slope}, Angle:{Angle}, Direction:{Direction}, TypeParent: {TypeParent}, Parent: {ObjectInfoList}, Layer: {Layer}";
		}
	}
}
