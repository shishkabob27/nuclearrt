using CTFAK.FileReaders;
using CTFAK.Memory;

public class PakBuilder
{
	private List<PakEntry> entries = [];

	public void Build(IFileReader reader, DirectoryInfo outputPath)
	{
		//read the game data
		var gameData = reader.getGameData();

		//images
		foreach (var image in gameData.Images.Items)
		{
			var entry = new PakEntry { Type = PakAssetType.Image, ID = (uint)image.Key };

			using var imageStream = new MemoryStream();
			image.Value.bitmap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Png);
			entry.Size = (uint)imageStream.Length;

			entry.Data = imageStream.ToArray();
			entries.Add(entry);
		}

		int offset = 4 + 4 + entries.Count * 13; // magic + count + entry table
		foreach (var entry in entries)
		{
			entry.Offset = (uint)offset;
			offset += (int)entry.Size;
		}

		using var pak = File.Create(outputPath + "/assets.pak");
		using var writer = new ByteWriter(pak);

		//header
		writer.WriteAscii("NRTP"); // magic (Nuclear RT PAK)
		writer.WriteUInt32((uint)entries.Count);

		//write entries
		foreach (var entry in entries)
		{
			writer.WriteInt8((byte)entry.Type);
			writer.WriteUInt32(entry.ID);
			writer.WriteUInt32(entry.Offset);
			writer.WriteUInt32(entry.Size);
		}

		foreach (var entry in entries)
		{
			pak.Write(entry.Data, 0, (int)entry.Size);
		}

		pak.Flush();
		pak.Close();
	}
}

public enum PakAssetType
{
	Image = 0,
	Sound = 1,
	Font = 2,
	Binary = 3
}

public class PakEntry
{
	public PakAssetType Type { get; set; }
	public uint ID { get; set; }
	public uint Offset { get; set; }
	public uint Size { get; set; }
	public byte[] Data { get; set; } = [];
}
