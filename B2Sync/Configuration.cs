using KeePass.App.Configuration;

namespace B2Sync
{
	public sealed class Configuration
	{
		private const string accountIdName = "B2SyncAccountId";
		private const string keyIdName = "B2SyncKeyId";
		private const string applicationKeyName = "B2SyncApplicationKey";
		private const string bucketIdName = "B2SyncBucketId";
		private const string syncOnSaveName = "B2SyncSyncOnSave";

		private AceCustomConfig _config;

		public string AccountId
		{
			get => _config.GetString(accountIdName, "");
			set => _config.SetString(accountIdName, value);
		}
		public string KeyId
		{
			get => _config.GetString(keyIdName, "");
			set => _config.SetString(keyIdName, value);
		}
		public string ApplicationKey
		{
			get => _config.GetString(applicationKeyName, "");
			set => _config.SetString(applicationKeyName, value);
		}
		public string BucketId
		{
			get => _config.GetString(bucketIdName, "");
			set => _config.SetString(bucketIdName, value);
		}
		public bool SyncOnSave
		{
			get => _config.GetBool(syncOnSaveName, false);
			set => _config.SetBool(syncOnSaveName, value);
		}

		public Configuration(AceCustomConfig config)
		{
			_config = config;

			if (_config.GetString(accountIdName) != null)
				return;
			_config.SetString(accountIdName, "");
			_config.SetString(keyIdName, "");
			_config.SetString(applicationKeyName, "");
			_config.SetString(bucketIdName, "");
			_config.SetBool(syncOnSaveName, false);
		}
	}
}