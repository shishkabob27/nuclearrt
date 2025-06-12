using System;
using System.Diagnostics;
using System.IO;

namespace CTFAK.Utils
{
	public static class Logger
	{
		public delegate void EventLogger(string log, ConsoleColor color);
		public static event EventLogger OnLogged;

		private static Action<string>? UILogAction = null;
		public static void SetUILogAction(Action<string>? logAction)
		{
			UILogAction = logAction;
		}

		static StreamWriter _writer;
		static Logger()
		{
			File.Delete("Latest.log");
			_writer = new StreamWriter("Latest.log", false);
			_writer.AutoFlush = true;
		}

		public static void Log(object text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White)
		{
			Log(text.ToString(), logToScreen, color);

		}
		public static void LogWarning(object text)
		{
			Log(text.ToString(), true, ConsoleColor.Yellow);

		}
		public static void Log(string text, bool logToScreen = true, ConsoleColor color = ConsoleColor.White)
		{
			if (UILogAction != null)
			{
				UILogAction(text);
			}

			_writer.WriteLine(text);
		}
	}
}
