using CTFAK.FileReaders;
using CTFAK.Utils;
using System.Drawing;

public class GameDataParser
{
	public void Parse(IFileReader reader, DirectoryInfo outputPath)
	{
		//read the game data
		var gameData = reader.getGameData();

		Directory.CreateDirectory(outputPath + "/assets");

		//export icon bitmap
		if (gameData.Icon != null) gameData.Icon.Save(outputPath + "/assets/icon.png");

		//image bank
		if (gameData.Images.Items.Count > 0)
		{
			Logger.Log($"Exporting {gameData.Images.Items.Count} images");
			Directory.CreateDirectory(outputPath + "/assets/images");
			foreach (var img in gameData.Images.Items)
			{
				img.Value.bitmap.Save($"{outputPath}/assets/images/{img.Key}.png");
			}
		}

		//sounds
		if (gameData.Sounds.Items.Count > 0)
		{
			Logger.Log($"Exporting {gameData.Sounds.Items.Count} sounds");
			Directory.CreateDirectory(outputPath + "/assets/sounds");
			foreach (var sound in gameData.Sounds.Items)
			{
				string name = sound.Name.Replace("\0", string.Empty);
				File.WriteAllBytes($"{outputPath}/assets/sounds/{name}.wav", sound.Data);
			}
		}

		//fonts
		if (gameData.Fonts.Items.Count > 0)
		{
			Logger.Log($"Exporting {gameData.Fonts.Items.Count} fonts");
			Directory.CreateDirectory(outputPath + "/assets/fonts");

			foreach (var font in gameData.Fonts.Items)
			{
				//look through windows fonts folder and find the font
				var fontsFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
				FileInfo[] fontFiles = fontsFolder.GetFiles("*.ttf");
				foreach (var fontFile in fontFiles)
				{
					if (fontFile.Name.StartsWith(font.Value.FaceName.Replace("\0", string.Empty), StringComparison.OrdinalIgnoreCase))
					{
						File.WriteAllBytes($"{outputPath}/assets/fonts/{font.Value.FaceName.Replace("\0", string.Empty)}.ttf", File.ReadAllBytes(fontFile.FullName));
						break;
					}
				}
			}
		}
	}
}
