using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace NuclearRTExporter
{
	public partial class MainWindow : Window
	{
		private TextBlock? logTextBlock;

		private bool exportSuccess = false;

		public MainWindow()
		{
			InitializeComponent();
			InitializeControls();

			CTFAK.Utils.Logger.SetUILogAction(Log);

			var exportSettings = Program.ParseArguments();

			Setup(exportSettings);
		}

		private void Setup(ExportSettings settings)
		{
			if (settings == null)
			{
				Log("No args provided.");
				return;
			}

			Log("Starting in auto-export mode...");

			Log($"CCN Path: {settings.CcnPath}");
			Log($"Output Path: {settings.OutputPath}");

			// Start export automatically
			_ = Task.Run(async () =>
			{
				await Task.Delay(500); // Brief delay to ensure UI is ready
				await Dispatcher.UIThread.InvokeAsync(async () =>
							{
								await StartExport(settings.CcnPath, settings.OutputPath);
							});

				if (exportSuccess)
				{
					await Task.Delay(2000);
					Environment.Exit(0);
				}
			});
		}
		private void InitializeControls()
		{
			logTextBlock = this.FindControl<TextBlock>("LogTextBlock");
		}

		private async Task StartExport(string ccnPath, string outputPath)
		{
			try
			{
				await Task.Run(() => RunExport(ccnPath, outputPath));
			}
			catch (Exception ex)
			{
				Log($"Error: {ex.Message}");
			}
		}

		private void RunExport(string ccnPath, string outputPath)
		{
			NuclearRTExporter exporter = new NuclearRTExporter(Log);
			exportSuccess = exporter.Run(ccnPath, outputPath);
		}

		public void Log(string message)
		{
			Dispatcher.UIThread.Post(() =>
			{
				if (logTextBlock != null)
				{
					string timestamp = DateTime.Now.ToString("HH:mm:ss");
					string newText = $"[{timestamp}] {message}\n{logTextBlock.Text}";
					logTextBlock.Text = newText;
				}
			});
		}
	}
}
