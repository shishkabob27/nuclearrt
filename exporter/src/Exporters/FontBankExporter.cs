using System.Drawing.Text;
using System.Text;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.Utils;

public class FontBankExporter : BaseExporter
{
	public FontBankExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		var fontBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "FontBank.template.cpp");
		var fontBank = File.ReadAllText(fontBankPath);

		var fontBankData = new StringBuilder();
		if (GameData.Fonts.Items.Count != 0) { fontBankData.AppendLine($"Fonts.reserve({GameData.Fonts.Items.Count});"); }


		//Get the font families and file names
		Dictionary<string, List<string>> fontFamilies = new Dictionary<string, List<string>>(); // font family name, font file names
		var fontsFolder = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
		FileInfo[] fontFiles = fontsFolder.GetFiles();
		foreach (var fontFile in fontFiles)
		{
			//go through each file and try to find one with the same name as a Application Font
			List<string> appFontNames = [];
			using (PrivateFontCollection fontCollection = new PrivateFontCollection())
			{
				fontCollection.AddFontFile(fontFile.FullName);
				foreach (var font in fontCollection.Families)
				{
					if (!fontFamilies.TryGetValue(font.Name, out List<string> fontFileNames))
					{
						fontFileNames = [];
						fontFamilies.Add(font.Name, fontFileNames);
					}

					fontFileNames.Add(fontFile.Name);
				}
			}
		}

		foreach (var font in GameData.Fonts.Items)
		{
			fontBankData.Append($"Fonts[{font.Handle}] = new FontInfo({font.Handle}, \"{SanitizeString(font.Value.FaceName.Replace("\0", ""))}\", \"{GetFontFileName(font, fontFamilies)}\", {font.Value.Width}, {font.Value.Height}, {font.Value.Escapement}, {font.Value.Orientation}, {font.Value.Weight}, {(font.Value.Italic == 1 ? true : false).ToString().ToLower()}, {(font.Value.Underline == 1 ? true : false).ToString().ToLower()}, {(font.Value.StrikeOut == 1 ? true : false).ToString().ToLower()});\n");
		}

		fontBank = fontBank.Replace("{{ FONTS }}", fontBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "FontBank.cpp"), fontBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "FontBank.template.cpp"));
	}

	public static string GetFontFileName(FontItem font, Dictionary<string, List<string>> fontFamilies)
	{
		foreach (var fontFamily in fontFamilies)
		{
			if (fontFamily.Key.ToLower().Contains(font.Value.FaceName.Replace("\0", string.Empty).ToLower()))
			{
				return fontFamily.Value.FirstOrDefault();
			}
		}
		Logger.Log($"Font file name not found for font \"{font.Value.FaceName.Replace("\0", string.Empty)}\"");
		return string.Empty;
	}
}
