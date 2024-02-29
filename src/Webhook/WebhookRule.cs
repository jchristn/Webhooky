namespace Webhook
{
    using System;
    using System.Text.Json.Serialization;
    using Watson.ORM.Core;

    /// <summary>
    /// Webhook rule.
    /// </summary>
    [Table("webhookrules")]
    public class WebhookRule
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
        /// Name.
        /// </summary>
        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name { get; set; } = null;

        /// <summary>
        /// Operation type.
        /// </summary>
        [Column("operationtype", false, DataTypes.Nvarchar, 512, false)]
        public string OperationType { get; set; } = null;

        /// <summary>
        /// Maximum number of attempts.
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
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxAttempts));
                _MaxAttempts = value;
            }
        }

        /// <summary>
        /// Retry interval in milliseconds.
        /// </summary>
        [Column("retryintervalms", false, DataTypes.Int, false)]
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
        /// Timeout in milliseconds.
        /// </summary>
        [Column("timeoutms", false, DataTypes.Int, false)]
        public int TimeoutMs
        {
            get
            {
                return _TimeoutMs;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(TimeoutMs));
                _TimeoutMs = value;
            }
        }

        #endregion

        #region Private-Members

        private int _MaxAttempts = 10; 
        private int _RetryIntervalMs = (30 * 1000); // 30 seconds
        private int _TimeoutMs = (60 * 1000); // 1 minute

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebhookRule()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
