using System;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Forms;
using B2Sync.Properties;
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

		public override bool Initialize(IPluginHost host)
		{
			if (host == null) return false;
			_pluginHost = host;
			_config = new Configuration(_pluginHost.CustomConfig);

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

			ToolStripMenuItem tsmidd = new ToolStripMenuItem
			{
				Text = "Set Application Key"
			};
			tsmi.DropDownItems.Add(tsmidd);

			return tsmi;
		}

		private void OnOptionsClicked(object sender, EventArgs e)
		{
			// Called when the menu item is clicked
			//_pluginHost.Database.MergeIn(EventLogLink, PwMergeMethod.Synchronize);
		}

		private void OnFileSaved(object sender, FileSavedEventArgs e)
		{
			MessageService.ShowInfo("B2Sync has been notified that the user tried to save to the following file:",
				e.Database.IOConnectionInfo.Path, "Result: " + (e.Success ? "success." : "failed.")
				                                  , _pluginHost.Database.Name);
		}
	}
}
