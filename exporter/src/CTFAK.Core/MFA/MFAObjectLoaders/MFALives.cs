using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFALives : ObjectLoader
	{
		public uint Player;
		public List<int> Images = new();
		public int Flags;
		public int DisplayType;
		public int Font;
		public int Width;
		public int Height;

		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			Player = reader.ReadUInt32();
			Images = new List<int>();
			var imgCount = reader.ReadInt32();
			for (int i = 0; i < imgCount; i++)
			{
				Images.Add(reader.ReadInt32());
			}

			DisplayType = reader.ReadInt32();
			Flags = reader.ReadInt32();
			Font = reader.ReadInt32();
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
		}
	}
}
