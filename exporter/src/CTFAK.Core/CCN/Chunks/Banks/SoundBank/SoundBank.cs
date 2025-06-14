using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.Core.CCN.Chunks.Banks.SoundBank
{
	public class SoundBank : ChunkLoader
	{
		public static event CTFAKCore.SaveHandler OnSoundLoaded;

		public int NumOfItems = 0;
		public int References = 0;
		public List<SoundItem> Items = new List<SoundItem>();
		public bool IsCompressed = true;

		public override void Read(ByteReader reader)
		{
			Items = new List<SoundItem>();
			NumOfItems = reader.ReadInt32();

			for (int i = 0; i < NumOfItems; i++)
			{
				if (Settings.Fusion3Seed && !Settings.isMFA) continue;

				var item = new SoundItem();

				item.IsCompressed = IsCompressed;
				item.Read(reader);
				OnSoundLoaded?.Invoke(i, NumOfItems);

				Items.Add(item);
			}
		}
	}

	public class SoundBase : ChunkLoader
	{
		public override void Read(ByteReader reader)
		{
		}
	}

	public class SoundItem : SoundBase
	{
		public int Checksum;
		public uint References;
		public uint Flags;
		public bool IsCompressed = false;
		public uint Handle;
		public string Name;
		public byte[] Data;
		public int Size;


		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			var start = reader.Tell();

			Handle = reader.ReadUInt32() - 1;
			Checksum = reader.ReadInt32();

			References = reader.ReadUInt32();
			var decompressedSize = reader.ReadInt32();
			Flags = reader.ReadByte();
			reader.Skip(3);
			var res = reader.ReadInt32();
			var nameLenght = reader.ReadInt32();
			ByteReader soundData = new(new byte[0]);
			if (IsCompressed && Flags != 33)
			{
				Size = reader.ReadInt32();
				soundData = new ByteReader(Decompressor.DecompressBlock(reader, Size));
			}
			else
				soundData = new ByteReader(reader.ReadBytes(decompressedSize));

			Name = soundData.ReadWideString(nameLenght).Trim();
			if (Flags == 33) soundData.Seek(0);
			Data = soundData.ReadBytes((int)soundData.Size());
			//Logger.Log(Name + " || " + Handle);
		}
	}
}
