using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFAFormattedText : ObjectLoader
	{
		public int Width;
		public int Height;
		public Color Color;
		public string Data = string.Empty;

		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			reader.ReadUInt32();
			Color = reader.ReadColor();
			Data = reader.ReadAscii(reader.ReadInt32());
		}
	}
}
