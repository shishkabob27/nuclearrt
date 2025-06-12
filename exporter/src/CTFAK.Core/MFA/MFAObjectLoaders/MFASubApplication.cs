using CTFAK.Memory;
using CTFAK.MFA;
using CTFAK.MFA.MFAObjectLoaders;

namespace CTFAK.Core.MFA.MFAObjectLoaders
{
	public class MFASubApplication : ObjectLoader
	{
		public string fileName;
		public int width;
		public int height;
		public int flaggyflag;
		public int frameNum;
		public override void Read(ByteReader reader)
		{
			base.Read(reader);
			reader.ReadInt32();
		}
	}
}
