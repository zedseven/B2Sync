using System;
using B2Net;
using B2Net.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeePassLib;
using System.IO;
using KeePass.DataExchange;
using KeePass.Plugins;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace B2Sync
{
	public class Synchronization
	{
		private B2Client _client;
		private Configuration _config;

		public bool Connected { get; private set; } = false;

		public Synchronization(Configuration config)
		{
			_config = config;

			if (_config.AccountId == null || _config.KeyId == null || _config.ApplicationKey == null || _config.BucketId == null ||
				_config.AccountId.Length <= 0 || _config.KeyId.Length <= 0 || _config.ApplicationKey.Length <= 0 || _config.BucketId.Length <= 0) return;

			InitClient();
		}

		public bool InitClient()
		{
			Connected = false;

			B2Options options = new B2Options
			{
				AccountId = _config.AccountId,
				KeyId = _config.KeyId,
				ApplicationKey = _config.ApplicationKey,
				BucketId = _config.BucketId,
				PersistBucket = true
			};
			try
			{
				_client = new B2Client(B2Client.Authorize(options));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				MessageService.ShowWarning("B2Sync", "An exception occurred when attempting to connect to B2:",
					e.Message, e.StackTrace, e.InnerException?.Message, e.InnerException?.StackTrace);
				return false;
			}

			if (!_client.Capabilities.Capabilities.Contains("readFiles") ||
			    !_client.Capabilities.Capabilities.Contains("writeFiles"))
				return false;

			MessageService.ShowInfo("B2Sync", "Connected to B2 successfully.");

			Connected = true;
			return true;
		}

		public async Task<string> DownloadDbAsync(string dbName)
		{
			if (!Connected) return null;

			List<B2Bucket> buckets = await _client.Buckets.GetList();
			B2Bucket bucket = buckets.Find(b => b.BucketId == _config.BucketId);
			B2File file = await _client.Files.DownloadByName(dbName, bucket.BucketName);
			string tempPath = Path.Combine(Path.GetTempPath(), "KeePass", "B2Sync", file.FileName);
			using (FileStream stream = File.OpenWrite(tempPath))
				stream.Write(file.FileData, 0, file.FileData.Length);
			return tempPath;
		}

		public async Task<bool> UploadDbAsync(PwDatabase localDb)
		{
			if (!Connected) return false;

			string localPath = localDb.IOConnectionInfo.Path;
			byte[] fileData;
			using (FileStream fileStream = File.OpenRead(localPath))
			{
				if (!fileStream.CanRead)
					return false;
				
				fileData = new byte[fileStream.Length];
				const int chunkSize = 128;
				int bytesRead = 0;
				int lastRead;
				do
				{
					lastRead = fileStream.Read(fileData, bytesRead, chunkSize);
					bytesRead += lastRead;
				} while (lastRead > 0);
			}

			List<B2Bucket> buckets = await _client.Buckets.GetList();
			B2Bucket bucket = buckets.Find(b => b.BucketId == _config.BucketId);
			B2File file = await _client.Files.Upload(fileData, Path.GetFileName(localPath), _config.BucketId);

			return file != null;
		}

		public async Task<bool> SynchronizeDbAsync(IPluginHost host)
		{
			if(!Connected) return false;

			PwDatabase sourceDb = host.Database;
			string remoteDbPath = await DownloadDbAsync(sourceDb.Name + ".kdbx");
			IOConnectionInfo connInfo = IOConnectionInfo.FromPath(remoteDbPath);
			FileFormatProvider formatter = host.FileFormatPool.Find("KeePass KDBX (2.x)");

			bool? importResult = ImportUtil.Import(sourceDb, formatter, new[] { connInfo }, true, host.MainWindow, false, host.MainWindow);
			return importResult.GetValueOrDefault(false);
		}
	}
}
