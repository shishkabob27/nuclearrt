using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CTFAK.Utils
{
	public static class Utils
	{
		public static string ClearName(string ogName)
		{
			var str = string.Join("", ogName.Split(Path.GetInvalidFileNameChars()));
			str = str.Replace("?", "");
			return str;
		}
		public static string ReadUniversal(this ByteReader reader, int len = -1)
		{
			if (Settings.Unicode) return reader.ReadWideString(len);
			else return reader.ReadAscii(len);
		}
		public static byte[] GetBuffer(this ByteWriter writer)
		{
			var buf = ((MemoryStream)writer.BaseStream).GetBuffer();
			Array.Resize(ref buf, (int)writer.Size());
			return buf;
		}
		public static Bitmap ResizeImage(this Bitmap imgToResize, int size) => ResizeImage(imgToResize, size, size);
		public static Bitmap ResizeImage(this Bitmap imgToResize, int width, int height) => ResizeImage(imgToResize, new Size(width, height));
		public static Bitmap ResizeImage(this Bitmap imgToResize, Size size)
		{
			var destRect = new Rectangle(0, 0, size.Width, size.Height);
			var destImage = new Bitmap(size.Width, size.Height);

			destImage.SetResolution(imgToResize.Width, imgToResize.Height);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(imgToResize, destRect, 0, 0, imgToResize.Width, imgToResize.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}

			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}
		public static string GetHex(this byte[] data, int count = -1, int position = 0)
		{
			var actualCount = count;
			if (actualCount == -1) actualCount = data.Length;
			string temp = "";
			for (int i = 0; i < actualCount; i++)
			{
				temp += data[i].ToString("X2");
				temp += " ";
			}
			return temp;
		}
		public static T[] To1DArray<T>(T[,] input)
		{
			// Step 1: get total size of 2D array, and allocate 1D array.
			int size = input.Length;
			T[] result = new T[size];

			// Step 2: copy 2D array elements into a 1D array.
			int write = 0;
			for (int i = 0; i <= input.GetUpperBound(0); i++)
			{
				for (int z = 0; z <= input.GetUpperBound(1); z++)
				{
					result[write++] = input[i, z];
				}
			}

			// Step 3: return the new array.
			return result;
		}
	}
}
