using System.Text;
using CTFAK.CCN.Chunks.Objects;
using CTFAK.Utils;

public class ExtensionFolderExporter : BaseExporter
{
	public ExtensionFolderExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		var extensions = GetAllExtensionClassNames();

		//find each folder in runtime/extensions/ and copy them to the root output folder
		var extensionsFolder = Path.Combine(OutputPath.FullName, "extensions");
		foreach (var extension in extensions)
		{
			var extensionFolder = Path.Combine(extensionsFolder, extension);
			if (Directory.Exists(extensionFolder))
			{
				FileUtils.CopyFilesRecursively(extensionFolder, OutputPath.FullName);
			}
		}

		Directory.Delete(extensionsFolder, true);
	}

	public List<string> GetAllExtensionClassNames()
	{
		var extensions = new List<string>();
		foreach (var oi in GameData.frameitems)
		{
			if (oi.Value.properties is ObjectCommon common)
			{
				if (common.ExtensionOffset == 0 || common.ExtensionData == null)
					continue;

				ExtensionExporter extension = ExtensionExporterRegistry.GetExporter(common.Identifier);
				if (extension == null)
					continue;

				extensions.Add(extension.CppClassName);
			}
		}
		extensions = extensions.Distinct().ToList();

		return extensions;
	}
}
