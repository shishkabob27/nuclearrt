using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA
{
	public class MFAFrame : ChunkLoader
	{
		public string Name = "ERROR";
		public int SizeX;
		public int SizeY;
		public Color Background;
		public int MaxObjects;
		public List<MFAObjectInfo> Items = new List<MFAObjectInfo>();
		public int Handle;
		public int LastViewedX;
		public int LastViewedY;
		public List<MFAItemFolder> Folders = new List<MFAItemFolder>();
		public List<MFAObjectInstance> Instances = new List<MFAObjectInstance>();
		public BitDict Flags = new BitDict(new string[]
		{
						"GrabDesktop",
						"KeepDisplay",
						"BackgroundCollisions",
						"DisplayFrameTitle",
						"ResizeToScreen",
						"ForceLoadOnCall",
						"NoDisplaySurface",
						"ScreenSaverSetup",
						"TimerBasedMovements",
						"MochiAds",
						"NoGlobalEvents",
						"Unk1",
						"DontInclude",
						"DontEraseBG",
						"Unk2",
						"ForceLoadOnCallIgnore"
		});

		public string Password;
		public string UnkString = "";
		public List<Color> Palette = new Color[256].ToList();
		public int StampHandle;
		public int ActiveLayer;
		public List<MFALayer> Layers = new List<MFALayer>();
		public MFAEvents Events;
		public MFAChunks Chunks;
		public MFATransition FadeIn;
		public MFATransition FadeOut;
		public int PaletteSize;

		public override void Read(ByteReader reader)
		{
			Handle = reader.ReadInt32();
			Name = reader.AutoReadUnicode();
			SizeX = reader.ReadInt32();
			SizeY = reader.ReadInt32();
			Background = reader.ReadColor();
			Flags.flag = reader.ReadUInt32();

			MaxObjects = reader.ReadInt32();

			reader.ReadInt32();//garbage
			var password = reader.ReadBytes(reader.ReadInt32());


			LastViewedX = reader.ReadInt32();
			LastViewedY = reader.ReadInt32();

			PaletteSize = reader.ReadInt32();
			Palette = new List<Color>();
			for (int i = 0; i < 256; i++)
			{
				Palette.Add(reader.ReadColor());
			}

			StampHandle = reader.ReadInt32();
			ActiveLayer = reader.ReadInt32();
			int layersCount = reader.ReadInt32();
			for (int i = 0; i < layersCount; i++)
			{
				var layer = new MFALayer();
				layer.Read(reader);
				Layers.Add(layer);
			}

			if (reader.ReadByte() == 1)
			{
				FadeIn = new MFATransition();
				FadeIn.Read(reader);
			}

			if (reader.ReadByte() == 1)
			{
				FadeOut = new MFATransition();
				FadeOut.Read(reader);
			}
			var frameItemsCount = reader.ReadInt32();
			for (int i = 0; i < frameItemsCount; i++)
			{
				var frameitem = new MFAObjectInfo();
				frameitem.Read(reader);

				Items.Add(frameitem);
			}
			var folderCount = reader.ReadInt32();
			for (int i = 0; i < folderCount; i++)
			{
				var folder = new MFAItemFolder();
				folder.Read(reader);
				Folders.Add(folder);
			}
			var instancesCount = reader.ReadInt32();
			for (int i = 0; i < instancesCount; i++)
			{
				var inst = new MFAObjectInstance();
				inst.Read(reader);
				Instances.Add(inst);
			}

			// var unkCount = reader.ReadInt32();
			// for (int i = 0; i < unkCount; i++)
			// {
			// UnkBlocks.Add(reader.ReadBytes(32));
			// }

			Events = new MFAEvents();
			Events.Read(reader);


			//Chunks = new MFAChunks();
			// Chunks.Log = true;
			//Chunks.Read(reader);

		}
	}
}
