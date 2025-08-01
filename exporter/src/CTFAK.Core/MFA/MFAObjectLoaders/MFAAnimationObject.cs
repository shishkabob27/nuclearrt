using CTFAK.CCN.Chunks;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTFAK.MFA.MFAObjectLoaders
{
	public class MFAAnimationObject : ObjectLoader
	{
		public Dictionary<int, MFAAnimation> Items = new Dictionary<int, MFAAnimation>();
		public bool _isExt;

		public override void Read(ByteReader reader)
		{
			base.Read(reader);

			if (reader.ReadByte() == 1)
			{
				var animationCount = reader.ReadUInt32();
				for (int i = 0; i < animationCount; i++)
				{
					var item = new MFAAnimation();
					item.Read(reader);
					Items.Add(i, item);
				}
			}
		}
	}

	public class MFAAnimation : ChunkLoader
	{
		public string Name = "";
		public List<MFAAnimationDirection> Directions;

		public override void Read(ByteReader reader)
		{
			Name = reader.AutoReadUnicode();
			var directionCount = reader.ReadInt32();
			Directions = new List<MFAAnimationDirection>();
			for (int i = 0; i < directionCount; i++)
			{
				var direction = new MFAAnimationDirection();
				direction.Read(reader);
				Directions.Add(direction);
			}
		}
	}

	public class MFAAnimationDirection : ChunkLoader
	{
		public int Index;
		public int MinSpeed;
		public int MaxSpeed;
		public int Repeat;
		public int BackTo;
		public List<int> Frames = new List<int>();

		public override void Read(ByteReader reader)
		{
			Index = reader.ReadInt32();
			MinSpeed = reader.ReadInt32();
			MaxSpeed = reader.ReadInt32();
			Repeat = reader.ReadInt32();
			BackTo = reader.ReadInt32();
			var animCount = reader.ReadInt32();
			for (int i = 0; i < animCount; i++)
			{
				Frames.Add(reader.ReadInt32());
			}
		}
	}
}
