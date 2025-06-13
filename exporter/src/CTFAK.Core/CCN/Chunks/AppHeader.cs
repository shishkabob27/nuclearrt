using CTFAK.Memory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTFAK.Utils;

namespace CTFAK.CCN.Chunks
{
	public class AppHeader : ChunkLoader
	{
		public int Size;
		public int WindowWidth;
		public int WindowHeight;
		public int InitialScore;
		public int InitialLives;
		public int NumberOfFrames;
		public BitDict Flags = new BitDict(new string[]
		{
						"HeadingMaximized",
						"NoHeading",
						"FitInsideBars",
						"MachineIndependentSpeed",
						"ResizeDisplay",
						"MusicOn",
						"SoundOn",
						"DontDisplayMenu",
						"MenuBar",
						"MaximizedOnBoot",
						"MultiSamples",
						"ChangeResolutionMode",
						"SwitchToFromFullscreen",
						"Protected",
						"Copyright",
						"OneFile"
		});
		public BitDict NewFlags = new BitDict(new string[]
		{
						"SamplesOverFrames",
						"RelocFiles",
						"RunFrame",
						"PlaySamplesWhenUnfocused",
						"NoMinimizeBox",
						"NoMaximizeBox",
						"NoThickFrame",
						"DoNotCenterFrame",
						"IgnoreInputOnScreensaver",
						"DisableClose",
						"HiddenAtStart",
						"VisualThemes",
						"VSync",
						"RunWhenMinimized",
						"MDI",
						"RunWhileResizing"
		});

		public BitDict OtherFlags = new BitDict(new string[]
		{
						"DebuggerShortcuts",
						"Unknown1",
						"Unknown2",
						"DontShareSubData",
						"Unknown3",
						"Unknown4",
						"Unknown5",
						"ShowDebugger",
						"Unknown6",
						"Unknown7",
						"Unknown8",
						"Unknown9",
						"Unknown10",
						"Unknown11",
						"Direct3D9or11",
						"Direct3D8or11"
		});

		public Color BorderColor;
		public int FrameRate;
		public short GraphicsMode;
		public Controls Controls;
		public int WindowsMenuIndex;
		public override void Read(ByteReader reader)
		{
			var start = reader.Tell();
			if (!Settings.Old) Size = reader.ReadInt32();
			Flags.flag = (uint)reader.ReadInt16();
			NewFlags.flag = (uint)reader.ReadInt16();
			GraphicsMode = reader.ReadInt16();
			OtherFlags.flag = (uint)reader.ReadInt16();
			WindowWidth = reader.ReadInt16();
			WindowHeight = reader.ReadInt16();
			InitialScore = (int)(reader.ReadUInt32() ^ 0xffffffff);
			InitialLives = (int)(reader.ReadUInt32() ^ 0xffffffff);
			Controls = new Controls();
			if (Settings.Old) reader.Skip(56);
			else Controls.Read(reader);
			BorderColor = reader.ReadColor();
			NumberOfFrames = reader.ReadInt32();
			if (Settings.Old) return;
			FrameRate = reader.ReadInt32();
			WindowsMenuIndex = reader.ReadInt32();
		}
	}

	public class Controls : ChunkLoader
	{
		public List<PlayerControl> Items;

		public override void Read(ByteReader reader)
		{
			Items = new List<PlayerControl>();
			for (int i = 0; i < 4; i++)
			{
				var item = new PlayerControl(reader);
				Items.Add(item);
				item.ControlType = reader.ReadInt16();
			}

			for (int i = 0; i < 4; i++)
			{
				Items[i].Keys.Read();
			}
		}
	}

	public class PlayerControl
	{
		ByteReader _reader;

		public int ControlType;
		public Keys Keys;

		public PlayerControl(ByteReader reader)
		{
			this._reader = reader;
			Keys = new Keys(_reader);
		}
	}

	public class Keys
	{
		ByteReader _reader;

		public short Up;
		public short Down;
		public short Left;
		public short Right;
		public short Button1;
		public short Button2;
		public short Button3;
		public short Button4;

		public Keys(ByteReader reader)
		{
			this._reader = reader;
		}

		public void Read()
		{
			Up = _reader.ReadInt16();
			Down = _reader.ReadInt16();
			Left = _reader.ReadInt16();
			Right = _reader.ReadInt16();
			Button1 = _reader.ReadInt16();
			Button2 = _reader.ReadInt16();
			Button3 = _reader.ReadInt16();
			Button4 = _reader.ReadInt16();
		}
	}
}
