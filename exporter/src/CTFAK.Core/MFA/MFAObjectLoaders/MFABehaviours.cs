using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class Behaviours : ChunkLoader
	{
		List<Behaviour> _items = new List<Behaviour>();

		public override void Read(ByteReader reader)
		{
			var count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var item = new Behaviour();
				item.Read(reader);
				_items.Add(item);
			}
		}
	}

	class Behaviour : ChunkLoader
	{
		public string Name = "ERROR";
		public byte[] Data;

		public override void Read(ByteReader reader)
		{
			Name = reader.AutoReadUnicode();
			Data = reader.ReadBytes((int)reader.ReadUInt32());
		}
	}
}
