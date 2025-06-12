using System.Collections.Generic;
using System.IO;
using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
	public class BinaryFile : ChunkLoader
	{
		public string name;
		public byte[] data;

		public override void Read(ByteReader reader)
		{
			int size = reader.ReadInt16();

			if (Settings.isMFA)
				reader.Skip(2);

			name = reader.ReadYuniversal(size);

			if (!Settings.isMFA)
				data = reader.ReadBytes(reader.ReadInt32());
		}
	}

	public class BinaryFiles : ChunkLoader
	{
		public List<BinaryFile> files = new List<BinaryFile>();
		public int count;

		public override void Read(ByteReader reader)
		{
			count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				BinaryFile file = new BinaryFile();
				file.Read(reader);
				files.Add(file);
			}
		}
	}
}
