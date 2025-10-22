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

		//sounds
		foreach (var sound in gameData.Sounds.Items)
		{
			var entry = new PakEntry { Type = PakAssetType.Sound, ID = (uint)sound.Handle };
			entry.Size = (uint)sound.Data.Length;
			entry.Data = sound.Data;
			entries.Add(entry);
		}

		//fonts
		//TODO: this is bad, redo this
		var fontsFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
		FileInfo[] fontFiles = fontsFolder.GetFiles("*.ttf");
		foreach (var font in gameData.Fonts.Items)
		{
			var entry = new PakEntry { Type = PakAssetType.Font, ID = font.Handle };
			string fontName = font.Value.FaceName.Replace("\0", string.Empty);

			bool found = false;
			foreach (var fontFile in fontFiles)
			{
				if (fontFile.Name.StartsWith(fontName, StringComparison.OrdinalIgnoreCase))
				{
					entry.Data = File.ReadAllBytes(fontFile.FullName);
					entry.Size = (uint)entry.Data.Length;
					found = true;
					break;
				}
			}

			if (!found)
			{
				Console.WriteLine($"Font {fontName} not found");
				continue;
			}

			entries.Add(entry);
		}

		//calculate offsets
		uint dataOffset = 12; // header size
		foreach (var entry in entries)
		{
			entry.Offset = dataOffset;
			dataOffset += entry.Size;
		}

		//create quake pak file
		using var pak = File.Create(outputPath + "/assets.pak");
		using var writer = new ByteWriter(pak);

		//write header
		writer.WriteAscii("PACK"); // magic
		writer.WriteUInt32(dataOffset); // directory offset
		writer.WriteUInt32((uint)(entries.Count * 64)); // directory size

		//write file data
		foreach (var entry in entries)
		{
			pak.Write(entry.Data, 0, (int)entry.Size);
		}

		//write directory
		foreach (var entry in entries)
		{
			string fileName = entry.Type switch
			{
				PakAssetType.Image => $"images/{entry.ID}.png",
				PakAssetType.Sound => $"sounds/{entry.ID}.{GetAudioExtension(entry.Data[0..4])}",
				PakAssetType.Font => $"fonts/{entry.ID}.ttf",
				_ => $"binary/{entry.ID}.bin"
			};

			//pad filename to 56 bytes
			byte[] fileNameBytes = System.Text.Encoding.ASCII.GetBytes(fileName);
			byte[] paddedFileName = new byte[56];
			Array.Copy(fileNameBytes, paddedFileName, Math.Min(fileNameBytes.Length, 56));
			writer.WriteBytes(paddedFileName);

			writer.WriteUInt32(entry.Offset);
			writer.WriteUInt32(entry.Size);
		}

		pak.Flush();
		pak.Close();
	}

	string GetAudioExtension(byte[] magic)
	{
		if (magic[0] == 0xFF && magic[1] == 0xFB ||
			magic[0] == 0xFF && magic[1] == 0xF3 ||
			magic[0] == 0xFF && magic[1] == 0xF2 ||
			magic[0] == 0x49 && magic[1] == 0x44 && magic[2] == 0x33
		)
			return "mp3";

		if (magic[0] == 0x52 && magic[1] == 0x49 && magic[2] == 0x46 && magic[3] == 0x46)
			return "wav";

		if (magic[0] == 0x4F && magic[1] == 0x67 && magic[2] == 0x67 && magic[3] == 0x53)
			return "ogg";

		return "wav";
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
