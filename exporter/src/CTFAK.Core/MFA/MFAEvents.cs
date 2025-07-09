using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CTFAK.CCN.Chunks;
using CTFAK.CCN.Chunks.Frame;
using CTFAK.Memory;
using CTFAK.Utils;

namespace CTFAK.MFA
{
	public class MFAEvents : ChunkLoader
	{
		public const string EventData = "Evts";
		public const string CommentData = "Rems";
		public const string ObjectData = "EvOb";
		public const string EventEditorData = "EvCs";
		public const string ObjectListData = "EvEd";
		public const string TimeListData = "EvEd";
		public const string EditorPositionData = "EvTs";
		public const string EditorLineData = "EvLs";
		public const string UnknownEventData = "E2Ts";
		public const string EventEnd = "!DNE";
		public List<EventGroup> Items = new List<EventGroup>();
		public ushort Version;
		public ushort FrameType;
		public List<Comment> Comments = new List<Comment>();
		public List<EventObject> Objects = new List<EventObject>();
		public ushort ConditionWidth;
		public ushort ObjectHeight;
		public List<ushort> ObjectTypes = new List<ushort>();
		public List<ushort> ObjectHandles = new List<ushort>();
		public List<ushort> ObjectFlags = new List<ushort>();
		public List<string> Folders = new List<string>();
		public uint X;
		public uint Y;
		public uint CaretType;
		public uint CaretX;
		public uint CaretY;
		public uint LineY;
		public uint LineItemType;
		public uint EventLine;
		public uint EventLineY;
		public byte[] Saved;
		public int EditorDataUnk;
		public uint EventDataLen;
		public uint CommentDataLen;
		public byte[] _cache;
		public bool _ifMFA;

		public override void Read(ByteReader reader)
		{
			uint size = reader.ReadUInt32();
			long endOffset = reader.Tell() + size;
			if (size == 0)
				return;

			Items = new List<EventGroup>();

			while (true)
			{
				string name = reader.ReadAscii(4);

				if (name == EventData || name == "STVE")
				{
					EventDataLen = reader.ReadUInt32();
					uint end = (uint)(reader.Tell() + EventDataLen);

					while (true)
					{
						EventGroup evGrp = new EventGroup();
						evGrp.isMFA = true;
						evGrp.Read(reader);
						Items.Add(evGrp);
						if (reader.Tell() >= end) break;

						//MFA global events have 2 bytes of padding at the end
						if (reader.Tell() == end - 2)
						{
							reader.Skip(2);
							break;
						}
					}
				}
				else if (name == CommentData || name == "SMER")
				{
					Comments = new List<Comment>(reader.ReadInt32());
					for (int i = 0; i < Comments.Capacity; i++)
					{
						Comment comment = new Comment();
						comment.Read(reader);
						Comments.Add(comment);
					}
				}
				else if (name == "SPRG")
				{
					Items = new List<EventGroup>(reader.ReadInt32());
					reader.Skip(4);
					for (int i = 0; i < Items.Capacity; i++)
					{
						EventGroup eventGroup = new EventGroup();
						eventGroup.Read(reader);
						Items.Add(eventGroup);
					}
				}
				else if (name == ObjectData || name == "SJBO")
				{
					Objects = new List<EventObject>();
					uint len = reader.ReadUInt32();
					for (int i = 0; i < len; i++)
					{
						EventObject eventObject = new EventObject();
						eventObject.Read(reader);
						Objects.Add(eventObject);

					}
				}
				else if (name == EventEditorData)
				{
					EditorDataUnk = reader.ReadInt32();
					ConditionWidth = reader.ReadUInt16();
					ObjectHeight = reader.ReadUInt16();
					reader.Skip(12);
				}
				else if (name == ObjectListData)
				{
					short count = reader.ReadInt16();
					short realCount = count;
					if (count == -1)
					{
						realCount = reader.ReadInt16();
					}

					ObjectTypes = new List<ushort>();
					for (int i = 0; i < realCount; i++)
					{
						ObjectTypes.Add(reader.ReadUInt16());
					}
					ObjectHandles = new List<ushort>();
					for (int i = 0; i < realCount; i++)
					{
						ObjectHandles.Add(reader.ReadUInt16());
					}
					ObjectFlags = new List<ushort>();
					for (int i = 0; i < realCount; i++)
					{
						ObjectFlags.Add(reader.ReadUInt16());
					}

					if (count == -1)
					{
						Folders = new List<string>();
						var folderCount = reader.ReadUInt16();
						for (int i = 0; i < folderCount; i++)
						{
							Folders.Add(reader.AutoReadUnicode());
						}
					}
				}
				else if (name == TimeListData)
				{
					throw new NotImplementedException("I don't like no timelist");
				}
				else if (name == EditorPositionData)
				{
					if (reader.ReadUInt16() != 1)//throw new NotImplementedException("Invalid chunkversion");
						X = reader.ReadUInt32();
					Y = reader.ReadUInt32();
					CaretType = reader.ReadUInt32();
					CaretX = reader.ReadUInt32();
					CaretY = reader.ReadUInt32();
				}
				else if (name == EditorLineData)
				{
					if (reader.ReadUInt16() != 1)//throw new NotImplementedException("Invalid chunkversion");
						LineY = reader.ReadUInt32();
					LineItemType = reader.ReadUInt32();
					EventLine = reader.ReadUInt32();
					EventLineY = reader.ReadUInt32();
				}
				else if (name == UnknownEventData || name == "TYAL")
				{
					reader.Skip(reader.ReadInt32());
				}
				else if (name == EventEnd)
				{
					// _cache = reader.ReadBytes(122);

					break;
				}
				else Logger.Log("UnknownGroup: " + name);

			}
		}
	}

	public class Comment : ChunkLoader
	{
		public uint Handle;
		public string Value;

		public override void Read(ByteReader reader)
		{
			Handle = reader.ReadUInt32();
			Value = reader.AutoReadUnicode();
		}
	}

	public class EventObject : ChunkLoader
	{
		public uint Handle;
		public ushort ObjectType;
		public ushort ItemType;
		public string Name;
		public string TypeName;
		public ushort Flags;
		public uint ItemHandle;
		public uint InstanceHandle;
		public string Code;
		public byte[] IconBuffer;
		public ushort SystemQualifier;

		public override void Read(ByteReader reader)
		{
			Handle = reader.ReadUInt32();
			ObjectType = reader.ReadUInt16();
			ItemType = reader.ReadUInt16();
			Name = reader.AutoReadUnicode();//Not Sure
			TypeName = reader.AutoReadUnicode();//Not Sure
			Flags = reader.ReadUInt16();
			if (ObjectType == 1)//FrameItemType
			{
				ItemHandle = reader.ReadUInt32();
				InstanceHandle = reader.ReadUInt32();
			}
			else if (ObjectType == 2)//ShortcutItemType
			{
				Code = reader.ReadAscii(4);
				//Logger.Log("Code: " + Code);
				if (Code == "OIC2")//IconBufferCode
				{
					IconBuffer = reader.ReadBytes(reader.ReadInt32());
				}
			}
			if (ObjectType == 3) //SystemItemType
			{
				SystemQualifier = reader.ReadUInt16();
			}

		}
	}
}
