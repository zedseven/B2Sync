using KeePass.App.Configuration;

namespace B2Sync
{
	/// <summary>
	/// Wraps the raw KeePass <see cref="AceCustomConfig"/> system with easier access to relevant configuration options.
	/// </summary>
	public sealed class Configuration
	{
		private const string KeyIdName = "B2SyncKeyId";
		private const string ApplicationKeyName = "B2SyncApplicationKey";
		private const string BucketIdName = "B2SyncBucketId";
		private const string SyncOnSaveName = "B2SyncSyncOnSave";
		private const string SyncOnLoadName = "B2SyncSyncOnLoad";

		private readonly AceCustomConfig _config;

		public string KeyId
		{
			get => _config.GetString(KeyIdName, "");
			set => _config.SetString(KeyIdName, value);
		}
		public string ApplicationKey
		{
			get => _config.GetString(ApplicationKeyName, "");
			set => _config.SetString(ApplicationKeyName, value);
		}
		public string BucketId
		{
			get => _config.GetString(BucketIdName, "");
			set => _config.SetString(BucketIdName, value);
		}
		public bool SyncOnSave
		{
			get => _config.GetBool(SyncOnSaveName, false);
			set => _config.SetBool(SyncOnSaveName, value);
		}
		public bool SyncOnLoad
		{
			get => _config.GetBool(SyncOnLoadName, false);
			set => _config.SetBool(SyncOnLoadName, value);
		}

		public Configuration(AceCustomConfig config)
		{
			_config = config;

			if (_config.GetString(KeyIdName, null) != null)
				return;
			_config.SetString(KeyIdName, "");
			_config.SetString(ApplicationKeyName, "");
			_config.SetString(BucketIdName, "");
			_config.SetBool(SyncOnSaveName, false);
			_config.SetBool(SyncOnLoadName, false);
		}
	}
}