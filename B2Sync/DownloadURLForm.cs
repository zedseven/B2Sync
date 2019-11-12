using System;
using System.Diagnostics;
using System.Windows.Forms;
using KeePass.Plugins;

namespace B2Sync
{
	public partial class DownloadUrlForm : Form
	{
		private const int SecondsPerHour = 3600;

		public DownloadUrlForm(IPluginHost host)
		{
			InitializeComponent();

			downloadUrlDisplay.Text = "";
			downloadUrlDisplay.Click += delegate
			{
				ProcessStartInfo sInfo = new ProcessStartInfo(downloadUrlDisplay.Text);
				Process.Start(sInfo);
			};

			createUrlButton.Click += async delegate
			{
				string dbName = Synchronization.GetDbFileName(host.Database);
				int duration = Math.Max((int) Math.Round(durationInput.Value) * SecondsPerHour, 1);
				downloadUrlDisplay.Text = await Synchronization.GetDownloadUrlWithAuth(dbName, duration);
			};
		}
	}
}
