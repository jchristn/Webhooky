namespace Webhook
{
    using DatabaseWrapper.Core;
    using System;

    /// <summary>
    /// Webhook settings.
    /// </summary>
    public class WebhookSettings
    {
        #region Public-Members

        /// <summary>
        /// Database settings.
        /// </summary>
        public DatabaseSettings Database
        {
            get
            {
                return _Database;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Database));
                _Database = value;
            }
        }
        
        /// <summary>
        /// Directory to store request body data.
        /// </summary>
        public string RequestsDirectory
        {
            get
            {
                return _RequestsDirectory;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(RequestsDirectory));
                value = value.Replace("\\", "/");
                if (!value.EndsWith("/")) value += "/";
                _RequestsDirectory = value;
            }
        }

        /// <summary>
        /// Directory to store response body data.
        /// </summary>
        public string ResponsesDirectory
        {
            get
            {
                return _ResponsesDirectory;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(ResponsesDirectory));
                value = value.Replace("\\", "/");
                if (!value.EndsWith("/")) value += "/";
                _ResponsesDirectory = value;
            }
        }

        /// <summary>
        /// Polling interval for tasks to run or events to expire.
        /// </summary>
        public int PollIntervalMs
        {
            get
            {
                return _PollIntervalMs;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(PollIntervalMs));
                _PollIntervalMs = value;
            }
        }

        /// <summary>
        /// Number of milliseconds to persist response body data.
        /// </summary>
        public int ResponseRetentionMs
        {
            get
            {
                return _ResponseRetentionMs;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(ResponseRetentionMs));
                _ResponseRetentionMs = value;
            }
        }

        /// <summary>
        /// Batch size for processing tasks to run or events to expire.
        /// </summary>
        public int BatchSize
        {
            get
            {
                return _BatchSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(BatchSize));
                _BatchSize = value;
            }
        }

        #endregion

        #region Private-Members

        private DatabaseSettings _Database = new DatabaseSettings("./webhooks.db");
        private string _RequestsDirectory = "./requests/";
        private string _ResponsesDirectory = "./responses/";
        private int _PollIntervalMs = 10000;
        private int _ResponseRetentionMs = (1000 * 60 * 60 * 24); // 1 day
        private int _BatchSize = 32;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebhookSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
