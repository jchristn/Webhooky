namespace Webhook.Services
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Storage service.
    /// </summary>
    public class StorageService
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Public-Members

        /// <summary>
        /// Stream buffer size.
        /// </summary>
        public int StreamBufferSize
        {
            get
            {
                return _StreamBufferSize;
            }
            set
            {
                if (value < 1) throw new ArgumentException("StreamBufferSize must be greater than zero.");
                _StreamBufferSize = value;
            }
        }

        /// <summary>
        /// Retrieve the free space in the storage repository.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                string driveLetter = _BaseDirectory;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    driveLetter = Path.GetFullPath(_BaseDirectory).Substring(0, 3);
                }
                else
                {
                    // do nothing
                }

                return new DriveInfo(driveLetter).AvailableFreeSpace;
            }
        }

        #endregion

        #region Private-Members

        private string _BaseDirectory = null;
        private int _StreamBufferSize = 65536;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        /// <param name="directory">Directory.</param>
        public StorageService(string directory)
        {
            if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));

            _BaseDirectory = directory;
            _BaseDirectory = _BaseDirectory.Replace("\\", "/");
            if (!_BaseDirectory.EndsWith("/")) _BaseDirectory += "/";

            if (!Directory.Exists(_BaseDirectory)) Directory.CreateDirectory(_BaseDirectory);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Read data from the supplied object.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>Byte data from the object.</returns>
        public async Task<byte[]> Read(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (!Exists(key)) throw new FileNotFoundException("File does not exist.");
            return await File.ReadAllBytesAsync(GetFilename(key));
        }

        /// <summary>
        /// Access the FileStream for a given object with read permission.  Dispose of the stream when finished.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>FileStream.</returns>
        public FileStream ReadStream(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (!Exists(key)) throw new FileNotFoundException("File does not exist.");
            FileStream fs = new FileStream(GetFilename(key), FileMode.Open, FileAccess.Read);
            return fs;
        }

        /// <summary>
        /// Access the FileStream for a given object with both read and write permissions.  Dispose of the stream when finished.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>FileStream.</returns>
        public FileStream ReadWriteStream(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (!Exists(key)) throw new FileNotFoundException("File does not exist.");
            FileStream fs = new FileStream(GetFilename(key), FileMode.Open, FileAccess.ReadWrite);
            return fs;
        }

        /// <summary>
        /// Write an object.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="data">Byte data from the object.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task Write(string key, byte[] data, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (Exists(key)) throw new FileNotFoundException("File already exists.");
            await File.WriteAllBytesAsync(GetFilename(key), data, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Access the FileStream for a given object with write permission.  Dispose of the stream when finished.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>FileStream.</returns>
        public FileStream WriteStream(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return new FileStream(GetFilename(key), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Write an object.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <param name="contentLength">Number of bytes to write.</param>
        /// <param name="stream">Stream from which data should be read.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task Write(string key, long contentLength, Stream stream, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (Exists(key)) throw new FileNotFoundException("File already exists.");

            using (FileStream fs = new FileStream(GetFilename(key), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                long bytesRemaining = contentLength;
                byte[] buffer = new byte[StreamBufferSize];

                while (bytesRemaining > 0)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                    if (bytesRead > 0)
                    {
                        bytesRemaining -= bytesRead;
                        await fs.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Copy an object.
        /// </summary>
        /// <param name="source">Source key.</param>
        /// <param name="target">Target key.</param>
        /// <param name="contentLength">Content length.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task Copy(string source, string target, long contentLength, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (!Exists(source)) throw new FileNotFoundException("Source does not exist.");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            if (Exists(target)) throw new FileNotFoundException("Target already exists.");

            using (FileStream fsSource = new FileStream(GetFilename(source), FileMode.Open, FileAccess.Read))
            {
                using (FileStream fsTarget = new FileStream(GetFilename(target), FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    long bytesRemaining = contentLength;
                    byte[] buffer = new byte[StreamBufferSize];

                    while (bytesRemaining > 0)
                    {
                        int bytesRead = await fsSource.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                        if (bytesRead > 0)
                        {
                            bytesRemaining -= bytesRead;
                            await fsTarget.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Move the target object with the source object.  The source object will be deleted.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="target">Target object.</param>
        /// <param name="overwrite">Overwrite the target object if it already exists.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Task.</returns>
        public async Task Move(string source, string target, bool overwrite = false, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            if (String.IsNullOrEmpty(target)) throw new ArgumentNullException(nameof(target));
            if (!Exists(source)) throw new FileNotFoundException("Specified source file does not exist.");

            if (Exists(target) && !overwrite)
            {
                throw new InvalidOperationException("Specified target file already exists.");
            }
            else
            {
                Delete(target);
            }

            using (FileStream srcFs = ReadStream(source))
            {
                using (FileStream targetFs = WriteStream(target))
                {
                    await srcFs.CopyToAsync(targetFs, token).ConfigureAwait(false);
                }
            }

            Delete(source);
        }

        /// <summary>
        /// Check of an object exists.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>True if exists.</returns>
        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            return File.Exists(GetFilename(key));
        }

        /// <summary>
        /// Delete an object.
        /// </summary>
        /// <param name="key">Object key.</param>
        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (File.Exists(GetFilename(key))) File.Delete(GetFilename(key));
        }

        /// <summary>
        /// Retrieve the size of a given object.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>Long.</returns>
        public long GetSize(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            FileInfo fi = new FileInfo(GetFilename(key));
            return fi.Length;
        }

        /// <summary>
        /// Retrieve the filename of a given object.
        /// </summary>
        /// <param name="key">Object key.</param>
        /// <returns>Filename.</returns>
        public string GetFilename(string key)
        {
            return _BaseDirectory + key;
        }

        #endregion

        #region Private-Methods

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}