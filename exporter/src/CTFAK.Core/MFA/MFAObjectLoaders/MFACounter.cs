using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFACounter : ObjectLoader
	{
		public int Value;
		public int Minimum;
		public int Maximum;
		public uint DisplayType;
		public uint CountFlags;
		public Color Color1;
		public uint VerticalGradient;
		public Color Color2;
		public int CountType;
		public int Width;
		public int Height;
		public List<int> Images;
		public uint Font;

		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			Value = reader.ReadInt32();
			Minimum = reader.ReadInt32();
			Maximum = reader.ReadInt32();
			DisplayType = reader.ReadUInt32();
			CountFlags = reader.ReadUInt32();
			Color1 = reader.ReadColor();
			Color2 = reader.ReadColor();
			VerticalGradient = reader.ReadUInt32();
			CountType = reader.ReadInt32();
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			Images = new List<int>();
			var imageCount = reader.ReadUInt32();
			for (int i = 0; i < imageCount; i++)
			{
				Images.Add((int)reader.ReadUInt32());
			}

			Font = reader.ReadUInt32();

		}
	}
}
