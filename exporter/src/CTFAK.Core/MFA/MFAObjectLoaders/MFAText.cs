using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFAText : ObjectLoader
	{
		public List<MFAParagraph> Items;
		public uint Width;
		public uint Height;
		public uint Font;
		public Color Color;
		public uint Flags;
		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			Width = reader.ReadUInt32();
			Height = reader.ReadUInt32();
			Font = reader.ReadUInt32();
			Color = reader.ReadColor();
			Flags = reader.ReadUInt32();
			reader.ReadUInt32();
			Items = new List<MFAParagraph>();
			var parCount = reader.ReadUInt32();
			for (int i = 0; i < parCount; i++)
			{
				var par = new MFAParagraph();
				par.Read(reader);
				Items.Add(par);
			}
		}
	}

	public class MFAParagraph : ChunkLoader
	{
		public string Value;
		public uint Flags;

		public override void Read(ByteReader reader)
		{
			Value = reader.AutoReadUnicode();
			Flags = reader.ReadUInt32();
		}
	}
}
