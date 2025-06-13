using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.CCN.Chunks.Banks
{
	public class FontBank : ChunkLoader
	{
		public bool Compressed;
		public bool Debug;
		public List<FontItem> Items = new List<FontItem>();

		public override void Read(ByteReader reader)
		{
			if ((Settings.Old || Settings.Fusion3Seed) && !Settings.isMFA) return;//TODO FIX FIX FIX
			var count = reader.ReadInt32();
			int offset = 0;
			if (Settings.Build > 284 && !Debug) offset = -1;

			Items = new List<FontItem>();
			for (int i = 0; i < count; i++)
			{
				var item = new FontItem();
				item.Compressed = Compressed;
				item.Read(reader);
				item.Handle += (uint)offset;
				Items.Add(item);
			}
		}
	}

	public class FontItem : ChunkLoader
	{
		public bool Compressed;
		public uint Handle;
		public int Checksum;
		public int References;
		public LogFont Value;

		public override void Read(ByteReader reader)
		{
			Handle = reader.ReadUInt32();
			ByteReader dataReader = null;
			if (Compressed)
			{
				dataReader = Decompressor.DecompressAsReader(reader, out var decompSize);
			}
			else dataReader = reader;

			var currentPos = dataReader.Tell();
			Checksum = dataReader.ReadInt32();
			References = dataReader.ReadInt32();
			var size = dataReader.ReadInt32();
			Value = new LogFont();
			Value.Read(dataReader);


		}
	}

	public class LogFont : ChunkLoader
	{
		private int _height;
		private int _width;
		private int _escapement;
		private int _orientation;
		private int _weight;
		private byte _italic;
		private byte _underline;
		private byte _strikeOut;
		private byte _charSet;
		private byte _outPrecision;
		private byte _clipPrecision;
		private byte _quality;
		private byte _pitchAndFamily;
		private string _faceName;

		public int Height => _height;
		public int Width => _width;
		public int Escapement => _escapement;
		public int Orientation => _orientation;
		public int Weight => _weight;
		public byte Italic => _italic;
		public byte Underline => _underline;
		public byte StrikeOut => _strikeOut;
		public byte CharSet => _charSet;
		public byte OutPrecision => _outPrecision;
		public byte ClipPrecision => _clipPrecision;
		public byte Quality => _quality;
		public byte PitchAndFamily => _pitchAndFamily;
		public string FaceName => _faceName;

		public override void Read(ByteReader reader)
		{
			_height = reader.ReadInt32() * -1;
			_width = reader.ReadInt32();
			_escapement = reader.ReadInt32();
			_orientation = reader.ReadInt32();
			_weight = reader.ReadInt32();
			_italic = reader.ReadByte();
			_underline = reader.ReadByte();
			_strikeOut = reader.ReadByte();
			_charSet = reader.ReadByte();
			_outPrecision = reader.ReadByte();
			_clipPrecision = reader.ReadByte();
			_quality = reader.ReadByte();
			_pitchAndFamily = reader.ReadByte();
			_faceName = reader.ReadYuniversal(32);
		}
	}

	public class TrueTypeMeta : ChunkLoader
	{
		public List<TTM> TTFMetas = new List<TTM>();

		public override void Read(ByteReader reader)
		{
			var end = reader.Tell() + reader.Size();
			while (reader.Tell() < end)
			{
				var newTTM = new TTM();
				newTTM.Read(reader);
				TTFMetas.Add(newTTM);
			}
		}

		public class TTM
		{
			string FontName;
			int FontSize;
			int FontStyle;
			bool Bold;
			bool Italic;
			bool Underline;
			bool Strikeout;
			int ScriptType;

			public void Read(ByteReader reader)
			{
				FontSize = reader.ReadInt32();
				FontSize = -(FontSize + 6);
				reader.ReadInt32(); //Idk Yet
				reader.ReadInt32(); //Idk Yet
				reader.ReadInt32(); //Idk Yet
				FontStyle = reader.ReadByte();
				Bold = reader.ReadByte() > 01;
				reader.ReadInt16(); //Idk Yet
				Italic = reader.ReadByte() != 00;
				Underline = reader.ReadByte() != 00;
				Strikeout = reader.ReadByte() != 00;
				ScriptType = reader.ReadByte();
				if (ScriptType > 0)
					ScriptType = 179 - ScriptType;
				FontName = reader.ReadWideString(32).TrimEnd((char)0);
				reader.ReadInt32(); //Idk Yet
			}
		}
	}

	public class TrueTypeFonts : ChunkLoader
	{
		public List<byte[]> Fonts = new();

		public override void Read(ByteReader reader)
		{
			var end = reader.Tell() + reader.Size();
			while (reader.Tell() < end)
				Fonts.Add(Decompressor.Decompress(reader, out int decomp));
		}
	}
}
