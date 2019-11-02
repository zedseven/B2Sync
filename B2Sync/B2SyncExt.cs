using B2Sync.Properties;
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib.Utility;
using System;
using System.Windows.Forms;

namespace B2Sync
{
	public sealed class B2SyncExt : Plugin
	{
		private IPluginHost _pHost;

		private Configuration _config;

		//TODO: Add metadata tags to methods and use more descriptive variable names

		public override bool Initialize(IPluginHost host)
		{
			if (host == null) return false;

			_pHost = host;
			_config = new Configuration(_pHost.CustomConfig);
			Interface.Init(this, _pHost);
			Synchronization.Init(_config);

			_pHost.MainWindow.FileSaved += OnFileSaved;

			return true;
		}

		public override void Terminate()
		{
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			// Provide a menu item for the main location(s)
			if (t != PluginMenuType.Main) return null; // No menu items in other locations

			//TODO: Move all const strings like below into Resources
			//TODO: Add another option about maintaining version history on B2

			ToolStripMenuItem tsmi = new ToolStripMenuItem
			{
				Text = "B2Sync Options",
				Image = Resources.MenuIcon
			};

			ToolStripMenuItem tsmis = new ToolStripMenuItem
			{
				Text = "Synchronize DB with B2"
			};
			tsmis.Click += OnSyncClicked;
			tsmi.DropDownItems.Add(tsmis);

			ToolStripMenuItem tsmirc = new ToolStripMenuItem
			{
				Text = "(Re)connect to B2"
			};
			tsmirc.Click += delegate (object sender, EventArgs e)
			{
				Synchronization.InitClient();
				tsmis.Enabled = Synchronization.Connected;
			};
			tsmi.DropDownItems.Add(tsmirc);

			ToolStripMenuItem tsmisos = new ToolStripMenuItem
			{
				Text = "Synchronize on Save",
				Checked = _config.SyncOnSave
			};
			tsmisos.Click += delegate(object sender, EventArgs e)
			{
				_config.SyncOnSave = !_config.SyncOnSave;
				((ToolStripMenuItem) sender).Checked = _config.SyncOnSave;
			};
			tsmi.DropDownItems.Add(tsmisos);

			ToolStripMenuItem tsmio = new ToolStripMenuItem
			{
				Text = "Configure B2 Keys..."
			};
			tsmio.Click += OnOptionsClicked;
			tsmi.DropDownItems.Add(tsmio);

			tsmi.DropDownItems.Add(tsmis);

			return tsmi;
		}

		private void OnOptionsClicked(object sender, EventArgs e)
		{
			// Called when the menu item is clicked
			OptionsForm optionsForm = new OptionsForm(this, _config);
			if (optionsForm.ShowDialog() != DialogResult.OK)
				return;
		}

		private async void OnSyncClicked(object sender, EventArgs e)
		{
			// Called when the menu item is clicked
			//Task.Run(_sync.UploadDbAsync(_pHost.Database);
			await Synchronization.SynchronizeDbAsync(_pHost);
			/*if(Synchronization.UploadDb(_pHost.Database))
				MessageService.ShowInfo("B2Sync", "Database synced successfully.");
			else
				MessageService.ShowWarning("B2Sync", "Database sync failed.");*/
		}

		public void OptionsFormTextChanged(object sender, EventArgs e)
		{
			if (sender.GetType() != typeof(TextBox))
				return;

			TextBox textBox = (TextBox) sender;
			switch (textBox.Name)
			{
				case "accountIdInput":
					_config.AccountId = textBox.Text;
					break;
				case "keyIdInput":
					_config.KeyId = textBox.Text;
					break;
				case "applicationKeyInput":
					_config.ApplicationKey = textBox.Text;
					break;
				case "bucketIdInput":
					_config.BucketId = textBox.Text;
					break;
				default:
					return;
			}
		}

		private void OnFileSaved(object sender, FileSavedEventArgs e)
		{
			MessageService.ShowInfo("B2Sync has been notified that the user tried to save to the following file:",
				e.Database.IOConnectionInfo.Path, "Result: " + (e.Success ? "success." : "failed."));

			if (!_config.SyncOnSave) return;
			if(!Synchronization.Connected)
				MessageService.ShowWarning("B2Sync", "B2Sync is set to synchronize on DB save, but it is not connected.");
			Synchronization.SynchronizeDb(_pHost);
		}
	}
}
