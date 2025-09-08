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
				PakAssetType.Sound => $"sounds/{entry.ID}.wav",
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
