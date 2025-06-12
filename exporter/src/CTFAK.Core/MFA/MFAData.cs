using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Banks;
using CTFAK.Core.CCN.Chunks.Banks.SoundBank;
using CTFAK.Memory;
using CTFAK.MMFParser.MFA.Loaders;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;


namespace CTFAK.MFA
{
	public class MFAData
	{
		public static readonly string FontBankId = "ATNF";
		public static readonly string ImageBankId = "AGMI";
		public static readonly string MusicBankId = "ASUM";
		public static readonly string SoundBankId = "APMS";

		public short MfaVersion;
		public short MfaSubversion;
		public int Product;
		public int BuildVersion;
		public int LangId = 32;

		public string Name;
		public string Description;
		public string Path;

		public BinaryFiles binaryFiles;

		public FontBank Fonts = new();
		public SoundBank Sounds = new();
		public MusicBank Music = new();
		public AGMIBank Icons = new();
		public AGMIBank Images = new();


		public string Author;
		public string Copyright;
		public string Company;
		public string Version;

		public byte[] Stamp;

		public int WindowX;
		public int WindowY;

		public MFAValueList GlobalValues;
		public MFAValueList GlobalStrings;
		public Color BorderColor;

		public BitDict DisplayFlags = new BitDict(new string[]
		{
						"MaximizedOnBoot",
						"ResizeDisplay",
						"FullscreenAtStart",
						"AllowFullscreen",
						"Heading",
						"HeadingWhenMaximized",
						"MenuBar",
						"MenuOnBoot",
						"NoMinimize",
						"NoMaximize",
						"NoThickFrame",
						"NoCenter",
						"DisableClose",
						"HiddenAtStart",
						"MDI",
						"Unknown1",
						"Unknown2",
						"Unknown3",
						"Unknown4",
						"Unknown5",
						"Unknown6",
						"Unknown7",
						"Unknown8",
						"Unknown9",
						"Unknown10",
						"Unknown11",
						"Unknown12",
						"Unknown13",
						"Unknown14",
						"Unknown15",
						"Unknown16",
						"Unknown17",
						"Unknown18",
						"Unknown19",
						"Unknown20"
		});

		public BitDict GraphicFlags = new BitDict(new string[]
		{
						"MultiSamples",
						"MachineIndependentSpeed",
						"SamplesOverFrames",
						"PlaySamplesWhenUnfocused",
						"IgnoreInputOnScreensaver",
						"DirectX",
						"VRAM",
						"VisualThemes",
						"VSync",
						"RunWhenMinimized",
						"RunWhenResizing",
						"EnableDebuggerShortcuts",
						"NoDebugger",
						"NoSubappSharing",
						"Direct3D9",
						"Direct3D8",
						"Unknown1",
						"Unknown2",
						"Unknown3",
						"IncludePreloaderFlash",
						"DontGenerateHTMLFlash",
						"Unknown4",
						"DisableIME",
						"ReduceCPUUsage",
						"Unknown5",
						"UseHighPerformanceGPU",
						"Profiling",
						"DontProfileAtStart",
						"Direct3D11",
						"PremultipliedAlpha",
						"DontOptimizeEvents",
						"RecordSlowLoops",
		});

		public string HelpFile;
		public string unknown_string; //Found in original mfa build 283 after help file
		public string unknown_string_2; //Found in original mfa build 283 after build path
		public int InitialScore;
		public int InitialLifes;
		public int FrameRate;
		public int BuildType;
		public string BuildPath = "";
		public string CommandLine;
		public string Aboutbox;
		public uint MenuSize;
		public AppMenu Menu;
		private int windowMenuIndex;
		public Dictionary<Int32, Int32> menuImages;
		public byte[] GlobalEvents;
		public int GraphicMode;
		public int IcoCount;
		public int QualCount;
		public MFAControls Controls;
		public List<int> IconImages;
		public List<Tuple<int, string, string, int, string>> Extensions;
		public List<Tuple<string, int>> CustomQuals;
		public List<MFAFrame> Frames;
		public MFAChunks Chunks;

