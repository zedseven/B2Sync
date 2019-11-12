using B2Net;
using B2Net.Models;
using KeePass.DataExchange;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace B2Sync
{
	/// <summary>
	/// Facilitates all of the communication and synchronization with Backblaze B2 systems.
	/// </summary>
	public static class Synchronization
	{
		private static readonly string[] RequiredPerms = { "readFiles", "writeFiles" };

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

			//Attempt to establish a connection using the credentials provided
			try
			{
				client = new B2Client(_config.KeyId, _config.ApplicationKey);
			}
			catch (AuthorizationException)
			{
				Interface.UpdateStatus("Unable to authenticate with Backblaze B2 servers. Please make sure the keys you are using are valid.");
				return null;
			}
			catch (SocketException)
			{
				Interface.UpdateStatus("Unable to reach Backblaze B2 servers. Check your internet connection.");
				return null;
			}
			catch (WebException)
			{
				Interface.UpdateStatus("Unable to reach Backblaze B2 servers. Check your internet connection.");
				return null;
			}
			catch (HttpRequestException)
			{
				Interface.UpdateStatus("Unable to reach Backblaze B2 servers. Check your internet connection.");
				return null;
			}
			catch (AggregateException)
			{
				Interface.UpdateStatus("Unable to reach Backblaze B2 servers. Check your internet connection.");
				return null;
			}

			//Verify that the credentials being used are specific to a single bucket, and that we know what that bucket is
			if (string.IsNullOrWhiteSpace(client.Capabilities.BucketName) ||
			    string.IsNullOrWhiteSpace(client.Capabilities.BucketId))
			{
				Interface.UpdateStatus(
					"The key used is not specific to a single bucket. Please create a new key that is restricted to the bucket where you would like to store the database.");
				return null;
			}
			
			//Verify that the credentials have sufficient permissions for the required operations
			if (!RequiredPerms.IsSubsetOf(client.Capabilities.Capabilities))
			{
				Interface.UpdateStatus("The key used does not have the necessary permissions. It is missing the following: " +
				                       string.Join(", ", RequiredPerms.Except(client.Capabilities.Capabilities)));
				return null;
			}

			Interface.UpdateStatus("Connected to B2 successfully.");

			return client;
		}

		/// <summary>
		/// Downloads the database with <paramref name="dbName" /> from the bucket <paramref name="client" /> has access to, and stores it in a directory in the temporary location of the environment.
		/// </summary>
		/// <param name="client">The <see cref="B2Client" /> created by <see cref="GetClient" /> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database to download from the B2 bucket.</param>
		/// <returns>The filepath where the database was downloaded to temporarily, or <see langword="null" /> if the download failed.</returns>
		public static async Task<string> DownloadDbAsync(B2Client client, string dbName)
		{
			if (client == null)
				return null;

			Interface.UpdateStatus("Downloading database...");

			//Download to memory
			B2File file = await client.Files.DownloadByName(dbName, client.Capabilities.BucketName);

			if (file.Size <= 0)
			{
				Interface.UpdateStatus("The database does not exist on B2 to download.");
				return null;
			}

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
		/// <returns>The filepath where the database was downloaded to temporarily, or <see langword="null" /> if the download failed.</returns>
		public static async Task<string> DownloadDbAsync(string dbName)
		{
			return await DownloadDbAsync(GetClient(), dbName);
		}


		/// <summary>
		/// Uploads the locally-stored database <paramref name="localDb" /> to the bucket that <paramref name="client" /> has access to.
		/// </summary>
		/// <param name="client">The <see cref="B2Client" /> created by <see cref="GetClient" /> with access to a bucket to upload to.</param>
		/// <param name="localDb">The local database to upload.</param>
		/// <returns><see langword="true" /> if the upload was successful, or <see langword="false" /> otherwise.</returns>
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
				B2UploadUrl uploadUrl = await client.Files.GetUploadUrl(client.Capabilities.BucketId);
				B2File file = await client.Files.Upload(fileData, Path.GetFileName(localPath), uploadUrl, true,
					client.Capabilities.BucketId);
			}
			catch (SocketException)
			{
				Interface.UpdateStatus("Unable to upload the database to B2.");
				return false;
			}
			catch (WebException)
			{
				Interface.UpdateStatus("Unable to upload the database to B2.");
				return false;
			}
			catch (HttpRequestException)
			{
				Interface.UpdateStatus("Unable to upload the database to B2.");
				return false;
			}
			catch (AggregateException)
			{
				Interface.UpdateStatus("Unable to upload the database to B2.");
				return false;
			}

			Interface.UpdateStatus("Database upload successful.");

			return true;
		}

		/// <summary>
		/// Uploads the locally-stored database <paramref name="localDb" /> to the bucket that plugin configuration allows access to.
		/// </summary>
		/// <param name="localDb">The local database to upload.</param>
		/// <returns><see langword="true" /> if the upload was successful, or <see langword="false" /> otherwise.</returns>
		public static async Task<bool> UploadDbAsync(PwDatabase localDb)
		{
			return await UploadDbAsync(GetClient(), localDb);
		}


		/// <summary>
		/// Synchronizes the local database that <paramref name="host" /> has open with the database (if any) by the same name stored on the bucket that <paramref name="client" /> has access to.
		/// </summary>
		/// <param name="client">The <see cref="B2Client" /> created by <see cref="GetClient" /> with access to a bucket to synchronize with.</param>
		/// <param name="host">The <see cref="IPluginHost" /> that hosts the currently-running instance of <see cref="B2SyncExt" />.</param>
		/// <returns><see langword="true" /> if the synchronization was successful, or <see langword="false" /> otherwise.</returns>
		public static async Task<bool> SynchronizeDbAsync(B2Client client, IPluginHost host)
		{
			if(client == null)
				return false;

			Synchronizing = true;

			Interface.UpdateStatus("Synchronizing database with B2...");

			//Download the remote copy
			PwDatabase sourceDb = host.Database;
			string remoteDbPath = await DownloadDbAsync(client, GetDbFileName(sourceDb));

			bool localMatchesRemote = true;

			//If the file exists on the remote server, synchronize it with the local copy
			if(remoteDbPath != null)
			{
				string localHash = HashFileOnDisk(sourceDb.IOConnectionInfo.Path);
				string remoteHash = HashFileOnDisk(remoteDbPath);

				localMatchesRemote = localHash == remoteHash;
				
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
					{
						Interface.UpdateStatus("Something went wrong while synchronizing the local copy with the remote one.");
						return false;
					}
				}
			}

			//Upload the local copy to the server once all synchronization is completed
			bool uploadResult = ((!localMatchesRemote || remoteDbPath == null) && await UploadDbAsync(client, sourceDb)) || localMatchesRemote;

			if (uploadResult)
				if (!localMatchesRemote || remoteDbPath == null)
					Interface.UpdateStatus("Synchronized database with B2 successfully.");
				else
					Interface.UpdateStatus(
						"No synchronization was necessary. The database is in sync with the copy on B2.");
			else
				Interface.UpdateStatus("Something went wrong while uploading to B2.");

			Synchronizing = false;

			return uploadResult;
		}

		/// <summary>
		/// Synchronizes the local database that <paramref name="host" /> has open with the database (if any) by the same name stored on the bucket that plugin configuration allows access to.
		/// </summary>
		/// <param name="host">The <see cref="IPluginHost" /> that hosts the currently-running instance of <see cref="B2SyncExt" />.</param>
		/// <returns><see langword="true" /> if the synchronization was successful, or <see langword="false" /> otherwise.</returns>
		public static async Task<bool> SynchronizeDbAsync(IPluginHost host)
		{
			return await SynchronizeDbAsync(GetClient(), host);
		}


		/// <summary>
		/// Retrieves the friendly download URL for a database if it exists on B2.
		/// </summary>
		/// <param name="client">The <see cref="B2Client" /> created by <see cref="GetClient" /> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <returns>The friendly download URL of the database on B2, if it exists.</returns>
		public static string GetFriendlyUrl(B2Client client, string dbName)
		{
			return client != null ? client.Files.GetFriendlyDownloadUrl(dbName, client.Capabilities.BucketName) : null;
		}

		/// <summary>
		/// Retrieves the friendly download URL for a database if it exists on B2.
		/// </summary>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <returns>The friendly download URL of the database on B2, if it exists.</returns>
		public static string GetFriendlyUrl(string dbName)
		{
			return GetFriendlyUrl(GetClient(), dbName);
		}


		/// <summary>
		/// Creates a pre-authorized download URL for the database with <paramref name="dbName" /> that is valid for <paramref name="duration" />.
		/// </summary>
		/// <param name="client">The <see cref="B2Client" /> created by <see cref="GetClient" /> with access to a bucket (hopefully containing the DB).</param>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <param name="duration">The duration (in seconds) for the link to be valid for. Defaults to 86400s (1 day), minimum of 1s, and maximum of 604800s (1 week).</param>
		/// <returns>A pre-authorized download URL for the database.</returns>
		public static async Task<string> GetDownloadUrlWithAuth(B2Client client, string dbName, int duration = 86400)
		{
			return client == null
				? null
				: GetFriendlyUrl(client, dbName) + "?Authorization=" +
				  (await client.Files.GetDownloadAuthorization(dbName, duration, client.Capabilities.BucketId))
				  .AuthorizationToken;
		}

		/// <summary>
		/// Creates a pre-authorized download URL for the database with <paramref name="dbName" /> that is valid for <paramref name="duration" />.
		/// See the <a href="https://www.backblaze.com/b2/docs/b2_get_download_authorization.html">B2 API doc</a> for more detail.
		/// </summary>
		/// <param name="dbName">The filename (with extension) of the database on the B2 bucket.</param>
		/// <param name="duration">The duration (in seconds) for the link to be valid for. Defaults to 86400s (1 day), minimum of 1s, and maximum of 604800s (1 week).</param>
		/// <returns>A pre-authorized download URL for the database.</returns>
		public static async Task<string> GetDownloadUrlWithAuth(string dbName, int duration = 86400)
		{
			return await GetDownloadUrlWithAuth(GetClient(), dbName, duration);
		}


		/// <summary>
		/// Returns the filename of a given <see cref="PwDatabase" /> on disk.
		/// </summary>
		/// <param name="db">The password database to get the filename of.</param>
		/// <returns>The filename of <paramref name="db" />.</returns>
		public static string GetDbFileName(PwDatabase db)
		{
			return Path.GetFileName(db.IOConnectionInfo.Path);
		}


		/// <summary>
		/// Computes the SHA1 hash of the given <paramref name="input" /> in string (hex) format.
		/// </summary>
		/// <param name="input">A blob of input data to compute the hash of.</param>
		/// <returns>The SHA1 hash of <paramref name="input" /> in string (hex) format for quick comparison.</returns>
		private static string Hash(byte[] input)
		{
			using (SHA1Managed sha1 = new SHA1Managed())
			{
				byte[] hash = sha1.ComputeHash(input);
				return string.Concat(hash.Select(b => b.ToString("x2")));
			}
		}

		/// <summary>
		/// Computes the SHA1 hash of the file at <paramref name="path" /> on disk in string (hex) format.
		/// </summary>
		/// <param name="path">The location on disk of the file to compute the hash of.</param>
		/// <returns>The SHA1 hash of the file at <paramref name="path" /> in string (hex) format for quick comparison.</returns>
		private static string HashFileOnDisk(string path)
		{
			return Hash(File.ReadAllBytes(path));
		}


		/// <summary>
		/// Determines whether a set is a subset of another.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="first" /> and <paramref name="second" />.</typeparam>
		/// <param name="first">An <see cref="IEnumerable{T}" /> that is unknown to be a subset of <paramref name="second" />.</param>
		/// <param name="second">An <see cref="IEnumerable{T}" /> that may fully contain <paramref name="first" />.</param>
		/// <returns>Whether or not <paramref name="first" /> is a subset of <paramref name="second" />.</returns>
		private static bool IsSubsetOf<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			return !first.Except(second).Any();
		}
	}
}
