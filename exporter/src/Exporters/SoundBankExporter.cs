using System.Text;

public class SoundBankExporter : BaseExporter
{
	public SoundBankExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		var soundBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "SoundBank.template.cpp");
		var soundBank = File.ReadAllText(soundBankPath);

		var soundBankData = new StringBuilder();
		if (GameData.Sounds.Items.Count != 0) { soundBankData.AppendLine($"Sounds.reserve({GameData.Sounds.Items.Count});"); }
		foreach (var sounds in GameData.Sounds.Items)
		{
			soundBankData.AppendLine($"Sounds[{sounds.Handle}] = new SoundInfo({sounds.Handle}, \"{SanitizeString(sounds.Name.Replace("\0", ""))}\", \"{PakBuilder.GetAudioExtension(sounds.Data[0..4])}\");\n");
		}

		soundBank = soundBank.Replace("{{ SOUNDS }}", soundBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "SoundBank.cpp"), soundBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "SoundBank.template.cpp"));
	}
}