		public void Read(ByteReader reader)
		{
			reader.ReadAscii(4);
			MfaVersion = reader.ReadInt16();
			MfaSubversion = reader.ReadInt16();
			Product = reader.ReadInt32();
			BuildVersion = reader.ReadInt32();
			//reader.ReadInt32();//unknown
			Settings.Build = BuildVersion;
			LangId = reader.ReadInt32();
			Name = reader.AutoReadUnicode();
			Description = reader.AutoReadUnicode();
			Path = reader.AutoReadUnicode();
			var stampLen = reader.ReadInt32();
			Stamp = reader.ReadBytes(stampLen);

			Logger.Log("Reading Fonts");
			if (reader.ReadAscii(4) != FontBankId) throw new Exception("Invalid Font Bank");
			Fonts.Compressed = false;
			Fonts.Read(reader);

			Logger.Log("Reading Sounds");
			if (reader.ReadAscii(4) != SoundBankId) throw new Exception("Invalid Sound Bank");
			Sounds.IsCompressed = false;
			Sounds.Read(reader);

			Logger.Log("Reading Music");
			if (reader.ReadAscii(4) != MusicBankId) throw new Exception("Invalid Music Bank");
			Music.Read(reader);

			Logger.Log("Reading Icons");
			if (reader.ReadAscii(4) != "AGMI") throw new Exception("Invalid Icon Bank: ");
			Icons.Read(reader);

			Logger.Log("Reading Images");
			if (reader.ReadAscii(4) != "AGMI") throw new Exception("Invalid Image Bank");
			Images.Read(reader);
			var nam = reader.AutoReadUnicode();
			Debug.Assert(Name == nam);

			Author = reader.AutoReadUnicode();
			var desc = reader.AutoReadUnicode();
			Debug.Assert(Description == desc);
			//Helper.CheckPattern(, Description);
			Copyright = reader.AutoReadUnicode();
			Company = reader.AutoReadUnicode();
			Version = reader.AutoReadUnicode();
			WindowX = reader.ReadInt32();
			WindowY = reader.ReadInt32();
			BorderColor = reader.ReadColor();
			DisplayFlags.flag = reader.ReadUInt32();
			GraphicFlags.flag = reader.ReadUInt32();

			HelpFile = reader.AutoReadUnicode();
			unknown_string = reader.AutoReadUnicode();


			InitialScore = reader.ReadInt32();
			InitialLifes = reader.ReadInt32();
			FrameRate = reader.ReadInt32();
			BuildType = reader.ReadInt32();
			BuildPath = reader.AutoReadUnicode();
			unknown_string_2 = reader.AutoReadUnicode();
			CommandLine = reader.AutoReadUnicode();
			Aboutbox = reader.AutoReadUnicode();
			reader.ReadUInt32();

			binaryFiles = new BinaryFiles();
			binaryFiles.Read(reader);

			Controls = new MFAControls();
			Controls.Read(reader);

			MenuSize = reader.ReadUInt32();
			var currentPosition = reader.Tell();
			try
			{
				Menu = new AppMenu();
				Menu.Read(reader);
			}
			catch { }
			reader.Seek(MenuSize + currentPosition);

			windowMenuIndex = reader.ReadInt32();
			menuImages = new Dictionary<Int32, Int32>();
			int miCount = reader.ReadInt32();
			for (int i = 0; i < miCount; i++)
			{
				int id = reader.ReadInt32();
				menuImages[id] = reader.ReadInt32();
			}


			GlobalValues = new MFAValueList();
			GlobalValues.Read(reader);
			GlobalStrings = new MFAValueList();
			GlobalStrings.Read(reader);
			GlobalEvents = reader.ReadBytes(reader.ReadInt32());
			GraphicMode = reader.ReadInt32();



			IcoCount = reader.ReadInt32();
			IconImages = new List<int>();
			for (int i = 0; i < IcoCount; i++)
			{
				IconImages.Add(reader.ReadInt32());
			}

			QualCount = reader.ReadInt32();
			CustomQuals = new List<Tuple<string, int>>();
			for (int i = 0; i < QualCount; i++) //qualifiers
			{
				var name = reader.ReadAscii(reader.ReadInt32());
				var handle = reader.ReadInt32();
				CustomQuals.Add(new Tuple<string, int>(name, handle));
			}

			var extCount = reader.ReadInt32();
			Extensions = new List<Tuple<int, string, string, int, string>>();
			for (int i = 0; i < extCount; i++) //extensions
			{
				var handle = reader.ReadInt32();
				var filename = reader.AutoReadUnicode();
				var name = reader.AutoReadUnicode();
				var magic = reader.ReadInt32();
				var subType = reader.AutoReadUnicode();
				reader.Skip(4);
				var tuple = new Tuple<int, string, string, int, string>(handle, filename, name, magic, subType);
				Extensions.Add(tuple);
			}

			if (reader.PeekInt32() > 900)
			{
				reader.ReadInt16();
			}
			//
			List<int> frameOffsets = new List<int>();
			var offCount = reader.ReadInt32();
			for (int i = 0; i < offCount; i++)
			{
				frameOffsets.Add(reader.ReadInt32());
			}


			var nextOffset = reader.ReadInt32();
			Frames = new List<MFAFrame>();
			foreach (var item in frameOffsets)
			{
				reader.Seek(item);
				var testframe = new MFAFrame();
				testframe.Read(reader);

				//Check if frame is included, if not, don't add to list of frames
				if (!testframe.Flags.GetFlag("DontInclude")) Frames.Add(testframe);
			}

			reader.Dispose();
			return;
		}
	}

	public static class MFAUtils
	{
		public static string AutoReadUnicode(this ByteReader reader)
		{
			var len = reader.ReadInt16();
			short check = reader.ReadInt16();
			//Debug.Assert(check == -32768);
			return reader.ReadWideString(len);
		}

		public static void AutoWriteUnicode(this ByteWriter writer, string value)
		{
			if (value == null) value = "";
			writer.WriteInt16((short)value.Length);
			writer.Skip(1);
			writer.WriteInt8(0x80);
			writer.WriteUnicode(value, false);
		}
	}
}
