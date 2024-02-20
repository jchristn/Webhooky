namespace Webhook
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Webhook event status enumeration.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WebhookEventStatusEnum
    {
        /// <summary>
        /// Created.
        /// </summary>
        [EnumMember(Value = "Created")]
        Created,
        /// <summary>
        /// AttemptFailed.
        /// </summary>
        [EnumMember(Value = "AttemptFailed")]
        AttemptFailed,
        /// <summary>
        /// TaskFailed.
        /// </summary>
        [EnumMember(Value = "TaskFailed")]
        TaskFailed,
        /// <summary>
        /// Succeeded.
        /// </summary>
        [EnumMember(Value = "Succeeded")]
        Succeeded,
        /// <summary>
        /// Expired.
        /// </summary>
        [EnumMember(Value = "Expired")]
        Expired,
    }
}
