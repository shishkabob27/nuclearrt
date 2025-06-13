using System;
using System.Collections.Generic;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks.Objects
{
	public class AlterableValues : ChunkLoader
	{
		public List<int> Items = new List<int>();
		public BitDict Flags = new BitDict(new string[]{});

		public override void Read(ByteReader reader)
		{
			var count = reader.ReadInt16();
			for (int i = 0; i < count; i++)
			{
				try
				{
					Items.Add(reader.ReadInt32());
				}
				catch
				{
					break;
				}
				//Logger.Log($"Reading AltVal {i}: {Items[i]}");
			}
			try
			{
				Flags.flag = reader.ReadUInt32();
			}
			catch { }
		}
	}

	public class AlterableStrings : ChunkLoader
	{
		public List<string> Items = new List<string>();

		public override void Read(ByteReader reader)
		{
			var count = reader.ReadInt16();
			for (int i = 0; i < count; i++)
			{
				try
				{
					Items.Add(reader.ReadWideString());
				}
				catch
				{
					break;
				}
				//Logger.Log($"Reading AltStr {i}: {Items[i]}");
			}
		}
	}
}
