using B2Net;
using B2Net.Models;
using KeePass.DataExchange;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace B2Sync
{
	public static class Synchronization
	{
		private static B2Client _client;
		private static Configuration _config;

		public static bool Initialized { get; private set; } = false;
		public static bool Connected { get; private set; } = false;

		public static void Init(Configuration config)
		{
			if(Initialized) return;

			_config = config;

			Initialized = true;

			if (_config.AccountId == null || _config.KeyId == null || _config.ApplicationKey == null || _config.BucketId == null ||
				_config.AccountId.Length <= 0 || _config.KeyId.Length <= 0 || _config.ApplicationKey.Length <= 0 || _config.BucketId.Length <= 0) return;

			InitClient();
		}

		public static bool InitClient()
		{
			Connected = false;

			B2Options options = new B2Options
			{
				AccountId = _config.AccountId,
				KeyId = _config.KeyId,
				ApplicationKey = _config.ApplicationKey,
				BucketId = _config.BucketId,
				PersistBucket = true,
				RequestTimeout = 100
			};
			try
			{
				_client = new B2Client(B2Client.Authorize(options));
				//_client = new B2Client(_config.AccountId, _config.ApplicationKey, _config.KeyId);
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

		public static async Task<string> DownloadDbAsync(string dbName)
		{
			if (!Connected) return null;

			List<B2Bucket> buckets = await _client.Buckets.GetList();
			B2Bucket bucket = buckets.Find(b => b.BucketId == _config.BucketId);
			B2File file = await _client.Files.DownloadByName(dbName, bucket.BucketName);
			string tempPath = Path.Combine(Path.GetTempPath(), "KeePass", "B2Sync", file.FileName);
			using (MemoryStream ms = new MemoryStream(file.FileData))
			{
				using (FileStream fs = File.OpenWrite(tempPath))
				{
					//fs.Write(file.FileData, 0, file.FileData.Length);
					ms.CopyTo(fs);
					fs.Flush(true);
				}
			}
			return tempPath;
		}

		public static string DownloadDb(string dbName) => DownloadDbAsync(dbName).Result;

		public static async Task<bool> UploadDbAsync(PwDatabase localDb)
		{
			if (!Connected) return false;

			Interface.UpdateStatus("Uploading database...");
			Interface.ShowWorkingBar();

			string localPath = localDb.IOConnectionInfo.Path;
			byte[] fileData;
			using (FileStream fs = File.OpenRead(localPath))
			{
				if (!fs.CanRead)
					return false;

				using (MemoryStream ms = new MemoryStream())
				{
					fs.CopyTo(ms);
					fileData = ms.ToArray();
				}
			}

			try
			{
				MessageService.ShowInfo("B2Sync", "Loaded local database into memory.", "Uploading.");
				//B2File file = await _client.Files.Upload(fileData, Path.GetFileName(localPath), _config.BucketId);
				B2UploadUrl uploadUrl = await _client.Files.GetUploadUrl(_config.BucketId);
				MessageService.ShowInfo("B2Sync", "Got upload URL:", uploadUrl.UploadUrl, "Local Path:", localPath);
				B2File file = await _client.Files.Upload(fileData, Path.GetFileName(localPath), uploadUrl, true,
					_config.BucketId);
				MessageService.ShowInfo("B2Sync", "File uploaded successfully.", "File SHA1: " + file.ContentSHA1);
			}
			catch (Exception e)
			{
				MessageService.ShowFatal("B2Sync", "Exception:", e.Message, e.StackTrace, e.InnerException?.Message, e.InnerException?.StackTrace);
			}

			Interface.HideWorkingBar();
			Interface.UpdateStatus("Database upload successful.");

			return true;
		}

		public static bool UploadDb(PwDatabase localDb) => UploadDbAsync(localDb).Result;

		public static async Task<bool> SynchronizeDbAsync(IPluginHost host)
		{
			if(!Connected) return false;

			PwDatabase sourceDb = host.Database;
			string remoteDbPath = await DownloadDbAsync(sourceDb.Name + ".kdbx");
			IOConnectionInfo connInfo = IOConnectionInfo.FromPath(remoteDbPath);
			FileFormatProvider formatter = host.FileFormatPool.Find("KeePass KDBX (2.x)");

			bool? importResult = ImportUtil.Import(sourceDb, formatter, new[] { connInfo }, true, host.MainWindow, false, host.MainWindow);
			return importResult.GetValueOrDefault(false);
		}

		public static bool SynchronizeDb(IPluginHost host) => SynchronizeDbAsync(host).Result;
	}
}
