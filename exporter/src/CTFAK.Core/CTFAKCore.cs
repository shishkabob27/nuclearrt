
using CTFAK.FileReaders;

namespace CTFAK
{
	public class CTFAKCore
	{
		public delegate void SaveHandler(int index, int all);

		public static IFileReader currentReader;
		public static string parameters = "";
		public static string path = "";
	}
}
