using System;
using System.Windows.Forms;

namespace B2Sync
{
	public partial class OptionsForm : Form
	{
		private readonly B2SyncExt _ext;
		private readonly Configuration _config;

		public OptionsForm(B2SyncExt ext, Configuration config)
		{
			InitializeComponent();

			_ext = ext;
			_config = config;
		}

		private void OptionsForm_Load(object sender, EventArgs e)
		{
			applicationKeyInput.Text = _config.ApplicationKey;
			keyIdInput.Text = _config.KeyId;
			bucketIdInput.Text = _config.BucketId;
		}

		private void OptionsFormTextChanged(object sender, EventArgs e)
			=> _ext.OptionsFormTextChanged(sender, e);
	}
}
