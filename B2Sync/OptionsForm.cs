using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace B2Sync
{
	public partial class OptionsForm : Form
	{
		private B2SyncExt _ext;
		private Configuration _config;

		public OptionsForm(B2SyncExt ext, Configuration config)
		{
			InitializeComponent();

			_ext = ext;
			_config = config;
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
			accountIdInput.Text = _config.AccountId;
			applicationKeyInput.Text = _config.ApplicationKey;
			keyIdInput.Text = _config.KeyId;
			bucketIdInput.Text = _config.BucketId;
		}

		public void OptionsFormTextChanged(object sender, EventArgs e)
		{
			_ext.OptionsFormTextChanged(sender, e);
		}
	}
}
