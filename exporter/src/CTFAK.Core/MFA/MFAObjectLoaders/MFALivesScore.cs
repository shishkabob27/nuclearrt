using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFALivesScore : ObjectLoader
	{
		public int Player;
		public bool Text;
		public Color Color1;
		public int Width;
		public int Height;
		public List<int> Images;
		public uint Font;

		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			Player = reader.ReadInt32();

			Images = new List<int>();
			var imageCount = reader.ReadUInt32();
			for (int i = 0; i < imageCount; i++)
			{
				Images.Add((int)reader.ReadUInt32());
			}

			Text = reader.ReadInt32() != 0;
			Color1 = reader.ReadColor();
			Font = reader.ReadUInt32();
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
		}
	}
}
