using System.Text;

public class ImageBankExporter : BaseExporter
{
	public ImageBankExporter(Exporter exporter) : base(exporter) { }

	public override void Export()
	{
		TextureSheetBuilder.Initialize(GameData);
		
		var imageBankPath = Path.Combine(RuntimeBasePath.FullName, "source", "ImageBank.template.cpp");
		var imageBank = File.ReadAllText(imageBankPath);

		var imageBankData = new StringBuilder();
		if (GameData.Images.Items.Values.Count != 0) { imageBankData.AppendLine($"Images.reserve({GameData.Images.Items.Values.Count});"); }
		foreach (var image in GameData.Images.Items.Values)
		{
			int mosaicIndex = 0;
			int mosaicX = 0;
			int mosaicY = 0;
			
			if (TextureSheetBuilder.ImageAtlasMetadata.TryGetValue(image.Handle, out var metadata))
			{
				mosaicIndex = metadata.AtlasIndex;
				mosaicX = metadata.X;
				mosaicY = metadata.Y;
			}
			
			imageBankData.Append($"Images[{image.Handle}] = std::make_shared<ImageInfo>({image.Handle}, {image.Width}, {image.Height}, {image.HotspotX}, {image.HotspotY}, {image.ActionX}, {image.ActionY}, {mosaicIndex}, {mosaicX}, {mosaicY}, {ColorToRGB(image.Transparent)});\n");
		}

		imageBank = imageBank.Replace("{{ IMAGES }}", imageBankData.ToString());

		SaveFile(Path.Combine(OutputPath.FullName, "source", "ImageBank.cpp"), imageBank.ToString());
		File.Delete(Path.Combine(OutputPath.FullName, "source", "ImageBank.template.cpp"));
	}
}
