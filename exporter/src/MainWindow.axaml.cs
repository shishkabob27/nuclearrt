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
		private ScrollViewer? logScrollViewer;

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

			Log($"CCN Path: {settings.CcnPath}");
			Log($"Output Path: {settings.OutputPath}");
			Log($"Build Type: {settings.BuildType}");

			// Start export automatically
			_ = Task.Run(async () =>
			{
				await Task.Delay(500); // Brief delay to ensure UI is ready
				await Dispatcher.UIThread.InvokeAsync(async () =>
							{
								await StartExport(settings);
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
			logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
		}

		private async Task StartExport(ExportSettings settings)
		{
			try
			{
				await Task.Run(() => RunExport(settings));
			}
			catch (Exception ex)
			{
				Log($"Error: {ex.Message}");
			}
		}

		private void RunExport(ExportSettings settings)
		{
			NuclearRTExporter exporter = new NuclearRTExporter(Log);
			exportSuccess = exporter.Run(settings);
		}

		public void Log(string message)
		{
			Dispatcher.UIThread.Post(() =>
			{
				if (logTextBlock != null && logScrollViewer != null)
				{
					bool wasAtBottom = Math.Abs(logScrollViewer.Offset.Y - logScrollViewer.ScrollBarMaximum.Y) < 1.0;

					string timestamp = DateTime.Now.ToString("HH:mm:ss");
					string newLogEntry = $"[{timestamp}] {message}";

					if (string.IsNullOrEmpty(logTextBlock.Text))
					{
						logTextBlock.Text = newLogEntry;
					}
					else
					{
						logTextBlock.Text = $"{logTextBlock.Text}\n{newLogEntry}";
					}

					if (wasAtBottom)
					{
						logScrollViewer.ScrollToEnd();
					}
				}
			});
		}
	}
}
