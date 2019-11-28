using B2Sync.Properties;
using KeePass.Forms;
using KeePass.Plugins;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace B2Sync
{
	/// <summary>
	/// A KeePass plugin that facilitates synchronization of databases with Backblaze B2 buckets.
	/// </summary>
	public sealed class B2SyncExt : Plugin
	{
		private IPluginHost _host;

		private Configuration _config;

		public override Image SmallIcon
		{
			get { return Resources.MenuIcon; }
		}

		public override bool Initialize(IPluginHost host)
		{
			if (host == null) return false;

			_host = host;
			_config = new Configuration(_host.CustomConfig);
			Interface.Init(_host);
			Synchronization.Init(_config);

			_host.MainWindow.FileSaved += OnFileSaved;
			_host.MainWindow.FileOpened += OnFileOpened;

			return true;
		}

		public override void Terminate()
		{
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			if (t != PluginMenuType.Main)
				return null; // No menu items in other locations

			ToolStripMenuItem tsmi = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_B2Sync_Options,
				Image = Resources.MenuIcon
			};

			ToolStripMenuItem tsmiSync = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_Synchronize_DB_with_B2
			};
			tsmiSync.Click += OnSyncClicked;

			ToolStripMenuItem tsmiSyncOnSave = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_Synchronize_on_Save,
				Checked = _config.SyncOnSave
			};
			tsmiSyncOnSave.Click += delegate (object sender, EventArgs e)
			{
				_config.SyncOnSave = !_config.SyncOnSave;
				((ToolStripMenuItem) sender).Checked = _config.SyncOnSave;
			};

			ToolStripMenuItem tsmiSyncOnLoad = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_Synchronize_on_Load,
				Checked = _config.SyncOnLoad
			};
			tsmiSyncOnLoad.Click += delegate (object sender, EventArgs e)
			{
				_config.SyncOnLoad = !_config.SyncOnLoad;
				((ToolStripMenuItem) sender).Checked = _config.SyncOnLoad;
			};

			ToolStripMenuItem tsmiKeys = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_Configure_B2_Keys
			};
			tsmiKeys.Click += delegate
			{
				OptionsForm optionsForm = new OptionsForm(this, _config);
				optionsForm.ShowDialog();
			};

			ToolStripMenuItem tsmiDownloadUrl = new ToolStripMenuItem
			{
				Text = Resources.B2SyncExt_GetMenuItem_Get_Download_URL
			};
			tsmiDownloadUrl.Click += delegate
			{
				DownloadUrlForm friendlyUrlForm = new DownloadUrlForm(_host);
				friendlyUrlForm.ShowDialog();
			};

			tsmi.DropDownItems.Add(tsmiSync);
			tsmi.DropDownItems.Add(tsmiSyncOnSave);
			tsmi.DropDownItems.Add(tsmiSyncOnLoad);
			tsmi.DropDownItems.Add(tsmiKeys);
			tsmi.DropDownItems.Add(tsmiDownloadUrl);

			return tsmi;
		}

		private async void OnSyncClicked(object sender, EventArgs e)
		{
			await Synchronization.SynchronizeDbAsync(_host);
		}

		public void OptionsFormTextChanged(object sender, EventArgs e)
		{
			if (sender.GetType() != typeof(TextBox))
				return;

			TextBox textBox = (TextBox) sender;
			switch (textBox.Name)
			{
				case "keyIdInput":
					_config.KeyId = textBox.Text;
					break;
				case "applicationKeyInput":
					_config.ApplicationKey = textBox.Text;
					break;
				default:
					return;
			}
		}

		private async void OnFileSaved(object sender, FileSavedEventArgs e)
		{
			//Check to make sure the save isn't part of an already-ongoing synchronization to prevent infinite loops
			if (!_config.SyncOnSave || Synchronization.Synchronizing)
				return;
			await Synchronization.SynchronizeDbAsync(_host);
		}

		private async void OnFileOpened(object sender, FileOpenedEventArgs e)
		{
			//Check to make sure the load isn't part of an already-ongoing synchronization to prevent infinite loops
			if (!_config.SyncOnLoad || Synchronization.Synchronizing)
				return;
			await Synchronization.SynchronizeDbAsync(_host);
		}
	}
}
