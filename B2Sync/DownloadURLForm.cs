﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using KeePass.Plugins;

namespace B2Sync
{
	public partial class DownloadURLForm : Form
	{
		private const int SecondsPerHour = 3600;

		public DownloadURLForm(IPluginHost host)
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
				string dbName = host.Database.Name + ".kdbx";
				int duration = Math.Max((int) Math.Round(durationInput.Value) * SecondsPerHour, 1);
				downloadUrlDisplay.Text = await Synchronization.GetDownloadUrlWithAuth(dbName, duration);
			};
		}
	}
}
