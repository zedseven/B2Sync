using System;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using System.Windows.Forms;
using B2Sync.Properties;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Utility;

namespace B2Sync
{
	public sealed class B2SyncExt : Plugin
	{
		private IPluginHost _pluginHost;

		private Configuration _config;
		private Synchronization _sync;

		public override bool Initialize(IPluginHost host)
		{
			if (host == null) return false;

			_pluginHost = host;
			_config = new Configuration(_pluginHost.CustomConfig);
			_sync = new Synchronization(_config);

			_pluginHost.MainWindow.FileSaved += OnFileSaved;

			return true;
		}

		public override void Terminate()
		{
		}

		public override ToolStripMenuItem GetMenuItem(PluginMenuType t)
		{
			// Provide a menu item for the main location(s)
			if (t != PluginMenuType.Main) return null; // No menu items in other locations

			ToolStripMenuItem tsmi = new ToolStripMenuItem
			{
				Text = "B2Sync Options",
				Image = Resources.MenuIcon
			};
			tsmi.Click += OnOptionsClicked;

			ToolStripMenuItem tsmirc = new ToolStripMenuItem
			{
				Text = "(Re)connect to B2"
			};
			tsmirc.Click += OnReconnectClicked;
			tsmi.DropDownItems.Add(tsmirc);

			ToolStripMenuItem tsmis = new ToolStripMenuItem
			{
				Text = "Synchronize DB with B2"
			};
			tsmirc.Click += OnSyncClicked;
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

		private void OnReconnectClicked(object sender, EventArgs e)
		{
			// Called when the menu item is clicked
			_sync.InitClient();
		}

		private void OnSyncClicked(object sender, EventArgs e)
		{
			// Called when the menu item is clicked
			//Task.Run(_sync.UploadDbAsync(_pluginHost.Database);
			if(_sync.UploadDbAsync(_pluginHost.Database).Result)
				MessageService.ShowInfo("B2Sync", "Database synced successfully.");
			else
				MessageService.ShowWarning("B2Sync", "Database sync failed.");
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
				e.Database.IOConnectionInfo.Path, "Result: " + (e.Success ? "success." : "failed.")
				                                  , _pluginHost.Database.Name);
		}
	}
}
