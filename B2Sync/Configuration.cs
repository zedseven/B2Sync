using System;
using KeePass.App.Configuration;

namespace B2Sync
{
	public sealed class Configuration
	{
		private AceCustomConfig _config;

		public string AccountId
		{
			get => _config.GetString("AccountId");
			set => _config.SetString("AccountId", value);
		}
		public string KeyId
		{
			get => _config.GetString("KeyId");
			set => _config.SetString("KeyId", value);
		}
		public string ApplicationKey
		{
			get => _config.GetString("ApplicationKey");
			set => _config.SetString("ApplicationKey", value);
		}
		public string BucketId
		{
			get => _config.GetString("BucketId");
			set => _config.SetString("BucketId", value);
		}

		public Configuration(AceCustomConfig config)
		{
			_config = config;
		}
	}
}