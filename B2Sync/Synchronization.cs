using System;
using B2Net;
using B2Net.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeePassLib;
using System.IO;

namespace B2Sync
{
	public class Synchronization
	{
		private B2Client _client;
		private Configuration _config;

		public Synchronization(Configuration config)
		{
			_config = config;

			if (_config.AccountId.Length <= 0 || _config.KeyId.Length <= 0 || _config.ApplicationKey.Length <= 0 ||
			    _config.BucketId.Length <= 0) return;
			B2Options options = new B2Options
			{
				AccountId = _config.AccountId,
				KeyId = _config.KeyId,
				ApplicationKey = _config.ApplicationKey,
				BucketId = _config.BucketId,
				PersistBucket = true
			};
			_client = new B2Client(B2Client.Authorize(options));

			if (!_client.Capabilities.Capabilities.Contains("readFiles") ||
			    !_client.Capabilities.Capabilities.Contains("writeFiles"))
				return;
		}

		public async Task<string> DownloadDbAsync(string dbName)
		{
			List<B2Bucket> buckets = await _client.Buckets.GetList();
			B2Bucket bucket = buckets.Find(b => b.BucketId == _config.BucketId);
			B2File file = await _client.Files.DownloadByName(dbName, bucket.BucketName);
			string tempPath = Path.Combine(Path.GetTempPath(), "KeePass", "B2Sync", file.FileName);
			using (FileStream stream = File.OpenWrite(tempPath))
				stream.Write(file.FileData, 0, file.FileData.Length);
			return tempPath;
		}

		public async Task SynchronizeDbAsync(PwDatabase sourceDb)
		{
			string remoteDbPath = await DownloadDbAsync(sourceDb.Name + ".kdbx");
		}
	}
}
