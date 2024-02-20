namespace Webhook
{
    using System;
    using System.Text.Json.Serialization;
    using Watson.ORM.Core;

    /// <summary>
    /// Webhook event.
    /// </summary>
    [Table("webhookevents")]
    public class WebhookEvent
    {
        #region Public-Members

        /// <summary>
        /// ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// GUID.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Target GUID.
        /// </summary>
        [Column("targetguid", false, DataTypes.Nvarchar, 64, false)]
        public string TargetGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Rule GUID.
        /// </summary>
        [Column("ruleguid", false, DataTypes.Nvarchar, 64, false)]
        public string RuleGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Operation type.
        /// </summary>
        [Column("operationtype", false, DataTypes.Nvarchar, 64, false)]
        public string OperationType { get; set; } = null;

        /// <summary>
        /// Content length.
        /// </summary>
        [Column("contentlength", false, DataTypes.Int, false)]
        public int ContentLength
        {
            get
            {
                return _ContentLength;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(ContentLength));
                _ContentLength = value;
            }
        }

        /// <summary>
        /// URL.
        /// </summary>
        [Column("url", false, DataTypes.Nvarchar, 256, false)]
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Url));
                _Uri = new Uri(value);
                _Url = value;
            }
        }

        /// <summary>
        /// URI.
        /// </summary>
        public Uri Uri
        {
            get
            {
                return _Uri;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Uri));
                _Uri = value;
                _Url = _Uri.ToString();
            }
        }

        /// <summary>
        /// Content type.
        /// </summary>
        [Column("contenttype", false, DataTypes.Nvarchar, 128, false)]
        public string ContentType
        {
            get
            {
                return _ContentType;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(ContentType));
                _ContentType = value;
            }
        }

        /// <summary>
        /// HTTP status to expect on success.
        /// </summary>
        [Column("expectstatus", false, DataTypes.Int, false)]
        public int ExpectStatus
        {
            get
            {
                return _ExpectStatus;
            }
            set
            {
                if (value < 100 || value > 599) throw new ArgumentOutOfRangeException(nameof(ExpectStatus));
                _ExpectStatus = value;
            }
        }

        /// <summary>
        /// Retry interval in milliseconds.
        /// </summary>
        public int RetryIntervalMs
        {
            get
            {
                return _RetryIntervalMs;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(RetryIntervalMs));
                _RetryIntervalMs = value;
            }
        }

        /// <summary>
        /// Attempt number.
        /// </summary>
        [Column("attemptnumber", false, DataTypes.Int, false)]
        public int Attempt
        {
            get
            {
                return _Attempt;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Attempt));
                _Attempt = value;
            }
        }

        /// <summary>
        /// Maximum attempts.
        /// </summary>
        [Column("maxattempts", false, DataTypes.Int, false)]
        public int MaxAttempts
        {
            get
            {
                return _MaxAttempts;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxAttempts));
                _MaxAttempts = value;
            }
        }

        /// <summary>
        /// HTTP status last received.
        /// </summary>
        [Column("httpstatus", false, DataTypes.Int, false)]
        public int HttpStatus
        {
            get
            {
                return _HttpStatus;
            }
            set
            {
                if (value < 0 || value > 599) throw new ArgumentOutOfRangeException(nameof(HttpStatus));
                _HttpStatus = value;
            }
        }

        /// <summary>
        /// Timestamp when added, UTC.
        /// </summary>
        [Column("addedutc", false, DataTypes.DateTime, false)]
        public DateTime AddedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when last attempted, UTC.
        /// </summary>
        [Column("lastattemptutc", false, DataTypes.DateTime, true)]
        public DateTime? LastAttemptUtc { get; set; } = null;

        /// <summary>
        /// Timestamp for next attempt, UTC.
        /// </summary>
        [Column("nextattemptutc", false, DataTypes.DateTime, true)]
        public DateTime? NextAttemptUtc { get; set; } = null;

        /// <summary>
        /// Timestamp for last failure, UTC.
        /// </summary>
        [Column("lastfailureutc", false, DataTypes.DateTime, true)]
        public DateTime? LastFailureUtc { get; set; } = null;

        /// <summary>
        /// Timestamp for success, UTC.
        /// </summary>
        [Column("successutc", false, DataTypes.DateTime, true)]
        public DateTime? SuccessUtc { get; set; } = null;
        
        /// <summary>
        /// Timestamp for failed, UTC.
        /// </summary>
        [Column("failedutc", false, DataTypes.DateTime, true)]
        public DateTime? FailedUtc { get; set; } = null;

        #endregion

        #region Private-Members

        private string _Url = null;
        private Uri _Uri = null;
        private string _ContentType = "application/json";
        private int _ContentLength = 0;
        private int _ExpectStatus = 200;
        private int _RetryIntervalMs = 10000;
        private int _HttpStatus = 0;
        private int _Attempt = 0;
        private int _MaxAttempts = 5;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebhookEvent()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
