using CTFAK.FileReaders;
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
			Directory.CreateDirectory(outputPath + "/assets/images");
			int i = 0;
			foreach (var img in gameData.Images.Items)
			{
				//this.Log($"Exporting image {i++}/{gameData.Images.Items.Count}");
				img.Value.bitmap.Save($"{outputPath}/assets/images/{img.Key}.png");
			}
		}

		//sounds
		if (gameData.Sounds.Items.Count > 0)
		{
			Directory.CreateDirectory(outputPath + "/assets/sounds");
			int i = 0;
			foreach (var sound in gameData.Sounds.Items)
			{
				//this.Log($"Exporting sound {i++}/{gameData.Sounds.Items.Count}");
				string name = sound.Name.Replace("\0", string.Empty);
				File.WriteAllBytes($"{outputPath}/assets/sounds/{name}.wav", sound.Data);
			}
		}

		//this.Log("Resources extracted successfully!");
	}
}
