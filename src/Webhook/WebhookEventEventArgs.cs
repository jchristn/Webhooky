namespace Webhook
{
    using System;

    /// <summary>
    /// Webhook event arguments.
    /// </summary>
    public class WebhookEventArgs : EventArgs
    {
        #region Public-Members

        /// <summary>
        /// Webhook event.
        /// </summary>
        public WebhookEvent Event { get; set; } = null;

        /// <summary>
        /// Status.
        /// </summary>
        public WebhookEventStatusEnum Status { get; set; } = WebhookEventStatusEnum.Created;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public WebhookEventArgs()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
