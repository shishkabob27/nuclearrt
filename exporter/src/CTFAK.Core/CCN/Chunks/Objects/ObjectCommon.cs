using CTFAK.Core.CCN.Chunks.Objects;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using static CTFAK.CCN.Constants;

namespace CTFAK.CCN.Chunks.Objects
{
	public class ObjectCommon : ChunkLoader
	{
		private short _valuesOffset;
		private short _stringsOffset;
		private uint _fadeinOffset;
		private uint _fadeoutOffset;
		private short _movementsOffset;
		private short _animationsOffset;
		private short _systemObjectOffset;
		private short _counterOffset;
		private short _extensionOffset;

		public short ValuesOffset => _valuesOffset;
		public short StringsOffset => _stringsOffset;
		public uint FadeinOffset => _fadeinOffset;
		public uint FadeoutOffset => _fadeoutOffset;
		public short MovementsOffset => _movementsOffset;
		public short AnimationsOffset => _animationsOffset;
		public short SystemObjectOffset => _systemObjectOffset;
		public short CounterOffset => _counterOffset;
		public short ExtensionOffset => _extensionOffset;


		public string Identifier;

		public Animations Animations;

		public BitDict Preferences = new BitDict(new string[]
				{
								"Backsave",
								"ScrollingIndependant",
								"QuickDisplay",
								"Sleep",
								"LoadOnCall",
								"Global",
								"BackEffects",
								"Kill",
								"InkEffects",
								"Transitions",
								"FineCollisions",
								"AppletProblems"
				}
		);

		public BitDict Flags = new BitDict(new string[]
				{
								"DisplayInFront",
								"Background",
								"Backsave",
								"RunBeforeFadeIn",
								"Movements",
								"Animations",
								"TabStop",
								"WindowProc",
								"Values",
								"Sprites",
								"InternalBacksave",
								"ScrollingIndependant",
								"QuickDisplay",
								"NeverKill",
								"NeverSleep",
								"ManualSleep",
								"Text",
								"DoNotCreateAtStart",
								"FakeSprite",
								"FakeCollisions"
				}
		);

		public BitDict NewFlags = new BitDict(new string[]
				{
								"DoNotSaveBackground",
								"SolidBackground",
								"CollisionBox",
								"VisibleAtStart",
								"ObstacleSolid",
								"ObstaclePlatform",
								"AutomaticRotation"
				}
		);

		public Color BackColor;
		public ObjectInfo Parent;
		public Counters Counters;
		public SubApplication SubApplication;
		public byte[] ExtensionData;
		public int ExtensionPrivate;
		public int ExtensionId;
		public int ExtensionVersion;
		public AlterableValues Values;
		public AlterableStrings Strings;
		public Movements Movements;
		public Text Text;
		public Counter Counter;
		public short[] _qualifiers = new short[8];
		public static int what = 0;

		public ObjectCommon(ObjectInfo parent) { this.Parent = parent; }
		public override void Read(ByteReader reader)
		{
			var currentPosition = reader.Tell();

			reader.Skip(6);
			bool check = reader.ReadInt32() == 0;
			reader.Skip(-6);

			_movementsOffset = reader.ReadInt16();
			_animationsOffset = reader.ReadInt16();
			reader.Skip(2);
			_counterOffset = reader.ReadInt16();
			_systemObjectOffset = reader.ReadInt16();
			reader.Skip(2);

			Flags.flag = reader.ReadUInt32();

			//var end = reader.Tell() + 8 * 2;
			for (int i = 0; i < 8; i++)
			{
				_qualifiers[i] = reader.ReadInt16();
			}

			//reader.Seek(end);
			_extensionOffset = reader.ReadInt16();
			_valuesOffset = reader.ReadInt16();
			_stringsOffset = reader.ReadInt16();

			NewFlags.flag = reader.ReadUInt16();
			Preferences.flag = reader.ReadUInt16();

			Identifier = reader.ReadAscii(4);
			BackColor = reader.ReadColor();
			_fadeinOffset = reader.ReadUInt32();
			_fadeoutOffset = reader.ReadUInt32();
			var unknown2 = reader.ReadUInt16();

			if (_animationsOffset > 0)
			{
				reader.Seek(currentPosition + _animationsOffset);
				Animations = new Animations();
				Animations.Read(reader);
			}

			if (_valuesOffset > 0)
			{
				reader.Seek(currentPosition + _valuesOffset);
				Values = new AlterableValues();
				Values.Read(reader);
			}

			if (_stringsOffset > 0)
			{
				reader.Seek(currentPosition + _stringsOffset);
				Strings = new AlterableStrings();
				Strings.Read(reader);
			}

			if (_movementsOffset > 0)
			{
				if (Settings.Old)
				{
					reader.Seek(currentPosition + _movementsOffset);
					Movements = new Movements();
					var newMovement = new Movement();
					newMovement.Read(reader);
					Movements.Items.Add(newMovement);
				}
				else
				{
					reader.Seek(currentPosition + _movementsOffset);

					Movements = new Movements();
					Movements.Read(reader);
				}



			}

			if (_systemObjectOffset > 0)
			{
				reader.Seek(currentPosition + _systemObjectOffset);
				switch (Identifier)
				{
					//Text
					case "XTÿÿ":
					case "TE":
					case "TEXT":
						Text = new Text();
						Text.Read(reader);
						break;
					//Counter
					case "TRÿÿ":
					case "CNTR":
					case "SCORE":
					case "SCRE":
					case "LIVE":
					case "CN":
					case "LIVES":
						Counters = new Counters();
						Counters.Read(reader);
						break;
					//Sub-Application
					case "CCA ":
						SubApplication = new SubApplication();
						SubApplication.Read(reader);
						break;
				}
			}

			if (_extensionOffset > 0)
			{

				reader.Seek(currentPosition + _extensionOffset);

				var dataSize = reader.ReadInt32() - 20;
				reader.Skip(4); //maxSize;
				ExtensionVersion = reader.ReadInt32();
				ExtensionId = reader.ReadInt32();
				ExtensionPrivate = reader.ReadInt32();
				if (dataSize != 0)
				{
					ExtensionData = reader.ReadBytes(dataSize);
				}
				else ExtensionData = new byte[0];

			}

			if (_counterOffset > 0)
			{
				reader.Seek(currentPosition + _counterOffset);
				Counter = new Counter();
				Counter.Read(reader);
			}
		}
	}
}
