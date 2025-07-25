using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks
{
	public enum ChunkFlags
	{
		NotCompressed = 0,
		Compressed = 1,
		Encrypted = 2,
		CompressedAndEncrypted = 3
	}

	public class Chunk
	{
		public short Id;
		public ChunkFlags Flag;
		public int Size;

		public byte[] Read(ByteReader reader)
		{
			Id = reader.ReadInt16();
			Flag = (ChunkFlags)reader.ReadInt16();
			Size = reader.ReadInt32();
			var rawData = reader.ReadBytes(Size);
			var dataReader = new ByteReader(rawData);
			byte[] ChunkData = null;

			switch (Flag)
			{
				case ChunkFlags.Encrypted:
					ChunkData = dataReader.ReadBytes(Size);
					Decryption.TransformChunk(ChunkData);
					break;
				case ChunkFlags.CompressedAndEncrypted:
					ChunkData = Decryption.DecodeMode3(dataReader.ReadBytes(Size), Id, out var DecompressedSize);
					break;
				case ChunkFlags.Compressed:
					ChunkData = Decompressor.Decompress(dataReader, out DecompressedSize);
					break;
				case ChunkFlags.NotCompressed:
					ChunkData = dataReader.ReadBytes(Size);
					break;
			}

			if (ChunkData == null)
			{
				Logger.Log($"Chunk data is null for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
			}
			if (ChunkData?.Length == 0 && Id != 32639)
			{
				Logger.Log($"Chunk data is empty for chunk {ChunkList.ChunkNames[Id]} with flag {Flag}");
			}
			return ChunkData;
		}
	}

	public abstract class ChunkLoader
	{
		public abstract void Read(ByteReader reader);
	}
}
