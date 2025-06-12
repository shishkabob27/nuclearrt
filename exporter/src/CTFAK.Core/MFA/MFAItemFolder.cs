using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA
{
	public class MFAItemFolder : ChunkLoader
	{
		public List<uint> Items = new List<uint>();
		public string Name;
		public uint UnkHeader;
		public bool isSimple;

		public override void Read(ByteReader reader)
		{
			UnkHeader = reader.ReadUInt32();
			if (UnkHeader == 0x70000004)
			{
				isSimple = false;
				Name = reader.AutoReadUnicode();
				Items = new List<uint>();
				var count = reader.ReadUInt32();
				for (int i = 0; i < count; i++)
				{
					Items.Add(reader.ReadUInt32());
				}
			}
			else
			{
				isSimple = true;
				Name = null;
				Items = new List<uint>();
				Items.Add(reader.ReadUInt32());
			}
		}
	}
}
