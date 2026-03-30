using System.Drawing;
using System.Linq;
using CTFAK.CCN;
using CTFAK.Core.CCN.Chunks.Banks.ImageBank;
using CTFAK.FileReaders;

public class CollisionMaskBuilder
{
	public struct CollisionMask
	{
		public int Handle;
		public byte[] Data;
	}

	private const int ALPHA_THRESHOLD = 128; //TODO: verify if this is accurate

	public static List<CollisionMask> BuildCollisionMask(GameData gameData)
	{
		var collisionMasks = new List<CollisionMask>();

		foreach (var image in gameData.Images.Items.Values)
		{
			var collisionMask = new CollisionMask { Handle = image.Handle };
			Bitmap bitmap = image.bitmap;
			BinaryWriter writer = new BinaryWriter(new MemoryStream());
			
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x += 8)
				{
					byte mask = 0;
					for (int i = 0; i < 8; i++)
					{
						if (x + i < image.Width)
						{
							bool isSolid = bitmap.GetPixel(x + i, y).A > ALPHA_THRESHOLD;
							if (isSolid)
							{
								mask |= (byte)(1 << (7 - i));
							}
						}
					}
					writer.Write(mask);
				}
			}

			collisionMask.Data = ((MemoryStream)writer.BaseStream).ToArray();
			collisionMasks.Add(collisionMask);
			writer.Dispose();
		}

		return collisionMasks;
	}
}
