using System.Text;

public class FontBankExporter : BaseExporter
{
	public FontBankExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		var fontBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "FontBank.template.cpp");
		var fontBank = File.ReadAllText(fontBankPath);

		var fontBankData = new StringBuilder();
		if (GameData.Fonts.Items.Count != 0) { fontBankData.AppendLine($"Fonts.reserve({GameData.Fonts.Items.Count});"); }
		foreach (var font in GameData.Fonts.Items)
		{
			fontBankData.Append($"Fonts[{font.Handle}] = std::make_shared<FontInfo>({font.Handle}, \"{SanitizeString(font.Value.FaceName.Replace("\0", ""))}\", {font.Value.Width}, {font.Value.Height}, {font.Value.Escapement}, {font.Value.Orientation}, {font.Value.Weight}, {(font.Value.Italic == 1 ? true : false).ToString().ToLower()}, {(font.Value.Underline == 1 ? true : false).ToString().ToLower()}, {(font.Value.StrikeOut == 1 ? true : false).ToString().ToLower()});\n");
		}

		fontBank = fontBank.Replace("{{ FONTS }}", fontBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "FontBank.cpp"), fontBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "FontBank.template.cpp"));
	}
}
