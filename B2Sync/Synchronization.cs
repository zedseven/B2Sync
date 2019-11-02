﻿using B2Net;
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

		//TODO: Automatically reconnect and test for internet connection

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
				//_client = new B2Client(B2Client.Authorize(options));
				_client = new B2Client(_config.KeyId, _config.ApplicationKey);
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

			//TODO: Perform more rigorous tests to ensure that the provided credentials will be usable

			MessageService.ShowInfo("B2Sync", "Connected to B2 successfully.");

			Connected = true;
			return true;
		}

		public static async Task<string> DownloadDbAsync(string dbName)
		{
			if (!Connected) return null;

			Interface.UpdateStatus("Downloading database...");

			//Download to memory
			//List<B2Bucket> buckets = await _client.Buckets.GetList();
			//B2Bucket bucket = buckets.Find(b => b.BucketId == _config.BucketId);
			B2File file = await _client.Files.DownloadByName(dbName, _client.Capabilities.BucketName); //TODO: Investigate if this fails on credentials valid for more than one bucket at a time

			if (file.Size <= 0) //TODO: Might need to find an alternate way to check for file existence
				return null;

			//Write the file to a temporary location
			string tempDir = Path.Combine(Path.GetTempPath(), "KeePass", "B2Sync");
			string tempPath = Path.Combine(tempDir, file.FileName);
			if (!Directory.Exists(tempDir))
				Directory.CreateDirectory(tempDir);
			using (MemoryStream ms = new MemoryStream(file.FileData))
			{
				using (FileStream fs = File.OpenWrite(tempPath))
				{
					ms.CopyTo(fs);
					fs.Flush(true);
				}
			}

			Interface.UpdateStatus("Database download successful.");
			
			return tempPath;
		}

		public static string DownloadDb(string dbName) => DownloadDbAsync(dbName).Result;

		public static async Task<bool> UploadDbAsync(PwDatabase localDb)
		{
			if (!Connected) return false;

			Interface.UpdateStatus("Uploading database...");

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
				//B2File file = await _client.Files.Upload(fileData, Path.GetFileName(localPath), _config.BucketId);
				B2UploadUrl uploadUrl = await _client.Files.GetUploadUrl(_config.BucketId);
				B2File file = await _client.Files.Upload(fileData, Path.GetFileName(localPath), uploadUrl, true,
					_config.BucketId);
			}
			catch (Exception e)
			{
				MessageService.ShowFatal("B2Sync", "Exception:", e.Message, e.StackTrace, e.InnerException?.Message, e.InnerException?.StackTrace);
			}

			Interface.UpdateStatus("Database upload successful.");

			return true;
		}

		public static bool UploadDb(PwDatabase localDb) => UploadDbAsync(localDb).Result;

		public static async Task<bool> SynchronizeDbAsync(IPluginHost host)
		{
			if(!Connected) return false;

			Interface.UpdateStatus("Synchronizing database with B2...");

			//Download the remote copy
			PwDatabase sourceDb = host.Database;
			string remoteDbPath = await DownloadDbAsync(sourceDb.Name + ".kdbx");

			//If the file exists on the remote server, synchronize it with the local copy
			if(remoteDbPath != null)
			{
				IOConnectionInfo connInfo = IOConnectionInfo.FromPath(remoteDbPath);
				FileFormatProvider formatter = host.FileFormatPool.Find("KeePass KDBX (2.x)");

				//TODO: Disable sync-on-save for the duration of the import and remove from recent files

				bool? importResult = ImportUtil.Import(sourceDb, formatter, new[] {connInfo}, true, host.MainWindow,
					false, host.MainWindow);

				//Since the Import operation automatically adds it to the list of recent files, remove it from the list afterwards
				host.MainWindow.FileMruList.RemoveItem(remoteDbPath);

				//Remove the copy of the database from the temp location
				File.Delete(remoteDbPath);

				if (!importResult.GetValueOrDefault(false))
					return false;
			}

			//Upload the local copy to the server once all synchronization is completed
			bool uploadResult = await UploadDbAsync(sourceDb);

			Interface.UpdateStatus("Synchronized database with B2 successfully.");

			return uploadResult;
		}

		public static bool SynchronizeDb(IPluginHost host) => SynchronizeDbAsync(host).Result;
	}
}
