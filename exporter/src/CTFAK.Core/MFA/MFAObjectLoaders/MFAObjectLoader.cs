using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class ObjectLoader : ChunkLoader
	{
		public int ObjectFlags;
		public int NewObjectFlags;
		public Color BackgroundColor;
		public short[] Qualifiers = new short[8];
		public MFAValueList Values;
		public MFAValueList Strings;
		public MFAMovements Movements;
		public Behaviours Behaviours;
		public MFATransition FadeIn;
		public MFATransition FadeOut;

		public override void Read(ByteReader reader)
		{
			ObjectFlags = reader.ReadInt32();
			NewObjectFlags = reader.ReadInt32();
			BackgroundColor = reader.ReadColor();
			var end = reader.Tell() + 2 * (8 + 1);
			for (int i = 0; i < 8; i++)
			{
				var value = reader.ReadInt16();
				// if(value==-1)
				// {
				// break;
				// }
				Qualifiers[i] = value;

			}
			reader.Seek(end);

			Values = new MFAValueList();
			Values.Read(reader);
			Strings = new MFAValueList();
			Strings.Read(reader);
			Movements = new MFAMovements();
			Movements.Read(reader);
			Behaviours = new Behaviours();
			Behaviours.Read(reader);
			if (reader.ReadByte() == 1)
			{
				FadeIn = new MFATransition();
				FadeIn.Read(reader);
			}

			if (reader.ReadByte() == 1)
			{
				FadeOut = new MFATransition();
				FadeOut.Read(reader);
			}
		}
	}
}
