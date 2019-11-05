using B2Net;
using B2Net.Models;
using KeePass.DataExchange;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Serialization;
using KeePassLib.Utility;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace B2Sync
{
	/// <summary>
	/// Facilitates all of the communication and synchronization with Backblaze B2 systems.
	/// </summary>
	public static class Synchronization
	{
		private static Configuration _config;

		private static bool Initialized { get; set; }

		public static bool Synchronizing { get; private set; }

		public static void Init(Configuration config)
		{
			if(Initialized) return;

			_config = config;

			Initialized = true;
		}

		private static B2Client GetClient()
		{
			B2Client client;

			try
			{
				client = new B2Client(_config.KeyId, _config.ApplicationKey);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				MessageService.ShowWarning("B2Sync", "An exception occurred when attempting to connect to B2:",
					e.Message, e.StackTrace, e.InnerException?.Message, e.InnerException?.StackTrace);
				return null;
			}

			if (!client.Capabilities.Capabilities.Contains("readFiles") ||
			    !client.Capabilities.Capabilities.Contains("writeFiles"))
				return null;

			//TODO: Perform more rigorous tests to ensure that the provided credentials will be usable

			Interface.UpdateStatus("Connected to B2 successfully.");

			return client;
		}

		/// <summary>
		/// Downloads the database with <paramref name="dbName" /> from the bucket <paramref name="client" /> has access to, and stores it in a directory in the temporary location of the environment.
		/// </summary>
		/// <param name="client">The <see cref="B2Client"/> created by <see cref="GetClient"/> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database to download from the B2 bucket.</param>
		/// <returns>The filepath where the database was downloaded to temporarily, or <see langword="true">null</see> if the download failed.</returns>
		public static async Task<string> DownloadDbAsync(B2Client client, string dbName)
		{
			if (client == null)
				return null;

			Interface.UpdateStatus("Downloading database...");

			//Download to memory
			B2File file = await client.Files.DownloadByName(dbName, client.Capabilities.BucketName); //TODO: Investigate if this fails on credentials valid for more than one bucket at a time

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

		/// <summary>
		/// Downloads the database with <paramref name="dbName" /> from the bucket the plugin configuration allows access to, and stores it in a directory in the temporary location of the environment.
		/// </summary>
		/// <param name="dbName">The filename (with extension) of the database to download from the B2 bucket.</param>
		/// <returns>The filepath where the database was downloaded to temporarily, or <see langword="true">null</see> if the download failed.</returns>
		public static async Task<string> DownloadDbAsync(string dbName) => await DownloadDbAsync(GetClient(), dbName);


		/// <summary>
		/// Uploads the locally-stored database <paramref name="localDb"/> to the bucket that <paramref name="client"/> has access to.
		/// </summary>
		/// <param name="client">The <see cref="B2Client"/> created by <see cref="GetClient"/> with access to a bucket to upload to.</param>
		/// <param name="localDb">The local database to upload.</param>
		/// <returns><c>true</c> if the upload was successful, or <c>false</c> otherwise.</returns>
		public static async Task<bool> UploadDbAsync(B2Client client, PwDatabase localDb)
		{
			if (client == null)
				return false;

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
				B2UploadUrl uploadUrl = await client.Files.GetUploadUrl(_config.BucketId);
				B2File file = await client.Files.Upload(fileData, Path.GetFileName(localPath), uploadUrl, true,
					_config.BucketId);
			}
			catch (Exception e)
			{
				MessageService.ShowFatal("B2Sync", "Exception:", e.Message, e.StackTrace, e.InnerException?.Message, e.InnerException?.StackTrace);
			}

			Interface.UpdateStatus("Database upload successful.");

			return true;
		}

		/// <summary>
		/// Uploads the locally-stored database <paramref name="localDb"/> to the bucket that plugin configuration allows access to.
		/// </summary>
		/// <param name="localDb">The local database to upload.</param>
		/// <returns><c>true</c> if the upload was successful, or <c>false</c> otherwise.</returns>
		public static async Task<bool> UploadDbAsync(PwDatabase localDb) => await UploadDbAsync(GetClient(), localDb);


		/// <summary>
		/// Synchronizes the local database that <paramref name="host"/> has open with the database (if any) by the same name stored on the bucket that <paramref name="client"/> has access to.
		/// </summary>
		/// <param name="client">The <see cref="B2Client"/> created by <see cref="GetClient"/> with access to a bucket to synchronize with.</param>
		/// <param name="host">The <see cref="IPluginHost"/> that hosts the currently-running instance of <see cref="B2SyncExt"/>.</param>
		/// <returns><c>true</c> if the synchronization was successful, or <c>false</c> otherwise.</returns>
		public static async Task<bool> SynchronizeDbAsync(B2Client client, IPluginHost host)
		{
			if(client == null)
				return false;

			Synchronizing = true;

			Interface.UpdateStatus("Synchronizing database with B2...");

			//Download the remote copy
			PwDatabase sourceDb = host.Database;
			string remoteDbPath = await DownloadDbAsync(client, sourceDb.Name + ".kdbx");


			bool localMatchesRemote = true;

			//If the file exists on the remote server, synchronize it with the local copy
			if(remoteDbPath != null)
			{
				string localHash = HashFileOnDisk(sourceDb.IOConnectionInfo.Path);
				string remoteHash = HashFileOnDisk(remoteDbPath);

				localMatchesRemote = localHash == remoteHash;
				MessageService.ShowInfo(localHash, remoteHash);
				if (!localMatchesRemote)
				{
					IOConnectionInfo connInfo = IOConnectionInfo.FromPath(remoteDbPath);
					FileFormatProvider formatter = host.FileFormatPool.Find("KeePass KDBX (2.x)");

					bool? importResult = ImportUtil.Import(sourceDb, formatter, new[] {connInfo}, true, host.MainWindow,
						false, host.MainWindow);

					//Since the Import operation automatically adds it to the list of recent files, remove it from the list afterwards
					host.MainWindow.FileMruList.RemoveItem(remoteDbPath);

					//Remove the copy of the database from the temp location
					File.Delete(remoteDbPath);

					if (!importResult.GetValueOrDefault(false))
						return false;
				}
			}

			//Upload the local copy to the server once all synchronization is completed
			bool uploadResult = ((!localMatchesRemote || remoteDbPath == null) && await UploadDbAsync(client, sourceDb)) || localMatchesRemote;

			Interface.UpdateStatus("Synchronized database with B2 successfully.");

			Synchronizing = false;

			return uploadResult;
		}

		/// <summary>
		/// Synchronizes the local database that <paramref name="host"/> has open with the database (if any) by the same name stored on the bucket that plugin configuration allows access to.
		/// </summary>
		/// <param name="host">The <see cref="IPluginHost"/> that hosts the currently-running instance of <see cref="B2SyncExt"/>.</param>
		/// <returns><c>true</c> if the synchronization was successful, or <c>false</c> otherwise.</returns>
		public static async Task<bool> SynchronizeDbAsync(IPluginHost host)
			=> await SynchronizeDbAsync(GetClient(), host);


		/// <summary>
		/// Retrieves the friendly download URL for a database if it exists on B2.
		/// </summary>
		/// <param name="client">The <see cref="B2Client"/> created by <see cref="GetClient"/> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <returns>The friendly download URL of the database on B2, if it exists.</returns>
		public static string GetFriendlyUrl(B2Client client, string dbName)
			=> client?.Files.GetFriendlyDownloadUrl(dbName, client.Capabilities.BucketName);

		/// <summary>
		/// Retrieves the friendly download URL for a database if it exists on B2.
		/// </summary>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <returns>The friendly download URL of the database on B2, if it exists.</returns>
		public static string GetFriendlyUrl(string dbName)
			=> GetFriendlyUrl(GetClient(), dbName);


		/// <summary>
		/// Creates a pre-authorized download URL for the database with <paramref name="dbName"/> that is valid for <paramref name="duration"/>.
		/// </summary>
		/// <param name="client">The <see cref="B2Client"/> created by <see cref="GetClient"/> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <param name="duration">The duration (in seconds) for the link to be valid for. Defaults to 86400s (1 day), minimum of 1s, and maximum of 604800s (1 week).</param>
		/// <returns>A pre-authorized download URL for the database.</returns>
		public static async Task<string> GetDownloadUrlWithAuth(B2Client client, string dbName, int duration = 86400)
			=> GetFriendlyUrl(client, dbName) + "?Authorization=" + (await client.Files.GetDownloadAuthorization(dbName, duration, client.Capabilities.BucketId)).AuthorizationToken;

		/// <summary>
		/// Creates a pre-authorized download URL for the database with <paramref name="dbName"/> that is valid for <paramref name="duration"/>.
		/// </summary>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <param name="duration">The duration (in seconds) for the link to be valid for. Defaults to 86400s (1 day), minimum of 1s, and maximum of 604800s (1 week).</param>
		/// <returns>A pre-authorized download URL for the database.</returns>
		public static async Task<string> GetDownloadUrlWithAuth(string dbName, int duration = 86400)
			=> await GetDownloadUrlWithAuth(GetClient(), dbName, duration);


		/// <summary>
		/// Computes the SHA1 hash of the given <paramref name="input"/> in string (hex) format.
		/// </summary>
		/// <param name="input">A blob of input data to compute the hash of.</param>
		/// <returns>The SHA1 hash of <paramref name="input"/> in string (hex) format for quick comparison.</returns>
		private static string Hash(byte[] input)
		{
			using (SHA1Managed sha1 = new SHA1Managed())
			{
				byte[] hash = sha1.ComputeHash(input);
				return string.Concat(hash.Select(b => b.ToString("x2")));
			}
		}

		/// <summary>
		/// Computes the SHA1 hash of the file at <paramref name="path"/> on disk in string (hex) format.
		/// </summary>
		/// <param name="path">The location on disk of the file to compute the hash of.</param>
		/// <returns>The SHA1 hash of the file at <paramref name="path"/> in string (hex) format for quick comparison.</returns>
		private static string HashFileOnDisk(string path) => Hash(File.ReadAllBytes(path));
	}
}
