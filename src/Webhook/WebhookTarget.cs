namespace Webhook
{
    using System;
    using System.Net.Http;
    using System.Text.Json.Serialization;
    using Watson.ORM.Core;

    /// <summary>
    /// Webhook target.
    /// </summary>
    [Table("webhooktargets")]
    public class WebhookTarget
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
        /// HTTP status to expect for the request to be considered successful.
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

        #endregion

        #region Private-Members

        private string _Url = null;
        private Uri _Uri = null;
        private string _ContentType = "application/json";
        private int _ExpectStatus = 200;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebhookTarget()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
