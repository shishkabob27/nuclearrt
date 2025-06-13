using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static CTFAK.CTFAKCore;

namespace CTFAK.Core.CCN.Chunks.Banks.ImageBank
{
	public class ImageBank : ChunkLoader
	{
		public static List<Task> imageReadingTasks = new();

		public static List<Task> imageWritingTasks = new();
		public static int realGraphicMode = 4;
		public Dictionary<int, FusionImage> Items = new();
		public static event SaveHandler OnImageLoaded;


		public static FusionImage CreateImage()
		{
			if (Settings.TwoFivePlus)
				return new TwoFivePlusImage();
			return new NormalImage();
		}
		public override void Read(ByteReader reader)
		{
			// tysm LAK
			// This comment doesn't belong here, but I'm still keeping it
			if (CTFAKCore.parameters.Contains("-noimg")) return;

			var count = reader.ReadInt32();

			for (var i = 0; i < count; i++)
			{
				var newImg = CreateImage();
				newImg.Read(reader);
				OnImageLoaded?.Invoke(i, count);
				Items.Add(newImg.Handle, newImg);

				if (reader.Tell() >= reader.Size())
					break;
			}

			foreach (var task in imageReadingTasks) task.Wait();
			imageReadingTasks.Clear();
		}
	}
}
