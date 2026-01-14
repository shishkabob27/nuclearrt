using System.Drawing;
using System.Linq;
using CTFAK.CCN;
using CTFAK.Core.CCN.Chunks.Banks.ImageBank;
using CTFAK.FileReaders;
using RectpackSharp;

public class AtlasMetadata
{
	public int AtlasIndex { get; set; }
	public int X { get; set; }
	public int Y { get; set; }
}

public class TextureSheetBuilder
{
	public static Dictionary<int, AtlasMetadata> ImageAtlasMetadata
    {
        get
        {
            if (_imageAtlasMetadata == null)
            {
                _imageAtlasMetadata = new Dictionary<int, AtlasMetadata>();
            }
            return _imageAtlasMetadata;
        }
    }
    private static Dictionary<int, AtlasMetadata>? _imageAtlasMetadata;
    
    public static List<Bitmap> TextureSheets {
        get
        {
            return _textureSheets ?? new List<Bitmap>();
        }
    }
    private static List<Bitmap>? _textureSheets;

	public static void Initialize(GameData gameData)
	{
		if (_textureSheets == null)
		{
			_imageAtlasMetadata = new Dictionary<int, AtlasMetadata>();
			_textureSheets = BuildTextureSheet(gameData);
		}
	}

	private static List<Bitmap> BuildTextureSheet(GameData gameData)
    {
        if (_textureSheets != null && _textureSheets.Count > 0) return _textureSheets;
        
        var validImages = new List<(int index, FusionImage image)>();
        
        for (int i = 0; i < gameData.Images.Items.Count; i++)
        {
            var image = gameData.Images.Items.ElementAt(i).Value;
            if (image.bitmap != null)
            {
                validImages.Add((i, image));
            }
        }

        int atlasIndex = 0;
        int remainingStart = 0;
        var textureSheets = new List<Bitmap>();
        while (remainingStart < validImages.Count)
        {
            int remainingCount = validImages.Count - remainingStart;
            PackingRectangle[] rectangles = new PackingRectangle[remainingCount];
            
            for (int i = 0; i < remainingCount; i++)
            {
                var (index, image) = validImages[remainingStart + i];
                rectangles[i] = new PackingRectangle(0, 0, (uint)image.bitmap.Width, (uint)image.bitmap.Height, i);
            }

            int packedCount = remainingCount;
            PackingRectangle bounds = default;
            bool success = false;
            int lastAttemptCount = -1;

            while (!success && packedCount > 0)
            {
                if (packedCount == lastAttemptCount)
                {
                    remainingStart++;
                    packedCount = 0;
                    break;
                }

                try
                {
                    PackingRectangle[] attemptRectangles = new PackingRectangle[packedCount];
                    Array.Copy(rectangles, attemptRectangles, packedCount);
                    RectanglePacker.Pack(attemptRectangles, out bounds, maxBoundsWidth: 2048, maxBoundsHeight: 2048);
                    Array.Copy(attemptRectangles, rectangles, packedCount);
                    success = true;
                }
                catch
                {
                    lastAttemptCount = packedCount;
                    packedCount = Math.Max(1, packedCount / 2);
                }
            }

            if (packedCount == 0)
                continue;

            Bitmap atlas = new Bitmap((int)bounds.Width, (int)bounds.Height);
            using (Graphics g = Graphics.FromImage(atlas))
            {
                for (int i = 0; i < packedCount; i++)
                {
                    var rectangle = rectangles[i];
                    var (originalIndex, image) = validImages[remainingStart + rectangle.Id];
                    g.DrawImage(image.bitmap, (int)rectangle.X, (int)rectangle.Y, image.bitmap.Width, image.bitmap.Height);
                    
                    if (_imageAtlasMetadata != null)
                    {
                        _imageAtlasMetadata[image.Handle] = new AtlasMetadata
                        {
                            AtlasIndex = atlasIndex,
                            X = (int)rectangle.X,
                            Y = (int)rectangle.Y
                        };
                    }
                }
            }

            textureSheets.Add(atlas);

            remainingStart += packedCount;
            atlasIndex++;
        }

        return textureSheets;
    }
}