using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeePass.Plugins;

namespace B2Sync
{
	public static class Interface
	{
		private static B2SyncExt _ext;
		private static IPluginHost _pHost;

		public static bool Initialized { get; private set; } = false;

		public static void Init(B2SyncExt ext, IPluginHost host)
		{
			if (Initialized) return;

			_ext = ext;
			_pHost = host;

			Initialized = true;
		}

		public static void UpdateStatus(string message) => _pHost.MainWindow.SetStatusEx(message);

		public static void ShowWorkingBar() => _pHost.MainWindow.MainProgressBar.Visible = true;

		public static void HideWorkingBar() => _pHost.MainWindow.MainProgressBar.Visible = false;
	}
}
